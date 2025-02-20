using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HeatLossCalc.Views;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime;
using System.Windows;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace HeatLossCalc.Models
{
    [DisplayName("Настройки")]
    public class HeatLossSettings : INotifyPropertyChanged
    {

        [Category("1.Анализ пространств")]
        [DisplayName("Режим расчета")]
        [PropertyOrder(0)]
        [Description
            (
            "Внешние пространства - Рассчитывать внутренние пространства, которые граничат с внешними." +
            "Температурный - Рассчитывать внутренние пространства, c разницей 3 и более градусов.\n" +
            "Предварительно необходимо заполнить температуры в пространствах.\n" +
            "Объединенный - Объединение двух методов."
            )]
        [ItemsSource(typeof(ModeItemsSource))]
        public Mode ModeAnalysis
        {
            get { return _modeAnalysis; }
            set { _modeAnalysis = value; OnPropertyChanged("ModeAnalysis"); }
        }
        private Mode _modeAnalysis;



        [Category("1.Анализ пространств")]
        [DisplayName("Шаг сетки")]
        [PropertyOrder(1)]
        [Description("Шаг сетки вляет на точность и длительность расчет. Не рекомендуется ставить меньше 0.1. Задается в м.")]
        [ItemsSource(typeof(StepSizeItemsSource))]
        public double Step 
        {
            get { return _step; }
            set { _step = value; OnPropertyChanged("Step"); }
        }

        private double _step;

        [Browsable(false)]
        [JsonIgnore]
        public double StepRevit { get => Step / 304.8 * 1000; }

        [Category("1.Анализ пространств")]
        [DisplayName("Внешние пространства")]
        [PropertyOrder(2)]
        [Description("Список названий пространств, сумма объемов которых, образует внешнюю среду.")]
        public List<string> BoundarySpace
        {
            get { return _boundarySpace; }
            set { _boundarySpace = value; OnPropertyChanged("BoundarySpace"); }
        }

        private List<string> _boundarySpace = new List<string>();


        [Category("1.Анализ пространств")]
        [DisplayName("Горизонтальные плоскости")]
        [PropertyOrder(3)]
        [Description("Настройка для включения горизонтальных плоскостей в анализ.(Пол/Потолок)")]
        [ItemsSource(typeof(FaceHorizontalDirectionItemsSource))]
        public FaceHorizontalDirection FaceHorizontal
        {
            get { return _faceHorizontal; }
            set { _faceHorizontal = value; OnPropertyChanged("FaceHorizontal"); }
        }
        private FaceHorizontalDirection _faceHorizontal;

        [Category("1.Анализ пространств")]
        [DisplayName("Вертикальные плоскости")]
        [PropertyOrder(3)]
        [Description("Настройка для включения вертикальных плоскостей в анализ.(Стены)")]
        [ItemsSource(typeof(FaceVerticalDirectionItemsSource))]
        public bool FaceVertical
        {
            get { return _faceVertical; }
            set { _faceVertical = value; OnPropertyChanged("FaceVertical"); }
        }
        private bool _faceVertical;





        [Category("2.Расчет пространств")]
        [DisplayName("Приоритет")]
        [PropertyOrder(0)]
        [Description("Если луч пересекает элемент этой категории то остальные элементы не учитывается")]
        public List<string> CategoriesPriority
        {
            get { return _categoriesPriority; }
            set { _categoriesPriority = value; OnPropertyChanged("CategoriesPriority"); }
        }

        private List<string> _categoriesPriority = new List<string>();

        [Category("2.Расчет пространств")]
        [DisplayName("Замена")]
        [PropertyOrder(1)]
        [Description("Заменить наименование одного элемента на другой. Например для чтобы разный утеплитель учитывался как один.Для удаления элементов достаточно убрать их отображения с вида.")]
        public List<RenameElement> RenameElements
        {
            get { return _renameElements; }
            set { _renameElements = value; OnPropertyChanged("RenameElements"); }
        }

        private List<RenameElement> _renameElements = new List<RenameElement>();


        
        [Category("3.Вид")]
        [DisplayName("Синхронизация")]
        [Description("Синхронизация выделенных элементов во вкладке Пространства со вкладкой Вид")]
        [JsonIgnore]
        public bool IsViewSynchronization
        {
            get { return Properties.Settings.Default.IsViewSynchronization; }
            set 
            {
                Properties.Settings.Default.IsViewSynchronization = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged("IsViewSynchronization"); 
            }
        }

        [Category("4.Экспорт результатов")]
        [DisplayName("Путь")]
        [Description("Раположение папки для выгрузки")]
        [Editor(typeof(PropertyGridFolderPicker), typeof(PropertyGridFolderPicker))]
        public string DirectoryExportInExcel
        {
            get { return _directoryExportInExcel; }
            set { _directoryExportInExcel = value; OnPropertyChanged("DirectoryExportInExcel"); }
        }
        private string _directoryExportInExcel;

        [Category("5.Работа с настройками")]
        [DisplayName("Путь")]
        [Description("Путь для сохранения настроек")]
        [Editor(typeof(PropertyGridFilePicker), typeof(PropertyGridFilePicker))]
        [JsonIgnore]
        public string DirectorySave
        {
            get 
            { 
                return Properties.Settings.Default.DirectorySettings; 
            }
            set 
            {
                Properties.Settings.Default.DirectorySettings = value;
                Properties.Settings.Default.Save();
                OnPropertyChanged("DirectorySave"); 
            }
        }
     







        public HeatLossSettings() { }
        public HeatLossSettings(string directory)
        {
            DirectorySave = directory;
            RestoreSettings(directory);
        }
        public void RestoreSettings(string directory)
        {
            try
            {
                if (File.Exists(directory))
                {
                    //string directorySave = HTSettings.DirectorySave;
                    string json = File.ReadAllText(directory);
                    HeatLossSettings setting = JsonConvert.DeserializeObject<HeatLossSettings>(json);
                    this.DirectorySave = directory;
                    this.Step = setting.Step;
                    this.BoundarySpace = setting.BoundarySpace;
                    this.CategoriesPriority = setting.CategoriesPriority;
                    this.RenameElements = setting.RenameElements;
                    this.DirectoryExportInExcel = setting.DirectoryExportInExcel;
                    this.FaceHorizontal = setting.FaceHorizontal;
                    this.FaceVertical = setting.FaceVertical;
                    this.ModeAnalysis = setting.ModeAnalysis;
                    MessageBox.Show("Настройки восстановлены");
                }
                else
                {
                    var result = MessageBox.Show("Настройки по заданному пути не найдены! Установить стандартные значения?", "Подтвердите действие", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                    else
                    {
                        SetDefaultSettings();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Не удалось загрузить настройки");
            }
        }
        public void SaveSetting()
        {
            try
            {
                if (DirectorySave == null)
                {
                    MessageBox.Show("Не задан путь сохранения настроек");
                    return;
                }
                if (File.Exists(DirectorySave))
                {
                    var result = MessageBox.Show("Перезаписать настройки?", "Подтвердите действие", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Cancel)
                    {
                        return;
                    }
                }
                string json = JsonConvert.SerializeObject(this);
                File.WriteAllText(DirectorySave, json);
                MessageBox.Show("Настройки сохранены");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Не удалось сохранить настройки");
            }

        }
        private void SetDefaultSettings()
        {
            Step = 0.5;
            FaceHorizontal = FaceHorizontalDirection.All;
            FaceVertical = true;
            ModeAnalysis = Mode.Union;
            BoundarySpace.Add("Воздух");
            BoundarySpace.Add("Лоджия");
            CategoriesPriority.Add("Окна");
            CategoriesPriority.Add("Панели витража");
            CategoriesPriority.Add("Импосты витража");
            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Стены",
                OldSymbolName = "НО_Вентфасад_Керамогранит_Белый",
                NewCategoryName = "Стены",
                NewSymbolName = "НО_Вентфасад",
            });
            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Стены",
                OldSymbolName = "НО_Вентфасад_Керамогранит_Коричневый",
                NewCategoryName = "Стены",
                NewSymbolName = "НО_Вентфасад",
            });


            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Стены",
                OldSymbolName = "НО_Вентфасад_Керамогранит_Серый",
                NewCategoryName = "Стены",
                NewSymbolName = "НО_Вентфасад",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Окна",
                OldSymbolName = "ОК-2_1760х1750h",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Окна",
                OldSymbolName = "ББ-2Л 1800х2490",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Окна",
                OldSymbolName = "ББ-2П 1800х2490",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Окна",
                OldSymbolName = "900х1800_Рама60 без четверти",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Окна",
                OldSymbolName = "ББ-1Л 1500х2490Л",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Окна",
                OldSymbolName = "ОК-4_1760х1750h",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Импосты витража",
                OldSymbolName = "50х100",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });

            RenameElements.Add(new RenameElement()
            {
                OldCategoryName = "Панели витража",
                OldSymbolName = "ADSK_Cтеклопакет_25",
                NewCategoryName = "Окна",
                NewSymbolName = "ОК",
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }

   

    [DisplayName("Сопоставление")]
    public class RenameElement
    {
        [Category("Старые названия")]
        [DisplayName("Тип")]
        public string OldSymbolName { get; set; }


        [Category("Старые названия")]
        [DisplayName("Категория")]
        public string OldCategoryName { get; set; }

        [Category("Новые названия")]
        [DisplayName("Тип")]
        public string NewSymbolName { get; set; }


        [Category("Новые названия")]
        [DisplayName("Категория")]
        public string NewCategoryName { get; set; }
        public override string ToString()
        {
            return $"{OldCategoryName}/{OldSymbolName}=>{NewCategoryName}/{NewSymbolName}";
        }

    }


    public enum Mode
    {
        OutSide,
        Temperature,
        Union
    }

    public class ModeItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add(Mode.OutSide, "Внешние пространства");
            sizes.Add(Mode.Temperature, "Температурный");
            sizes.Add(Mode.Union, "Объединенный");
            return sizes;
        }
    }


    public enum FaceHorizontalDirection
    {
        All,
        Up,
        Down,
        None
    }

    public class FaceHorizontalDirectionItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add(FaceHorizontalDirection.All, "Учитывать все");
            sizes.Add(FaceHorizontalDirection.Up, "Только вверх");
            sizes.Add(FaceHorizontalDirection.Down, "Только вниз");
            sizes.Add(FaceHorizontalDirection.None, "Не учитывать");
            return sizes;
        }
    }

    public class FaceVerticalDirectionItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add(true, "Учитывать все");
            sizes.Add(false, "Не учитывать");
            return sizes;
        }
    }

    public  class StepSizeItemsSource : IItemsSource
    {
        public ItemCollection GetValues()
        {
            ItemCollection sizes = new ItemCollection();
            sizes.Add(0.1);
            sizes.Add(0.15);
            sizes.Add(0.2);
            sizes.Add(0.25);
            sizes.Add(0.3);
            sizes.Add(0.35);
            sizes.Add(0.4);
            sizes.Add(0.45);
            sizes.Add(0.5);
            return sizes;
        }
    }



}
