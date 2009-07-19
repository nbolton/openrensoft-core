using System;
using System.Management;

namespace Rensoft.ServerManagement
{
    /// <summary>
    /// Allows paths and scopes to be set up for WMI and/or ADSI.
    /// </summary>
    public abstract class ServerManager : MarshalByRefObject
    {
        /// <summary>
        /// Gets the scope to use for WMI.
        /// </summary>
        public ManagementScope WmiScope { get; private set; }

        /// <summary>
        /// Gets the path to use for ADSI.
        /// </summary>
        public string AdsiPath { get; private set; }

        /// <summary>
        /// Gets the Windows server name.
        /// </summary>
        public string ServerName { get; private set; }

        /// <summary>
        /// Initialize a ServerManager (both WMI and ADSI) for a specific server.
        /// </summary>
        /// <param name="server">Server used for WMI.</param>
        /// <param name="wmiNamespace">Namespace used for WMI.</param>
        /// <param name="adsiProvider">ADSI provider (e.g. WinNT://).</param>
        /// <param name="adsiContainer">ADSI container (e.g. ,Computer)</param>
        public ServerManager(string server, string wmiNamespace,
            string adsiProvider, string adsiContainer)
        {
            this.ServerName = server;

            // Set WMI path if namespace is set.
            if (!String.IsNullOrEmpty(wmiNamespace))
            {
                // Set the namespace path to base scope on.
                ManagementPath path = new ManagementPath();
                path.Server = server;
                path.NamespacePath = wmiNamespace;

                // Set up the scope with packet auth encryption.
                this.WmiScope = new ManagementScope(path);
                this.WmiScope.Options.Authentication =
                    AuthenticationLevel.PacketPrivacy;
            }

            // Set ADSI path if provider is set.
            if (!String.IsNullOrEmpty(adsiProvider))
            {
                this.AdsiPath = String.Format("{0}{1}{2}",
                    adsiProvider, server, adsiContainer);
            }
        }

        /// <summary>
        /// Initialize a ServerManager (WMI only) for a specific server.
        /// </summary>
        /// <param name="server">Server used for WMI.</param>
        /// <param name="wmiNamespace">Namespace used for WMI.</param>
        public ServerManager(string server, string wmiNamespace)
            : this(server, wmiNamespace, null, null) { }

        /// <summary>
        /// Initialize a ServerManager (ADSI only) for a specific server.
        /// </summary>
        /// <param name="server">Server used for ADSI.</param>
        /// <param name="adsiProvider">ADSI provider (e.g. WinNT://).</param>
        /// <param name="adsiContainer">ADSI container (e.g. ,Computer)</param>
        public ServerManager(string server, string adsiProvider, string adsiContainer)
            : this(server, null, adsiProvider, adsiContainer) { }
    }
}
