using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.Windows.Forms.DataViewing
{
    public class StatusRevertEventArgs : EventArgs
    {
        public Guid StatusGuid { get; private set; }
        public string RevertStatus { get; private set; }

        public StatusRevertEventArgs(Guid statusGuid)
        {
            this.StatusGuid = statusGuid;
        }

        public StatusRevertEventArgs(Guid statusGuid, string revertStatus)
            : this(statusGuid)
        {
            this.RevertStatus = revertStatus;
        }
    }
}
