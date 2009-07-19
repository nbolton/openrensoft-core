using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using Rensoft.ServerManagement.Security;
using System.DirectoryServices;
using System.Runtime.InteropServices;
using Rensoft.ServerManagement;

namespace Rensoft.ServerManagement.IIS
{
    public class VirtualServerManager : ServerManager
    {
        private VirtualServerPath defaultVirtualServerPath;
        private string defaultApplicationPoolPath;

        /// <summary>
        /// Gets the system IUSR password from the metabase.
        /// </summary>
        public string IusrPassword
        {
            get
            {
                try
                {
                    ManagementPath path = new ManagementPath();
                    path.RelativePath = this.defaultVirtualServerPath;
                    path.ClassName = "IISWebServerSetting";

                    ManagementObject settings = new ManagementObject(WmiScope, path, null);
                    PropertyDataCollection properties = settings.Properties;
                    return properties["AnonymousUserPass"].Value.ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Could not get the IUSR user password.", ex);
                }
            }
        }

        /// <summary>
        /// Gets the system IWAM password from the metabase.
        /// </summary>
        public string IwamPassword
        {
            get
            {
                try
                {
                    ManagementPath path = new ManagementPath();
                    path.RelativePath = this.defaultApplicationPoolPath;
                    path.ClassName = "IIsApplicationPoolSetting";

                    ManagementObject settings = new ManagementObject(WmiScope, path, null);
                    PropertyDataCollection properties = settings.Properties;

                    return properties["WAMUserPass"].Value.ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "Could not get the IWAM user password.", ex);
                }
            }
        }

        /// <summary>
        /// Initialize a new VirtualServerManager using a specified server.
        /// </summary>
        /// <param name="serverName">Specific server name.</param>
        public VirtualServerManager(string serverName)
            : this(serverName, 1, "DefaultAppPool") { }

        /// <summary>
        /// Initialize a new VirtualServerManager using a specified server
        /// and default virtual server.
        /// </summary>
        /// <param name="serverName">Specific server name.</param>
        /// <param name="defaultSite">Default virtual server.</param>
        public VirtualServerManager(
            string serverName,
            int defaultVirtualServer,
            string defaultAppPool)
            : base(serverName, @"Root\MicrosoftIISv2", "IIS://", "/W3SVC")
        {
            this.defaultVirtualServerPath =
                new VirtualServerPath(defaultVirtualServer);

            this.defaultApplicationPoolPath =
                "IIsApplicationPool='W3SVC/AppPools/" + defaultAppPool + "'";
        }

