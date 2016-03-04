using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace pyrochild.effects.common
{
    internal partial class InputBoxForm : Form
    {
        Size lbltextoriginalsize;
        Size pnlwhiteoroginalsize;
        InputBox.ValidationMode validationmode;
        char[] validationchars;
        public InputBoxForm(string text, string defaultvalue, string caption, char[] validationchars, InputBox.ValidationMode validationmode)
        {
            InitializeComponent();
            this.pnlWhite.Resize += new System.EventHandler(this.pnlWhite_Resize);
            this.txtOut.KeyPress += new KeyPressEventHandler(txtOut_KeyPress);
            this.lblText.Resize += new System.EventHandler(this.lblText_Resize);
            picIcon.Image = SystemIcons.Question.ToBitmap();
            lbltextoriginalsize = lblText.Size;
            pnlwhiteoroginalsize = pnlWhite.Size;
            this.validationchars = validationchars;
            this.validationmode = validationmode;
            this.lblText.Text = text;
            this.txtOut.Text = defaultvalue;
            this.Text = caption;
        }

        void txtOut_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (validationmode)
            {
                case InputBox.ValidationMode.Blacklist:
                    if (validationchars.Contains(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                    break;
                case InputBox.ValidationMode.Whitelist:
                    if (!validationchars.Contains(e.KeyChar))
                    {
                        e.Handled = true;
                    }
                    break;
            }
            if (e.KeyChar == '\b')
            {
                e.Handled = false;
            }
        }

        private void lblText_Resize(object sender, EventArgs e)
        {
            pnlWhite.Size += lblText.Size - lbltextoriginalsize;
        }

        private void pnlWhite_Resize(object sender, EventArgs e)
        {
            this.Size += pnlWhite.Size - pnlwhiteoroginalsize;
        }

        public string Value
        {
            get { return txtOut.Text; }
        }
    }

    /// <summary>
    /// A counterpart to the MessageBox class, designed to look similar (at least on Vista)
    /// </summary>
    public static class InputBox
    {
        public enum ValidationMode
        {
            Whitelist,
            Blacklist,
            None
        }
        public static DialogResult Show(string text, out string result)
        {
            return ShowCore(null, text, null, null, null, ValidationMode.None, out result);
        }
        public static DialogResult Show(IWin32Window owner, string text, out string result)
        {
            return ShowCore(owner, text, null, null, null, ValidationMode.None, out result);
        }
        public static DialogResult Show(string text, string defaultValue, out string result)
        {
            return ShowCore(null, text, defaultValue, null, null, ValidationMode.None, out result);
        }
        public static DialogResult Show(IWin32Window owner, string text, string defaultValue, out string result)
        {
            return ShowCore(owner, text, defaultValue, null, null, ValidationMode.None, out result);
        }
        public static DialogResult Show(string text, string defaultValue, string caption, out string result)
        {
            return ShowCore(null, text, defaultValue, caption, null, ValidationMode.None, out result);
        }
        public static DialogResult Show(IWin32Window owner, string text, string defaultValue, string caption, out string result)
        {
            return ShowCore(owner, text, defaultValue, caption, null, ValidationMode.None, out result);
        }
        public static DialogResult Show(string text, string defaultValue, string caption, char[] validationchars, ValidationMode validationmode, out string result)
        {
            return ShowCore(null, text, defaultValue, caption, validationchars, validationmode, out result);
        }
        public static DialogResult Show(IWin32Window owner, string text, string defaultValue, string caption, char[] validationchars, ValidationMode validationmode, out string result)
        {
            return ShowCore(owner, text, defaultValue, caption, validationchars, validationmode, out result);
        }

        private static DialogResult ShowCore(
            IWin32Window owner,
            string text,
            string defaultValue,
            string caption,
            char[] validationchars,
            ValidationMode validationmode,
            out string result)
        {
            InputBoxForm box = new InputBoxForm(text, defaultValue, caption, validationchars, validationmode);
            DialogResult retval = box.ShowDialog(owner);
            result = box.Value;
            return retval;
        }
    }
}
