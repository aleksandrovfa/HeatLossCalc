using Autodesk.Revit.Creation;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using HeatLossCalc.Core;
using HeatLossCalc.Models;
using HeatLossCalc.ViewModels.Utils;
using HelixToolkit.Wpf;
//using RevitGeometryExporter;
using ricaun.HelixToolkit.Wpf.Revit;
using ricaun.HelixToolkit.Wpf.Revit.Extensions;
using ricaun.Revit.UI.StatusBar;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using Newtonsoft.Json;
using HeatLossCalc.Views;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Media.Media3D;
using Revit.Async;
using System.Windows.Documents;

namespace HeatLossCalc.ViewModels
{
    public class HeatLossCalcViewModel : ViewModelBase
    {
        private ICommand _analyzeCommand;
        public ICommand AnalyzeCommand => _analyzeCommand ?? (_analyzeCommand = new RelayCommand(param => Analyze()));

        private ICommand _calcCommand;
        public ICommand CalcCommand => _calcCommand ?? (_calcCommand = new RelayCommand(param => Calc()));

        private ICommand _saveSettingCommand;
        public ICommand SaveSettingCommand => _saveSettingCommand ?? (_saveSettingCommand = new RelayCommand(param => SaveSetting()));

        private ICommand _restoreSettingCommand;
        public ICommand RestoreSettingCommand => _restoreSettingCommand ?? (_restoreSettingCommand = new RelayCommand(param => RestoreSetting()));

        private ICommand _exportCommand;
        public ICommand ExportCommand => _exportCommand ?? (_exportCommand = new RelayCommand(param => Export()));

        private ICommand _createAir;
        public ICommand CreateAirCommand => _createAir ?? (_createAir = new RelayCommand(param => CreateAir()));

        private HeatLossSettings _htSettings;
        public HeatLossSettings HTSettings
        {
            get { return _htSettings; }
            set { _htSettings = value; OnPropertyChanged("HTSettings"); }
        }
        public List<LevelWrap> Levels { get; set; } = new List<LevelWrap>();
        public ObservableCollection<SpaceAnalysis> SpaceAnalyses { get; set; } = new ObservableCollection<SpaceAnalysis>();
        public ObservableCollection<ElementFict> FamilySymbolsFict { get; set; } = new ObservableCollection<ElementFict>();

        public HelixViewport3D HelixViewport3D;
        public HeatLossCalcViewModel()
        {
            HTSettings = new HeatLossSettings(Properties.Settings.Default.DirectorySettings);

            List<Level> levels = new FilteredElementCollector(RevitApi.Doc)
               .OfClass(typeof(SpatialElement))
               .Cast<SpatialElement>()
               .Select(x => x.Level)
               .Distinct(new CompareLevel())
               .ToList();

            levels.Sort(new CompareLevel());
            Levels = levels
                .Select(x => new LevelWrap(x))
                .ToList();
        }
        private void Analyze()
        {
            try
            {
                var checkLevels = Levels
                    .Where(x => x.IsChecked)
                    .Select(x => x.Level)
                    .ToList();
                if (checkLevels.Count == 0)
                {
                    MessageBox.Show("Не выбрано ни одного уровня");
                    return;
                }

                List<Space> spaces = new FilteredElementCollector(RevitApi.Doc)
                    .OfClass(typeof(SpatialElement))
                    .Cast<SpatialElement>()
                    .Where(x => x.Location != null)
                    .Where(x => !HTSettings.BoundarySpace.Contains(x.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()))
                    .Where(x => checkLevels.Contains(x.Level, new CompareLevel()))
                    .Cast<Space>()
                    .ToList();

                List<Space> airs = new FilteredElementCollector(RevitApi.Doc)
                    .OfClass(typeof(SpatialElement))
                    .Cast<SpatialElement>()
                    .Where(x => x.Location != null)
                    .Where(x => HTSettings.BoundarySpace.Contains(x.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()))
                    //.Where(x => checkLevels.Contains(x.Level, new CompareLevel()))
                    .Cast<Space>()
                    .ToList();



                if (spaces.Count == 0)
                {
                    MessageBox.Show("Не найдено пространств для расчета");
                    return;
                }
                if (airs.Count == 0)
                {
                    MessageBox.Show("Не найдены внешние пространства для расчета внешнего контура");
                    return;
                }
                if (!(RevitApi.Doc.ActiveView is View3D))
                {
                    MessageBox.Show("Перейдите на 3D вид для построения лучей");
                    return;
                }


                SpaceAnalyses.Clear();
                List<SpaceAnalysis> spaceAnalyses = new List<SpaceAnalysis>();

                foreach (var space in spaces)
                {
                    SpaceAnalysis spaceAnalysis = new SpaceAnalysis(space, HTSettings);
                    spaceAnalyses.Add(spaceAnalysis);
                }



                using (var progressBar = new RevitProgressBar())
                {
                    progressBar.SetHasCancelButton(true);
                    progressBar.Run("Анализ пространств", spaceAnalyses, (spaceAnalysis) =>
                    {
                        if (!progressBar.IsCancelling())
                        {
                            progressBar.SetCurrentOperation($"Анализ пространства: {spaceAnalysis.Space.Name}");
                            Debug.Write($"ПРОСТРАНСТВО {spaceAnalysis.Space.Name}");


                            spaceAnalysis.Analyze();

                            Debug.Write($"количество лучей {spaceAnalysis.Rays.Count}");
                            if (spaceAnalysis.Rays.Count > 0)
                            {
                                SpaceAnalyses.Add(spaceAnalysis);
                            }
                            //spaceAnalysis.CalcArea();
                        }
                    });
                }

                //Calc();



                MessageBox.Show("Конец");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());
            }

        }

