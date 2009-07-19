using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.Windows.Forms.DataAction
{
    public class DataActionAfterEventArgs : EventArgs
    {
        private object data;

        public bool Cancelled { get; private set; }
        public Guid StatusGuid { get; private set; }

        public DataActionAfterEventArgs(Guid statusGuid, bool cancelled, object data)
        {
            this.StatusGuid = statusGuid;
            this.Cancelled = cancelled;
            this.data = data;
        }

        public TValue GetData<TValue>()
        {
            return (TValue)data;
        }

        public bool CheckType<TData>()
        {
            if (data != null)
            {
                return data.GetType() == typeof(TData);
            }
            else
            {
                return false;
            }
        }
    }
}
