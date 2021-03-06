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
using System.Windows;
using System.Windows.Controls;
using GingerCore;
using GingerCore.DataSource;
using Ginger.UserControls;

namespace Ginger.DataSource
{
    /// <summary>
    /// Interaction logic for ExplorerBusinessFlowsPage.xaml
    /// </summary>
    public partial class DataSourcesPage : Page
    {
        public string Path { get; set; }

        public DataSourcesPage()
        {
            InitializeComponent();
            SetDataSourcesGridView();
            SetGridData();

            // grdDataSources.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddDataSource));
            grdDataSources.btnDelete.AddHandler(Button.ClickEvent, new RoutedEventHandler(DeleteDataSource));
            grdDataSources.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGrid));
        }

        private void SetDataSourcesGridView()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = DataSourceBase.Fields.Name, Header = "Data Source Name", WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = DataSourceBase.Fields.DSType, Header = "Data Source Type", WidthWeight = 250 });
            view.GridColsView.Add(new GridColView() { Field = DataSourceBase.Fields.FilePath, Header = "File Path", WidthWeight = 250 });
            grdDataSources.SetAllColumnsDefaultView(view);
            grdDataSources.InitViewItems();
        }

        //private void AddDataSource(object sender, RoutedEventArgs e)
        //{
        //    AddNewDataSourcePage CDSP = new AddNewDataSourcePage();
        //    CDSP.ShowAsWindow();

        //    DataSourceBase dsDetails = CDSP.DSDetails;

        //    if (dsDetails != null)
        //    {                
        //        App.LocalRepository.SaveNewItem(dsDetails, Path);      
        //        App.LocalRepository.AddItemToCache(dsDetails);
        //        grdDataSources.DataSourceList.Add(dsDetails);                
        //    }
        //}
        
        private void DeleteDataSource(object sender, RoutedEventArgs e)
        {
        }
        private void RefreshGrid(object sender, RoutedEventArgs e)
        {
            SetGridData();
        }

        private void SetGridData()
        {
            grdDataSources.DataSourceList = App.LocalRepository.GetSolutionDataSources(includeSubFolders: true);
        }
    }
}
