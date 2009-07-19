using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.Windows.Forms.DataAction
{
    public class DataActionEventArgs : EventArgs
    {
        public object Data { get; private set; }
        public string UserMessage { get; set; }
        public bool Cancelled { get; set; }

        public DataActionEventArgs(object data)
        {
            this.Data = data;
        }

        public TValue GetData<TValue>()
        {
            return (TValue)Data;
        }

        public void ReplaceData(object newData)
        {
            this.Data = newData;
        }
    }
}