        private void Calc()
        {
            using (var progressBar = new RevitProgressBar())
            {
                progressBar.SetHasCancelButton(true);
                progressBar.Run("Расчет площади", SpaceAnalyses, (spaceAnalysis) =>
                {
                    if (!progressBar.IsCancelling())
                    {
                        progressBar.SetCurrentOperation($"Расчет площади: {spaceAnalysis.Space.Name}");
                        Debug.Write($"ПРОСТРАНСТВО {spaceAnalysis.Space.Name}");
                        //if (spaceAnalysis.Space.Name == "Пространство 138")
                        //{
                        //    var x = 4;
                        //}
                        spaceAnalysis.CalcArea();
                    }
                });
            }
            FamilySymbolsFict.Clear();
            SpaceAnalyses.SelectMany(x => x.Areas.SelectMany(s => s.ElementsFict))
                .Distinct(new CompareElementFict())
                .ToList()
                .ForEach(x => FamilySymbolsFict.Add(x));

        }

        internal void FindElement(List<SpaceAnalysis> spaces, List<RayArea> rayareas, HeatLossCalcView heatLossCalcView)
        {
            if (HelixViewport3D is null)
            {
                HelixViewport3D = new HelixViewport3D();
                HelixViewport3D.ZoomExtentsWhenLoaded = true;
                heatLossCalcView.GridView.Children.Add(HelixViewport3D);
            }
            HelixViewport3D.Clear();

            foreach (SpaceAnalysis s in spaces)
            {
                var edgeArray = s.Space.GetSolid().Edges;
                foreach (Edge edge in edgeArray)
                {
                    HelixViewport3D.Add(edge.ToVisual3D());
                }
            }

            if (rayareas.Count == 0)
            {
                HelixViewport3D.Add(spaces.SelectMany(x => x.Areas.SelectMany(r => r.Rays.Select(v => v.Line))));
            }
            else
            {
                HelixViewport3D.Add(rayareas.SelectMany(p => p.Rays.Select(v => v.Line)));
            }

            var elements = spaces

                .SelectMany(x => x.Rays.SelectMany(v => v.Elements))
                .Distinct(new CompareElement())
                .ToList();
            elements.ForEach(x => HelixViewport3D.Add(x));
            RevitApi.DocUI.Selection.SetElementIds(spaces.Select(x => x.Space.Id).ToList());
        }

        internal void FindAirs( HeatLossCalcView heatLossCalcView)
        {
            if (HelixViewport3D is null)
            {
                HelixViewport3D = new HelixViewport3D();
                HelixViewport3D.ZoomExtentsWhenLoaded = true;
                heatLossCalcView.GridView.Children.Add(HelixViewport3D);
            }
            HelixViewport3D.Clear();

            List<Space> airs = new FilteredElementCollector(RevitApi.Doc)
                    .OfClass(typeof(SpatialElement))
                    .Cast<SpatialElement>()
                    .Where(x => HTSettings.BoundarySpace.Contains(x.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()))
                    //.Where(x => checkLevels.Contains(x.Level, new CompareLevel()))
                    .Cast<Space>()
                    .ToList();


            foreach (Space s in airs)
            {
                var edgeArray = s.GetSolid().Edges;
                foreach (Edge edge in edgeArray)
                {
                    HelixViewport3D.Add(edge.ToVisual3D());
                }
            }
        }





