#region License
/*
Copyright © 2014-2018 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using Amdocs.Ginger.Common;
using GingerCoreNET.GeneralLib;
using GingerPlugInsNET.PlugInsLib;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;
using GingerPlugInsNET.DriversLib;
using System.IO;
using Amdocs.Ginger.CoreNET.PlugInsLib;
using amdocs.ginger.GingerCoreNET;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;
using GingerPlugInsNET.ActionsLib;
using Amdocs.Ginger.CoreNET.SolutionRepositoryLib.RepositoryObjectsLib.ActionsLib.Common;

namespace GingerCoreNET.PlugInsLib
{
    public class PluginsManager
    {
        private ObservableList<PluginPackage> mPluginPackages = new ObservableList<PluginPackage>();

        public class DriverInfo
        {
            public string Name { get; set; }
            public string PluginPackageFolder { get; set; }
        }

        public List<DriverInfo> GetAllDrivers()
        {
            //TODO: cache !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            List<DriverInfo> drivers = new List<DriverInfo>();

            foreach (PluginPackage p in mPluginPackages)
            {
                foreach (string s in p.GetDrivers())
                {
                    DriverInfo di = new DriverInfo();
                    di.Name = s;
                    di.PluginPackageFolder = p.Folder;
                    drivers.Add(di);
                }
            }
            return drivers;
        }

        public List<GingerAction> GetDriverActions(DriverInfo DI)
        {
            List<GingerAction> actions = new List<GingerAction>();
            PluginPackage p = (from x in mPluginPackages where x.Folder == DI.PluginPackageFolder select x).FirstOrDefault();
            PluginDriverBase driver = p.GetDriver(DI.Name);
            foreach (ActionHandler AH in driver.ActionHandlers)
            {
                GingerAction GA = new GingerAction(AH.ID);
                UpdateActionParamTypes(GA, AH.MethodInfo);
                actions.Add(GA);
            }
            return actions;
        }

        ObservableList<StandAloneAction> list;
        public ObservableList<StandAloneAction> GetStandAloneActions()
        {
            if (list == null)
            {
                list = new ObservableList<StandAloneAction>();
                foreach (PluginPackage p in mPluginPackages)
                {
                    foreach (StandAloneAction SAA in p.GetStandAloneActions())
                    {
                        list.Add(SAA);
                    }
                }
            }
            return list;
        }

        private void UpdateActionParamTypes(GingerAction gA, MethodInfo methodInfo)
        {
            foreach (ParameterInfo PI in methodInfo.GetParameters())
            {
                if (PI.ParameterType != typeof(GingerAction))
                {
                    ActionParam AP = gA.InputParams.GetOrCreateParam(PI.Name);
                    AP.ParamType = PI.ParameterType;
                }
            }
        }

        public void AddPluginPackage(string folder)
        {
            PluginPackage p = new PluginPackage();
            p.Folder = folder;
            p.PluginID = folder;
            mPluginPackages.Add(p);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            string s = args.LoadedAssembly.FullName + " " + args.LoadedAssembly.Location;
            Console.WriteLine(s);
        }

        static Assembly LoadFromSameFolder(object sender, ResolveEventArgs args)
        {
            string folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string assemblyPath = Path.Combine(folderPath, new AssemblyName(args.Name).Name + ".dll");
            if (File.Exists(assemblyPath))
            {
                Assembly assembly = Assembly.LoadFrom(assemblyPath);


                return assembly;
            }
            else
            {
                return null;
            }
        }

        internal DriverInfo GetDriverInfo(string PluginDriverName)
        {
            foreach (DriverInfo di in GetAllDrivers())
            {
                if (di.Name == PluginDriverName)
                {
                    return di;
                }
            }
            return null;
        }

        internal ActionHandler GetStandAloneActionHandler(string pluginID, string ID)
        {
            foreach (PluginPackage PP in mPluginPackages)
            {
                ActionHandler AH = PP.GetStandAloneActionHandler(ID);
                if (AH != null)
                {
                    return AH;
                }
            }
            throw new Exception("Action handler not found for Action ID: " + ID);
        }

        static List<PluginPackage> mInstalledPluginPackages = null;

        // Get list of installed plugins in Ginger folder 'PluginPackages'
        public List<PluginPackage> GetInstalledPluginPackages()
        {
            if (mInstalledPluginPackages != null)
            {
                //TODO: check for new added plugins

                return mInstalledPluginPackages;
            }

            mInstalledPluginPackages = new List<PluginPackage>();

            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (path.Contains("GingerWPF"))   // We are running from GingerWPF in debug mode
            {
                path = path.Replace(@"GingerWPF\bin\Debug", "");   // temp need to be Ginger installation folder !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            }

            if (path.Contains("GingerCoreNETUnitTest"))   // We are running from GingerWPF in debug mode
            {
                path = path.Replace(@"GingerCoreNETUnitTest\bin\Debug\netcoreapp2.0", "");
            }

            string pluginPackagesPath = Path.Combine(path, "PluginPackages");

            // Each directory is a plugin package

            foreach (string d in Directory.GetDirectories(pluginPackagesPath))
            {
                PluginPackage p = new PluginPackage();
                p.Folder = d;
                p.PluginID = d;     // FIXME ?!            
                mInstalledPluginPackages.Add(p);
            }
            return mInstalledPluginPackages;
        }
    }
}