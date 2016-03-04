using System.Threading;
using System.Windows.Forms;

namespace pyrochild.effects.common
{
    partial class PresetDropdown<T>
    {
        private void InitializeComponent()
        {
            this.comboBox = new ComboBox();
            this.SuspendLayout();
            // 
            // comboBox1
            // 
            this.comboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            if (Thread.CurrentThread.GetApartmentState() == ApartmentState.STA)
            {
                this.comboBox.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
                this.comboBox.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            }
            this.comboBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.comboBox.FormattingEnabled = true;
            this.comboBox.Location = new System.Drawing.Point(0, 0);
            this.comboBox.Margin = new System.Windows.Forms.Padding(0);
            this.comboBox.Name = "comboBox1";
            this.comboBox.Size = new System.Drawing.Size(100, 21);
            this.comboBox.TabIndex = 0;
            this.comboBox.DrawItem += new DrawItemEventHandler(DefaultDrawItem);
            this.comboBox.MeasureItem += new MeasureItemEventHandler(DefaultMeasureItem);
            this.comboBox.SelectedIndexChanged += new System.EventHandler(comboBox_SelectedIndexChanged);
            this.comboBox.DropDownHeight = int.MaxValue;
            // 
            // PresetDropdown
            // 
            this.Controls.Add(this.comboBox);
            this.Name = "PresetDropdown";
            this.Size = new System.Drawing.Size(100, 21);
            this.ResumeLayout(false);

            foreach (ComboBox cb in Controls)
            {
                cb.Resize += (sender, e) => {
                    if (!cb.Focused)
                        comboBox.SelectionLength = 0;
                };
            }
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            this.Height = this.comboBox.Height;

            base.OnSizeChanged(e);
        }

        private System.Windows.Forms.ComboBox comboBox;
    }
}