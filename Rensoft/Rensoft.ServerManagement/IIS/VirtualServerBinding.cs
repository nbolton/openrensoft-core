using System;
using System.Management;

namespace Rensoft.ServerManagement.IIS
{
    /// <summary>
    /// Virtual servers must be bound to a hostname as a means
    /// of access. Any number of hostnames can be bound to
    /// any virtual server.
    /// </summary>
    public class VirtualServerBinding : MarshalByRefObject
    {
        private string ip;
        private int port;
        private string hostname;

        /// <summary>
        /// IP address (leave null for default IP).
        /// </summary>
        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        /// <summary>
        /// Port number to bind to (usually port 80).
        /// </summary>
        public int Port
        {
            get { return port; }
            set { port = value; }
        }

        /// <summary>
        /// Hostname to bind to an IIS virtual server.
        /// </summary>
        public string Hostname
        {
            get { return hostname; }
            set { hostname = value; }
        }

        /// <summary>
        /// Initialize a new VirtualServerBinding.
        /// </summary>
        /// <param name="hostname">Hostname to bind to.</param>
        /// <param name="ip">IP address to bind to.</param>
        /// <param name="manager">For ManagementBaseObject conversion.</param>
        public VirtualServerBinding(
            string hostname)
            : this(null, 80, hostname) { }

        /// <summary>
        /// Initialize a new VirtualServerBinding.
        /// </summary>
        /// <param name="ip">IP address to bind to.</param>
        /// <param name="hostname">Hostname to bind to.</param>
        /// <param name="port">Port number to bind to.</param>
        /// <param name="manager">For ManagementBaseObject conversion.</param>
        public VirtualServerBinding(
            string ip, int port)
            : this(ip, port, null) { }

        /// <summary>
        /// Initialize a new VirtualServerBinding.
        /// </summary>
        /// <param name="hostname">Hostname to bind to.</param>
        /// <param name="ip">IP address to bind to.</param>
        /// <param name="port">Port number to bind to.</param>
        /// <param name="manager">For ManagementBaseObject conversion.</param>
        public VirtualServerBinding(
            string ip, int port, string hostname)
        {
            this.IP = ip;
            this.port = port;
            this.Hostname = hostname;
        }

        /// <summary>
        /// Returns binding as a ManagementBaseObject.
        /// </summary>
        /// <returns>ManagementBaseObject representation.</returns>
        public ManagementBaseObject ToBaseObject(VirtualServerManager manager)
        {
            ManagementPath path = new ManagementPath();
            path.ClassName = "ServerBinding";

            ManagementClass bindingClass = new ManagementClass(
                manager.WmiScope, path, null);
            ManagementObject binding = bindingClass.CreateInstance();

            binding["Hostname"] = Hostname;
            binding["Port"] = port.ToString();
            binding["IP"] = IP;
            binding.Put();

            return binding as ManagementBaseObject;
        }
    }
}
