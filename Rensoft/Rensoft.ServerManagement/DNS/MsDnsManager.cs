using System;
using System.Management;
using System.Collections.Generic;

namespace Rensoft.ServerManagement.DNS
{
    /// <summary>
    /// Manage DNS zones and records using WMI.
    /// </summary>
    public class MsDnsManager : ServerManager
    {
        /// <summary>
        /// Initialize a new DnsManager using a specified server.
        /// </summary>
        /// <param name="serverName">Specific server name.</param>
        public MsDnsManager(string serverName)
            : base(serverName, @"Root\MicrosoftDNS") { }

        /// <summary>
        /// Creates a new DNS zone and all associated record types.
        /// </summary>
        /// <param name="zone">DNS zone to create.</param>
        public void CreateZone(MsDnsZone zone)
        {
            ManagementPath path = new ManagementPath("MicrosoftDNS_Zone");
            ManagementClass newZone = new ManagementClass(WmiScope, path, null);
            ManagementBaseObject inParams =
                newZone.GetMethodParameters("CreateZone");

            inParams.Properties["ZoneName"].Value = zone.Name;
            inParams.Properties["ZoneType"].Value = (int)zone.Type;

            try
            {
                newZone.InvokeMethod("CreateZone", inParams, null);
            }
            catch (ManagementException ex)
            {
                throw new Exception("Could not create new DNS zone. " +
                    "It is possible that this domain already exists.", ex);
            }

            // Defaults the SOA, and removes NS records.
            DefaultSoaRecord(zone);
            DeleteNsRecords(zone);

            // Now create the records for the zone.
            CreateRecords(zone.NsRecords);
            CreateRecords(zone.MxRecords);
            CreateRecords(zone.ARecords);
            CreateRecords(zone.CnameRecords);
        }

        /// <summary>
        /// Deletes a DNS zone.
        /// </summary>
        /// <param name="zone">Zone to delete.</param>
        public void DeleteZone(MsDnsZone zone)
        {
            // Select NS records where the owner is that of domain.
            ObjectQuery query = new ObjectQuery("SELECT * FROM "
                + "MicrosoftDNS_Zone WHERE Name = '" + zone.Name + "'");
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection foundZones = searcher.Get();

            // Delete all NS records.
            foreach (ManagementObject foundZone in foundZones)
            {
                foundZone.Delete();
            }
        }

        public void CreateRecords(List<MsDnsRecord> mixedRecordList)
        {
            List<MsDnsARecord> aRecords = new List<MsDnsARecord>();
            List<MsDnsCnameRecord> cnameRecords = new List<MsDnsCnameRecord>();
            List<MsDnsMxRecord> mxRecords = new List<MsDnsMxRecord>();
            List<MsDnsNsRecord> nsRecords = new List<MsDnsNsRecord>();

            foreach (MsDnsRecord record in mixedRecordList)
            {
                switch (record.DnsType)
                {
                    case MsDnsRecordType.A:
                        aRecords.Add(record as MsDnsARecord);
                        break;

                    case MsDnsRecordType.Cname:
                        cnameRecords.Add(record as MsDnsCnameRecord);
                        break;

                    case MsDnsRecordType.Mx:
                        mxRecords.Add(record as MsDnsMxRecord);
                        break;

                    case MsDnsRecordType.Ns:
                        nsRecords.Add(record as MsDnsNsRecord);
                        break;
                }
            }

            CreateRecords(aRecords);
            CreateRecords(cnameRecords);
            CreateRecords(mxRecords);
            CreateRecords(nsRecords);
        }

        /// <summary>
        /// Creates a list of A records.
        /// </summary>
        /// <param name="records">List of records to create.</param>
        public void CreateRecords(List<MsDnsARecord> recordList)
        {
            ManagementPath path = new ManagementPath("MicrosoftDNS_AType");
            ManagementClass zone = new ManagementClass(WmiScope, path, null);
            ManagementBaseObject p = zone.GetMethodParameters(
                "CreateInstanceFromPropertyData");

            foreach (MsDnsARecord record in recordList)
            {
                p.Properties["DnsServerName"].Value = WmiScope.Path.Server;
                p.Properties["TTL"].Value = record.TTL;
                p.Properties["ContainerName"].Value = record.Container;
                p.Properties["OwnerName"].Value = record.Owner;
                p.Properties["IPAddress"].Value = record.Value;
                zone.InvokeMethod("CreateInstanceFromPropertyData", p, null);
            }
        }

        /// <summary>
        /// Creates a list of CNAME (conacle name) records.
        /// </summary>
        /// <param name="records">List of records to create.</param>
        public void CreateRecords(List<MsDnsCnameRecord> recordList)
        {
            ManagementPath path = new ManagementPath("MicrosoftDNS_CNAMEType");
            ManagementClass zone = new ManagementClass(WmiScope, path, null);
            ManagementBaseObject p = zone.GetMethodParameters(
                "CreateInstanceFromPropertyData");

            foreach (MsDnsCnameRecord record in recordList)
            {
                if (string.IsNullOrEmpty(record.Name))
                {
                    throw new InvalidOperationException("CNAME records must have a name specified.");
                }

                p.Properties["DnsServerName"].Value = WmiScope.Path.Server;
                p.Properties["TTL"].Value = record.TTL;
                p.Properties["PrimaryName"].Value = record.Value;
                p.Properties["ContainerName"].Value = record.Container;
                p.Properties["OwnerName"].Value = record.Owner;
                zone.InvokeMethod("CreateInstanceFromPropertyData", p, null);
            }
        }

