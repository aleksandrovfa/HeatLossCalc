using Microsoft.Win32;
using System.IO.Packaging;
using System.Reflection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Navigation;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace HeatLossCalc.Views
{
    /// <summary>
    /// Interaction logic for PropertyGridFilePicker.xaml
    /// </summary>
    public partial class PropertyGridFilePicker : UserControl,ITypeEditor
    {
        public PropertyGridFilePicker()
        {
            //InitializeComponent();
            this.LoadViewFromUri("/HeatLossCalc;component/views/propertygridfilepicker.xaml");
        }

        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value",
                typeof(string), 
                typeof(PropertyGridFilePicker), 
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));



        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            Binding binding = new Binding("Value");
            binding.Source = propertyItem;
            binding.Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay;
            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }

        private void PickFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == true && fd.CheckFileExists)
            {
                Value = fd.FileName;
            }
        }
    }

   
}