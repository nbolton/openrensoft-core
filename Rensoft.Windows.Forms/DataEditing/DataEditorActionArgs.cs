using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rensoft.Windows.Forms.DataAction;

namespace Rensoft.Windows.Forms.DataEditing
{
    public class DataEditorActionArgs : DataActionArgs
    {
        public DataEditorAction Action { get; private set; }

        public DataEditorActionArgs(object data, Guid statusGuid, DataEditorAction action)
            : base(data, statusGuid)
        {
            this.Action = action;
        }
    }
}
