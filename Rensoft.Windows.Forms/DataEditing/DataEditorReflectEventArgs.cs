using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.Windows.Forms.DataEditing
{
    public class DataEditorReflectEventArgs : EventArgs
    {
        private object data;
        public DataEditorMode Mode { get; private set; }

        public DataEditorReflectEventArgs(object data, DataEditorMode mode)
        {
            this.data = data;
            this.Mode = mode;
        }

        public TValue GetData<TValue>()
        {
            return (TValue)data;
        }
    }
}
