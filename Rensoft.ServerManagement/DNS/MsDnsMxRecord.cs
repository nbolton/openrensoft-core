using System;
using System.Management;

namespace Rensoft.ServerManagement.DNS
{
    public class MsDnsMxRecord : MsDnsRecord
    {
        private int priority;

        /// <summary>
        /// Gets or sets the prority of this MX record.
        /// </summary>
        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public MsDnsMxRecord(string name, string value,
            MsDnsZone zone, int ttl, int priority)
            : base(name, value, zone, ttl)
        {
            this.Priority = priority;
        }

        public static MsDnsMxRecord Parse(ManagementObject record, MsDnsZone zone)
        {
            string data = (string)record.Properties["RecordData"].Value;
            string[] dataSplit = data.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int priority = int.Parse(dataSplit[0]);
            string value = dataSplit[1];

            MsDnsMxRecord dnsRecord = new MsDnsMxRecord(
                (string)record.Properties["OwnerName"].Value,
                value,
                zone,
                (int)(UInt32)record.Properties["TTL"].Value,
                priority);

            return dnsRecord;
        }
    }
}