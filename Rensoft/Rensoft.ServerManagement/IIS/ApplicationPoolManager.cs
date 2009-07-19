using System;
using System.DirectoryServices;
using Rensoft.ServerManagement.Security;

namespace Rensoft.ServerManagement.IIS
{
    public class ApplicationPoolManager : ServerManager
    {
        /// <summary>
        /// Intialize a new ApplicationPoolManager using a specified server.
        /// </summary>
        /// <param name="serverName">NetBIOS server name.</param>
        public ApplicationPoolManager(string serverName)
            : base(serverName, "IIS://", "/W3SVC/AppPools") { }

        public bool Exists(ApplicationPool appPool)
        {
            try
            {
                return DirectoryEntry.Exists(AdsiPath + "/" + appPool.Name);
            }
            catch (Exception ex)
            {
                throwIfNotSupported(ex);

                throw new Exception(
                    "Could not check existance of application pool '" + appPool.Name + "'", ex);
            }
        }

        private void throwIfNotSupported(Exception ex)
        {
            if (ex.Message == "Unknown error (0x80005000)")
            {
                throw new NotSupportedException(
                    "The server '" + ServerName + "' does not support IIS ADSI.");
            }
        }

        /// <summary>
        /// Create a new Application pool.
        /// </summary>
        /// <param name="applicationPool">Application pool to create.</param>
        public void Create(ApplicationPool applicationPool)
        {
            try
            {
                DirectoryEntry appPools = new DirectoryEntry(AdsiPath);

                // Attempt to add the new application pool.
                DirectoryEntry newPool = appPools.Children.Add(
                    applicationPool.Name, "IIsApplicationPool");
                
                newPool.CommitChanges();

                try
                {
                    if (applicationPool.IdentityType ==
                        ApplicationPoolIdentityType.SpecificUser)
                    {
                        // Update application pool with user and password.
                        newPool.Properties["WamUserName"].Value =
                            applicationPool.User.Username;
                        newPool.Properties["WamUserPass"].Value =
                            applicationPool.User.Password;
                    }

                    newPool.Properties["AppPoolIdentityType"].Value =
                        (int)applicationPool.IdentityType;
                    newPool.CommitChanges();
                }
                catch (Exception ex)
                {
                    throw new Exception(
                        "An error occured while setting the " +
                        "properties of the new IIS Application Pool.", ex);
                }
            }
            catch (Exception ex)
            {
                throwIfNotSupported(ex);

                throw new Exception(
                    "Could not create the IIS Application Pool, " +
                    "'" + applicationPool.Name + "'.", ex);
            }
        }

        public void Modify(string appPoolName, ApplicationPool modified)
        {
            try
            {
                DirectoryEntry appPools = new DirectoryEntry(AdsiPath);
                DirectoryEntry appPoolEntry = appPools.Children.Find(
                    appPoolName, "IIsApplicationPool");

                if (appPoolName != modified.Name)
                {
                    appPoolEntry.Rename(modified.Name);
                }

                if (modified.IdentityType ==
                    ApplicationPoolIdentityType.SpecificUser)
                {
                    WindowsUser user = modified.User;
                    appPoolEntry.Properties["WamUserName"].Value = user.Username;
                    appPoolEntry.Properties["WamUserPass"].Value = user.Password;
                }

                appPoolEntry.Properties["AppPoolIdentityType"].Value =
                    (int)modified.IdentityType;
                appPoolEntry.CommitChanges();
            }
            catch (Exception ex)
            {
                throwIfNotSupported(ex);

                throw new Exception(
                    "An error occured while updating " +
                    "the IIS Application Pool.", ex);
            }
        }

        /// <summary>
        /// Remove an existing application pool.
        /// </summary>
        public void Remove(ApplicationPool applicationPool)
        {
            DirectoryEntry appPool = null;

            try
            {
                DirectoryEntry appPools = new DirectoryEntry(AdsiPath);

                appPool = appPools.Children.Find(
                    applicationPool.Name, "IIsApplicationPool");

                // Remove the found application pool.
                appPools.Children.Remove(appPool);
            }
            catch (Exception ex)
            {
                throwIfNotSupported(ex);

                throw new Exception("An error occured while removing " +
                    "the application pool. Check that it exists.", ex);
            }
        }
    }
}
