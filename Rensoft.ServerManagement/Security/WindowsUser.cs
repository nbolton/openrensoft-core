using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.DirectoryServices;

namespace Rensoft.ServerManagement.Security
{
    /// <summary>
    /// Propertys for a windows user.
    /// </summary>
    [Flags]
    public enum WindowsUserFlag
    {
        NormalAccount = 0x0200,
        PasswordCannotChange = 0x0040,
        PasswordNeverExpires = 0x10000,

        ADS_UF_SCRIPT = 1,         // 0x1
        ADS_UF_ACCOUNTDISABLE = 2,         // 0x2
        ADS_UF_HOMEDIR_REQUIRED = 8,         // 0x8
        ADS_UF_LOCKOUT = 16,        // 0x10
        ADS_UF_PASSWD_NOTREQD = 32,        // 0x20
        ADS_UF_PASSWD_CANT_CHANGE = 64,        // 0x40
        ADS_UF_ENCRYPTED_TEXT_PASSWORD_ALLOWED = 128,       // 0x80
        ADS_UF_TEMP_DUPLICATE_ACCOUNT = 256,       // 0x100
        ADS_UF_NORMAL_ACCOUNT = 512,       // 0x200
        ADS_UF_INTERDOMAIN_TRUST_ACCOUNT = 2048,      // 0x800
        ADS_UF_WORKSTATION_TRUST_ACCOUNT = 4096,      // 0x1000
        ADS_UF_SERVER_TRUST_ACCOUNT = 8192,      // 0x2000
        ADS_UF_DONT_EXPIRE_PASSWD = 65536,     // 0x10000
        ADS_UF_MNS_LOGON_ACCOUNT = 131072,    // 0x20000
        ADS_UF_SMARTCARD_REQUIRED = 262144,    // 0x40000
        ADS_UF_TRUSTED_FOR_DELEGATION = 524288,    // 0x80000
        ADS_UF_NOT_DELEGATED = 1048576,   // 0x100000
        ADS_UF_USE_DES_KEY_ONLY = 2097152,   // 0x200000
        ADS_UF_DONT_REQUIRE_PREAUTH = 4194304,   // 0x400000
        ADS_UF_PASSWORD_EXPIRED = 8388608,   // 0x800000
        ADS_UF_TRUSTED_TO_AUTHENTICATE_FOR_DELEGATION = 16777216   // 0x1000000

    }

    /// <summary>
    /// Containts all information for a Windows user.
    /// </summary>
    public class WindowsUser : MarshalByRefObject
    {
        private string fullName;
        private string username;
        private string password;
        private string description;
        private WindowsUserFlag flags;
        private List<WindowsUserGroup> groups;
        private SecurityIdentifier sid;

        /// <summary>
        /// Gets or sets the SID which is required for updating the user.
        /// </summary>
        public SecurityIdentifier Sid
        {
            get { return sid; }
            set { sid = value; }
        }

        /// <summary>
        /// Gets or sets the full name of the user, can be blank.
        /// </summary>
        public string FullName
        {
            get { return fullName; }
            set { fullName = value; }
        }

        /// <summary>
        /// Gets or sets the username of the user.
        /// </summary>
        public string Username
        {
            get { return username; }
            set { username = value; }
        }

        /// <summary>
        /// Gets or sets the user's password.
        /// </summary>

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        /// <summary>
        /// Gets or sets the description of the user.
        /// </summary>

        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets or sets the user properties flags.
        /// </summary>
        public WindowsUserFlag Flags
        {
            get { return flags; }
            set { flags = value; }
        }

        /// <summary>
        /// Gets or sets the groups which the user belongs to.
        /// </summary>
        public List<WindowsUserGroup> Groups
        {
            get { return groups; }
            set { groups = value; }
        }

        /// <summary>
        /// Initialize a new WindowsUser.
        /// </summary>
        public WindowsUser(
            string username,
            string password,
            string fullName,
            string description,
            WindowsUserFlag flags)
        {
            this.FullName = fullName;
            this.Username = username;
            this.Password = password;
            this.Description = description;
            this.Flags = flags;
            this.Groups = new List<WindowsUserGroup>();
        }

        /// <summary>
        /// Initialize a new WindowsUser with an initial group.
        /// </summary>
        /// <param name="initialGroup">Adds the user to this group.</param>
        public WindowsUser(
            string username,
            string password,
            string fullName,
            string description,
            WindowsUserFlag flags,
            WindowsUserGroup initialGroup)
            : this(username, password, fullName, description, flags)
        {
            this.Groups.Add(initialGroup);
        }

        /// <summary>
        /// Initialize a new WindowsUser.
        /// </summary>
        internal WindowsUser(
            string username,
            string password,
            string fullName,
            string description,
            WindowsUserFlag flags,
            SecurityIdentifier sid)
            : this(username, password, fullName, description, flags)
        {
            this.Sid = sid;
        }

        /// <summary>
        /// Initialize a new WindowsUser with an initial group.
        /// </summary>
        /// <param name="initialGroup">Adds the user to this group.</param>
        internal WindowsUser(
            string username,
            string password,
            string fullName,
            string description,
            WindowsUserFlag flags,
            SecurityIdentifier sid,
            WindowsUserGroup initialGroup)
            : this(username, password, fullName, description, flags, sid)
        {
            this.Groups.Add(initialGroup);
        }

        public static WindowsUser Parse(DirectoryEntry entry)
        {
            WindowsUser wu = new WindowsUser(
                (string)entry.Properties["name"].Value,
                string.Empty,
                (string)entry.Properties["fullName"].Value,
                (string)entry.Properties["description"].Value,
                (WindowsUserFlag)entry.Properties["userFlags"].Value);

            wu.Sid = new SecurityIdentifier((byte[])entry.Properties["objectSid"].Value, 0);
            return wu;
        }
    }
}