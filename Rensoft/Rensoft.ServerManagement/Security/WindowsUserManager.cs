using System;
using System.Collections.Generic;
using System.Text;
using System.DirectoryServices;
using System.Security.Principal;
using System.Runtime.InteropServices;
using Rensoft.ServerManagement;
using System.Linq;

namespace Rensoft.ServerManagement.Security
{
    public class WindowsUserManager : ServerManager
    {
        /// <summary>
        /// Initialize a new Windows User ServerManager.
        /// </summary>
        /// <param name="serverName"></param>
        public WindowsUserManager(string serverName)
            : base(serverName, "WinNT://", ",Computer") { }

        /// <summary>
        /// Creates user based on object fields and properties, as well
        /// as adding the user to specified groups.
        /// </summary>
        /// <param name="user">Windows user to create.</param>
        public SecurityIdentifier Create(WindowsUser windowsUser)
        {
            if (windowsUser.Username.Length > 20)
            {
                throw new Exception(
                    "The username '" + windowsUser.Username + "' is longer than " +
                    "20 characters, which is not allowed in Windows.");
            }

            DirectoryEntry theServer = new DirectoryEntry(AdsiPath);
            DirectoryEntry newUser = theServer.Children.Add(windowsUser.Username, "user");

            newUser.Properties["userFlags"].Value = (int)windowsUser.Flags;
            newUser.Properties["description"].Value = windowsUser.Description;
            newUser.Properties["fullName"].Value = windowsUser.FullName;
            setPassword(newUser, windowsUser.Password);

            try
            {
                // Create the user.
                newUser.CommitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while trying to " +
                    "create the Windows user account. Please verify " +
                    "that it doesn't already exist.", ex);
            }

            // Retrieve SID bytes from ADSI for remote DACL modification.
            byte[] sidBytes = (byte[])newUser.Properties["objectSid"].Value;
            windowsUser.Sid = new SecurityIdentifier(sidBytes, 0);

            // Now add the user to it's user groups.
            DirectoryEntry adsiGroup;
            foreach (WindowsUserGroup group in windowsUser.Groups)
            {
                try
                {
                    adsiGroup = theServer.Children.Find(group.Name, "group");
                    Object[] users = new Object[] { newUser.Path.ToString() };
                    adsiGroup.Invoke("Add", users);
                }
                catch (Exception ex)
                {
                    throw new Exception("An error occured while " +
                        "trying to add the Windows user account to " +
                        "the '" + group.Name + "' group.", ex);
                }
            }

            return windowsUser.Sid;
        }

        /// <summary>
        /// Changes the information for a Windows user. This does not
        /// yet support groups, but may do at some point in the future.
        /// </summary>
        /// <param name="oldUser">Used to locate existing user.</param>
        /// <param name="newUser">All information will be applied.</param>
        /// <returns>The previous username.</returns>
        public string Update(WindowsUser windowsUser)
        {
            if (windowsUser.Sid == null)
            {
                throw new NullReferenceException(
                    "Windows user SID for '" + windowsUser.Username +
                    "' cannot be null as it is needed for searching.");
            }

            // Lookup the username string from the SID.
            WindowsUser current = Get(windowsUser.Sid);

            DirectoryEntry server = getServerEntry();
            DirectoryEntry user = getUserEntry(server, current.Username);

            if (user.Name != windowsUser.Username)
            {
                user.Rename(windowsUser.Username);
            }

            user.Properties["userFlags"].Value = (int)windowsUser.Flags;
            user.Properties["description"].Value = windowsUser.Description;
            user.Properties["fullName"].Value = windowsUser.FullName;
            setPassword(user, windowsUser.Password);

            try
            {
                user.CommitChanges();
            }
            catch (Exception ex)
            {
                throw new Exception("An error occured while trying to update " +
                    "the client's Windows user account. Please verify that it " +
                    "exists and that the new information is valid.", ex);
            }

            // Reutrn previous username.
            return current.Username;
        }

        public WindowsUser Get(string username)
        {
            WindowsUser windowsUser = Find(username);
            if (windowsUser == null)
            {
                throw new OperationCanceledException(
                    "Could not find a user with the username '" + username + "'.");
            }
            return windowsUser;
        }

        public WindowsUser Get(SecurityIdentifier sid)
        {
            WindowsUser windowsUser = Find(sid);
            if (windowsUser == null)
            {
                throw new OperationCanceledException(
                    "Could not find a user with the SID '" + sid.ToString() + "'.");
            }
            return windowsUser;
        }

