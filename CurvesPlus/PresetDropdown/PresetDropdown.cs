using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Serialization;
using PaintDotNet;

namespace pyrochild.effects.common
{
    public sealed partial class PresetDropdown<T> : Control
        where T : ICloneable
    {
        private FileSystemWatcher fsw;

        public PresetDropdown(IServiceProvider ServiceProvider, string OwnerName, T DefaultPreset, XmlAttributeOverrides XmlAttributeOverrides)
        {
            InitializeComponent();
            Services = ServiceProvider;
            this.OwnerName = OwnerName;

            defaultPreset = current = DefaultPreset;

            xao = XmlAttributeOverrides;

            this.SetStyle(
                ControlStyles.FixedHeight
                | ControlStyles.Selectable
                | ControlStyles.ResizeRedraw,
                true);

            SuspendEvents();
            PopulateDropdown();
            ResumeEvents();
        }

        void fsw_Event(object sender, EventArgs e)
        {
            if (!EventsSuspended)
            {
                Form owner = this.FindForm();
                if (owner != null)
                {
                    if (!owner.IsHandleCreated)
                    {
                        owner.CreateControl();
                    }
                    Action action = delegate
                                    {
                                        string name = CurrentName;
                                        T preset = current;
                                        PopulateDropdown();
                                        current = preset;
                                        SetPresetByName(name);
                                    };
                    try
                    {
                        this.Invoke(action);
                    }
                    catch { }
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            var dir = GetPresetDir();
            if (dir != null)
            {
                fsw = new FileSystemWatcher(GetPresetDir());
                fsw.Changed += new FileSystemEventHandler(fsw_Event);
                fsw.Created += new FileSystemEventHandler(fsw_Event);
                fsw.Deleted += new FileSystemEventHandler(fsw_Event);
                fsw.Renamed += new RenamedEventHandler(fsw_Event);
                fsw.EnableRaisingEvents = true;
            }
        }

        private void PopulateDropdown()
        {
            try
            {
                comboBox.Items.Clear();
                comboBox.Items.Add(new PresetDropdownItem<T>("Default", defaultPreset));
                comboBox.Items.Add(new PresetDropdownItem<T>("Custom", current));
                comboBox.Items.Add(new PresetDropdownItem<T>());
                comboBox.Items.AddRange(LoadPresets());
                comboBox.Items.Add(new PresetDropdownItem<T>());
                comboBox.Items.Add(new PresetDropdownItem<T>("Save current as preset...", SavePreset));
                comboBox.Items.Add(new PresetDropdownItem<T>("Manage presets...", ManagePresets));
                comboBox.SelectedIndex = 0;
            }
            catch(Exception e)
            {
                SuspendEvents();
                comboBox.Items.Clear();
                comboBox.Items.Add(new PresetDropdownItem<T>(e.ToString(), () => { }));
                comboBox.SelectedIndex = 0;
                using (var g = Graphics.FromImage(new Bitmap(1, 1)))
                {
                    comboBox.DropDownWidth = (int)g.MeasureString(e.ToString(), Font).Width;
                }
            }
        }

        public PresetDropdownItem<T> this[int index]
        {
            get
            {
                return (PresetDropdownItem<T>)comboBox.Items[index];
            }
        }

        private void SavePreset()
        {
            string name;
            if (InputBox.Show(this, "Type a name for the new preset.", null, null, Path.GetInvalidFileNameChars(), InputBox.ValidationMode.Blacklist, out name) == DialogResult.OK)
            {
                if (name == "") name = "Untitled Preset";

                var path = Path.Combine(GetPresetDir(), Path.ChangeExtension(name, ".xml"));
                if (!File.Exists(path)
                    || MessageBox.Show(
                        this,
                        "The file already exists. Would you like to replace the file with the new preset?",
                        "", 
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2) == DialogResult.Yes)
                {
                    AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

                    FileStream fs = null;
                    try
                    {
                        fs = new FileStream(path, FileMode.Create);
                        xmlSerializer.Serialize(fs, current);
                        fs.Close();
                        T preset = current;
                        SuspendEvents();
                        PopulateDropdown();
                        ResumeEvents();
                        current = preset;
                        SetPresetByName(name);
                    }
                    catch(Exception e)
                    {
                        MessageBox.Show(this, "Error saving preset:\n\n" + e.ToString(), "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        if (fs != null)
                        {
                            fs.Dispose();
                        }
                    }

                    AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
                }
            }
        }

        private void ManagePresets()
        {
            Services.GetService<PaintDotNet.AppModel.IShellService>().LaunchFolder(this, GetPresetDir());
        }

        private PresetDropdownItem<T>[] LoadPresets()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

            var ret = new List<PresetDropdownItem<T>>();
            var dir = GetPresetDir();
            if (dir == null)
            {
                throw new IOException("Couldn't access Preset directory");
            }
            foreach (string file in Directory.GetFiles(dir))
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(file, FileMode.Open);
                    T t = LoadPreset(fs);
                    if (t != null)
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        if (name.ToUpperInvariant() == "DEFAULT")
                        {
                            defaultPreset = t;
                            comboBox.Items.RemoveAt(0);
                            comboBox.Items.Insert(0, new PresetDropdownItem<T>("Default", defaultPreset));
                        }
                        else
                        {
                            ret.Add(new PresetDropdownItem<T>(name, t));
                        }
                    }
                }
                catch (Exception)
                {
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;

            return ret.ToArray();
        }

        private T LoadPreset(Stream stream)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var retval = (T)xmlSerializer.Deserialize(stream);
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
            return retval;
        }

        object lastsel;
        void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!EventsSuspended)
            {
                SuspendEvents();
                var item = comboBox.SelectedItem as PresetDropdownItem<T>;
                switch (item.Type)
                {
                    case PresetDropdownItem<T>.ItemType.Separator:
                    case PresetDropdownItem<T>.ItemType.Command:
                        if (item.Action != null)
                        {
                            item.Action();
                        }
                        comboBox.SelectedItem = lastsel;
                        break;
                    case PresetDropdownItem<T>.ItemType.Preset:
                        OnPresetChanged(/*item*/);
                        break;
                }
                lastsel = comboBox.SelectedItem;
                ResumeEvents();
            }
        }

        //private void OnPresetChanged(PresetDropdownItem<T> item)
        //{
        //    var handler = PresetChanged;
        //    if (handler != null)
        //    {
        //        T newpreset = default(T);
        //        if (item.Name == "Custom")
        //        {
        //            newpreset = current;
        //        }
        //        else if (item.Preset != null)
        //        {
        //            newpreset = (T)item.Preset.Clone();
        //        }
        //        current = newpreset;
        //        var args = new PresetChangedEventArgs<T>(item.Name, newpreset);
        //        handler(this, args);
        //    }
        //}

        public void OnPresetChanged()
        {
            var item = comboBox.SelectedItem as PresetDropdownItem<T>;
            if (item != null)
            {
                var handler = PresetChanged;
                if (handler != null)
                {
                    T newpreset = default(T);
                    if (item.Name == "Custom")
                    {
                        newpreset = current;
                    }
                    else if (item.Preset != null)
                    {
                        newpreset = (T)item.Preset.Clone();
                    }
                    current = newpreset;
                    var args = new PresetChangedEventArgs<T>(item.Name, newpreset);
                    handler(this, args);
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            bool handled = false;
            if (!EventsSuspended)
            {
                int index = comboBox.SelectedIndex;
                switch (keyData)
                {
                    case Keys.Up:
                        handled = true;
                        if (index > 0)
                        {
                            switch (((PresetDropdownItem<T>)comboBox.Items[index - 1]).Type)
                            {
                                case PresetDropdownItem<T>.ItemType.Separator:
                                    comboBox.SelectedIndex -= 2;
                                    break;
                                default:
                                    comboBox.SelectedIndex--;
                                    break;
                            }
                        }
                        break;
                    case Keys.Down:
                        handled = true;
                        if (index < comboBox.Items.Count - 1)
                        {
                            switch (((PresetDropdownItem<T>)comboBox.Items[index + 1]).Type)
                            {
                                case PresetDropdownItem<T>.ItemType.Separator:
                                    var itemtogoto = (PresetDropdownItem<T>)comboBox.Items[index + 2];
                                    if (itemtogoto.Type == PresetDropdownItem<T>.ItemType.Preset)
                                        comboBox.SelectedIndex += 2;
                                    break;
                                default:
                                    comboBox.SelectedIndex++;
                                    break;
                            }
                        }
                        break;
                }
            }
            return handled || base.ProcessCmdKey(ref msg, keyData);
        }

        public void DefaultMeasureItem(object sender, MeasureItemEventArgs e)
        {
            switch (drawMode)
            {
                case DrawMode.Normal:
                case DrawMode.OwnerDrawFixed:
                    e.ItemHeight = comboBox.ItemHeight;
                    break;
                case DrawMode.OwnerDrawVariable:
                    if (MeasureItem != null)
                    {
                        MeasureItem(sender, e);
                    }
                    break;
            }
        }

        public void DefaultDrawItem(object sender, DrawItemEventArgs e)
        {
            var item = comboBox.Items[e.Index] as PresetDropdownItem<T>;
            switch (drawMode)
            {
                case DrawMode.Normal:
                    switch (item.Type)
                    {
                        case PresetDropdownItem<T>.ItemType.Separator:
                            float midy = (e.Bounds.Top + e.Bounds.Bottom) / 2f;
                            e.Graphics.DrawLine(
                                new Pen(SystemColors.Highlight),
                                e.Bounds.Left + 5,
                                midy,
                                e.Bounds.Right - 5,
                                midy);
                            break;

                        default:
                            e.DrawBackground();
                            e.DrawFocusRectangle();
                            e.Graphics.DrawString(
                                item.Name,
                                comboBox.Font,
                                new SolidBrush(e.ForeColor),
                                e.Bounds);
                            break;
                    }
                    break;
                case DrawMode.OwnerDrawFixed:
                case DrawMode.OwnerDrawVariable:
                    if (DrawItem != null)
                    {
                        DrawItem(sender, e);
                    }
                    else
                    {
                        goto case DrawMode.Normal;
                    }
                    break;
            }
        }

        public string GetPresetDir()
        {
            try
            {
                var retval = Path.Combine(Path.Combine(
                            //Services.GetService<IAppInfoService>().UserDataDirectory,
                            Services.GetService<PaintDotNet.AppModel.IUserFilesService>().UserFilesPath,
                            "Effect Presets"),
                            OwnerName);
                if (!Directory.Exists(retval))
                    Directory.CreateDirectory(retval);
                return retval;
            }
            catch
            {
                return null;
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly = null;

            //event subscribers get first dibs
            if (AssemblyResolve != null)
            {
                assembly = AssemblyResolve(sender, args);
                if (assembly != null) return assembly;
            }

            //then we try
            try
            {
                assembly = Assembly.Load(args.Name);
                if (assembly != null) return assembly;
            }
            catch { }

            return Assembly.GetExecutingAssembly();
        }

        private IServiceProvider Services;

        public string OwnerName
        { get; private set; }

        private T defaultPreset;
        public T Default { get { return (T)defaultPreset.Clone(); } }

        private T current;
        public T Current
        {
            get
            {
                //SwitchToCustom();
                return current;
            }
            set
            {
                SwitchToCustom();
                current = value;
            }
        }

        public string CurrentName
        {
            get
            {
                var item = comboBox.SelectedItem as PresetDropdownItem<T>;
                if (item != null)
                {
                    return item.Name;
                }
                else { return null; }
            }
        }

        private XmlSerializer xs;
        private XmlSerializer xmlSerializer
        {
            get
            {
                if (xs == null)
                {
                    xs = new XmlSerializer(typeof(T), XmlAttributeOverrides, new Type[] { typeof(Color) }, null, null);
                }
                return xs;
            }
        }

        private XmlAttributeOverrides xao;
        public XmlAttributeOverrides XmlAttributeOverrides
        {
            get
            {
                return xao;
            }
        }

        private int suspendcount = 0;
        private bool EventsSuspended { get { return suspendcount > 0; } }
        private void SuspendEvents() { suspendcount++; }
        private void ResumeEvents() { suspendcount--; }

        private void SwitchToCustom()
        {
            //if (!EventsSuspended)
            {
                SuspendEvents();
                if (comboBox.Items.Count > 1)
                {
                    comboBox.SelectedIndex = 1;
                }
                ResumeEvents();
            }
        }

        private DrawMode drawMode = DrawMode.Normal;
        public DrawMode DrawMode
        {
            get { return drawMode; }
            set
            {
                drawMode = value;
                comboBox.DrawMode = DrawMode.OwnerDrawVariable;
            }
        }

        public event PresetChangedEventHandler<T> PresetChanged;
        public event MeasureItemEventHandler MeasureItem;
        public event DrawItemEventHandler DrawItem;
        public static event ResolveEventHandler AssemblyResolve;

        public void SetPresetByName(string name)
        {
            foreach (PresetDropdownItem<T> item in comboBox.Items)
            {
                if (item.Type == PresetDropdownItem<T>.ItemType.Preset
                    && item.Name == name)
                {
                    comboBox.SelectedItem = item;
                    return;
                }
            }
            if (name != null)
                SwitchToCustom();
        }

        public void AddPreset(T preset, string name)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            var filename = Path.ChangeExtension(name, ".xml");
            var dir = GetPresetDir();
            if (dir != null)
            {
                var path = Path.Combine(dir, filename);
                if (!File.Exists(path))
                {
                    using (FileStream fs = new FileStream(path, FileMode.CreateNew))
                    {
                        xmlSerializer.Serialize(fs, preset);
                    }
                    PopulateDropdown();
                }
            }
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
        }

        public void AddPreset(Stream stream, string name)
        {
            AddPreset(LoadPreset(stream), name);
        }
    }
}