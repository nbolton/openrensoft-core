using System;

namespace Rensoft.ServerManagement.DNS
{
    public class MsDnsARecord : MsDnsRecord
    {
        public MsDnsARecord(string name, string value, MsDnsZone zone, int ttl)
            : base(name, value, zone, ttl) { }

        internal static MsDnsARecord Parse(System.Management.ManagementObject record, MsDnsZone zone)
        {
            MsDnsARecord dnsRecord = new MsDnsARecord(
                (string)record.Properties["OwnerName"].Value,
                (string)record.Properties["RecordData"].Value,
                zone,
                (int)(UInt32)record.Properties["TTL"].Value);

            return dnsRecord;
        }
    }
}
