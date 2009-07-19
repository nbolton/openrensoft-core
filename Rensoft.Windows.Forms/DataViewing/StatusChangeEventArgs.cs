using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rensoft.Windows.Forms.DataViewing
{
    public class StatusChangeEventArgs : EventArgs
    {
        public Guid StatusGuid { get; set; }
        public string StatusText { get; private set; }
        public Cursor Cursor { get; private set; }

        public StatusChangeEventArgs(string statusText)
            : this(statusText, Cursors.WaitCursor) { }

        public StatusChangeEventArgs(string statusText, Cursor cursor)
        {
            this.StatusGuid = Guid.NewGuid();
            this.Cursor = cursor;
            this.StatusText = statusText;
        }
    }
}
