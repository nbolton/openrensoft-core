using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.Windows.Forms.DataAction
{
    public class DataActionBeforeEventArgs : EventArgs
    {
        public object Data { get; private set; }
        public Guid StatusGuid { get; set; }
        public bool Cancel { get; set; }

        public DataActionBeforeEventArgs(object data)
        {
            this.Data = data;
        }

        public TValue GetData<TValue>()
        {
            return (TValue)Data;
        }
    }
}
