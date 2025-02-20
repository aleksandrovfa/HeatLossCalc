using Autodesk.Revit.DB;
using HelixToolkit.Wpf;
using ricaun.HelixToolkit.Wpf.Revit;
using ricaun.HelixToolkit.Wpf.Revit.Extensions;
using ricaun.HelixToolkit.Wpf.Revit.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace ricaun.HelixToolkit.Wpf.Revit.Extensions
{
    internal static class HelixViewport3DExtension
    {
        public static void Add(this HelixViewport3D HelixViewport3D, Visual3D visual)
        {
            if (visual is null) return;
            HelixViewport3D?.Children.Add(visual);
            return;
        }
        public static void Add(this HelixViewport3D HelixViewport3D, IEnumerable<Visual3D> visuals)
        {
            foreach (var visual in visuals)
            {
                HelixViewport3D.Add(visual);
            }
            return;
        }
        public static void Add(this HelixViewport3D HelixViewport3D, Element element)
        {
            HelixViewport3D.Add(PreviewWindowRevitUtils.GetVisual3D(element));
            return;
        }

        public static void Add(this HelixViewport3D HelixViewport3D, GeometryObject geometryObject)
        {
            HelixViewport3D.Add(geometryObject.ToVisual3D());
            return;
        }
        public static void Add(this HelixViewport3D HelixViewport3D, IEnumerable<GeometryObject> geometryObjects)
        {
            foreach (var geometryObject in geometryObjects)
                HelixViewport3D.Add(geometryObject);
            return;
        }

        public static void Clear(this HelixViewport3D HelixViewport3D)
        {
            HelixViewport3D.Children.Clear();
            HelixViewport3D.Add(new DefaultLights());
        }
    }
}
