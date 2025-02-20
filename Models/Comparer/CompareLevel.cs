using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatLossCalc.Models
{
    internal class CompareLevel : IEqualityComparer<Level>, IComparer<Level>
    {

        public int Compare(Level x, Level y)
        {
            return Convert.ToInt32((x.Elevation - y.Elevation) * 1000);
        }
        public bool Equals(Level x, Level y)
        {
            return (x.Id.IntegerValue == y.Id.IntegerValue);
        }

        public int GetHashCode(Level obj)
        {
            return obj.Id.IntegerValue;
        }
    }
}