        private DirectoryEntry getServerEntry()
        {
            return new DirectoryEntry(AdsiPath);
        }

        private DirectoryEntry getUserEntry(DirectoryEntry server, string username)
        {
            if (username == null)
            {
                throw new ArgumentNullException("username");
            }
            return server.Children.Find(username, "User");
        }

        private void setPassword(DirectoryEntry user, string password)
        {
            user.Invoke("SetPassword", new Object[] { password });
        }

        public void SetPassword(SecurityIdentifier sid, string password)
        {
            SetPassword(Get(sid).Username, password);
        }

        public void SetPassword(string username, string password)
        {
            DirectoryEntry server = getServerEntry();
            DirectoryEntry user = getUserEntry(server, username);
            setPassword(user, password);
        }

        /// <summary>
        /// Permenantly remove a Windows user.
        /// </summary>
        /// <param name="user">User to remove.</param>
        public void Delete(string userName)
        {
            try
            {
                DirectoryEntry server = new DirectoryEntry(AdsiPath);
                DirectoryEntry userEntry = server.Children.Find(userName, "User");
                server.Children.Remove(userEntry);
            }
            catch (Exception ex)
            {
                throw new Exception(
                    "An error occured while trying to delete the client's " +
                    "Windows user account. Please verify that it exists.", ex);
            }
        }

        /// <summary>
        /// Searches for a user with matching SID. This method can be
        /// fairly intensive depending on the number of total users.
        /// </summary>
        /// <param name="sid">SID of user to search for.</param>
        /// <returns>Username for matching user.</returns>
        public WindowsUser Find(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw new NullReferenceException("Security identifier cannot be null.");
            }

            return GetAll().Where(w => w.Sid == sid).FirstOrDefault();

            //DirectoryEntry server = new DirectoryEntry(AdsiPath);

            //foreach (DirectoryEntry entry in server.Children.OfType("User"))
            //{
            //    WindowsUser windowsUser = parseDirectoryEntry(entry);
            //    if (windowsUser.Sid == sid)
            //    {
            //        return windowsUser;
            //    }
            //}

            //throw new OperationCanceledException(
            //    "Could not find a user with the SID '" + sid.ToString() + "'.");
        }

        public IEnumerable<WindowsUser> GetAll()
        {
            DirectoryEntry server = new DirectoryEntry(AdsiPath);
            return server.Children.OfType("User").Select(e => WindowsUser.Parse(e));
        }

        public WindowsUser Find(string username)
        {
            if (username == null)
            {
                throw new NullReferenceException("Username cannot be null.");
            }

            return GetAll().Where(w => w.Username == username).FirstOrDefault();

            //DirectoryEntry server = new DirectoryEntry(AdsiPath);
            //DirectoryEntry entry = server.Children.Find(username, "User");
            //WindowsUser wu = null;

            //if (entry != null)
            //{
            //    wu = parseDirectoryEntry(entry);
            //}

            //return wu;
        }

        /// <summary>
        /// Checks the existance of a user by attempting to search for it's SID.
        /// </summary>
        /// <param name="sid">User SID to check the existance of.</param>
        /// <returns>A value indicating the existance of the user.</returns>
        public bool Exists(SecurityIdentifier sid)
        {
            return GetAll().Where(u => u.Sid == sid).Count() != 0;

            //try
            //{
            //    Find(sid);
            //    return true;
            //}
            //catch (OperationCanceledException)
            //{
            //    return false;
            //}
        }

        /// <summary>
        /// Checks the existance of a user using the username.
        /// </summary>
        /// <param name="username">Username to check.</param>
        /// <returns>A value indicating the existance of the user.</returns>
        public bool Exists(string username)
        {
            return GetAll().Where(u => u.Username == username).Count() != 0;

            //try
            //{
            //    DirectoryEntry server = new DirectoryEntry(AdsiPath);
            //    return (server.Children.Find(username) != null);
            //}
            //catch (COMException ex)
            //{
            //    if (ex.ErrorCode == -2147463164)
            //    {
            //        // User not found error.
            //        return false;
            //    }
            //    throw ex;
            //}
        }

        public void GrantLogonAsService(WindowsUser windowsUser)
        {
            LsaManaged lsa = new LsaManaged();
            lsa.AddPrivileges(windowsUser.Username, "SeServiceLogonRight");
        }
    }
}
