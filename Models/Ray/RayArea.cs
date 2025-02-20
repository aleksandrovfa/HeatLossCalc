using Autodesk.Revit.DB;
using HeatLossCalc.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static HeatLossCalc.Models.UtilsDirection;

namespace HeatLossCalc.Models
{
    public class RayArea :  IEnumerable<Ray>
    {
        public List<Ray> Rays {  get; }
        public List<ElementFict> ElementsFict { get; }
        public Direction Direction { get ; }
        public string DirectionRu { get => Direction.ToStringRu(); }
        public bool IsSelected { get; set; } = false;
        public double Temp { get; } 
        public RayArea(IEnumerable<Ray> rays)  
        {
            Rays = rays.ToList();
            double step = rays.FirstOrDefault().HTSettings.Step;
            Area = rays.Count() * step * step;
           
            ElementsFict = rays.FirstOrDefault().ElementsFict;
            Direction = rays.FirstOrDefault().Direction;
            Temp = rays.FirstOrDefault().SpaceFinish.LookupParameter("ADSK_Температура в помещении").AsDouble() - 273.15;
            List<string> distinctCategory = ElementsFict
                .Select(x => x.CategoryName)
                .Distinct()
                .ToList();

            if (distinctCategory.Count == 1 && distinctCategory.FirstOrDefault() == "Стены")
                AreaCalc = Area * 1.3;
            else
                AreaCalc = Area;
            
        }
        public double Area { get; set; }
        public double AreaCalc { get; set; }
        public IEnumerator<Ray> GetEnumerator()
        {
            return Rays.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
