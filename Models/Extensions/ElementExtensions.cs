using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeatLossCalc.Models
{
    public static class ElementExtensions
    {
        public static Solid GetSolid(this Element element, Transform transform)
        {
            if (element != null)
            {
                Solid solid = element.GetSolid();
                if (transform != null)
                    return SolidUtils.CreateTransformed(solid, transform);
                else
                    return solid;
            }
            else return null;
        }
        public static Solid GetSolid(this Element element)
        {
            Solid solid = null;
            if (element != null)
            {
                Options options = new Options();
                GeometryElement ge = element.get_Geometry(options);
                solid = solid.AddGeometryElement(ge);

            }
            return solid;

        }
        public static Solid AddSolid(this Solid solid1, Solid solid2)
        {

            try
            {
                if (solid1 != null && solid2 != null)
                    return BooleanOperationsUtils.ExecuteBooleanOperation(solid1, solid2, BooleanOperationsType.Union);

                if (solid1 == null && solid2 != null)
                    return solid2;

                if (solid1 != null && solid2 == null)
                    return solid1;

                return null;
            }
            catch (InvalidOperationException)
            {
                return solid1;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return null;
            }

        }
        private static Solid AddGeometryElement(this Solid solid, GeometryElement ge)
        {
            if (ge != null)
            {
                foreach (var item in ge)
                {
                    if (item is Solid)
                    {
                        solid = solid.AddSolid((Solid)item);
                    }
                    if (item is GeometryElement)
                    {
                        solid = solid.AddGeometryElement((GeometryElement)item);
                    }
                    if (item is GeometryInstance)
                    {
                        solid = solid.AddGeometryElement(((GeometryInstance)item).GetInstanceGeometry());
                    }
                }
            }
            return solid;
        }
    }
}
