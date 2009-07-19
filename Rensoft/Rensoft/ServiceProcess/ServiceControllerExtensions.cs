using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Threading;

namespace Rensoft.ServiceProcess
{
    public static class ServiceControllerExtensions
    {
        public static bool Start(this ServiceController sc, TimeSpan timeout)
        {
            sc.Start();
            return waitForStatus(sc, timeout, ServiceControllerStatus.Running);
        }

        public static bool Stop(this ServiceController sc, TimeSpan timeout)
        {
            sc.Stop();
            return waitForStatus(sc, timeout, ServiceControllerStatus.Stopped);
        }

        private static bool waitForStatus(ServiceController sc, TimeSpan timeout, ServiceControllerStatus status)
        {
            DateTime startTime = DateTime.Now;
            while (DateTime.Now <= (startTime + timeout))
            {
                Thread.Sleep(1000);
                sc.Refresh();

                if (sc.Status == status)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
