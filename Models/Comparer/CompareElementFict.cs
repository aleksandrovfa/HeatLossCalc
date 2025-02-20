using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatLossCalc.Models
{
    public class CompareElementFict : IComparer<ElementFict>, IEqualityComparer<ElementFict>
    {
        public int Compare(ElementFict x, ElementFict y)
        {
            return x.SymbolName.CompareTo(y.SymbolName);
        }

        public bool Equals(ElementFict x, ElementFict y)
        {
            return x.SymbolName == y.SymbolName;
        }

        public int GetHashCode(ElementFict obj)
        {
            return (obj.SymbolName + obj.CategoryName).GetHashCode();
        }
    }
}
