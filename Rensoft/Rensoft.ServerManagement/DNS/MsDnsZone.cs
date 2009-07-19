using System;
using System.Collections.Generic;

namespace Rensoft.ServerManagement.DNS
{
    /// <summary>
    /// Used for specifying the zone's type.
    /// </summary>
    public enum ZoneType
    {
        PrimaryZone = 0,
        SecondaryZone = 1,
        StubZone = 2,
        ZoneForwarder = 3
    }

    /// <summary>
    /// Holds all information for DNS zone.
    /// </summary>
    public class MsDnsZone : MarshalByRefObject
    {
        private string name;
        private ZoneType type;
        private List<MsDnsARecord> aRecords;
        private List<MsDnsCnameRecord> cnameRecords;
        private List<MsDnsMxRecord> mxRecords;
        private List<MsDnsNsRecord> nsRecords;
        private MsDnsSoaRecord soaRecord;

        /// <summary>
        /// Gets or sets the type of zone.
        /// </summary>
        public ZoneType Type
        {
            get { return type; }
            set { type = value; }
        }

        /// <summary>
        /// Gets or sets the domain name which this zone represents.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the list of A records.
        /// </summary>
        public List<MsDnsARecord> ARecords
        {
            get { return aRecords; }
            set { aRecords = value; }
        }

        /// <summary>
        /// Gets or sets the list of CNAME records.
        /// </summary>
        public List<MsDnsCnameRecord> CnameRecords
        {
            get { return cnameRecords; }
            set { cnameRecords = value; }
        }

        /// <summary>
        /// Gets or sets the list of MX records.
        /// </summary>
        public List<MsDnsMxRecord> MxRecords
        {
            get { return mxRecords; }
            set { mxRecords = value; }
        }

        /// <summary>
        /// Gets or sets the list of NS records.
        /// </summary>
        public List<MsDnsNsRecord> NsRecords
        {
            get { return nsRecords; }
            set { nsRecords = value; }
        }

        /// <summary>
        /// Gets or sets the Start Of Authority record.
        /// </summary>
        public MsDnsSoaRecord SoaRecord
        {
            get { return soaRecord; }
            set { soaRecord = value; }
        }

        public List<MsDnsRecord> MixedReords
        {
            get
            {
                List<MsDnsRecord> recordList = new List<MsDnsRecord>();

                foreach (MsDnsARecord record in aRecords)
                {
                    recordList.Add(record);
                }

                foreach (MsDnsCnameRecord record in cnameRecords)
                {
                    recordList.Add(record);
                }

                foreach (MsDnsMxRecord record in mxRecords)
                {
                    recordList.Add(record);
                }

                foreach (MsDnsNsRecord record in nsRecords)
                {
                    recordList.Add(record);
                }

                recordList.Add(soaRecord);

                return recordList;
            }
        }

        /// <summary>
        /// Initialize a new DnsZone.
        /// </summary>
        /// <param name="domain">Domain name to represent.</param>
        /// <param name="primaryServer">FQDN server name for SOA.</param>
        /// <param name="responsibleParty">Responsible party for SOA.</param>
        public MsDnsZone(
            string domain,
            ZoneType type,
            string primaryServer,
            string responsibleParty)
        {
            this.Name = domain;
            this.Type = type;
            this.ARecords = new List<MsDnsARecord>();
            this.CnameRecords = new List<MsDnsCnameRecord>();
            this.MxRecords = new List<MsDnsMxRecord>();
            this.NsRecords = new List<MsDnsNsRecord>();
            this.SoaRecord = new MsDnsSoaRecord(
                this, primaryServer, responsibleParty);
        }
    }
}
