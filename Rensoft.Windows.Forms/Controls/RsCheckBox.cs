using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.ComponentModel;

namespace Rensoft.Windows.Forms.Controls
{
    public class RsCheckBox : CheckBox
    {
        private bool fireCheckedChangedEvent = true;

        public event CancelEventHandler BeforeCheckedChanged;

        public RsCheckBox()
        {
            AutoCheck = false;
            MouseUp += new MouseEventHandler(RsCheckBox_MouseUp);
        }

        void RsCheckBox_MouseUp(object sender, MouseEventArgs e)
        {
            CancelEventArgs cancelArgs = new CancelEventArgs();
            OnBeforeCheckedChanged(cancelArgs);

            if (!cancelArgs.Cancel)
            {
                toggleChecked();
                OnCheckedChanged(e);
            }
        }

        private void toggleChecked()
        {
            fireCheckedChangedEvent = false;
            Checked = !Checked;
            fireCheckedChangedEvent = true;
        }

        protected virtual void OnBeforeCheckedChanged(CancelEventArgs e)
        {
            if (BeforeCheckedChanged != null) BeforeCheckedChanged(this, e);
        }

        protected override void OnCheckedChanged(EventArgs e)
        {
            if (fireCheckedChangedEvent)
            {
                base.OnCheckedChanged(e);
            }
        }
    }
}
