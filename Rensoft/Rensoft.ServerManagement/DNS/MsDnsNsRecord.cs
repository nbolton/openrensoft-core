using System;
using System.Management;

namespace Rensoft.ServerManagement.DNS
{
    public class MsDnsNsRecord : MsDnsRecord
    {
        public MsDnsNsRecord(string name, string value, MsDnsZone zone, int ttl)
            : base(name, value, zone, ttl) { }

        internal static MsDnsNsRecord Parse(ManagementObject record, MsDnsZone zone)
        {
            MsDnsNsRecord dnsRecord = new MsDnsNsRecord(
                (string)record.Properties["OwnerName"].Value,
                (string)record.Properties["RecordData"].Value,
                zone,
                (int)(UInt32)record.Properties["TTL"].Value);

            return dnsRecord;
        }
    }
}
