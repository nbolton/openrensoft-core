using System;
using System.Collections.Generic;
using System.Text;

namespace Rensoft.ServerManagement.DNS
{
    /// <summary>
    /// DNS Start of Authority record.
    /// </summary>
    public class MsDnsSoaRecord : MsDnsRecord
    {
        private string primaryServer;
        private string responsibleParty;
        private int expireLimit;
        private int minimumTtl;
        private int refreshInterval;
        private int retryDelay;

        /// <summary>
        /// Gets or sets the time, in seconds, before an unresponsive
        /// zone is no longer authoritative.
        /// </summary>
        public int ExpireLimit
        {
            get { return expireLimit; }
            set { expireLimit = value; }
        }

        /// <summary>
        /// Gets or sets the lower limit on the time, in seconds, that
        /// a DNS Server or Caching resolver are allowed to cache any
        /// resource record from the zone to which this record belongs.
        /// </summary>
        public int MinimumTTL
        {
            get { return minimumTtl; }
            set { minimumTtl = value; }
        }

        /// <summary>
        /// Gets or sets the authoritative DNS Server for the zone to
        /// which the record belongs.
        /// </summary>
        public string PrimaryServer
        {
            get { return primaryServer; }
            set { primaryServer = value; }
        }

        /// <summary>
        /// Gets or sets the time, in seconds, before the zone containing
        /// this record should be refreshed.
        /// </summary>
        public int RefreshInterval
        {
            get { return refreshInterval; }
            set { refreshInterval = value; }
        }

        /// <summary>
        /// Gets or sets the name of the responsible party for the zone to
        /// which the record belongs.
        /// </summary>
        public string ResponsibleParty
        {
            get { return responsibleParty; }
            set { responsibleParty = value; }
        }

        /// <summary>
        /// Gets or sets the time, in seconds, before retrying a failed
        /// refresh of the zone to which this record belongs.
        /// </summary>
        public int RetryDelay
        {
            get { return retryDelay; }
            set { retryDelay = value; }
        }

        /// <summary>
        /// Gets the serial number of the SOA record (RFC1912).
        /// </summary>
        public int SerialNumber
        {
            get
            {
                return int.Parse(DateTime.Now.Year.ToString() +
                    DateTime.Now.Month.ToString().PadLeft(2, '0') +
                    DateTime.Now.Day.ToString().PadLeft(2, '0') + "01");
            }
        }

        /// <summary>
        /// Initialize a new DNS SOA record.
        /// </summary>
        public MsDnsSoaRecord(
            MsDnsZone zone,
            string primaryServer,
            string responsibleParty,
            int expireLimit,
            int minimumTtl,
            int refreshInterval,
            int retryDelay)
            : base(null, null, zone, minimumTtl)
        {
            this.PrimaryServer = primaryServer;
            this.ResponsibleParty = responsibleParty;
            this.ExpireLimit = expireLimit;
            this.MinimumTTL = minimumTtl;
            this.RefreshInterval = refreshInterval;
            this.RetryDelay = retryDelay;
        }

        /// <summary>
        /// Initialize a new DNS SOA record with default values.
        /// </summary>
        public MsDnsSoaRecord(
            MsDnsZone zone,
            string primaryServer,
            string responsibleParty)
            : this(
            zone,
            primaryServer,
            responsibleParty,
            1209600, // RFC ExpireLimit
            86400, // RFC MinimumTTL
            3600, // RFC RefreshInterval
            600) { } // RFC RetryDelay
    }
}
