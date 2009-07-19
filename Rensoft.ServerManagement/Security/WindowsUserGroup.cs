using System;

namespace Rensoft.ServerManagement.Security
{
    /// <summary>
    /// Contains information for a Windows user group.
    /// </summary>
    public class WindowsUserGroup : MarshalByRefObject
    {
        private string name;
        
        /// <summary>
        /// Gets or sets the name of the user group.
        /// </summary>
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        /// <summary>
        /// Initializes a new WindowsUserGroup with a name.
        /// </summary>
        /// <param name="name">Name of user group.</param>
        public WindowsUserGroup(string name)
        {
            this.Name = name;
        }
    }
}
