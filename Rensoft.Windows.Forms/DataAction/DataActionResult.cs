using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.Windows.Forms.DataAction
{
    public class DataActionResult
    {
        public object Data { get; private set; }
        public Guid StatusGuid { get; private set; }
        public bool Cancelled { get; private set; }
        public string UserMessage { get; private set; }

        public DataActionResult(
            object data,
            Guid statusGuid,
            bool cancelled, 
            string userMessage)
        {
            this.Data = data;
            this.StatusGuid = statusGuid;
            this.Cancelled = cancelled;
            this.UserMessage = userMessage;
        }
    }
}
