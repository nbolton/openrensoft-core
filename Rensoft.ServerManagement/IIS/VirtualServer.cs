using System;
using System.Collections.Generic;
using System.Management;
using Rensoft.ServerManagement.Security;
using System.Linq;

namespace Rensoft.ServerManagement.IIS
{
    /// <summary>
    /// Contains properties for an IIS virtual server.
    /// </summary>
    public class VirtualServer
    {
        private IEnumerable<VirtualServerBinding> bindings;
        private VirtualServerPath path;
        private string homeDirectory;
        private string description;
        private string applicationPool;
        private VirtualServerAuthFlag authFlags;
        private WindowsUser anonymousUser;
        private VirtualServerAccessFlag accessFlags;
        private string redirectUrl;
        private VirtualServerRedirectFlag redirectFlags;

        /// <summary>
        /// Gets or sets the bindings for this virtual server.
        /// </summary>
        public IEnumerable<VirtualServerBinding> Bindings
        {
            get { return bindings; }
            set { bindings = value; }
        }

        /// <summary>
        /// Gets or sets the location of the virtual server in the metabase.
        /// </summary>
        public VirtualServerPath Path
        {
            get { return path; }
            set { path = value; }
        }

        /// <summary>
        /// Gets or sets the home directory where files are stored.
        /// </summary>
        public string HomeDirectory
        {
            get { return homeDirectory; }
            set { homeDirectory = value; }
        }

        /// <summary>
        /// Gets or sets the text to describe the virtual server.
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets or sets the application pool of the virtual server.
        /// </summary>
        public string ApplicationPool
        {
            get { return applicationPool; }
            set { applicationPool = value; }
        }

        /// <summary>
        /// Gets or sets the authentication flags.
        /// </summary>
        public VirtualServerAuthFlag AuthFlags
        {
            get { return authFlags; }
            set { authFlags = value; }
        }

        /// <summary>
        /// Gets or sets the anonymous user.
        /// </summary>
        public WindowsUser AnonymousUser
        {
            get { return anonymousUser; }
            set { anonymousUser = value; }
        }

        /// <summary>
        /// Gets or sets the access flags.
        /// </summary>
        public VirtualServerAccessFlag AccessFlags
        {
            get { return accessFlags; }
            set { accessFlags = value; }
        }

        /// <summary>
        /// Gets or sets the redirect URL. This should be left null
        /// for no redirect.
        /// </summary>
        public string RedirectUrl
        {
            get { return redirectUrl; }
            set { redirectUrl = value; }
        }

        /// <summary>
        /// Gets or sets the flags used as appendidges to HttpRedirect.
        /// </summary>
        public VirtualServerRedirectFlag RedirectFlags
        {
            get { return redirectFlags; }
            set { redirectFlags = value; }
        }

        /// <summary>
        /// Gets the string compatable with the HttpRedirect property 
        /// of the IIsWebVirtualDirSetting WMI class. Null is returned
        /// if the string cannot be constructed.
        /// </summary>
        public string HttpRedirect
        {
            get
            {
                if (String.IsNullOrEmpty(redirectUrl))
                {
                    return null;
                }

                List<string> parts = new List<string>();
                parts.Add(redirectUrl);

                // Shorter variable name for neater code.
                VirtualServerRedirectFlag flags = RedirectFlags;

                if ((flags & VirtualServerRedirectFlag.ExactDestination) != 0)
                {
                    parts.Add("EXACT_DESTINATION");
                }

                if ((flags & VirtualServerRedirectFlag.ChildOnly) != 0)
                {
                    parts.Add("CHILD_ONLY");
                }

                if ((flags & VirtualServerRedirectFlag.Permanent) != 0)
                {
                    parts.Add("PERMANENT");
                }

                if ((flags & VirtualServerRedirectFlag.Temporary) != 0)
                {
                    parts.Add("TEMPORARY");
                }

                return String.Join(", ", parts.ToArray());
            }
        }

        /// <summary>
        /// Initializes a new Virtual Server (all prarameters - protected).
        /// </summary>
        /// <param name="description">Shows in the IIS snap-in as comment.</param>
        /// <param name="homeDirectory">Location of the website files.</param>
        /// <param name="applicationPool">Application pool to use.</param>
        /// <param name="bindings">Any number of bindings.</param>
        /// <param name="authFlags">
        /// Basic should be used if HTTPS is not enabled, and NTLM should be
        /// used to enable windows integrated authentication.
        /// </param>
        /// <param name="accessFlags">Reflects user access to website.</param>
        /// <param name="redirectUrl">URL to redirect to, null accepted.</param>
        /// <param name="redirectFlags">HttpRedirect append flags.</param>
        protected VirtualServer(
            string description,
            string homeDirectory,
            WindowsUser anonymousUser,
            string applicationPool,
            IEnumerable<VirtualServerBinding> bindings,
            VirtualServerAuthFlag authFlags,
            VirtualServerAccessFlag accessFlags,
            string redirectUrl,
            VirtualServerRedirectFlag redirectFlags,
            VirtualServerPath path)
        {
            this.description = description;
            this.homeDirectory = homeDirectory;
            this.applicationPool = applicationPool;
            this.bindings = bindings;
            this.authFlags = authFlags;
            this.accessFlags = accessFlags;
            this.redirectUrl = redirectUrl;
            this.redirectFlags = redirectFlags;
            this.anonymousUser = anonymousUser;
            this.path = path;
        }