        /// <summary>
        /// Creates a list of MX (mail exchange) records.
        /// </summary>
        /// <param name="records">List of records to create.</param>
        public void CreateRecords(List<MsDnsMxRecord> recordList)
        {
            ManagementPath path = new ManagementPath("MicrosoftDNS_MXType");
            ManagementClass zone = new ManagementClass(WmiScope, path, null);
            ManagementBaseObject p = zone.GetMethodParameters(
                "CreateInstanceFromPropertyData");

            foreach (MsDnsMxRecord record in recordList)
            {
                p.Properties["DnsServerName"].Value = WmiScope.Path.Server;
                p.Properties["TTL"].Value = record.TTL;
                p.Properties["ContainerName"].Value = record.Container;
                p.Properties["OwnerName"].Value = record.Owner;
                p.Properties["Preference"].Value = record.Priority;
                p.Properties["MailExchange"].Value = record.Value;
                zone.InvokeMethod("CreateInstanceFromPropertyData", p, null);
            }
        }

        /// <summary>
        /// Creates a list of NS (name server) records.
        /// </summary>
        /// <param name="records">List of records to create.</param>
        public void CreateRecords(List<MsDnsNsRecord> records)
        {
            ManagementPath path = new ManagementPath("MicrosoftDNS_NSType");
            ManagementClass zone = new ManagementClass(WmiScope, path, null);
            ManagementBaseObject p = zone.GetMethodParameters(
                "CreateInstanceFromPropertyData");

            foreach (MsDnsNsRecord record in records)
            {
                p.Properties["DnsServerName"].Value = WmiScope.Path.Server;
                p.Properties["TTL"].Value = record.TTL;
                p.Properties["ContainerName"].Value = record.Container;
                p.Properties["OwnerName"].Value = record.Owner;
                p.Properties["NSHost"].Value = record.Value;
                zone.InvokeMethod("CreateInstanceFromPropertyData", p, null);
            }
        }

        /// <summary>
        /// Deletes all the NS records from a specified zone.
        /// </summary>
        /// <param name="zone">Zone to remove NS records from.</param>
        public void DeleteNsRecords(MsDnsZone zone)
        {
            // Select NS records where the owner is that of domain.
            ObjectQuery query = new ObjectQuery("SELECT * FROM "
                + "MicrosoftDNS_NSType WHERE OwnerName = '" + zone.Name + "'");
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection records = searcher.Get();

            // Forceably remove all ns records.
            foreach (ManagementObject record in records)
            {
                record.Delete();
            }
        }

        /// <summary>
        /// This will default the SOA record from a zone so that
        /// the TTL, refresh and expiry times comply with RFC standard
        /// as well as setting the responsible person and name servers.
        /// <param name="zone">Zone to be defaulted.</param>
        /// </summary>
        public void DefaultSoaRecord(MsDnsZone zone)
        {
            // Select SOA records where the owner is that of domain.
            ObjectQuery query = new ObjectQuery("SELECT * FROM "
                + "MicrosoftDNS_SOAType WHERE OwnerName = '" + zone.Name + "'");
            ManagementObjectSearcher searcher =
                new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection recordCollection = searcher.Get();

            foreach (ManagementObject record in recordCollection)
            {
                ManagementBaseObject p = record.GetMethodParameters("Modify");
                p.Properties["ExpireLimit"].Value = zone.SoaRecord.ExpireLimit;
                p.Properties["MinimumTTL"].Value = zone.SoaRecord.MinimumTTL;
                p.Properties["PrimaryServer"].Value = zone.SoaRecord.PrimaryServer;
                p.Properties["RefreshInterval"].Value = zone.SoaRecord.RefreshInterval;
                p.Properties["ResponsibleParty"].Value = zone.SoaRecord.ResponsibleParty;
                p.Properties["RetryDelay"].Value = zone.SoaRecord.RetryDelay;
                p.Properties["SerialNumber"].Value = zone.SoaRecord.SerialNumber;
                record.InvokeMethod("Modify", p, null);
            }
        }

        public List<MsDnsZone> GetZones(bool getRecords)
        {
            // Select SOA records where the owner is that of domain.
            ObjectQuery query = new ObjectQuery("SELECT * FROM MicrosoftDNS_Zone");
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection recordCollection = searcher.Get();

            List<MsDnsZone> zoneList = new List<MsDnsZone>();

            foreach (ManagementObject record in recordCollection)
            {
                MsDnsZone zone = new MsDnsZone(
                    (string)record.Properties["Name"].Value,
                    (ZoneType)(UInt32)record.Properties["ZoneType"].Value,
                    string.Empty,
                    string.Empty);

                if (getRecords)
                {
                    zone.ARecords = GetARecords(zone);
                    zone.CnameRecords = GetCnameRecords(zone);
                    zone.MxRecords = GetMxRecords(zone);
                    zone.NsRecords = GetNsRecords(zone);
                    zone.ARecords = GetARecords(zone);
                }

                zoneList.Add(zone);
            }

            return zoneList;
        }

