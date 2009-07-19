using System;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Security.AccessControl;

namespace Rensoft.ServerManagement.FileSystem
{
    /// <summary>
    /// Manages creation, deletion, modification and duplication
    /// of physical drives. Security is also managed in this class
    /// depending on the ACL within the directory object.
    /// </summary>
    public class FileSystemManager : MarshalByRefObject
    {
        /// <summary>
        /// Creates a local directory. The security will be applied to
        /// the new directory as per the ACL within the directory object.
        /// </summary>
        public void CreateDirectory(FileSystemDirectory directory)
        {
            Directory.CreateDirectory(directory.Path);
            Directory.SetAccessControl(directory.Path, directory.GetSecurity());
        }

        public void MoveDirectory(FileSystemDirectory source, FileSystemDirectory destination)
        {
            Directory.Move(source.Path, destination.Path);
        }

        /// <summary>
        /// Deletes the specified directory and, if indidated, any
        /// subdirectories in the directory.
        /// </summary>
        /// <param name="directory">Directory to delete.</param>
        /// <param name="recursive">Delete all subdirectories.</param>
        public void DeleteDirectory(FileSystemDirectory directory, bool recursive)
        {
            Directory.Delete(directory.Path, true);
        }

        /// <summary>
        /// Copies one local directory recursively, applying security.
        /// </summary>
        /// <param name="source">Directory you want to duplicate.</param>
        /// <param name="target">Destination directory.</param>
        public void CopyDirectory(
            FileSystemDirectory source, FileSystemDirectory target)
        {
            CopyDirectoryRecursive(source.Path, target.Path);
            Directory.SetAccessControl(target.Path, target.GetSecurity());
        }

        /// <summary>
        /// Copies one local directory recursively without applying security.
        /// </summary>
        /// <param name="source">Directory you want to duplicate.</param>
        /// <param name="target">Destination directory.</param>
        public void CopyDirectoryRecursive(string source, string target)
        {
            if (target[target.Length - 1] != Path.DirectorySeparatorChar)
            {
                target += Path.DirectorySeparatorChar;
            }

            if (!Directory.Exists(target))
            {
                Directory.CreateDirectory(target);
            }

            String[] files = Directory.GetFileSystemEntries(source);
            foreach (string element in files)
            {
                // Recursively copy sub directories.
                if (Directory.Exists(element))
                {
                    this.CopyDirectoryRecursive(
                        element, target + Path.GetFileName(element));
                }
                else
                {
                    File.Copy(element, target + Path.GetFileName(element), true);
                }
            }
        }
    }
}
