using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.Windows.Forms.DataAction
{
    public class DataActionArgs
    {
        public object Data { get; private set; }
        public Guid StatusGuid { get; private set; }

        public DataActionArgs(object data, Guid statusGuid)
        {
            this.Data = data;
            this.StatusGuid = statusGuid;
        }
    }
}
