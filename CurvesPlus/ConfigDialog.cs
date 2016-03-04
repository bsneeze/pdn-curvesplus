/////////////////////////////////////////////////////////////////////////////////
// Paint.NET                                                                   //
// Copyright (C) dotPDN LLC, Rick Brewster, Tom Jackson, and contributors.     //
// Portions Copyright (C) Microsoft Corporation. All Rights Reserved.          //
// See src/Resources/Files/License.txt for full licensing and attribution      //
// details.                                                                    //
// .                                                                           //
// Modifications Copyright © 2007-2016 Zach Walker                             //
/////////////////////////////////////////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows.Forms;
using System.Xml.Serialization;
using PaintDotNet;
using PaintDotNet.Effects;
using pyrochild.effects.common;

namespace pyrochild.effects.curvesplus
{
    public sealed class ConfigDialog
        : EffectConfigDialog
    {
        private CurveControl curveControl;
        private Dictionary<ChannelMode, CurveControl> curveControls;
        private System.ComponentModel.IContainer components = null;
        private EventHandler curveControlValueChangedDelegate;
        private EventHandler<EventArgs<Point>> curveControlCoordinatesChangedDelegate;
        private ResourceManager rm = Properties.Resources.ResourceManager;
        private CheckBox[] maskCheckBoxes;
        private EventHandler maskCheckChanged;
        private TableLayoutPanel tableLayoutMain;
        private TableLayoutPanel tableLayoutPanelMask;
        private ComboBox modeComboBox;
        private Label labelCoordinates;
        private Label labelHelpText;
        private Button cancelButton;
        private Button okButton;
        private Button resetButton;
        private Channel InputMode;
        private Channel OutputMode;
        private RadioButton optSpline;
        private RadioButton optLine;
        private ComboBox[] modes = new ComboBox[2];
        private RadioButton optDraw;
        private TableLayoutPanel tableLayoutPanel1;
        private PresetDropdown<CurvesPlusXMLFile> presetDropdown;
        private long[][] histogram;

        public ConfigDialog()
        {
            InitializeComponent();

            curveControlValueChangedDelegate = this.curveControl_ValueChanged;
            curveControlCoordinatesChangedDelegate = this.curveControl_CoordinatesChanged;

            optLine.Image = new Bitmap(typeof(CurvesPlus), "images.line.png");
            optSpline.Image = new Bitmap(typeof(CurvesPlus), "images.spline.png");
            optDraw.Image = new Bitmap(typeof(CurvesPlus), "images.pencil.png");

            this.Text = CurvesPlus.StaticDialogName;
            this.cancelButton.Text = "Cancel";
            this.okButton.Text = "OK";
            this.resetButton.Text = "Reset";
            this.modeComboBox.Items.Clear();
            foreach (string s in Enum.GetNames(typeof(ChannelMode)))
            {
                this.modeComboBox.Items.Add(rm.GetString(s));
            }

            this.maskCheckChanged = new EventHandler(MaskCheckChanged);

            this.curveControls = new Dictionary<ChannelMode, CurveControl>();
            this.curveControls.Add(ChannelMode.Rgb, new CurveControlRgb());
            this.curveControls.Add(ChannelMode.L, new CurveControlLuminosity());
            this.curveControls.Add(ChannelMode.A, new CurveControlAlpha());
            this.curveControls.Add(ChannelMode.Cmyk, new CurveControlCmyk());
            this.curveControls.Add(ChannelMode.Advanced, new CurveControlAdvanced());
            this.curveControls.Add(ChannelMode.Hsv, new CurveControlHsv());
        }

        private void AddDefaultPresets()
        {
           // presetDropdown.SuspendEvents();
            string resheader = "pyrochild.effects.curvesplus.Presets.";
            foreach (string resname in Assembly.GetExecutingAssembly().GetManifestResourceNames())
            {
                if (resname.StartsWith(resheader))
                {
                    string name = Path.GetFileNameWithoutExtension(resname.Substring(resheader.Length));

                    presetDropdown.AddPreset(Assembly.GetExecutingAssembly().GetManifestResourceStream(resname), name);
                }
            }
            //presetDropdown.PopulateDropdown();
            //presetDropdown.ResumeEvents();
        }

        void presetDropdown_PresetChanged(object sender, PresetChangedEventArgs<CurvesPlusXMLFile> e)
        {
            if (!TokenUpdatesSuspended)
                InitDialogFromToken(e.Preset.ToConfigToken());
            FinishTokenUpdate();
            curveControl.Invalidate();
        }

        private static XmlAttributeOverrides GetXao()
        {
            XmlAttributeOverrides xao = new XmlAttributeOverrides();

            XmlAttributes xa1 = new XmlAttributes();
            xa1.XmlAttribute = new XmlAttributeAttribute();

            xao.Add(typeof(Point), "X", xa1);
            xao.Add(typeof(Point), "Y", xa1);

            XmlAttributes xa2 = new XmlAttributes();
            xa2.XmlArrayItems.Add(new XmlArrayItemAttribute("Curve", typeof(Point[])));

            xao.Add(typeof(CurvesPlusXMLFile), "ControlPoints", xa2);

            return xao;
        }

        protected override void InitialInitToken()
        {
            theEffectToken = new ConfigToken();
        }

        protected override void InitTokenFromDialog()
        {
            if (!TokenUpdatesSuspended)
            {
                lock (EffectToken)
                {
                    ConfigToken token = EffectToken as ConfigToken;
                    token.ColorTransferMode = curveControl.ColorTransferMode;
                    token.ControlPoints = new SortedList<int, int>[curveControl.ControlPoints.Length];

                    for (int i = 0; i < curveControl.ControlPoints.Length; i++)
                    {
                        token.ControlPoints[i] = new SortedList<int, int>(curveControl.ControlPoints[i]);
                    }

                    token.InputMode = InputMode;
                    token.OutputMode = OutputMode;
                    token.Preset = presetDropdown.CurrentName;
                    token.CurveDrawMode = CurveDrawMode;
                    if (curveControl.Histogram != null)
                        curveControl.Histogram.HistogramValues = token.Uop.Apply(histogram);
                }
            }
        }

        private int tokenUpdateSuspendCount = 0;
        private void SuspendTokenUpdates() { tokenUpdateSuspendCount++; }
        private void ResumeTokenUpdates() { tokenUpdateSuspendCount--; }
        private bool TokenUpdatesSuspended { get { return tokenUpdateSuspendCount > 0; } }

        protected override void InitDialogFromToken(EffectConfigToken effectToken)
        {
            SuspendTokenUpdates();
            ConfigToken token = (ConfigToken)effectToken;

            InputMode = token.InputMode;
            OutputMode = token.OutputMode;
            foreach (string s in Enum.GetNames(typeof(ChannelMode)))
            {
                if (token.ColorTransferMode.ToString() == s)
                {
                    modeComboBox.SelectedItem = rm.GetString(s);
                    break;
                }
            }

            if (token.ColorTransferMode == ChannelMode.Advanced)
            {
                foreach (string s in Enum.GetNames(typeof(Channel)))
                {
                    if (InputMode.ToString() == s)
                    {
                        modes[0].SelectedItem = rm.GetString(s);
                    }

                    if (OutputMode.ToString() == s)
                    {
                        modes[1].SelectedItem = rm.GetString(s);
                    }
                }
            }
            //we set to linear first to avoid the long proc time of pencil -> spline
            CurveDrawMode = CurveDrawMode.Linear;
            CurveDrawMode = token.CurveDrawMode;
            curveControl.PokeWithPencil();

            curveControl.ControlPoints = (SortedList<int, int>[])token.ControlPoints.Clone();
            curveControl.Invalidate();
            curveControl.Update();
            if (presetDropdown != null)
            {
                if(token.Preset!=null)presetDropdown.Current = CurvesPlusXMLFile.FromConfigToken(token);
                presetDropdown.SetPresetByName(token.Preset);
            }

            ResumeTokenUpdates();
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.labelCoordinates = new System.Windows.Forms.Label();
            this.modeComboBox = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanelMask = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.resetButton = new System.Windows.Forms.Button();
            this.labelHelpText = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.optSpline = new System.Windows.Forms.RadioButton();
            this.optDraw = new System.Windows.Forms.RadioButton();
            this.optLine = new System.Windows.Forms.RadioButton();
            //this.presetDropdown = new PaintDotNet.HeadingLabel();
            this.tableLayoutMain.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelCoordinates
            // 
            this.labelCoordinates.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelCoordinates.Location = new System.Drawing.Point(191, 23);
            this.labelCoordinates.Name = "labelCoordinates";
            this.labelCoordinates.Size = new System.Drawing.Size(68, 27);
            this.labelCoordinates.TabIndex = 25;
            this.labelCoordinates.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // modeComboBox
            // 
            this.tableLayoutMain.SetColumnSpan(this.modeComboBox, 2);
            this.modeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.modeComboBox.Location = new System.Drawing.Point(3, 26);
            this.modeComboBox.Name = "modeComboBox";
            this.modeComboBox.Size = new System.Drawing.Size(108, 21);
            this.modeComboBox.TabIndex = 23;
            this.modeComboBox.SelectedIndexChanged += new System.EventHandler(this.modeComboBox_SelectedIndexChanged);
            // 
            // tableLayoutPanelMask
            // 
            this.tableLayoutPanelMask.AutoSize = true;
            this.tableLayoutPanelMask.ColumnCount = 1;
            this.tableLayoutMain.SetColumnSpan(this.tableLayoutPanelMask, 4);
            this.tableLayoutPanelMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanelMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMask.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanelMask.Location = new System.Drawing.Point(2, 314);
            this.tableLayoutPanelMask.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanelMask.Name = "tableLayoutPanelMask";
            this.tableLayoutPanelMask.RowCount = 1;
            this.tableLayoutPanelMask.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanelMask.Size = new System.Drawing.Size(0, 0);
            this.tableLayoutPanelMask.TabIndex = 24;
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.ColumnCount = 4;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 88F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 74F));
            this.tableLayoutMain.Controls.Add(this.cancelButton, 3, 5);
            this.tableLayoutMain.Controls.Add(this.okButton, 2, 5);
            this.tableLayoutMain.Controls.Add(this.resetButton, 0, 5);
            this.tableLayoutMain.Controls.Add(this.labelHelpText, 0, 4);
            this.tableLayoutMain.Controls.Add(this.modeComboBox, 0, 1);
            this.tableLayoutMain.Controls.Add(this.tableLayoutPanelMask, 0, 3);
            this.tableLayoutMain.Controls.Add(this.labelCoordinates, 3, 1);
            this.tableLayoutMain.Controls.Add(this.tableLayoutPanel1, 2, 1);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(6, 6);
            this.tableLayoutMain.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 6;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 29F));
            this.tableLayoutMain.Size = new System.Drawing.Size(262, 400);
            this.tableLayoutMain.TabIndex = 24;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.cancelButton.Location = new System.Drawing.Point(191, 374);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(68, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // okButton
            // 
            this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.okButton.Location = new System.Drawing.Point(117, 374);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(68, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // resetButton
            // 
            this.resetButton.Location = new System.Drawing.Point(3, 374);
            this.resetButton.Name = "resetButton";
            this.resetButton.Size = new System.Drawing.Size(81, 23);
            this.resetButton.TabIndex = 3;
            this.resetButton.Click += new System.EventHandler(this.resetButton_Click);
            // 
            // labelHelpText
            // 
            this.tableLayoutMain.SetColumnSpan(this.labelHelpText, 4);
            this.labelHelpText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.labelHelpText.Location = new System.Drawing.Point(3, 342);
            this.labelHelpText.Name = "labelHelpText";
            this.labelHelpText.Size = new System.Drawing.Size(256, 29);
            this.labelHelpText.TabIndex = 26;
            this.labelHelpText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 6F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 22F));
            this.tableLayoutPanel1.Controls.Add(this.optSpline, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.optDraw, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.optLine, 1, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(115, 24);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(1);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(72, 25);
            this.tableLayoutPanel1.TabIndex = 27;
            // 
            // optSpline
            // 
            this.optSpline.Appearance = System.Windows.Forms.Appearance.Button;
            this.optSpline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optSpline.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.optSpline.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Highlight;
            this.optSpline.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.HotTrack;
            this.optSpline.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.optSpline.Location = new System.Drawing.Point(3, 3);
            this.optSpline.Name = "optSpline";
            this.optSpline.Size = new System.Drawing.Size(16, 19);
            this.optSpline.TabIndex = 0;
            this.optSpline.TabStop = true;
            this.optSpline.UseVisualStyleBackColor = false;
            this.optSpline.MouseLeave += new System.EventHandler(this.drawModeButton_MouseLeave);
            this.optSpline.MouseEnter += new System.EventHandler(this.drawModeButton_MouseEnter);
            this.optSpline.CheckedChanged += new System.EventHandler(this.DrawMode_Changed);
            // 
            // optDraw
            // 
            this.optDraw.Appearance = System.Windows.Forms.Appearance.Button;
            this.optDraw.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optDraw.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.optDraw.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Highlight;
            this.optDraw.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.HotTrack;
            this.optDraw.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.optDraw.Location = new System.Drawing.Point(53, 3);
            this.optDraw.Name = "optDraw";
            this.optDraw.Size = new System.Drawing.Size(16, 19);
            this.optDraw.TabIndex = 2;
            this.optDraw.UseVisualStyleBackColor = false;
            this.optDraw.MouseLeave += new System.EventHandler(this.drawModeButton_MouseLeave);
            this.optDraw.MouseEnter += new System.EventHandler(this.drawModeButton_MouseEnter);
            this.optDraw.CheckedChanged += new System.EventHandler(this.DrawMode_Changed);
            // 
            // optLine
            // 
            this.optLine.Appearance = System.Windows.Forms.Appearance.Button;
            this.optLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.optLine.FlatAppearance.BorderColor = System.Drawing.Color.Black;
            this.optLine.FlatAppearance.CheckedBackColor = System.Drawing.SystemColors.Highlight;
            this.optLine.FlatAppearance.MouseDownBackColor = System.Drawing.SystemColors.HotTrack;
            this.optLine.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.optLine.Location = new System.Drawing.Point(25, 3);
            this.optLine.Name = "optLine";
            this.optLine.Size = new System.Drawing.Size(16, 19);
            this.optLine.TabIndex = 1;
            this.optLine.TabStop = true;
            this.optLine.UseVisualStyleBackColor = false;
            this.optLine.MouseLeave += new System.EventHandler(this.drawModeButton_MouseLeave);
            this.optLine.MouseEnter += new System.EventHandler(this.drawModeButton_MouseEnter);
            this.optLine.CheckedChanged += new System.EventHandler(this.DrawMode_Changed);
            // 
            // ConfigDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(274, 412);
            this.Controls.Add(this.tableLayoutMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.MinimumSize = new System.Drawing.Size(290, 448);
            this.Name = "ConfigDialog";
            this.Padding = new System.Windows.Forms.Padding(6);
            this.Controls.SetChildIndex(this.tableLayoutMain, 0);
            this.tableLayoutMain.ResumeLayout(false);
            this.tableLayoutMain.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        void drawModeButton_MouseEnter(object sender, EventArgs e)
        {
            ButtonBase control = sender as ButtonBase;
            control.FlatAppearance.BorderSize = 2;
        }

        void drawModeButton_MouseLeave(object sender, EventArgs e)
        {
            ButtonBase control = sender as ButtonBase;
            control.FlatAppearance.BorderSize = 1;
        }

        protected override void OnLoad(EventArgs e)
        {
            presetDropdown = new PresetDropdown<CurvesPlusXMLFile>(Services, Path.GetFileNameWithoutExtension(GetType().Assembly.CodeBase), CurvesPlusXMLFile.CreateDefault(), GetXao());

            this.tableLayoutMain.SetColumnSpan(this.presetDropdown, 4);
            this.presetDropdown.Dock = System.Windows.Forms.DockStyle.Fill;
            this.presetDropdown.Location = new System.Drawing.Point(3, 3);
            this.presetDropdown.Name = "presetDropdown";
            this.presetDropdown.TabIndex = 0;
            this.presetDropdown.TabStop = false;

            AddDefaultPresets();

            this.presetDropdown.PresetChanged += presetDropdown_PresetChanged;
            this.tableLayoutMain.Controls.Add(this.presetDropdown, 0, 0);

            this.okButton.Select();
            base.OnLoad(e);
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void curveControl_ValueChanged(object sender, EventArgs e)
        {
            this.FinishTokenUpdate();
            this.presetDropdown.Current = CurvesPlusXMLFile.FromConfigToken((ConfigToken)EffectToken);
        }

        private void curveControl_CoordinatesChanged(object sender, EventArgs<Point> e)
        {
            Point pt = e.Data;
            string newText;

            if (pt.X >= 0)
            {
                string format = "({0}, {1})";
                newText = string.Format(format, pt.X, pt.Y);
            }
            else
            {
                newText = string.Empty;
            }

            if (newText != labelCoordinates.Text)
            {
                labelCoordinates.Text = newText;
                labelCoordinates.Update();
            }
        }

        private void resetButton_Click(object sender, System.EventArgs e)
        {
            curveControl.ResetControlPoints();
            this.FinishTokenUpdate();
        }

        private void MaskCheckChanged(object sender, System.EventArgs e)
        {
            for (int i = 0; i < maskCheckBoxes.Length; ++i)
            {
                if (maskCheckBoxes[i] == sender)
                {
                    curveControl.SetSelected(i, maskCheckBoxes[i].Checked);
                }
            }

            UpdateCheckboxEnables();
        }

        private void modeComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            CurveControl newCurveControl;
            ChannelMode colorTransferMode = ChannelMode.Rgb;

            if (modeComboBox.SelectedIndex >= 0)
            {
                foreach (string s in Enum.GetNames(typeof(ChannelMode)))
                {
                    if (modeComboBox.SelectedItem.ToString() == rm.GetString(s))
                    {
                        colorTransferMode = (ChannelMode)Enum.Parse(typeof(ChannelMode), s);
                    }
                }
            }

            newCurveControl = curveControls[colorTransferMode];

            if (curveControl != newCurveControl)
            {
                tableLayoutMain.Controls.Remove(curveControl);

                //reset the histogram before switching out the control
                if (curveControl != null && curveControl.Histogram != null)
                {
                    curveControl.Histogram.HistogramValues = histogram;
                }

                curveControl = newCurveControl;

                curveControl.Bounds = new Rectangle(0, 0, 258, 258);
                curveControl.BackColor = Color.White;
                tableLayoutMain.SetColumnSpan(this.curveControl, 4);
                curveControl.Dock = System.Windows.Forms.DockStyle.Fill;
                curveControl.ValueChanged += curveControlValueChangedDelegate;
                curveControl.CoordinatesChanged += curveControlCoordinatesChangedDelegate;
                curveControl.CurveDrawMode = CurveDrawMode;

                if (curveControl.Histogram != null)
                {
                    curveControl.Histogram.UpdateHistogram(EffectSourceSurface, Selection);
                    histogram = curveControl.Histogram.HistogramValues;
                }

                tableLayoutMain.Controls.Add(curveControl, 0, 2);

                if(!TokenUpdatesSuspended)
                {
                    FinishTokenUpdate();
                }

                int channels = newCurveControl.Channels;

                maskCheckBoxes = new CheckBox[channels];

                this.tableLayoutPanelMask.Controls.Clear();
                switch (colorTransferMode)
                {
                    case ChannelMode.Advanced:
                        ComboBox comboInputMode = new ComboBox();
                        ComboBox comboOutputMode = new ComboBox();
                        Label l1 = new Label();
                        Label l2 = new Label();
                        comboInputMode.DropDownStyle = comboOutputMode.DropDownStyle = ComboBoxStyle.DropDownList;
                        comboInputMode.SelectedIndexChanged += new EventHandler(comboInputMode_SelectedIndexChanged);
                        comboOutputMode.SelectedIndexChanged += new EventHandler(comboOutputMode_SelectedIndexChanged);
                        comboInputMode.Dock = comboOutputMode.Dock = DockStyle.Fill;
                        modes[0] = comboInputMode;
                        modes[1] = comboOutputMode;
                        l1.Text = "In:";
                        l2.Text = "Out:";
                        l1.AutoSize = l2.AutoSize = true;
                        l1.Dock = l2.Dock = DockStyle.Fill;
                        l1.TextAlign = l2.TextAlign = ContentAlignment.MiddleRight;
                        tableLayoutPanelMask.ColumnCount = 4;
                        tableLayoutPanelMask.ColumnStyles[0].SizeType = SizeType.AutoSize;
                        tableLayoutPanelMask.ColumnStyles[1].SizeType = SizeType.Percent;
                        tableLayoutPanelMask.ColumnStyles[2].SizeType = SizeType.AutoSize;
                        tableLayoutPanelMask.ColumnStyles[3].SizeType = SizeType.Percent;
                        tableLayoutPanelMask.ColumnStyles[1].Width = 50;
                        tableLayoutPanelMask.ColumnStyles[3].Width = 50;
                        foreach (string s in Enum.GetNames(typeof(Channel)))
                        {
                            comboInputMode.Items.Add(rm.GetString(s));
                            comboOutputMode.Items.Add(rm.GetString(s));
                        }
                        comboInputMode.SelectedItem = rm.GetString(InputMode.ToString());
                        comboOutputMode.SelectedItem = rm.GetString(OutputMode.ToString());
                        tableLayoutPanelMask.Controls.Add(l1, 0, 0);
                        tableLayoutPanelMask.Controls.Add(comboInputMode, 1, 0);
                        tableLayoutPanelMask.Controls.Add(l2, 2, 0);
                        tableLayoutPanelMask.Controls.Add(comboOutputMode, 3, 0);
                        break;
                    default:
                        this.tableLayoutPanelMask.ColumnCount = channels;
                        for (int i = 0; i < channels; ++i)
                        {
                            CheckBox checkbox = new CheckBox();
                            
                            checkbox.Checked = curveControl.GetSelected(i);
                            checkbox.CheckedChanged += maskCheckChanged;
                            checkbox.Text = curveControl.GetChannelName(i);
                            checkbox.FlatAppearance.BorderColor = curveControl.GetVisualColor(i).ToColor();
                            checkbox.AutoSize = true;
                            checkbox.Paint += new PaintEventHandler(checkbox_Paint);
                            this.tableLayoutPanelMask.Controls.Add(checkbox, i, 0);
                            this.tableLayoutPanelMask.ColumnStyles[i].SizeType = SizeType.AutoSize;
                            maskCheckBoxes[i] = checkbox;
                        }
                        
                        UpdateCheckboxEnables();
                        break;
                }
            }
        }

        void checkbox_Paint(object sender, PaintEventArgs e)
        {
            Rectangle r = ((Control)sender).ClientRectangle;
            r.Offset(-1, -1);
            Pen p = new Pen(((CheckBox)sender).FlatAppearance.BorderColor, 1.0f);
            e.Graphics.DrawRectangle(p, r);
        }

        void comboOutputMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (string s in Enum.GetNames(typeof(Channel)))
            {
                if (((ComboBox)sender).SelectedItem.ToString() == rm.GetString(s))
                {
                    OutputMode = (Channel)Enum.Parse(typeof(Channel), s);
                }
            }
            if(!TokenUpdatesSuspended)
                FinishTokenUpdate();
        }

        void comboInputMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (string s in Enum.GetNames(typeof(Channel)))
            {
                if (((ComboBox)sender).SelectedItem.ToString() == rm.GetString(s))
                {
                    InputMode = (Channel)Enum.Parse(typeof(Channel), s);
                }
            }
            if(!TokenUpdatesSuspended)
                FinishTokenUpdate();
        }

        private void UpdateCheckboxEnables()
        {
            int countChecked = 0;

            for (int i = 0; i < maskCheckBoxes.Length; ++i)
            {
                if (maskCheckBoxes[i].Checked)
                {
                    ++countChecked;
                }
            }

            if (maskCheckBoxes.Length == 1)
            {
                maskCheckBoxes[0].Enabled = false;
            }
        }

        private CurveDrawMode CurveDrawMode
        {
            get
            {
                if (optLine.Checked) return CurveDrawMode.Linear;
                else if (optDraw.Checked) return CurveDrawMode.Pencil;
                else return CurveDrawMode.Spline;
            }
            set
            {
                switch (value)
                {
                    case CurveDrawMode.Linear:
                        optLine.Checked = true;
                        break;
                    case CurveDrawMode.Spline:
                        optSpline.Checked = true;
                        break;
                    case CurveDrawMode.Pencil:
                        optDraw.Checked = true;
                        break;
                }
            }
        }
        private void DrawMode_Changed(object sender, EventArgs e)
        {
            if (((RadioButton)sender).Checked)
            {
                curveControl.CurveDrawMode = CurveDrawMode;

                if(!TokenUpdatesSuspended)
                    FinishTokenUpdate();
                curveControl.Invalidate();

                switch (CurveDrawMode)
                {
                    case CurveDrawMode.Linear:
                    case CurveDrawMode.Spline:
                        this.labelHelpText.Text = "Tip: Right-Click to remove control points.";
                        break;
                    case CurveDrawMode.Pencil:
                        this.labelHelpText.Text = "Tip: Right-Click to smooth curve.";
                        break;
                }
            }
        }
    }
}