using Autodesk.Revit.DB;
using HeatLossCalc.Models;
using HeatLossCalc.Properties;
using HeatLossCalc.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout.Serialization;
using MultiSelectTreeViewDemo;

namespace HeatLossCalc.Views
{
    /// <summary>
    /// Логика взаимодействия для HeatLossCalcView.xaml
    /// </summary>
    [XmlLangProperty("ru")]
    public partial class HeatLossCalcView : Window
    {

        private HeatLossCalcViewModel _viewModel;
        public HeatLossCalcView()
        {

            //InitializeComponent();
            this.LoadViewFromUri("/HeatLossCalc;component/views/heatlosscalcview.xaml");
            Debug.WriteLine("InitializeComponent MainView");
            _viewModel = new HeatLossCalcViewModel();
            DataContext = _viewModel;
            _viewModel.CloseRequest += (s, e) => this.Close();
            this.Title = this.Title + $" (ver.{Assembly.GetExecutingAssembly().GetName().Version})";
            //if (Properties.Settings.Default.XceedLayout != "test")
            //{
            //    LoadLayout(Properties.Settings.Default.XceedLayout);
            //}
            //this.Closed += HeatLossCalcView_Closed;
        }

        private void HeatLossCalcView_Closed(object sender, EventArgs e)
        {
            Properties.Settings.Default.XceedLayout = SaveLayout();
            Properties.Settings.Default.Save();
        }

        private void Button_Click_Find(object sender, RoutedEventArgs e)
        {
            List<SpaceAnalysis> spaces = this.Spaces.SelectedItems
                .Cast<object>()
                .Where(x => x is SpaceAnalysis)
                .Cast<SpaceAnalysis>()
                .Where(modelItem => modelItem != null)
                .ToList();

            List<RayArea> rayareas = this.Spaces.SelectedItems
                .Cast<object>()
                .Where(x => x is RayArea)
                .Cast<RayArea>()
                .Where(modelItem => modelItem != null)
                .ToList();
           _viewModel.FindElement(spaces,rayareas, this);
        }

        private void Button_Click_Find2(object sender, RoutedEventArgs e)
        {
            _viewModel.FindAirs(this);
        }



        private void Spaces_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Properties.Settings.Default.IsViewSynchronization)
            {
                Button_Click_Find(sender, e);
            }
        }

        private string SaveLayout()
        {
            try
            {
                XmlLayoutSerializer serializer = new XmlLayoutSerializer(this.DockingManage);
                using (var stream = new StringWriter())
                {
                    serializer.Serialize(stream);
                    return stream.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        private void LoadLayout(string layoutXml)
        {
            try
            {
                XmlLayoutSerializer serializer = new XmlLayoutSerializer(this.DockingManage);
                using (var stream = new StringReader(layoutXml))
                {
                    serializer.Deserialize(stream);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        //private void Expander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    DataGridRow row = FindVisualParent<DataGridRow>(sender as Expander);
        //    row.DetailsVisibility = System.Windows.Visibility.Visible;
        //}

        //private void Expander_Collapsed(object sender, RoutedEventArgs e)
        //{
        //    DataGridRow row = FindVisualParent<DataGridRow>(sender as Expander);
        //    row.DetailsVisibility = System.Windows.Visibility.Collapsed;
        //}

        //public T FindVisualParent<T>(DependencyObject child) where T : DependencyObject
        //{
        //    DependencyObject parentObject = VisualTreeHelper.GetParent(child);
        //    if (parentObject == null) return null;
        //    T parent = parentObject as T;
        //    if (parent != null)
        //        return parent;
        //    else
        //        return FindVisualParent<T>(parentObject);
        //}

        //public T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        //{
        //    DependencyObject childObject = VisualTreeHelper.GetChild(parent,0);
        //    if (childObject == null) return null;
        //    T child = childObject as T;
        //    if (child != null)
        //        return child;
        //    else
        //        return FindVisualChild<T>(parent);
        //}

        //private void DG_myStudents_RowDetailsVisibilityChanged(object sender, DataGridRowDetailsEventArgs e)
        //{

        //    DataGrid MainDataGrid = sender as DataGrid;
        //    var cell = MainDataGrid.CurrentCell;

        //    SpaceAnalysis spaces = (MainDataGrid.CurrentItem as SpaceAnalysis);
        //    if (spaces == null)
        //    {
        //        return;
        //    }
        //    //DataGrid DetailsDataGrid = e.DetailsElement as DataGrid;
        //    DataGrid DetailsDataGrid = FindVisualChild<DataGrid>(e.DetailsElement);
        //    DetailsDataGrid.ItemsSource = spaces.Areas;
        //}

        private void MultiSelectTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Properties.Settings.Default.IsViewSynchronization)
            {
                Button_Click_Find(sender, e);
            }
        }
    }

}
