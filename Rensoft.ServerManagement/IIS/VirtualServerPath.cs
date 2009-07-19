using System;
using System.Text.RegularExpressions;

namespace Rensoft.ServerManagement.IIS
{
    /// <summary>
    /// Translates between an integer ID and WMI relative path, so the
    /// identifier can be stored in a database record.
    /// </summary>
    public class VirtualServerPath : MarshalByRefObject
    {
        public static VirtualServerPath Empty
        {
            get { return new VirtualServerPath(0); }
        }

        /// <summary>
        /// Gets the WMI path.
        /// </summary>
        public string WmiPath { get; private set; }

        /// <summary>
        /// Gets the ID within the path.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Converts IIsWebServer path to integer ID.
        /// </summary>
        /// <param name="thePath">IIsWebServer relative path.</param>
        public VirtualServerPath(string wmiPath)
        {
            try
            {
                this.WmiPath = wmiPath;
                Regex regex = new Regex("IIsWebServer='W3SVC/([0-9]+)'");
                this.Id = Int32.Parse(regex.Replace(wmiPath, "$1"));
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't translate " +
                    "IIsWebServer relative path to integer ID.", ex);
            }
        }

        /// <summary>
        /// Takes ID and changes to path; IIsWebServer="W3SVC/1900806754"
        /// </summary>
        /// <param name="id">IIS Virtual Server ID.</param>
        public VirtualServerPath(int id)
        {
            this.Id = id;
            this.WmiPath = String.Format("IISWebServer='W3SVC/{0}'", id);
        }

        /// <summary>
        /// Returns the path's string representation.
        /// </summary>
        /// <returns>WMI style path string.</returns>
        public override string ToString()
        {
            return WmiPath;
        }

        public static implicit operator string(VirtualServerPath path)
        {
            return path.ToString();
        }
    }
}
