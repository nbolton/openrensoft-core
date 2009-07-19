using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using Rensoft.ServerManagement.Security;

namespace Rensoft.ServerManagement
{
    public static class DirectoryEntriesExtensions
    {
        public static IEnumerable<DirectoryEntry> OfType(this DirectoryEntries entries, string schemaClassName)
        {
            List<DirectoryEntry> list = new List<DirectoryEntry>();
            foreach (DirectoryEntry entry in entries)
            {
                if (entry.SchemaClassName == schemaClassName)
                {
                    list.Add(entry);
                }
            }
            return list;
        }
    }
}
