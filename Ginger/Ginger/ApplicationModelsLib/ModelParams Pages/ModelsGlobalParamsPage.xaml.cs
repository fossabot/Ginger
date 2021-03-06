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
using Amdocs.Ginger.Repository;
using Ginger;
using Ginger.ApplicationModelsLib.ModelOptionalValue;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.GeneralLib;
using GingerWPF.ApplicationModelsLib.APIModelWizard;
using GingerWPF.WizardLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace GingerWPF.ApplicationModelsLib.ModelParams_Pages
{
    public partial class ModelsGlobalParamsPage : Page
    {
        public ObservableList<GlobalAppModelParameter> mModelsGlobalParamsList;
        GenericWindow mGenericWindow = null;
        List<GlobalAppModelParameter> SelectedGlobalParamsFromDialogPage = new List<GlobalAppModelParameter>();
        List<GlobalAppModelParameter> GlobalParamsCopiedItemsList = new List<GlobalAppModelParameter>();
        string gridPlaceholderHeader = "Place Holder";
        bool mSelectionModePage;

        public ModelsGlobalParamsPage(bool selectionModePage = false)
        {
            InitializeComponent();
            mSelectionModePage = selectionModePage;
            mModelsGlobalParamsList = WorkSpace.Instance.SolutionRepository.GetAllRepositoryItems<GlobalAppModelParameter>();

            InitApplicationModelsGlobalParamsGrid();
            if (!selectionModePage)
                SetGridRowStyle();
        }

        private void ShowGlobalAndLocalParams(ObservableList<AppModelParameter> MergedParamsList)
        {
            mModelsGlobalParamsList = new ObservableList<GlobalAppModelParameter>();

            foreach (var param in MergedParamsList)
                mModelsGlobalParamsList.Add(param as GlobalAppModelParameter);
        }

        private void SetGridRowStyle()
        {
            //If row is dirty - show the row header (where line number) in different color
            Style st2 = xModelsGlobalParamsGrid.grdMain.RowHeaderStyle;
            DataTrigger DT2 = new DataTrigger();
            PropertyPath PT2 = new PropertyPath(nameof(GlobalAppModelParameter.IsDirty));
            DT2.Binding = new Binding() { Path = PT2 };
            DT2.Value = true;
            DT2.Setters.Add(new Setter(Control.BackgroundProperty, Brushes.LightPink));
            st2.Triggers.Add(DT2);
        }

        private void InitApplicationModelsGlobalParamsGrid()
        {
            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.PlaceHolder), Header = gridPlaceholderHeader, WidthWeight = 100, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.Description), Header = "Description", WidthWeight = 150, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.OptionalValuesString), Header = "Optional Values", WidthWeight = 80, ReadOnly = true, BindingMode = BindingMode.OneWay, AllowSorting = true });

            if (!mSelectionModePage)
            {
                view.GridColsView.Add(new GridColView() { Field = "...", WidthWeight = 8, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.xPageGrid.Resources["OpenEditPossibleValuesPage"] });
                view.GridColsView.Add(new GridColView() { Field = nameof(GlobalAppModelParameter.CurrentValue), Header = "Current Value", WidthWeight = 80, AllowSorting = true });

                xModelsGlobalParamsGrid.btnSaveSelectedChanges.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveSelectedGlobalParametersChanges));
                xModelsGlobalParamsGrid.btnSaveAllChanges.AddHandler(Button.ClickEvent, new RoutedEventHandler(SaveAllGlobalParametersChanges));
                xModelsGlobalParamsGrid.btnAdd.AddHandler(Button.ClickEvent, new RoutedEventHandler(AddGlobalParam));
                xModelsGlobalParamsGrid.SetbtnDeleteHandler(new RoutedEventHandler(DeleteSelectedEvent));
                xModelsGlobalParamsGrid.SetbtnClearAllHandler(DeleteAllEvent);
                xModelsGlobalParamsGrid.SetbtnCopyHandler(BtnCopyGlobalParamsClicked);
                xModelsGlobalParamsGrid.SetbtnPastHandler(BtnPastGlobalParamsClicked);

                xModelsGlobalParamsGrid.ShowSaveAllChanges = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowSaveSelectedChanges = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowEdit = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowCopy = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowPaste = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowCut = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowRefresh = Visibility.Collapsed;
                xModelsGlobalParamsGrid.ShowAdd = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowDelete = Visibility.Visible;
                xModelsGlobalParamsGrid.ShowClearAll = Visibility.Visible;
                xModelsGlobalParamsGrid.Grid.CanUserDeleteRows = false;

                xModelsGlobalParamsGrid.Grid.BeginningEdit += grdMain_BeginningEdit;
                xModelsGlobalParamsGrid.Grid.CellEditEnding += grdMain_CellEditEndingAsync;
            }

            xModelsGlobalParamsGrid.ShowTitle = Visibility.Collapsed;
            xModelsGlobalParamsGrid.SetAllColumnsDefaultView(view);
            xModelsGlobalParamsGrid.InitViewItems();

            xModelsGlobalParamsGrid.DataSourceList = mModelsGlobalParamsList;
            xModelsGlobalParamsGrid.AddToolbarTool("@Import_16x16.png", "Import Optional Values For Parameters", new RoutedEventHandler(ImportOptionalValuesForGlobalParameters));
        }

        private void ImportOptionalValuesForGlobalParameters(object sender, RoutedEventArgs e)
        {
            WizardWindow.ShowWizard(new AddModelOptionalValuesWizard(mModelsGlobalParamsList));
            xModelsGlobalParamsGrid.DataSourceList = mModelsGlobalParamsList;
        }


        string PlaceholderBeforeEdit = string.Empty;

        private void grdMain_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            ((RepositoryItemBase)xModelsGlobalParamsGrid.Grid.SelectedItem).SaveBackup();
            if (e.Column.SortMemberPath == nameof(GlobalAppModelParameter.PlaceHolder))
            {
                GlobalAppModelParameter selectedGlobalAppModelParameter = (GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem;
                PlaceholderBeforeEdit = selectedGlobalAppModelParameter.PlaceHolder;
            }
        }

        private async void grdMain_CellEditEndingAsync(object sender, DataGridCellEditEndingEventArgs e)
        {
            GlobalAppModelParameter CurrentGAMP = null;
            if (e.Column.Header.ToString() == gridPlaceholderHeader)
            {
                CurrentGAMP = (GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem;
                if (IsParamPlaceholderNameConflict(CurrentGAMP))
                    return;
            }

            if (e.Column.SortMemberPath == nameof(GlobalAppModelParameter.PlaceHolder))
            {
                GlobalAppModelParameter selectedGlobalAppModelParameter = (GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem;
                string NameAfterEdit = selectedGlobalAppModelParameter.PlaceHolder;
                if (PlaceholderBeforeEdit != NameAfterEdit)
                {
                    if (System.Windows.MessageBox.Show("The Global Parameter name may be used in Solution items Value Expression, Do you want to automatically update all those Value Expression instances with the parameter name change?", "Update Global Parameter Value Expression Instances", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question, System.Windows.MessageBoxResult.No) == MessageBoxResult.Yes)
                    {
                        await Task.Run(() =>
                        {
                            List<string> ListObj = new List<string>() { PlaceholderBeforeEdit, NameAfterEdit };
                            UpdateModelGlobalParamVeWithNameChange(ListObj);
                        });
                        MessageBox.Show("Update finished successfully." + Environment.NewLine + "Please do not forget to save all modified Business Flows", "Update Global Parameter Value Expression Instances", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information, System.Windows.MessageBoxResult.OK);
                    }
                }
            }
        }

        public static void UpdateModelGlobalParamVeWithNameChange(List<string> ListObj)
        {
            ObservableList<RepositoryItemBase> allVESupportingItems = App.LocalRepository.GetSolutionVEsupportedItems();
            Parallel.ForEach(allVESupportingItems, item =>
            {
                if (item is BusinessFlow)
                {
                    BusinessFlow bf = (BusinessFlow)item;
                    foreach (Activity activity in bf.Activities)
                        foreach (Act action in activity.Acts)
                        {
                            bool changedwasDone = false;
                            UpdateItemModelGlobalParamVeWithNameChange(action, ListObj[0], ListObj[1], ref changedwasDone);
                            if (changedwasDone == true && bf.IsDirty == false)
                                bf.SaveBackup();//so VF will show as Dirty
                        }
                }
                else
                {
                    bool changedwasDone2 = false;
                    UpdateItemModelGlobalParamVeWithNameChange(item, ListObj[0], ListObj[1], ref changedwasDone2);
                }
            });
        }

        public static void UpdateItemModelGlobalParamVeWithNameChange(object item, string prevParamName, string newParamName, ref bool changedWasDone)
        {
            if (item == null) return;

            string ValueToSearch = "{GlobalAppsModelsParam Name=" + prevParamName + "}";
            string ValueToReplace = "{GlobalAppsModelsParam Name=" + newParamName + "}";

            var properties = item.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                try
                {
                    if (mi.Name == "mBackupDic" || mi.Name == nameof(RepositoryItemBase.FileName) || mi.Name == nameof(RepositoryItemBase.FilePath) ||
                        mi.Name == nameof(RepositoryItemBase.ObjFolderName) || mi.Name == nameof(RepositoryItemBase.ObjFileExt) ||
                        mi.Name == nameof(RepositoryItemBase.ContainingFolder) || mi.Name == nameof(RepositoryItemBase.ContainingFolderFullPath) ||
                        mi.Name == nameof(Act.ActInputValues) || mi.Name == nameof(Act.ActReturnValues) || mi.Name == nameof(Act.ActFlowControls)) //needed?                   
                        continue;

                    //Get the attr value
                    PropertyInfo PI = item.GetType().GetProperty(mi.Name);
                    dynamic value = null;
                    if (mi.MemberType == MemberTypes.Property)
                        value = PI.GetValue(item);
                    else if (mi.MemberType == MemberTypes.Field)
                        value = item.GetType().GetField(mi.Name).GetValue(item);

                    if (value is IObservableList)
                    {
                        List<dynamic> list = new List<dynamic>();
                        foreach (object o in value)
                            UpdateItemModelGlobalParamVeWithNameChange(o, prevParamName, newParamName, ref changedWasDone);
                    }
                    else
                    {
                        if (value != null)
                        {
                            string stringValue = value.ToString();
                            if (string.IsNullOrEmpty(stringValue) == false && stringValue.Contains(ValueToSearch))
                            {
                                stringValue = stringValue.Replace(ValueToSearch, ValueToReplace);
                                PI.SetValue(item, stringValue);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Reporter.ToLog(eLogLevel.WARN, string.Format("Failed to updated the Model Global Param name change for the property '{0}' in the item '{1}'", mi.Name, item.ToString()), ex);
                }
            }
        }

        private bool IsParamPlaceholderNameConflict(GlobalAppModelParameter CurrentGAMP)
        {
            foreach (AppModelParameter GAMP in mModelsGlobalParamsList)
            {
                if (GAMP != CurrentGAMP && GAMP.PlaceHolder == CurrentGAMP.PlaceHolder)
                {
                    CurrentGAMP.PlaceHolder = PlaceholderBeforeEdit;
                    System.Windows.MessageBox.Show("Please specify unique value", "Optional Value Already Exists", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);
                    return true;
                }
            }
            return false;
        }

        private void AddGlobalParam(object sender, RoutedEventArgs e)
        {
            GlobalAppModelParameter newModelGlobalParam = new GlobalAppModelParameter();
            SetUniquePlaceHolderName(newModelGlobalParam);

            string newParamPlaceholder = newModelGlobalParam.PlaceHolder;

            while(InputBoxWindow.GetInputWithValidation("Add Model Global Parameter", "Parameter Name:", ref newParamPlaceholder, System.IO.Path.GetInvalidPathChars()))
            {
                newModelGlobalParam.PlaceHolder = newParamPlaceholder;
                if (!IsParamPlaceholderNameConflict(newModelGlobalParam))
                {
                    newModelGlobalParam.OptionalValuesList.Add(new OptionalValue() { Value = GlobalAppModelParameter.CURRENT_VALUE, IsDefault = true });
                    WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newModelGlobalParam);
                    break;
                }
            }
        }

        private void BtnCopyGlobalParamsClicked(object sender, RoutedEventArgs e)
        {
            GlobalParamsCopiedItemsList.Clear();
            foreach (GlobalAppModelParameter param in xModelsGlobalParamsGrid.Grid.SelectedItems)
                GlobalParamsCopiedItemsList.Add(param);
        }

        private void BtnPastGlobalParamsClicked(object sender, RoutedEventArgs e)
        {
            foreach (GlobalAppModelParameter param in GlobalParamsCopiedItemsList)
            {

                GlobalAppModelParameter newCopyGlobalParam = (GlobalAppModelParameter)param.CreateCopy();
                SetUniquePlaceHolderName(newCopyGlobalParam, true);
                WorkSpace.Instance.SolutionRepository.AddRepositoryItem(newCopyGlobalParam);
            }
        }

        public void SetUniquePlaceHolderName(GlobalAppModelParameter newModelGlobalParam, bool isCopy = false)
        {
            if (isCopy)
                newModelGlobalParam.PlaceHolder = newModelGlobalParam.PlaceHolder + "_Copy";
            else
                newModelGlobalParam.PlaceHolder = "{NewGlobalParameter}";

            if (mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder).FirstOrDefault() == null) return;

            List<GlobalAppModelParameter> samePlaceHolderList = mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder).ToList<GlobalAppModelParameter>();
            if (samePlaceHolderList.Count == 1 && samePlaceHolderList[0] == newModelGlobalParam) return; //Same internal object

            //Set unique name
            if (isCopy)
            {
                if ((mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder).FirstOrDefault()) != null)
                {
                    int counter = 2;
                    while ((mModelsGlobalParamsList.Where(x => x.PlaceHolder == newModelGlobalParam.PlaceHolder + counter).FirstOrDefault()) != null)
                        counter++;
                    newModelGlobalParam.PlaceHolder = newModelGlobalParam.PlaceHolder + counter;
                }
            }
            else
            {
                int counter = 2;
                while ((mModelsGlobalParamsList.Where(x => x.PlaceHolder == "{NewGlobalParameter_" + counter.ToString() + "}").FirstOrDefault()) != null)
                    counter++;
                newModelGlobalParam.PlaceHolder = "{NewGlobalParameter_" + counter.ToString() + "}";
            }
        }

        private void DeleteSelectedEvent(object sender, RoutedEventArgs e)
        {
            if (xModelsGlobalParamsGrid.Grid.SelectedItems.Count > 0)
            {
                string message = "After deletion there will be no way to restore deleted parameters.\nAre you sure that you want to delete the selected parameters?";
                if (System.Windows.MessageBox.Show(message, "Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    List<GlobalAppModelParameter> selectedItemsToDelete = new List<GlobalAppModelParameter>();
                    foreach (GlobalAppModelParameter selectedParam in xModelsGlobalParamsGrid.Grid.SelectedItems)
                        selectedItemsToDelete.Add(selectedParam);
                    foreach (GlobalAppModelParameter paramToDelete in selectedItemsToDelete)
                        DeleteGlobalParam(paramToDelete);
                }
            }
        }

        private void DeleteAllEvent(object sender, RoutedEventArgs e)
        {
            if (xModelsGlobalParamsGrid.Grid.Items.Count > 0)
            {
                string message = "After deletion there will be no way to restore deleted parameters.\nAre you sure that you want to delete All parameters?";
                if (System.Windows.MessageBox.Show(message, "Delete", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.No) == MessageBoxResult.Yes)
                    while (xModelsGlobalParamsGrid.Grid.SelectedItems.Count > 0)
                        DeleteGlobalParam((RepositoryItemBase)xModelsGlobalParamsGrid.Grid.SelectedItems[0]);
            }
        }

        private void DeleteGlobalParam(RepositoryItemBase globalParamToDelete)
        {
            WorkSpace.Instance.SolutionRepository.DeleteRepositoryItem(globalParamToDelete);
        }

        private void SaveAllGlobalParametersChanges(object sender, RoutedEventArgs e)
        {
            Save(true);
        }

        private void SaveSelectedGlobalParametersChanges(object sender, RoutedEventArgs e)
        {
            Save(false);
        }

        private void Save(bool saveAll)
        {
            int itemsSavedCount = 0;
            if (saveAll)
            {
                foreach (GlobalAppModelParameter globalParam in mModelsGlobalParamsList)
                    if (globalParam.IsDirty)
                    {
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(globalParam);
                        itemsSavedCount++;
                    }
            }
            else
            {
                foreach (GlobalAppModelParameter selectedGlobalParam in xModelsGlobalParamsGrid.Grid.SelectedItems)
                    if (selectedGlobalParam.IsDirty)
                    {
                        WorkSpace.Instance.SolutionRepository.SaveRepositoryItem(selectedGlobalParam);
                        itemsSavedCount++;
                    }
            }

            if (itemsSavedCount == 0)
            {
                if(saveAll)
                    System.Windows.MessageBox.Show("Nothing found to Save.", "Save All", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);
                else
                    System.Windows.MessageBox.Show("Nothing found to Save from the selected Global Parameters.", "Save Selected Parameters", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning, System.Windows.MessageBoxResult.OK);

            }
            else
            {
                if (saveAll)
                    System.Windows.MessageBox.Show("All Global Parameters have been saved", "Save All", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information, System.Windows.MessageBoxResult.OK);
                else
                    System.Windows.MessageBox.Show("Selected Global Parameter/s have been saved", "Save Selected", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information, System.Windows.MessageBoxResult.OK);
            }
        }

        private void OpenEditPossibleValuesPageButton_Click(object sender, RoutedEventArgs e)
        {
            GlobalAppModelParameter SelectedAMDP = (GlobalAppModelParameter)xModelsGlobalParamsGrid.CurrentItem;
            ObservableList<OptionalValue> list = SelectedAMDP.OptionalValuesList;
            ModelOptionalValuesPage MDPVP = new ModelOptionalValuesPage(SelectedAMDP);
            bool editWasDone = MDPVP.ShowAsWindow();

            if (editWasDone)
                SelectedAMDP.SaveBackup();
        }

        public List<GlobalAppModelParameter> ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Dialog)
        {
            Button selectBtn = new Button();
            selectBtn.Content = "Select";
            selectBtn.Click += new RoutedEventHandler(selectBtn_Click);
            ObservableList<Button> winButtons = new ObservableList<Button>();
            winButtons.Add(selectBtn);

            xModelsGlobalParamsGrid.ShowToolsBar = Visibility.Collapsed;
            xModelsGlobalParamsGrid.Grid.IsReadOnly = true;
            xModelsGlobalParamsGrid.Grid.MouseDoubleClick += selectBtn_Click;

            GenericWindow.LoadGenericWindow(ref mGenericWindow, null, windowStyle, "Add Global Parameter", this, winButtons, true, "Cancel", CloseWinClicked);

            return SelectedGlobalParamsFromDialogPage;
        }

        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var GAMP in xModelsGlobalParamsGrid.Grid.SelectedItems)
                SelectedGlobalParamsFromDialogPage.Add((GlobalAppModelParameter)GAMP);

            if (mGenericWindow != null)
                mGenericWindow.Close();
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            SelectedGlobalParamsFromDialogPage = null;
            mGenericWindow.Close();
        }
    }
}
