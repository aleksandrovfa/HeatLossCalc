using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HeatLossCalc.Core;
using HeatLossCalc.Views;
using Revit.Async;
//using RevitGeometryExporter;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;

namespace HeatLossCalc
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class HeatLossCalcCmd : IExternalCommand
    {
        public static HeatLossCalcView _view;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            Debug.Listeners.Clear();
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string name = Assembly.GetExecutingAssembly().GetName().Name;
            Debug.Listeners.Add(new RbsLogger.Logger($"{name}_{version}"));
            RevitApi.CommandData = commandData;

         


            RevitTask.Initialize(RevitApi.AppUI);

            try
            {
                Debug.WriteLine(" Analyze!!! Create!!!");
           
                _view = new HeatLossCalcView();

                WindowInteropHelper helper = new WindowInteropHelper(_view);
                helper.Owner = commandData.Application.MainWindowHandle;

                _view.Show();
              
                return Result.Succeeded;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + e.StackTrace);
                MessageBox.Show(e.Message + e.StackTrace, "Что то поломалось... Скоро починим");
                return Result.Failed;
            }
        }

        internal static void LoadAssembly(string dllName)
        {
            if (!File.Exists(dllName))
            {
                return;
            }

            string version = FileVersionInfo.GetVersionInfo(dllName).ProductVersion;

            Assembly assembly = FindLoadedAssembly(Path.GetFileNameWithoutExtension(dllName), version);

            if (assembly == null)
            {
                byte[] assemblyBytes = File.ReadAllBytes(dllName);
                Assembly.Load(assemblyBytes);
            }
        }


        private static Assembly FindLoadedAssembly(string name, string version)
        {
            AppDomain domain = AppDomain.CurrentDomain;
            var allAssemblies = domain.GetAssemblies();
            var ourAssemblies = allAssemblies.Where(x => x.GetName().Name == name);
            foreach (Assembly assembly in ourAssemblies)
            {
                string assemblyVersion = GetAssemblyVersion(assembly.FullName);

                if (assemblyVersion == version)
                    return assembly;
            }

            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(x =>
                x.GetName().Name == name
                && GetAssemblyVersion(x.FullName) == version)
                .FirstOrDefault();
        }

        private static string GetAssemblyVersion(string assemblyName)
        {
            return assemblyName.Split(',')[1].Split('=')[1];
        }



    }
}
