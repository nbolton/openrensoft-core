using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rensoft.Windows.Forms.DataAction;

namespace Rensoft.Windows.Forms.DataViewing
{
    internal class DataViewerActionResult : DataActionResult
    {
        public DataViewerAction Action { get; private set; }

        public DataViewerActionResult(
            object data,
            Guid statusGuid,
            bool cancelled,
            string userMessage,
            DataViewerAction action)
            : base(data, statusGuid, cancelled, userMessage)
        {
            this.Action = action;
        }
    }
}
