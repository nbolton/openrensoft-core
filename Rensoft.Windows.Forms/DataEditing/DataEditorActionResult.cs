using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rensoft.Windows.Forms.DataAction;

namespace Rensoft.Windows.Forms.DataEditing
{
    internal class DataEditorActionResult : DataActionResult
    {
        public DataEditorAction Action { get; private set; }

        public DataEditorActionResult(
            object data,
            Guid statusGuid,
            bool cancelled,
            string userMessage,
            DataEditorAction action)
            : base(data, statusGuid, cancelled, userMessage)
        {
            this.Action = action;
        }
    }
}