        private void SaveSetting()
        {
            HTSettings.SaveSetting();
        }
        private void RestoreSetting()
        {
            HTSettings.RestoreSettings(HTSettings.DirectorySave);
        }
        private void Export()
        {
            ExportToExcel.ExportSpace(SpaceAnalyses, HTSettings.DirectoryExportInExcel);
        }

        private async void CreateAir2()
        {
            try
            {
                await RevitTask.RunAsync(app =>
                {
                    var spaces = new FilteredElementCollector(RevitApi.Doc)
                        .WhereElementIsNotElementType()
                        .OfClass(typeof(SpatialElement))
                        .Cast<SpatialElement>()
                        .Cast<Space>()
                        .ToList();


                    List<XYZ> points = new List<XYZ>();
                    foreach (var space in spaces)
                    {
                        try
                        {
                            var bb = space.get_BoundingBox(null);
                            points.Add(bb.Min);
                            points.Add(bb.Max);
                        }
                        catch (Exception)
                        {


                        }

                    }

                    BoundingBoxXYZ boxXYZ = new BoundingBoxXYZ();
                    XYZ max = new XYZ
                        (
                        points.Select(x => x.X).Max(),
                        points.Select(x => x.Y).Max(),
                        points.Select(x => x.Z).Max()
                        );

                    XYZ min = new XYZ
                        (
                        points.Select(x => x.X).Min(),
                        points.Select(x => x.Y).Min(),
                        points.Select(x => x.Z).Min()
                        );

                    boxXYZ.Max = max;
                    boxXYZ.Min = min;

                    Outline outline = new Outline(min, max);
                    outline.Scale(1.2);

                    min = outline.MinimumPoint;
                    max = outline.MaximumPoint;

                    XYZ p1 = new XYZ(max.X, max.Y, max.Z);
                    XYZ p2 = new XYZ(min.X, max.Y, max.Z);
                    XYZ p3 = new XYZ(min.X, min.Y, max.Z);
                    XYZ p4 = new XYZ(max.X, min.Y, max.Z);

                    CurveArray curveArray = new CurveArray();

                    curveArray.Append(Line.CreateBound(p1, p2));
                    curveArray.Append(Line.CreateBound(p2, p3));
                    curveArray.Append(Line.CreateBound(p3, p4));
                    curveArray.Append(Line.CreateBound(p4, p1));

                    Transaction tr = new Transaction(RevitApi.Doc, "бла бла ");
                    tr.Start();
                    //BoundingBoxUV uv = new BoundingBoxUV();
                   


                    List<Level> levels = new FilteredElementCollector(RevitApi.Doc)
                                    .OfClass(typeof(Level))
                                    .Cast<Level>()
                                    .Distinct(new CompareLevel())
                                    .ToList();

                    levels.Sort(new CompareLevel());

                    Level lvl = levels.Where(x => x.FindAssociatedPlanViewId().IntegerValue != -1)
                                      .LastOrDefault();


                    SketchPlane pln = SketchPlane.Create(RevitApi.Doc, lvl.Id);
                    //pln.Pr
                    //View view = RevitApi.Doc.GetElement(new ElementId(6914251)) as View;
                    View view = RevitApi.Doc.GetElement(lvl.FindAssociatedPlanViewId()) as View; ;

                   
                    RevitApi.Doc.Create.NewSpaceBoundaryLines(pln, curveArray, view);
                    UV p = new UV((max.X + min.X) / 2, (max.Y + min.Y) / 2);
                    Space newspace = RevitApi.Doc.Create.NewSpace(lvl, p);
                    newspace.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Воздух");
                    newspace.get_Parameter(BuiltInParameter.ROOM_LOWER_OFFSET).Set(-lvl.Elevation);

                    tr.Commit();

                    MessageBox.Show($"{newspace.Id}");

                    RevitApi.DocUI.Selection.SetElementIds(new List<ElementId>() { newspace.Id });
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex.ToString());
            }

        }


        private async void CreateAir()
        {
            try
            {
                await RevitTask.RunAsync(app =>
                {
                    var spaces = new FilteredElementCollector(RevitApi.Doc)
                        .WhereElementIsNotElementType()
                        .OfClass(typeof(SpatialElement))
                        .Cast<SpatialElement>()
                        .Where(x => !HTSettings.BoundarySpace.Contains(x.get_Parameter(BuiltInParameter.ROOM_NAME).AsString()))
                        .Cast<Space>()
                        .ToList();


                    List<XYZ> points = new List<XYZ>();
                    foreach (var space in spaces)
                    {
                        try
                        {
                            var bb = space.get_BoundingBox(null);
                            points.Add(bb.Min);
                            points.Add(bb.Max);
                        }
                        catch (Exception)
                        {


                        }

                    }

                    BoundingBoxXYZ boxXYZ = new BoundingBoxXYZ();
                    XYZ max = new XYZ
                        (
                        points.Select(x => x.X).Max(),
                        points.Select(x => x.Y).Max(),
                        points.Select(x => x.Z).Max()
                        );

                    XYZ min = new XYZ
                        (
                        points.Select(x => x.X).Min(),
                        points.Select(x => x.Y).Min(),
                        points.Select(x => x.Z).Min()
                        );

                    boxXYZ.Max = max;
                    boxXYZ.Min = min;

                    Outline outline = new Outline(min, max);
                    outline.Scale(1.2);

                    min = outline.MinimumPoint;
                    max = outline.MaximumPoint;
                    //UV p = new UV((max.X + min.X) / 2, (max.Y + min.Y) / 2);
                    Line line = Line.CreateBound(min, max);
                    XYZ newp = min + line.Direction;
                    UV p = new UV(newp.X, newp.Y);

                    XYZ p1 = new XYZ(max.X, max.Y, max.Z);
                    XYZ p2 = new XYZ(min.X, max.Y, max.Z);
                    XYZ p3 = new XYZ(min.X, min.Y, max.Z);
                    XYZ p4 = new XYZ(max.X, min.Y, max.Z);
                     List<XYZ> ps = new List<XYZ>() {p1,p2,p3,p4};

                    CurveArray curveArray = new CurveArray();

                    curveArray.Append(Line.CreateBound(p1, p2));
                    curveArray.Append(Line.CreateBound(p2, p3));
                    curveArray.Append(Line.CreateBound(p3, p4));
                    curveArray.Append(Line.CreateBound(p4, p1));

                    Transaction tr = new Transaction(RevitApi.Doc, "бла бла ");
                    tr.Start();
                    //BoundingBoxUV uv = new BoundingBoxUV();


                    foreach (var levelWrap in Levels.Where(x => x.IsChecked))
                    {
                        CreateAirInLevel(levelWrap.Level, ps, p);
                    }
                    tr.Commit();

                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Debug.WriteLine(ex.ToString());
            }

        }

        private void CreateAirInLevel(Level level, List<XYZ> ps, UV p)
        {
            CurveArray curveArray = GetCurveArray(level.Elevation, ps);
            SketchPlane pln = SketchPlane.Create(RevitApi.Doc, level.Id);
            View view = RevitApi.Doc.GetElement(level.FindAssociatedPlanViewId()) as View; ;
            RevitApi.Doc.Create.NewSpaceBoundaryLines(pln, curveArray, view);
            Space newspace = RevitApi.Doc.Create.NewSpace(level, p);
            newspace.get_Parameter(BuiltInParameter.ROOM_NAME).Set("Воздух");
            newspace.get_Parameter(BuiltInParameter.ROOM_UPPER_OFFSET).Set(RevitApi.dictLevelCurrent[level.Name]/ 304.8);
        }

        private CurveArray GetCurveArray(double elevation, List<XYZ> ps)
        {
            XYZ p1 = new XYZ(ps[0].X, ps[0].Y, elevation);
            XYZ p2 = new XYZ(ps[1].X, ps[1].Y, elevation);
            XYZ p3 = new XYZ(ps[2].X, ps[2].Y, elevation);
            XYZ p4 = new XYZ(ps[3].X, ps[3].Y, elevation);

            CurveArray curveArray = new CurveArray();

            curveArray.Append(Line.CreateBound(p1, p2));
            curveArray.Append(Line.CreateBound(p2, p3));
            curveArray.Append(Line.CreateBound(p3, p4));
            curveArray.Append(Line.CreateBound(p4, p1));

            return curveArray;
        }
    }
}