        public bool Exists(VirtualServer virtualServer)
        {
            try
            {
                return DirectoryEntry.Exists(AdsiPath + "/" + virtualServer.Path.Id);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "Could not check existance of virtual " +
                    "service '" + virtualServer.Path.Id + "'.", ex);
            }
        }

        /// <summary>
        /// Creates a new IIS virtual server.
        /// </summary>
        /// <param name="virtualServer">Virtual server to create.</param>
        /// <param name="start">Start the virtual server.</param>
        /// <returns>Virtual server with updated path.</returns>
        public /*VirtualServer*/ void Create(VirtualServer virtualServer, bool start)
        {
            ManagementPath createPath = new ManagementPath();
            createPath.RelativePath = "IIsWebService.Name='W3SVC'";
            ManagementObject management = new ManagementObject(
                WmiScope, createPath, null);

            ManagementBaseObject inParams =
                management.GetMethodParameters("CreateNewSite");
            inParams["PathOfRootVirtualDir"] = virtualServer.HomeDirectory;
            inParams["ServerBindings"] = virtualServer.GetBaseObjectBindings(this);
            inParams["ServerComment"] = virtualServer.Description;

            // Execute the CreateNewSite method and get returned params.
            ManagementBaseObject outParams = management.
                InvokeMethod("CreateNewSite", inParams, null);

            // Set the new site path.
            virtualServer.Path = new VirtualServerPath(
                outParams.Properties["ReturnValue"].Value.ToString());

            ManagementPath settingsPath = new ManagementPath();
            settingsPath.RelativePath = virtualServer.Path;
            settingsPath.ClassName = "IIsWebServerSetting";

            ManagementObject siteSettings =
                new ManagementObject(WmiScope, settingsPath, null);
            PropertyDataCollection properties = siteSettings.Properties;

            // If anonymous access is set, then set the anonymous user values.
            if ((virtualServer.AuthFlags & VirtualServerAuthFlag.Anonymous) != 0)
            {
                WindowsUser user = virtualServer.AnonymousUser;
                properties["AnonymousUserName"].Value = user.Username;
                properties["AnonymousUserPass"].Value = user.Password;
            }

            // Set only if specified, otherwise DefaultAppPool is used.
            if (!string.IsNullOrEmpty(virtualServer.ApplicationPool))
            {
                properties["AppPoolId"].Value =
                    virtualServer.ApplicationPool;
            }

            // Apply access, auth and application pool values.
            properties["AuthFlags"].Value = (int)virtualServer.AuthFlags;
            properties["AccessFlags"].Value = (int)virtualServer.AccessFlags;
            
            // Update the settings.
            siteSettings.Put();

            // Take the ID for the virtual server and use root folder.
            ManagementPath rootSettingsPath = new ManagementPath(
                String.Format(
                    "IIsWebVirtualDirSetting='W3SVC/{0}/ROOT'",
                    virtualServer.Path.Id));

            ManagementObject rootSettings =
                new ManagementObject(WmiScope, rootSettingsPath, null);
            PropertyDataCollection rootProperties = rootSettings.Properties;

            // Assign AppFriendlyName always; it may be in the DefaultAppPool.
            rootProperties["AppFriendlyName"].Value = virtualServer.Description;
            rootProperties["HttpRedirect"].Value = virtualServer.HttpRedirect;
            rootSettings.Put();

            if (start)
            {
                // Start the virtual server if parameter is true.
                this.ChangeState(virtualServer, VirtualServerState.Start);
            }

            //return virtualServer;
        }

        public void Modify(
            VirtualServer virtualServer,
            VirtualServerModifyMode flags)
        {
            throw new NotImplementedException(
                "Modificaiton of IIS virtual servers is not yet supported.");
        }

        /// <summary>
        /// Remove an existing service from the database.
        /// </summary>
        /// <param name="hostingPath">Path to hosting service.</param>
        public void Remove(VirtualServer virtualServer)
        {
            DirectoryEntry removeSite;
            DirectoryEntry virtualServerList = new DirectoryEntry(AdsiPath);

            try
            {
                removeSite = virtualServerList.Children.Find(
                    virtualServer.Path.Id.ToString(), "IISWebServer");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "The virtual server with an identity " +
                    "of " + virtualServer.Path.Id + " does not " +
                    "exist, and so could not be removed.", ex);
            }

            try
            {
                // And completely remove the site.
                virtualServerList.Children.Remove(removeSite);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "The virtual server could not be removed.", ex);
            }
        }

        /// <summary>
        /// Changes the state of the virtual server.
        /// </summary>
        /// <param name="virtualServer">Virtual server to affect.</param>
        /// <param name="state">State to change to (e.g. Start).</param>
        public void ChangeState(
            VirtualServer virtualServer,
            VirtualServerState state)
        {
            ManagementPath path = new ManagementPath();
            path.ClassName = "IISWebServer";
            path.RelativePath = virtualServer.Path;
            ManagementObject site = new ManagementObject(
                WmiScope, path, null);

            switch (state)
            {
                case VirtualServerState.Start:
                    try
                    {
                        site.InvokeMethod("Start", null);
                    }
                    catch (COMException ex)
                    {
                        throw new Exception(
                            "Unable to start the virtual server. Another " +
                            "virtual server may already be using the port " +
                            "configured for this virtual server.", ex);
                    }
                    break;

                case VirtualServerState.Stop:
                    site.InvokeMethod("Stop", null);
                    break;

                case VirtualServerState.Pause:
                    site.InvokeMethod("Pause", null);
                    break;

                case VirtualServerState.Continue:
                    site.InvokeMethod("Continue", null);
                    break;
            }
        }
    }

    /// <summary>
    /// These flags be combined to perform specific modify operations.
    /// </summary>
    [Flags]
    public enum VirtualServerModifyMode
    {
        /// <summary>
        /// Only modify the virtual server directory.
        /// </summary>
        Directory = 0x01,

        /// <summary>
        /// Only modify the virtual server bindings.
        /// </summary>
        Bindings = 0x02,

        /// <summary>
        /// Only modify the virtual server auth, access and redirect flags.
        /// </summary>
        Flags = 0x04,

        /// <summary>
        /// Only modify the name of the application pool.
        /// </summary>
        ApplicationPool = 0x08,

        /// <summary>
        /// Modify all aspects of the virtual server.
        /// </summary>
        Everything = Directory | Bindings | Flags | ApplicationPool,
    }
}
