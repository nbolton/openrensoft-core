using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rensoft.DataEditing
{
    public interface IUniqueData
    {
        UniqueDataID UniqueDataID { get; }
        bool Equals(IUniqueData other);
    }
}
