using System;
using System.Collections.Generic;
using System.Text;

namespace Rensoft.ServerManagement.DNS
{
    /// <summary>
    /// Contains information for any type of record. This is inherited by
    /// such class representations of the A record, and CNAME record, etc.
    /// </summary>
    public abstract class MsDnsRecord : MarshalByRefObject
    {
        private string value;
        private string name;
        private MsDnsZone zone;
        private int ttl;

        /// <summary>
        /// Gets or sets the value part of the record. This would hold
        /// an IP address, or hostname for example.
        /// </summary>
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or sets the name part of the record. This represents the
        /// first part of the owner value of a record. For example, the owner
        /// is "fabrikham.contoso.com", where "fabrikham" is the name.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the parent zone part of the record. This represents
        /// the container of the record. For example, contoso.com.
        /// </summary>
        public MsDnsZone Zone
        {
            get { return zone; }
            set { zone = value; }
        }

        /// <summary>
        /// Gets or sets the TTL (time to live) part of the record. This
        /// represents how long a record will live.
        /// </summary>
        public int TTL
        {
            get { return ttl; }
            set { ttl = value; }
        }

        /// <summary>
        /// Gets the container part of the record. This is found as the Domain
        /// property of the Zone property in this object.
        /// </summary>
        public string Container
        {
            get { return Zone.Name; }
        }

        /// <summary>
        /// Gets the owner part of the record. This represents the full owner
        /// name of the record which is comprised of the Container and the Name.
        /// </summary>
        public string Owner
        {
            get
            {
                // For named records, concatenate with Container.
                if (!String.IsNullOrEmpty(Name))
                {
                    return String.Format("{0}.{1}", Name, Container);
                }

                // Owner same as container.
                return Container;
            }
        }

        /// <summary>
        /// Gets the enumerated type equivilent of the most derrived version of this object.
        /// </summary>
        public MsDnsRecordType DnsType
        {
            get
            {
                if (this is MsDnsARecord)
                {
                    return MsDnsRecordType.A;
                }
                else if (this is MsDnsCnameRecord)
                {
                    return MsDnsRecordType.Cname;
                }
                else if (this is MsDnsMxRecord)
                {
                    return MsDnsRecordType.Mx;
                }
                else if (this is MsDnsNsRecord)
                {
                    return MsDnsRecordType.Ns;
                }
                else if (this is MsDnsSoaRecord)
                {
                    return MsDnsRecordType.Soa;
                }
                else
                {
                    throw new NotSupportedException(
                        "Type " + GetType().FullName + " not supported.");
                }
            }
        }

        /// <summary>
        /// Initialize a full DNS record with all properties set.
        /// </summary>
        /// <param name="name">First part of the "owner" property.</param>
        /// <param name="value">IP address, hostname, etc.</param>
        /// <param name="zone">Parent zone for the record.</param>
        /// <param name="ttl">Record time to live value.</param>
        public MsDnsRecord(string name, string value, MsDnsZone zone, int ttl)
        {
            if (!string.IsNullOrEmpty(name))
            {
                // Convert container style name to small form name.
                name = name.Replace(zone.Name, null).Trim('.');
            }

            this.Name = name;
            this.Value = value;
            this.Zone = zone;
            this.TTL = ttl;
        }
    }
}