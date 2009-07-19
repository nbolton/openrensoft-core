using System;
using System.Collections.Generic;
using System.Text;
using Rensoft.ServerManagement.Security;

namespace Rensoft.ServerManagement.IIS
{
    public class ApplicationPool : MarshalByRefObject
    {
        private string name;
        private WindowsUser user;
        private ApplicationPoolIdentityType identityType;

        /// <summary>
        /// Gets or sets the name of the application pool.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Gets or sets the user to be granted anonymous access.
        /// </summary>
        public WindowsUser User
        {
            get { return user; }
            set { user = value; }
        }

        /// <summary>
        /// Gets or sets the type of application pool.
        /// </summary>
        public ApplicationPoolIdentityType IdentityType
        {
            get { return identityType; }
            set { identityType = value; }
        }

        /// <summary>
        /// Initialize a new IIS Application Pool.
        /// </summary>
        /// <param name="name">Name of the application pool.</param>
        /// <param name="user">User to run the application pool.</param>
        /// <param name="type">Identity type of application pool.</param>
        protected ApplicationPool(
            string name,
            WindowsUser user,
            ApplicationPoolIdentityType type)
        {
            this.Name = name;
            this.User = user;
            this.IdentityType = type;
        }

        /// <summary>
        /// Initialize a new IIS Application Pool, not for a specific user.
        /// </summary>
        /// <param name="name">Name of the application pool.</param>
        /// <param name="type">Any type except for SpecificUser.</param>
        public ApplicationPool(string name, ApplicationPoolIdentityType type)
            : this (name, null, type)
        {
            if (type == ApplicationPoolIdentityType.SpecificUser)
            {
                throw new ArgumentException(
                    "Setting a specific user is not " +
                    "supported by this constructor");
            }
        }

        /// <summary>
        /// Initialize a new IIS Application Pool for a specific user.
        /// </summary>
        /// <param name="name">Name of the application pool.</param>
        /// <param name="user">User to run the application pool.</param>
        public ApplicationPool(string name, WindowsUser user)
            : this(name, user, ApplicationPoolIdentityType.SpecificUser) { }
    }

    /// <summary>
    /// Application pool identity type. Reflects explicit values 
    /// used in the ADSI calls.
    /// </summary>
    public enum ApplicationPoolIdentityType
    {
        LocalSystem = 0,
        LocalService = 1,
        NetworkService = 2,
        SpecificUser = 3
    }
}
