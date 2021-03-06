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

using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common;
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.UserControls;
using GingerCore.GeneralLib;
using GingerWPF.ApplicationModelsLib.ModelParams_Pages;
using GingerWPF.WizardLib;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace GingerWPF.ApplicationModelsLib.APIModelWizard
{
    public partial class ModelParamsPage : Page
    {
        ApplicationAPIModel mAAMB;
        public ObservableList<AppModelParameter> ParamsList = new ObservableList<AppModelParameter>();
        public ObservableList<GlobalAppModelParameter> APIGlobalParamList = new ObservableList<GlobalAppModelParameter>();
        string GridPlaceholderHeader = "Place Holder";

        public ModelParamsPage(ApplicationAPIModel AAMB)
        {
            InitializeComponent();
            mAAMB = AAMB;
            ParamsList = AAMB.AppModelParameters;
            APIGlobalParamList = AAMB.GlobalAppModelParameters;
            InitModelParametersGrid();
            InitGlobalModelParametersGrid();

            mAAMB.AppModelParameters.CollectionChanged += LocalParameters_CollectionChanged;
            UpdateLocalParametersGridHeader();
            mAAMB.GlobalAppModelParameters.CollectionChanged += GloablParameters_CollectionChanged;
            UpdateGlobalParametersGridHeader();
        }

        private void InitGlobalModelParametersGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.RequiredAsInput), Header = "Required as Input", WidthWeight = 30, MaxWidth = 220, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.PlaceHolder), Header = GridPlaceholderHeader, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.Description), Header = "Description", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.OptionalValuesString), Header = "Optional Values", WidthWeight = 80, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["OpenEditGlobalParamPossibleValuesPage"] });

            xGlobalModelParametersGrid.SetAllColumnsDefaultView(view);
            xGlobalModelParametersGrid.InitViewItems();

            if (xGlobalModelParametersGrid.Grid != null)
            {
                xGlobalModelParametersGrid.Grid.BeginningEdit += grdGlobalParams_BeginningEdit;
                xGlobalModelParametersGrid.Grid.CellEditEnding += grdGlobalParams_CellEditEnding;
            }

            xGlobalModelParametersGrid.DataSourceList = APIGlobalParamList;
            if (APIGlobalParamList.Count > 0)
            {
                xGlobalModelParamsExpander.IsExpanded = true;
                Row2GlobalParams.Height = new GridLength(100, GridUnitType.Star);
            }
            else
            {
                Row2GlobalParams.Height = new GridLength(35);
            }

            xGlobalModelParametersGrid.Grid.CanUserDeleteRows = false;
            xGlobalModelParametersGrid.ShowRefresh = Visibility.Visible;
            xGlobalModelParametersGrid.ShowTitle = Visibility.Collapsed;
            xGlobalModelParametersGrid.ShowRefresh = Visibility.Visible;
            xGlobalModelParametersGrid.ShowUpDown = Visibility.Visible;
            xGlobalModelParametersGrid.ShowAdd = Visibility.Visible;
            xGlobalModelParametersGrid.ShowClearAll = Visibility.Visible;
            xGlobalModelParametersGrid.ShowDelete = Visibility.Visible;
            xGlobalModelParametersGrid.ShowEdit = Visibility.Collapsed;
            xGlobalModelParametersGrid.ShowCopyCutPast = Visibility.Collapsed;

            xGlobalModelParametersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddGlobalParam));
            xGlobalModelParametersGrid.btnRefresh.AddHandler(Button.ClickEvent, new RoutedEventHandler(RefreshGlobalParameters));
        }


        private void InitModelParametersGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.RequiredAsInput), Header = "Required as Input", WidthWeight = 30, MaxWidth = 220, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.PlaceHolder), Header = GridPlaceholderHeader, WidthWeight = 100 });
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.Path), Header = "Path", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.Description), Header = "Description", WidthWeight = 150 });
            view.GridColsView.Add(new GridColView() { Field = nameof(AppModelParameter.OptionalValuesString), Header = "Optional Values", WidthWeight = 80, ReadOnly = true, BindingMode = BindingMode.OneWay });
            view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.pageGrid.Resources["OpenEditLocalParamPossibleValuesPage"] });

            ModelParametersGrid.SetAllColumnsDefaultView(view);
            ModelParametersGrid.InitViewItems();

            if (ModelParametersGrid.Grid != null)
            {
                ModelParametersGrid.Grid.BeginningEdit += grdLocalParams_BeginningEdit;
                ModelParametersGrid.Grid.CellEditEnding += grdLocalParams_CellEditEnding;
            }

            ModelParametersGrid.DataSourceList = mAAMB.AppModelParameters;

            ModelParametersGrid.Grid.CanUserDeleteRows = false;
            ModelParametersGrid.ShowTitle = Visibility.Collapsed;
            ModelParametersGrid.ShowRefresh = Visibility.Collapsed;
            ModelParametersGrid.ShowUpDown = Visibility.Visible;
            ModelParametersGrid.ShowAdd = Visibility.Visible;
            ModelParametersGrid.ShowClearAll = Visibility.Visible;
            ModelParametersGrid.ShowDelete = Visibility.Visible;
            ModelParametersGrid.ShowEdit = Visibility.Collapsed;
            ModelParametersGrid.ShowCopyCutPast = Visibility.Visible;
            
            ModelParametersGrid.AddToolbarTool(eImageType.Merge, "Merge Selected Parameters", new RoutedEventHandler(MergeSelectedParams));
            ModelParametersGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddParamsRow));
            ModelParametersGrid.AddToolbarTool("@Upgrade_16x16.png", "Upload to Global Parameters", new RoutedEventHandler(UploadToGlobalParam));
            ModelParametersGrid.AddToolbarTool("@Import_16x16.png", "Import Optional Values For Parameters", new RoutedEventHandler(ImportOptionalValuesForParameters));
        }
        private void ImportOptionalValuesForParameters(object sender, RoutedEventArgs e)
        {            
            WizardWindow.ShowWizard(new AddModelOptionalValuesWizard(mAAMB));
            ModelParametersGrid.DataSourceList = mAAMB.AppModelParameters;
        }
        private void UploadToGlobalParam(object sender, RoutedEventArgs e)
        {
            AppModelParameter CurrentAMDP = (AppModelParameter)ModelParametersGrid.CurrentItem;

            GlobalAppModelParameter globalAppModelParameter = GlobalAppModelParameter.DuplicateAppModelParamAsGlobal(CurrentAMDP);

            ObservableList<GlobalAppModelParameter> ModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();

            foreach (GlobalAppModelParameter GAMDP in ModelsGlobalParamsList)
            {
                if (GAMDP != globalAppModelParameter && GAMDP.PlaceHolder == CurrentAMDP.PlaceHolder)
                {
                    MessageBox.Show("Global Model Parameters allready contains a parameter Place Holder with the same value", "Cannot upload the selected parameter" , System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);
                    return;
                }
            }
            mAAMB.AppModelParameters.Remove(CurrentAMDP);

            WorkSpace.Instance.SolutionRepository.AddRepositoryItem(globalAppModelParameter);

            AddGlobalParametertoAPIGlobalParameterList(mAAMB.GlobalAppModelParameters, globalAppModelParameter);
        }

        private void AddGlobalParam(object sender, RoutedEventArgs e)
        {
            ModelsGlobalParamsPage MGPP = new ModelsGlobalParamsPage(true);
            List<GlobalAppModelParameter> globalParamsToAdd = MGPP.ShowAsWindow();
            if (globalParamsToAdd != null)
                foreach (GlobalAppModelParameter GAMP in globalParamsToAdd)
                {
                    AddGlobalParametertoAPIGlobalParameterList(APIGlobalParamList, GAMP);
                }
        }

        private void AddGlobalParametertoAPIGlobalParameterList(ObservableList<GlobalAppModelParameter> APIGlobalParamList, GlobalAppModelParameter GAMP)
        {
            GlobalAppModelParameter newAPIGlobalParam = new GlobalAppModelParameter();
            newAPIGlobalParam.Guid = GAMP.Guid;
            newAPIGlobalParam.CurrentValue = GAMP.CurrentValue;
            newAPIGlobalParam.PlaceHolder = GAMP.PlaceHolder;
            newAPIGlobalParam.Description = GAMP.Description;
            foreach (OptionalValue ov in GAMP.OptionalValuesList)
            {
                OptionalValue newOV = new OptionalValue();
                newOV.Guid = ov.Guid;
                newOV.Value = ov.Value;
                newOV.IsDefault = ov.IsDefault;
                newAPIGlobalParam.OptionalValuesList.Add(newOV);
            }
            APIGlobalParamList.Add(newAPIGlobalParam);
        }

        private void RefreshGlobalParameters(object sender, RoutedEventArgs e)
        {
            WorkSpace.Instance.RefreshGlobalAppModelParams(mAAMB);
        }

        private void MergeSelectedParams(object sender, RoutedEventArgs e)
        {
            if (ModelParametersGrid.Grid.SelectedItems.Count < 1)
                return;

            string newParamName = ((AppModelParameter)ModelParametersGrid.Grid.SelectedItems[0]).PlaceHolder;

            if (InputBoxWindow.GetInputWithValidation("Merge Parameters", "Set Palceholder for Merged Parameters", ref newParamName, new char[0]))
            {
                //Create new Merged param
                AppModelParameter mergedParam = new AppModelParameter();
                mergedParam.PlaceHolder = newParamName;

                //Merged optional values
                SetMergedOptionalValues(mergedParam);

                //Set Grid selected index
                int selctedIndex = ModelParametersGrid.Grid.SelectedIndex;

                List<string> placeHoldersToReplace = new List<string>();
                //Save Placeholders and remove old params for merge, and add the new merged one

                List<AppModelParameter> tobeRemoved = new List<AppModelParameter>();
                for (int i = 0; i< ModelParametersGrid.Grid.SelectedItems.Count; i++)
                {
                    AppModelParameter paramToRemove = (AppModelParameter)ModelParametersGrid.Grid.SelectedItems[i];
                    placeHoldersToReplace.Add(paramToRemove.PlaceHolder);
                    tobeRemoved.Add(paramToRemove);
                }

                foreach(AppModelParameter Removeit in tobeRemoved)
                {
                    mAAMB.AppModelParameters.Remove(Removeit);
                }

                mAAMB.AppModelParameters.Add(mergedParam);
                GingerCore.General.DoEvents();
                ModelParametersGrid.DataSourceList.Move(ModelParametersGrid.DataSourceList.Count - 1, selctedIndex);

                //Update all places with new placeholder merged param name
                //TODO: Fix with New Reporter (on GingerWPF)
                if (System.Windows.MessageBox.Show(string.Format("Do you want to update the merged instances on all model configurations?"), "Models Parameters Merge", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.No) == System.Windows.MessageBoxResult.Yes)
                    mAAMB.UpdateParamsPlaceholder(mAAMB, placeHoldersToReplace, newParamName);
            }
        }

        private void SetMergedOptionalValues(AppModelParameter mergedParam)
        {
            ObservableList<OptionalValue> mergedOptionalValuesList = new ObservableList<OptionalValue>();

            foreach (AppModelParameter apiModelParam in ModelParametersGrid.Grid.SelectedItems)
            {
                foreach (OptionalValue paramOV in apiModelParam.OptionalValuesList)
                {
                    if (mergedOptionalValuesList.Where(x => x.Value == paramOV.Value).FirstOrDefault() == null)
                    {
                        OptionalValue ov = new OptionalValue();
                        ov.Value = paramOV.Value;
                        mergedOptionalValuesList.Add(ov);
                    }
                }
            }

            if (mergedOptionalValuesList.Count > 0)
            {
                //Set Default optional value to be as the default from the first optional values list
                OptionalValue defaultOV = ((AppModelParameter)ModelParametersGrid.Grid.SelectedItems[0]).OptionalValuesList.Where(x => x.IsDefault == true).FirstOrDefault();
                OptionalValue ovToSetAsDefault = mergedOptionalValuesList.Where(x => x.Value == defaultOV.Value).FirstOrDefault();
                ovToSetAsDefault.IsDefault = true;
            }

            mergedParam.OptionalValuesList = mergedOptionalValuesList;
        }

        string LocalParamValueBeforeEdit = string.Empty;
        private void grdLocalParams_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            LocalParamValueBeforeEdit = ((AppModelParameter)ModelParametersGrid.CurrentItem).PlaceHolder;
        }

        private void grdLocalParams_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            AppModelParameter CurrentAMDP = null;
            if (e.Column.Header.ToString() == GridPlaceholderHeader)
            {
                CurrentAMDP = (AppModelParameter)ModelParametersGrid.CurrentItem;
                if (CurrentAMDP != null && !IsParamPlaceholderNameConflict(CurrentAMDP))
                    mAAMB.UpdateParamsPlaceholder(mAAMB, new List<string> { LocalParamValueBeforeEdit }, CurrentAMDP.PlaceHolder);
            }
        }

        string GlobalParamOldValueBeforeEdit = string.Empty;
        private void grdGlobalParams_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            GlobalParamOldValueBeforeEdit = ((GlobalAppModelParameter)xGlobalModelParametersGrid.CurrentItem).PlaceHolder;
        }

        private void grdGlobalParams_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            GlobalAppModelParameter CurrentGAMDP = null;
            if (e.Column.Header.ToString() == GridPlaceholderHeader)
            {
                CurrentGAMDP = (GlobalAppModelParameter)xGlobalModelParametersGrid.CurrentItem;
                if (CurrentGAMDP != null && !CurrentGAMDP.PlaceHolder.Equals(GlobalParamOldValueBeforeEdit))
                {
                    System.Windows.MessageBox.Show("Global Parameter Place Holder cannot be edit.", "Global Parameter Edit", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);
                    CurrentGAMDP.PlaceHolder = GlobalParamOldValueBeforeEdit;
                }
            }
        }

        private bool IsParamPlaceholderNameConflict(AppModelParameter CurrentAMDP)
        {
            foreach (AppModelParameter AMDP in mAAMB.AppModelParameters)
            {
                if (AMDP != CurrentAMDP && AMDP.PlaceHolder == CurrentAMDP.PlaceHolder)
                {
                    CurrentAMDP.PlaceHolder = LocalParamValueBeforeEdit;
                    System.Windows.MessageBox.Show("Please specify unique value", "Optional Value Already Exists", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);
                    return true;
                }
            }
            foreach (AppModelParameter GAMDP in mAAMB.GlobalAppModelParameters)
            {
                if (GAMDP != CurrentAMDP && GAMDP.PlaceHolder == CurrentAMDP.PlaceHolder)
                {
                    CurrentAMDP.PlaceHolder = LocalParamValueBeforeEdit;
                    System.Windows.MessageBox.Show("Please specify unique value", "Optional Value Already Exists", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);
                    return true;
                }
            }
            return false;
        }

        private void OpenEditGlobalParamPossibleValuesPageButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalAppModelParameter SelectedAMGP = (GlobalAppModelParameter)xGlobalModelParametersGrid.CurrentItem;
            ModelOptionalValuesPage MDPVP = new ModelOptionalValuesPage(SelectedAMGP, true);
            MDPVP.ShowAsWindow();
        }

        private void OpenEditLocalParamPossibleValuesPageButton_Click(object sender, RoutedEventArgs e)
        {
            AppModelParameter SelectedAMDP = (AppModelParameter)ModelParametersGrid.CurrentItem;
            ModelOptionalValuesPage MDPVP = new ModelOptionalValuesPage(SelectedAMDP);
            MDPVP.ShowAsWindow();
        }

        private void AddParamsRow(object sender, RoutedEventArgs e)
        {
            AppModelParameter newAppModelParam = new AppModelParameter();
            SetUniquePlaceHolderName(newAppModelParam);
            ParamsList.Add(newAppModelParam);
        }

        public void SetUniquePlaceHolderName(AppModelParameter newAppModelParam)
        {
            newAppModelParam.PlaceHolder = "{NewPlaceHolder}";

            if (ParamsList.Where(x => x.PlaceHolder == newAppModelParam.PlaceHolder).FirstOrDefault() == null) return;

            List<AppModelParameter> samePlaceHolderList = ParamsList.Where(x => x.PlaceHolder == newAppModelParam.PlaceHolder).ToList<AppModelParameter>();
            if (samePlaceHolderList.Count == 1 && samePlaceHolderList[0] == newAppModelParam) return; //Same internal object

            //Set unique name
            int counter = 2;
            while ((ParamsList.Where(x => x.PlaceHolder == "{NewPlaceHolder_" + counter.ToString() + "}").FirstOrDefault()) != null)
                counter++;

            newAppModelParam.PlaceHolder = "{NewPlaceHolder_" + counter.ToString() + "}";
        }

        private void LocalParameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateLocalParametersGridHeader();
        }

        public void UpdateLocalParametersGridHeader()
        {
            xModelParamsExpanderLabel.Content = string.Format("Local Parameters ({0})", mAAMB.AppModelParameters.Count);
        }

        private void GloablParameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateGlobalParametersGridHeader();
        }

        public void UpdateGlobalParametersGridHeader()
        {
            xGlobalModelParamsExpanderLabel.Content = string.Format("Global Parameters ({0})", mAAMB.GlobalAppModelParameters.Count);
        }

        private void LocalParamsGrid_Expanded(object sender, RoutedEventArgs e)
        {
            Row1LocalParams.Height = new GridLength(100, GridUnitType.Star);
        }

        private void LocalParamsGrid_Collapsed(object sender, RoutedEventArgs e)
        {
            Row1LocalParams.Height = new GridLength(35);
        }

        private void GlobalParams_Expanded(object sender, RoutedEventArgs e)
        {
            Row2GlobalParams.Height = new GridLength(100, GridUnitType.Star);
        }

        private void GlobalParams_Collapsed(object sender, RoutedEventArgs e)
        {
            Row2GlobalParams.Height = new GridLength(35);
        }
    }
}
