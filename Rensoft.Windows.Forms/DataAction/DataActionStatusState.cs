using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rensoft.Windows.Forms.DataAction
{
    public class DataActionStatusState
    {
        public string Message { get; protected set; }
        public Cursor Cursor { get; protected set; }

        public DataActionStatusState(string message, Cursor cursor)
        {
            this.Message = message;
            this.Cursor = cursor;
        }
    }
}