        public List<MsDnsARecord> GetARecords(MsDnsZone zone)
        {
            ObjectQuery query = new ObjectQuery(
                "SELECT * FROM MicrosoftDNS_AType WHERE ContainerName = '" + zone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection recordCollection = searcher.Get();

            List<MsDnsARecord> recordList = new List<MsDnsARecord>();
            foreach (ManagementObject record in recordCollection)
            {
                recordList.Add(MsDnsARecord.Parse(record, zone));
            }
            return recordList;
        }

        public List<MsDnsMxRecord> GetMxRecords(MsDnsZone zone)
        {
            ObjectQuery query = new ObjectQuery(
                "SELECT * FROM MicrosoftDNS_MXType WHERE ContainerName = '" + zone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection recordCollection = searcher.Get();

            List<MsDnsMxRecord> recordList = new List<MsDnsMxRecord>();
            foreach (ManagementObject record in recordCollection)
            {
                recordList.Add(MsDnsMxRecord.Parse(record, zone));
            }
            return recordList;
        }

        public List<MsDnsCnameRecord> GetCnameRecords(MsDnsZone zone)
        {
            ObjectQuery query = new ObjectQuery(
                "SELECT * FROM MicrosoftDNS_CNAMEType WHERE ContainerName = '" + zone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection recordCollection = searcher.Get();

            List<MsDnsCnameRecord> recordList = new List<MsDnsCnameRecord>();
            foreach (ManagementObject record in recordCollection)
            {
                recordList.Add(MsDnsCnameRecord.Parse(record, zone));
            }
            return recordList;
        }

        public List<MsDnsNsRecord> GetNsRecords(MsDnsZone zone)
        {
            ObjectQuery query = new ObjectQuery(
                "SELECT * FROM MicrosoftDNS_NSType WHERE ContainerName = '" + zone.Name + "'");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection recordCollection = searcher.Get();

            List<MsDnsNsRecord> recordList = new List<MsDnsNsRecord>();
            foreach (ManagementObject record in recordCollection)
            {
                recordList.Add(MsDnsNsRecord.Parse(record, zone));
            }
            return recordList;
        }

        public void UpdateRecordTtl(MsDnsRecord dnsRecord)
        {
            string type = string.Empty;
            string value = string.Empty;

            if (dnsRecord is MsDnsARecord)
            {
                type = "MicrosoftDNS_AType";
                value = dnsRecord.Value;
            }
            else if (dnsRecord is MsDnsMxRecord)
            {
                MsDnsMxRecord mxRecord = dnsRecord as MsDnsMxRecord;
                type = "MicrosoftDNS_MXType";
                value = mxRecord.Priority + " " + mxRecord.Value;
            }
            else if (dnsRecord is MsDnsCnameRecord)
            {
                MsDnsCnameRecord cnameRecord = dnsRecord as MsDnsCnameRecord;
                type = "MicrosoftDNS_CNAMEType";
                value = cnameRecord.Value;
            }
            else
            {
                throw new NotSupportedException(
                    "Derrived DNS record type is not supported.");
            }

            ObjectQuery query = new ObjectQuery(
                "SELECT * FROM " + type + " " +
                "WHERE OwnerName = '" + dnsRecord.Owner + "' " +
                "AND RecordData = '" + dnsRecord.Value + "' ");

            ManagementObjectSearcher searcher = new ManagementObjectSearcher(WmiScope, query);
            ManagementObjectCollection recordCollection = searcher.Get();

            foreach (ManagementObject record in recordCollection)
            {
                record.Delete();
            }

            ManagementPath path = new ManagementPath(type);
            ManagementClass zone = new ManagementClass(WmiScope, path, null);
            ManagementBaseObject createParams = zone.GetMethodParameters(
                "CreateInstanceFromPropertyData");

            createParams.Properties["DnsServerName"].Value = WmiScope.Path.Server;
            createParams.Properties["TTL"].Value = dnsRecord.TTL;
            createParams.Properties["ContainerName"].Value = dnsRecord.Container;
            createParams.Properties["OwnerName"].Value = dnsRecord.Owner;

            if (dnsRecord is MsDnsARecord)
            {
                createParams.Properties["IPAddress"].Value = dnsRecord.Value;
            }
            else if (dnsRecord is MsDnsMxRecord)
            {
                MsDnsMxRecord mxRecord = dnsRecord as MsDnsMxRecord;
                createParams.Properties["Preference"].Value = mxRecord.Priority;
                createParams.Properties["MailExchange"].Value = dnsRecord.Value;
            }
            else if (dnsRecord is MsDnsCnameRecord)
            {
                createParams.Properties["PrimaryName"].Value = dnsRecord.Value;
            }

            zone.InvokeMethod("CreateInstanceFromPropertyData", createParams, null);
        }
    }
}