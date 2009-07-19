using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rensoft.Windows.Forms.DataAction;

namespace Rensoft.Windows.Forms.DataViewing
{
    public class DataViewerActionArgs : DataActionArgs
    {
        public DataViewerAction Action { get; private set; }

        public DataViewerActionArgs(object data, Guid statusGuid, DataViewerAction action)
            : base(data, statusGuid)
        {
            this.Action = action;
        }
    }
}
