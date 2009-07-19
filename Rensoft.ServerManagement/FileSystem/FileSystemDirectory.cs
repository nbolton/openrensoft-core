using System;
using System.Collections.Generic;
using System.Text;
using System.Security.AccessControl;
using System.IO;

namespace Rensoft.ServerManagement.FileSystem
{
    /// <summary>
    /// Represents a directory on the local file system,
    /// </summary>
    public class FileSystemDirectory : MarshalByRefObject
    {
        private List<FileSystemAccessRule> accessRules;
        private string path;

        /// <summary>
        /// Gets or sets the directory path.
        /// </summary>
        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        /// <summary>
        /// Gets or sets the access rules for the directory.
        /// </summary>
        public List<FileSystemAccessRule> AccessRules
        {
            get { return accessRules; }
            set { accessRules = value; }
        }

        /// <summary>
        /// Gets the file current security access control, with
        /// the access rules appended.
        /// </summary>
        public DirectorySecurity GetSecurity()
        {
            DirectorySecurity security =
                Directory.GetAccessControl(this.Path);

            foreach (FileSystemAccessRule rule in this.AccessRules)
            {
                security.AddAccessRule(rule);
            }

            return security;
        }

        /// <summary>
        /// Initialize new FileSystemDirectory.
        /// </summary>
        /// <param name="path">Local or UNC file path.</param>
        public FileSystemDirectory(string path)
        {
            this.path = path;
            this.accessRules = new List<FileSystemAccessRule>();
        }
    }
}
