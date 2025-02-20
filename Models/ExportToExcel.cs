using Autodesk.Revit.DB.ExtensibleStorage;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Excel = Microsoft.Office.Interop.Excel;
using Newtonsoft.Json.Linq;
using System.Windows.Shapes;
using System.Diagnostics;
using Microsoft.Office.Interop.Excel;
using ricaun.Revit.UI.StatusBar;

namespace HeatLossCalc.Models
{
    static class ExportToExcel
    {
        public static bool ExportSpace(ObservableCollection<SpaceAnalysis> spaceAnalyses, string directorySave)
        {
            try
            {
                if (!Directory.Exists(directorySave))
                {
                    MessageBox.Show("Неверно выбрана директория сохранения в настройках!",
                        "Ошибка!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
                Excel.Application app = new Excel.Application();
                Excel.Workbook book = app.Workbooks.Add(1);
                Excel.Worksheet Sheet = (Excel.Worksheet)book.Sheets[1];
                int row = 0;
                int col = 0;
                row++;
                Sheet.Cells[row, ++col] = "Уровень";
                Sheet.Cells[row, ++col] = "Id Пространства";
                Sheet.Cells[row, ++col] = "Номер";
                Sheet.Cells[row, ++col] = "Имя";
                Sheet.Cells[row, ++col] = "Площадь";
                Sheet.Cells[row, ++col] = "°C внутри";
                Sheet.Cells[row, ++col] = "°C снаружи";
                Sheet.Cells[row, ++col] = "Площадь";
                //Sheet.Cells[row, ++col] = "Площадь c коэфф";
                Sheet.Cells[row, ++col] = "Направление";
                Sheet.Cells[row, ++col] = "Категория";
                Sheet.Cells[row, ++col] = "Типоразмер";


                using (var progressBar = new RevitProgressBar())
                {
                    progressBar.SetHasCancelButton(true);
                    progressBar.Run("Экспорт данных", spaceAnalyses, (space) =>
                    {
                        if (!progressBar.IsCancelling())
                        {
                            progressBar.SetCurrentOperation($"Экспорт данных: {space.Space.Name}");
                            row++;
                            int rowStart1 = row;
                            col = 0;
                            Sheet.Cells[row, ++col] = space.Space.Level.Name;
                            Sheet.Cells[row, ++col] = space.Space.Id.IntegerValue;
                            Sheet.Cells[row, ++col] = space.Space.Number;
                            Sheet.Cells[row, ++col] = space.Space.Name;
                            Sheet.Cells[row, ++col] = space.AreaSize;
                            Sheet.Cells[row, ++col] = space.Temp;
                            for (int k = 0; k < space.Areas.Count; k++)
                            {
                                RayArea area = space.Areas[k];
                                if (k != 0)
                                    row++;
                                int rowStart2 = row;
                                col = 6;
                                Sheet.Cells[row, ++col] = area.Temp;
                                //Sheet.Cells[row, ++col] = area.Area;
                                Sheet.Cells[row, ++col] = area.AreaCalc;
                                Sheet.Cells[row, ++col] = area.DirectionRu;
                                for (int i = 0; i < area.ElementsFict.Count; i++)
                                {
                                    if (i != 0)
                                        row++;
                                    col = 9;
                                    Sheet.Cells[row, ++col] = area.ElementsFict[i].CategoryName;
                                    Sheet.Cells[row, ++col] = area.ElementsFict[i].SymbolName;
                                }
                                int rowFinish2 = row;
                                for (int i = 7; i <= 9; i++)
                                {
                                    MergeAndCenter(Sheet.Range[Sheet.Cells[rowStart2, i], Sheet.Cells[rowFinish2, i]]);
                                }
                                SetBorder(Sheet.Range[Sheet.Cells[rowStart2, 10], Sheet.Cells[rowFinish2, 11]]);

                            }
                            int rowFinish1 = row;
                            for (int i = 1; i <= 6; i++)
                            {
                                MergeAndCenter(Sheet.Range[Sheet.Cells[rowStart1, i], Sheet.Cells[rowFinish1, i]]);
                            }
                        }
                    });

                }
                string directory = directorySave + @"\" + $"HeatLossCalc_{DateTime.Now.ToString("yyyyMMdd HHmmss")}" + ".xlsx";
                Sheet.Columns.EntireColumn.AutoFit();
                book.SaveAs(directory);
                book.Close();
                app.Quit();
                MessageBox.Show("Завершено!", "Экспорт в Excel", MessageBoxButton.OK);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.ToString());
                return false;
            }

        }

        public static void MergeAndCenter(Excel.Range MergeRange)
        {
            MergeRange.Select();

            MergeRange.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            MergeRange.VerticalAlignment = XlVAlign.xlVAlignCenter;
            MergeRange.WrapText = false;
            MergeRange.Orientation = 0;
            MergeRange.AddIndent = false;
            MergeRange.IndentLevel = 0;
            MergeRange.ShrinkToFit = false;
            MergeRange.ReadingOrder = (int)(Constants.xlContext);
            MergeRange.MergeCells = false;
            //MergeRange.BorderAround();
            MergeRange.Merge(System.Type.Missing);
        }

        public static void SetBorder(Excel.Range MergeRange)
        {
            MergeRange.BorderAround(Weight: Excel.XlBorderWeight.xlMedium);
        }

        //Думаю можно удалить
        public static bool ExportSpace2(ObservableCollection<SpaceAnalysis> spaceAnalyses, string directorySave)
        {
            try
            {
                if (!Directory.Exists(directorySave))
                {
                    MessageBox.Show("Неверно выбрана директория сохранения в настройках!",
                        "Ошибка!",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return false;
                }
                Excel.Application app = new Excel.Application();
                Excel.Workbook book = app.Workbooks.Add(1);
                Excel.Worksheet Sheet = (Excel.Worksheet)book.Sheets[1];
                int row = 1;
                int col = 1;
                foreach (SpaceAnalysis space in spaceAnalyses)
                {
                    col = 1;
                    Sheet.Cells[row, col] = space.Space.Id.IntegerValue;
                    col++;
                    Sheet.Cells[row, col] = space.Space.Number;
                    col++;
                    Sheet.Cells[row, col].NumberFormat = "@";
                    Sheet.Cells[row, col] = space.Space.Name.ToString();
                    row++;

                }
                string directory = directorySave + @"\" + $"HeatLossCalc_{DateTime.Now.ToString("yyyyMMdd HHmmss")}" + ".xlsx";
                book.SaveAs(directory);
                book.Close();
                app.Quit();

                return true;
            }
            catch (Exception ex)
            {

                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.ToString());
                return false;
            }

        }
    }
}