        /// <summary>
        /// Initializes a new IIS Virtual Server with anonymous access.
        /// </summary>
        /// <param name="description">Shows in the IIS snap-in as comment.</param>
        /// <param name="homeDirectory">Location of the website files.</param>
        /// <param name="redirectUrl">URL to redirect to.</param>
        /// <param name="applicationPool">Application pool to use.</param>
        /// <param name="bindings">Any number of bindings.</param>
        /// <param name="anonymousUser">Anonymous windows user.</param>
        public VirtualServer(
            string description,
            string homeDirectory,
            string applicationPool,
            IEnumerable<VirtualServerBinding> bindings,
            VirtualServerAccessFlag accessFlags,
            WindowsUser anonymousUser,
            VirtualServerPath path)
            : this(
                description,
                homeDirectory,
                anonymousUser,
                applicationPool,
                bindings,
                VirtualServerAuthFlag.Anonymous,
                accessFlags,
                null,
                VirtualServerRedirectFlag.None,
                path) { }

        /// <summary>
        /// Initialize a new private IIS Virtual Server.
        /// </summary>
        /// <param name="description">Shows in the IIS snap-in as comment.</param>
        /// <param name="homeDirectory">Location of the website files.</param>
        /// <param name="redirectUrl">URL to redirect to.</param>
        /// <param name="applicationPool">Application pool to use.</param>
        /// <param name="bindings">Any number of bindings.</param>
        /// <param name="authFlags">
        /// Basic should be used if HTTPS is not enabled, and NTLM should be
        /// used to enable windows integrated authentication.
        /// </param>
        /// <param name="accessFlags">Reflects user access to website.</param>
        public VirtualServer(
            string description,
            string homeDirectory,
            string applicationPool,
            IEnumerable<VirtualServerBinding> bindings,
            VirtualServerAccessFlag accessFlags,
            VirtualServerAuthFlag authFlags,
            VirtualServerPath path)
            : this(
                description,
                homeDirectory,
                null,
                applicationPool,
                bindings,
                authFlags,
                accessFlags,
                null,
                VirtualServerRedirectFlag.None,
                path) { }

        /// <summary>
        /// Create a new IIS Virtual Server redirect.
        /// </summary>
        /// <param name="description">Shows in the IIS snap-in as comment.</param>
        /// <param name="redirectUrl">URL to redirect to.</param>
        /// <param name="redirectFlags">HttpRedirect append flags.</param>
        /// <param name="anonymousUser">Anonymous windows user.</param>
        /// <param name="homeDirectory">Required for redirect, not null.</param>
        /// <param name="bindings">Any number of bindings.</param>
        public VirtualServer(
            string description,
            string redirectUrl,
            VirtualServerRedirectFlag redirectFlags,
            WindowsUser anonymousUser,
            string homeDirectory,
            IEnumerable<VirtualServerBinding> bindings,
            VirtualServerPath path)
            : this(
            description,
            homeDirectory,
            anonymousUser,
            null,
            bindings,
            VirtualServerAuthFlag.Anonymous,
            VirtualServerAccessFlag.AccessRead,
            redirectUrl,
            redirectFlags,
            path) { }


        /// <summary>
        /// Gets the Bindings property as a list of ManagementBaseObject.
        /// </summary>
        public ManagementBaseObject[] GetBaseObjectBindings(VirtualServerManager manager)
        {
            return Bindings.Select(b => b.ToBaseObject(manager)).ToArray();
        }
    }

    /// <summary>
    /// Use to set the authentication method for a virtual server.
    /// </summary>
    public enum VirtualServerAuthFlag
    {
        None = 0,
        Anonymous = 1,
        Basic = 2,
        NTLM = 4,
        MD5 = 16,
        Passport = 64
    }

    /// <summary>
    /// Use to set the access method for the virtual server.
    /// </summary>
    [Flags]
    public enum VirtualServerAccessFlag
    {
        AccessRead = 1,
        AccessWrite = 2,
        AccessExecute = 4,
        AccessSource = 16,
        AccessScript = 512,
        AccessNoRemoteWrite = 1024,
        AccessNoRemoteRead = 4096,
        AccessNoRemoteExecute = 8192,
        AccessNoRemoteScript = 16384
    }

    /// <summary>
    /// Used to set the state of a virtual server.
    /// </summary>
    public enum VirtualServerState
    {
        Start,
        Stop,
        Pause,
        Continue
    }

    /// <summary>
    /// Flags which can be combined, then used as an argument for
    /// creating redirect virtual servers. More than one flag is allowed.
    /// Flags can be appended programmatically to the redirection string in
    /// HttpRedirect, or they can be configured by selecting checkboxes
    /// in the IIS user interface.
    /// </summary>
    [Flags]
    public enum VirtualServerRedirectFlag
    {
        /// <summary>
        /// Do not append any flags to the HttpRedirect property.
        /// </summary>
        None = 0,

        /// <summary>
        /// Indicates that the value provided for Destination should be
        /// considered an absolute target location.
        /// </summary>
        ExactDestination = 1,

        /// <summary>
        /// Alerts IIS that redirection should occur only once because the
        /// destination is in a subdirectory of the original URL. This flag
        /// avoids loops. Also, this flag instructs IIS only to redirect
        /// requests if they are to subfolders or files of the original URL.
        /// </summary>
        ChildOnly = 2,

        /// <summary>
        /// Indicates that this redirection is permanent for this resource.
        /// </summary>
        Permanent = 4,

        /// <summary>
        /// Indicates that this redirection is temporary for this resource.
        /// </summary>
        Temporary = 8
    }
}
