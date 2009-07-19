using System;
using System.Collections.Generic;
using System.Text;

namespace Rensoft.ServerManagement.DNS
{
    public class MsDnsCnameRecord : MsDnsRecord
    {
        public MsDnsCnameRecord(string name, string value, MsDnsZone zone, int ttl)
            : base(name, value, zone, ttl) { }

        internal static MsDnsCnameRecord Parse(System.Management.ManagementObject record, MsDnsZone zone)
        {
            MsDnsCnameRecord dnsRecord = new MsDnsCnameRecord(
                (string)record.Properties["OwnerName"].Value,
                (string)record.Properties["RecordData"].Value,
                zone,
                (int)(UInt32)record.Properties["TTL"].Value);

            return dnsRecord;
        }
    }
}
