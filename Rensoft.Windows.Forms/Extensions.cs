using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Rensoft.Windows.Forms
{
    public static class Extensions
    {
        public static int[] GetSelectedIndexArray(this ListView listView)
        {
            return listView.SelectedIndices.Cast<int>().ToArray();
        }
    }
}
