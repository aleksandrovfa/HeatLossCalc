using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using HeatLossCalc.Core;
using HeatLossCalc.ViewModels.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace HeatLossCalc.Models
{
    public class SpaceAnalysis : ViewModelBase
    {
        public Space Space { get; }
        public double Temp { get; }
        public double AreaSize { get; }

        public bool IsSelected { get; set; } = false;
        public HeatLossSettings HTSettings { get; }
        public List<Face> Faces { get; set; } = new List<Face>();
        public List<Ray> Rays { get; set; } = new List<Ray>();
        public ObservableCollection<RayArea> Areas
        {
            get { return _areas; }
            set { _areas = value; OnPropertyChanged("Areas"); }
        }

        private ObservableCollection<RayArea> _areas = new ObservableCollection<RayArea>();


        public SpaceAnalysis(Space space, HeatLossSettings settings)
        {
            Space = space;
            Temp = Space.LookupParameter("ADSK_Температура в помещении").AsDouble() - 273.15;
            AreaSize = Math.Round(Space.get_Parameter(BuiltInParameter.ROOM_AREA).AsDouble() * 0.09290304, 2);
            HTSettings = settings;
        }

        public void Analyze()
        {
            Faces = GetFaces();
            List<(Line, Space)> lines = GetLinesIntersectSpace();
            lines = FilterLines(lines);
            Rays = GetRays(lines);
        }



        public void CalcArea()
        {
            Areas.Clear();
            Areas = GetAreas();
        }

        private ObservableCollection<RayArea> GetAreas()
        {
            Rays.ForEach(x => x.UpdateElementFict(HTSettings));

            ObservableCollection<RayArea> RayAreas = new ObservableCollection<RayArea>();

            var groups = Rays.GroupBy(x =>
            {
                List<string> names = x.ElementsFict.Select(f => f.SymbolName).ToList();
                names.Add(x.Direction.ToStringRu());
                names.Sort();
                return String.Join(",", names);
            });

            foreach (var group in groups)
            {

                RayArea rayArea = new RayArea(group);
                Debug.WriteLine($" Площадь в м2: {rayArea.Area}");

                if (rayArea.Area > 0.05)
                {
                    RayAreas.Add(rayArea);
                }
            }
            return RayAreas;
        }

        private List<Ray> GetRays(List<(Line, Space)> lines)
        {
            Debug.WriteLine("Старт лучи");

            ElementMulticategoryFilter filterHorizontal = new ElementMulticategoryFilter(new List<BuiltInCategory>()
            {
                BuiltInCategory.OST_Walls,
                BuiltInCategory.OST_Floors,
                BuiltInCategory.OST_Doors,
                BuiltInCategory.OST_Windows,
                BuiltInCategory.OST_CurtainWallPanels,
                BuiltInCategory.OST_CurtainWallMullions,
                BuiltInCategory.OST_Columns,
                BuiltInCategory.OST_StructuralColumns
            });

            ReferenceIntersector riHo = new ReferenceIntersector(filterHorizontal, FindReferenceTarget.Element, (View3D)RevitApi.Doc.ActiveView);
            riHo.FindReferencesInRevitLinks = true;


            ElementMulticlassFilter filterVertical = new ElementMulticlassFilter(new List<Type>()
            {
                typeof(Floor),
                typeof(RoofBase)
            });
            ReferenceIntersector riVe = new ReferenceIntersector(filterVertical, FindReferenceTarget.Element, (View3D)RevitApi.Doc.ActiveView);
            riVe.FindReferencesInRevitLinks = true;



            List<Ray> rays = new List<Ray>();
            foreach ((Line, Space) line in lines)
            {
                List<ReferenceWithContext> context = new List<ReferenceWithContext>();
                if (line.Item1.Direction.Z == 0)
                {
                    context = riHo.Find(line.Item1.GetEndPoint(0), line.Item1.Direction).ToList();
                }
                else
                {
                    context = riVe.Find(line.Item1.GetEndPoint(0), line.Item1.Direction).ToList();
                }
                Ray ray = new Ray(Space, line.Item2, line.Item1, context, HTSettings);
                rays.Add(ray);
            }
            Debug.WriteLine($"Финиш лучи");
            return rays;
        }


        private List<(Line, Space)> FilterLines(List<(Line, Space)> lines)
        {
            Debug.WriteLine("Старт фильтрация линий");
            List<(Line, Space)> raysResult = new List<(Line, Space)>();
            foreach (var ray in lines)
            {
                if (HTSettings.ModeAnalysis == Mode.Union)
                {
                    if (HTSettings.BoundarySpace.Contains(ray.Item2.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()))
                    {
                        raysResult.Add(ray);
                    }

                    else if (Temp > 1)
                    {
                        double temp = ray.Item2.LookupParameter("ADSK_Температура в помещении").AsDouble() - 273.15;
                        if (temp > 1)
                        {
                            double diffTemp = temp - Temp;
                            if (diffTemp <= -3)
                            {
                                raysResult.Add(ray);
                            }
                        }
                    }
                }

                if (HTSettings.ModeAnalysis == Mode.OutSide)
                {
                    if (HTSettings.BoundarySpace.Contains(ray.Item2.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()))
                    {
                        raysResult.Add(ray);
                    }
                }

                if (HTSettings.ModeAnalysis == Mode.Temperature)
                {
                    if (Temp > 1)
                    {
                        double temp = ray.Item2.LookupParameter("ADSK_Температура в помещении").AsDouble() - 273.15;
                        if (temp > 1)
                        {
                            double diffTemp = temp - Temp;
                            if (diffTemp <= -3)
                            {
                                raysResult.Add(ray);
                            }
                        }
                    }
                }

            }
            Debug.WriteLine("Финиш фильтрация линий");
            return raysResult;
        }


        private List<(Line, Space)> GetLinesIntersectSpace()
        {
            List<(Line, Space)> lines = new List<(Line, Space)>();

            //Solid airSolid = null;


            BoundingBoxXYZ spaceBox = Space.get_BoundingBox(null);
            Outline outline = new Outline(spaceBox.Min, spaceBox.Max);
            outline.Scale(1.5);


            List<Space> spacesAround = new FilteredElementCollector(RevitApi.Doc)
                            .OfClass(typeof(SpatialElement))
                            .WherePasses(new BoundingBoxIntersectsFilter(outline))
                            //.Where(x => x.Id != Space.Id)
                            .Cast<Space>()
                            .ToList();




            Debug.WriteLine("Старт получения линий");

            SolidCurveIntersectionOptions opt = new SolidCurveIntersectionOptions();
            opt.ResultType = SolidCurveIntersectionMode.CurveSegmentsInside;
            foreach (Face face in Faces)
            {
                try
                {
                    if (face is PlanarFace || face is CylindricalFace)
                    {
                        BoundingBoxUV bbUV = face.GetBoundingBox();
                        double step = HTSettings.StepRevit;

                        if (face is CylindricalFace)
                        {
                            step = HTSettings.StepRevit / ((CylindricalFace)face).get_Radius(0).GetLength();

                        }
                        
                        for (double u = bbUV.Min.U + step / 2; u <= bbUV.Max.U; u += step)
                        {
                            for (double v = bbUV.Min.V + HTSettings.StepRevit / 2; v <= bbUV.Max.V; v += HTSettings.StepRevit)
                            {
                                UV pointUV = new UV(u, v);
                                if (face.IsInside(pointUV))
                                {
                                    XYZ pointXYZ = face.Evaluate(pointUV);
                                    // Line line = Line.CreateBound(pointXYZ, pointXYZ + face.FaceNormal * (1000/304.8));
                                    Line line = Line.CreateBound(pointXYZ, pointXYZ + face.ComputeNormal(pointUV) * 5);
                                    Line lineFind = null;
                                    Space spaceAround = null;

                                    foreach (var space in spacesAround)
                                    {
                                        SolidCurveIntersection intersect = space.GetSolid().IntersectWithCurve(line, opt);
                                        if (intersect.Count() > 0)
                                        {
                                            foreach (var curve in intersect)
                                            {
                                                XYZ pointInAroundSpace = curve.GetEndPoint(0) + face.ComputeNormal(pointUV) * RevitApi.App.ShortCurveTolerance * 500;
                                                double length = pointXYZ.DistanceTo(pointInAroundSpace);
                                                if (lineFind == null || length < lineFind.Length)
                                                {
                                                    lineFind = Line.CreateBound(pointXYZ, pointInAroundSpace);
                                                    spaceAround = space;
                                                }
                                            }
                                        }
                                    }
                                    if (lineFind != null && spaceAround.Id != Space.Id)
                                    {
                                        lines.Add((lineFind, spaceAround));
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

            }
            Debug.WriteLine("Финиш получения линий");
            return lines;

        }

        private List<Face> GetFaces()
        {
            List<Face> faces = new List<Face>();

            if (HTSettings.FaceVertical)
            {
                foreach (Face face in Space.GetSolid().Faces)
                {
                    if (face is PlanarFace)
                    {
                        if (((PlanarFace)face).FaceNormal.Z != 1 &&
                            ((PlanarFace)face).FaceNormal.Z != -1)
                        {
                            faces.Add(face);
                        }
                    }
                    else if (face is CylindricalFace)
                    {
                        faces.Add(face);
                    }

                }
            }

            if (HTSettings.FaceHorizontal == FaceHorizontalDirection.All)
            {
                foreach (Face face in Space.GetSolid().Faces)
                {
                    if (face is PlanarFace)
                    {
                        if (((PlanarFace)face).FaceNormal.Z == 1 ||
                            ((PlanarFace)face).FaceNormal.Z == -1)
                        {
                            faces.Add(face);
                        }
                    }

                }
            }

            if (HTSettings.FaceHorizontal == FaceHorizontalDirection.Up)
            {
                foreach (Face face in Space.GetSolid().Faces)
                {
                    if (face is PlanarFace)
                    {
                        if (((PlanarFace)face).FaceNormal.Z == 1)
                        {
                            faces.Add(face);
                        }
                    }

                }
            }

            if (HTSettings.FaceHorizontal == FaceHorizontalDirection.Down)
            {
                foreach (Face face in Space.GetSolid().Faces)
                {
                    if (face is PlanarFace)
                    {
                        if (((PlanarFace)face).FaceNormal.Z == -1)
                        {
                            faces.Add(face);
                        }
                    }
                }
            }
            return faces;
        }

    }


}
