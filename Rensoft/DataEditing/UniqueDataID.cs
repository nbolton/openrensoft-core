using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.DataEditing
{
    public class UniqueDataID : IEquatable<UniqueDataID>
    {
        private object data;

        public UniqueDataID(object data)
        {
            this.data = data;
        }

        public bool Equals(UniqueDataID other)
        {
            if ((data == null) || (other.data == null))
            {
                return false;
            }
            else
            {
                return data.Equals(other.data);
            }
        }
    }
} 