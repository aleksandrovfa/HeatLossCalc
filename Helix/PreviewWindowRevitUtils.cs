using Autodesk.Revit.DB;
using HelixToolkit.Wpf;
using ricaun.HelixToolkit.Wpf.Revit.Extensions;
using ricaun.HelixToolkit.Wpf.Revit.Utils;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace ricaun.HelixToolkit.Wpf.Revit
{
    /// <summary>
    /// PreviewWindowRevitExtension
    /// </summary>
    public static class PreviewWindowRevitUtils
    {
        static Options Options;
        /// <summary>
        /// GetVisual3D
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static Visual3D GetVisual3D(Element element)
        {
            DocumentUtils.Document = element.Document;
            Options options = Options ?? new Options()
            {
                DetailLevel = ViewDetailLevel.Fine
            };
            return element.get_Geometry(options).ToVisual3D();
        }

       
        
    }
}