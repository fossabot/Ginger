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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ginger.Environments;
using Ginger.Run;
using Ginger.UserControls;
using GingerCore;
using GingerCore.Actions;
using GingerCore.Helpers;
using GingerCore.DataSource;
using Ginger.Actions;
using Ginger.BusinessFlowWindows;

namespace Ginger.AnalyzerLib
{
    /// <summary>
    /// Interaction logic for AnalyzerPage.xaml
    /// </summary>
    public partial class AnalyzerPage : Page
    {
        private enum AnalyzedObject
        {
            Solution,
            BusinessFlow,
            RunSetConfig
        }

        private AnalyzedObject mAnalyzedObject;

        GenericWindow _pageGenericWin = null;

        ObservableList<AnalyzerItemBase> mIssues = new ObservableList<AnalyzerItemBase>();

        private Solution mSolution;
        private BusinessFlow businessFlow;
        private RunSetConfig mRunSetConfig;
        ObservableList<DataSourceBase> DSList;

        public bool BusyInProcess = false;

        private bool mAnalyzerCompleted;

        public bool IsAnalyzeDone
        {
            get { return mAnalyzerCompleted; }
        }

        public int TotalHighAndCriticalIssues
        {
            get { return (mIssues.Where(x => (x.Severity.ToString() == "High")).Count() + mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count()); }
        }

        private bool mAnalyzeDoneOnce = false;
        private bool mAnalyzeWithUI = true;

        public AnalyzerPage()
        {
            InitializeComponent();

            SetAnalyzerItemsGridView();
           
            AnalyzerItemsGrid.DataSourceList = mIssues;           
        }

        public void Init(Solution Solution)
        {
            mAnalyzedObject = AnalyzedObject.Solution;
            mSolution = Solution;
            AnalyzerItemsGrid.Title = "'" + mSolution.Name + "' Solution Issues";
        }

        public void Init(Solution Solution, BusinessFlow BusinessFlow)
        {
            mAnalyzedObject = AnalyzedObject.BusinessFlow;
            mSolution = Solution;
            businessFlow = BusinessFlow;
            AnalyzerItemsGrid.Title = "'" + BusinessFlow.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Issues";
        }

        internal void Init(Solution solution, Run.RunSetConfig RSC)
        {
            mRunSetConfig = RSC;
            mSolution = solution;
            mAnalyzedObject = AnalyzedObject.RunSetConfig;
            AnalyzerItemsGrid.Title = "'" + RSC.Name + "' " + GingerDicser.GetTermResValue(eTermResKey.RunSet) + " Issues";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (mAnalyzeDoneOnce == false)
                Analyze();
        }

        private void RerunButton_Click(object sender, RoutedEventArgs e)
        {
            if (BusyInProcess)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Please wait for current process to end.");
                return;
            }

            mIssues.Clear();
            CriticalAndHighIssuesLabelCounter.Content = "0";
            CriticalAndHighIssuesLabelCounter.Foreground = (SolidColorBrush)new BrushConverter().ConvertFromString("#20334f");   //"#20334f";
            CanAutoFixLableCounter.Content = "0";
            Analyze();
        }

        public void AnalyzeWithoutUI()
        {
            mAnalyzeWithUI = false;
            Analyze();
        }

        private void Analyze()
        {           
            // Each anlyzer will set to true once completed, this is prep for multi run in threads for speed
            BusyInProcess = true;
            mAnalyzerCompleted = false;
            mAnalyzeDoneOnce = true;
            try
            { 
                if (mAnalyzeWithUI)
                {
                    SetStatus("Analyzing Started");
                    StatusLabel.Visibility = Visibility.Visible;
                }
                switch (mAnalyzedObject)
                {
                    case AnalyzedObject.Solution:
                        RunSolutionAnalyzer();
                        break;
                    case AnalyzedObject.BusinessFlow:
                        Task t = Task.Factory.StartNew(() =>
                        {
                            RunBusinessFlowAnalyzer(businessFlow, true);
                        });
                        break;
                    case AnalyzedObject.RunSetConfig:
                        RunRunSetConfigAnalyzer(mRunSetConfig);
                        break;
                }
            }
            finally
            {
                BusyInProcess = false;
                mAnalyzerCompleted = true;
            }
        }

        private void SetAnalayzeProceesAsCompleted()
        {
            if (mAnalyzeWithUI)
            {
                SetStatus("Analyzer Completed");
                SetUIAfterAnalyzerCompleted();
            }

            mAnalyzerCompleted = true;
            mAnalyzeWithUI = true;
            BusyInProcess = false;
        }

        private void SetUIAfterAnalyzerCompleted()
        {
            try {
            Dispatcher.Invoke(() =>
            {
                StatusLabel.Visibility = Visibility.Collapsed;
            });

                if (mIssues.Count > 0)
                {

                    //sort- placing Critical & High on top
                    
                    Dispatcher.Invoke(() =>
                    {
                        ObservableList<AnalyzerItemBase> SortedList = new ObservableList<AnalyzerItemBase>();

                        foreach (AnalyzerItemBase issue in mIssues.OrderBy(nameof(AnalyzerItemBase.Severity)))
                            SortedList.Add(issue);
                        mIssues.ClearAll();
                        mIssues = SortedList;
                        AnalyzerItemsGrid.DataSourceList = mIssues;
                        AnalyzerItemsGrid.Grid.SelectedItem = mIssues[0];
                    });
                }
            }
            catch(Exception ex)
            {
                Reporter.ToLog(eLogLevel.ERROR, $"Method - {MethodBase.GetCurrentMethod().Name}, Error - {ex.Message}");
            }
        }

        private void RunRunSetConfigAnalyzer(RunSetConfig mRunSetConfig)
        {
            Task t = Task.Factory.StartNew(() =>
            {
                List<AnalyzerItemBase> issues = RunSetConfigAnalyzer.Analyze(mRunSetConfig);
                AddIssues(issues);
                //TODO: check agents is not dup in different GR


                // Check all GRs BFS
                foreach (GingerRunner GR in mRunSetConfig.GingerRunners)
                {
                    issues = AnalyzeGingerRunner.Analyze(GR, App.UserProfile.Solution.ApplicationPlatforms);
                    AddIssues(issues);

                    //Code to analyze Runner Unique Busines flow with Source BF
                    List<Guid> checkedGuidList = new List<Guid>();
                    foreach (BusinessFlow BF in GR.BusinessFlows)
                    {
                        if (!checkedGuidList.Contains(BF.Guid))//check if it already was analyzed
                        {
                            checkedGuidList.Add(BF.Guid);
                            BusinessFlow actualBf = App.LocalRepository.GetSolutionBusinessFlows().Where(x => x.Guid == BF.Guid).FirstOrDefault();
                            if (actualBf != null)
                                RunBusinessFlowAnalyzer(actualBf, false);
                        }
                    }
                    //Code to analyze Runner BF i.e. BFFlowControls
                    foreach (BusinessFlow BF in GR.BusinessFlows)
                    {
                        List<AnalyzerItemBase> fcIssues = AnalyzeRunnerBusinessFlow.Analyze(GR, BF);
                        AddIssues(fcIssues);
                    }
                }

                SetAnalayzeProceesAsCompleted();
            });
        }

        private void RunBusinessFlowAnalyzer(BusinessFlow businessFlow, bool markCompletion=true)
        {
                DSList = Ginger.App.LocalRepository.GetSolutionDataSources();
                SetStatus("Analyzing " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: ":  ") + businessFlow.Name);
                List<AnalyzerItemBase> issues = AnalyzeBusinessFlow.Analyze(mSolution, businessFlow);
                AddIssues(issues);
                foreach (Activity activitiy in businessFlow.Activities)
                {
                    issues = AnalyzeActivity.Analyze(businessFlow, activitiy);
                    AddIssues(issues);
                    foreach (Act action in activitiy.Acts)
                    {
                        List<AnalyzerItemBase> actionissues = AnalyzeAction.Analyze(businessFlow, activitiy, action, DSList);
                        AddIssues(actionissues);
                    }
                }

                if (markCompletion)
                    SetAnalayzeProceesAsCompleted();
        }

        private void SetStatus(string txt)
        {
            // GingerCore.General.DoEvents();

            StatusLabel.Dispatcher.Invoke(
               System.Windows.Threading.DispatcherPriority.Normal,
                   new Action(
                       delegate ()
                       {
                           StatusLabel.Content = txt;
                       }
           ));

        }


        private void RunSolutionAnalyzer()
        {
            mIssues.Clear();

            //TODO: once this analyzer is taking long time due to many checks, run it using parallel
            //ObservableList<BusinessFlow> BFs = App.LocalRepository.GetAllBusinessFlows();
            ObservableList<BusinessFlow> BFs = App.LocalRepository.GetSolutionBusinessFlows();

            // Run it in another task so UI gets updates
            Task t = Task.Factory.StartNew(() =>
            {

                List<AnalyzerItemBase> issues = AnalyzeSolution.Analyze(mSolution);
                AddIssues(issues);

                foreach (BusinessFlow BF in BFs)
                {
                    RunBusinessFlowAnalyzer(BF, false);
                }

                SetAnalayzeProceesAsCompleted();
            });            
        }


        void AddIssues(List<AnalyzerItemBase> issues)
        {

            StatusLabel.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                        delegate ()
                        {
                        foreach (AnalyzerItemBase AIB in issues)
                            {
                                mIssues.Add(AIB);
                                IssuesCounterLabel.Content = "Total Issues: ";
                                IssuesCountLabel.Content = mIssues.Count();
                                if ((mIssues.Where(x => (x.Severity.ToString() == "High")).Count()) > 0 || (mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count()) > 0)
                                {
                                    CriticalAndHighIssuesLabel.Content = "Total High & Critical Issues: ";
                                    CriticalAndHighIssuesLabelCounter.Content = (mIssues.Where(x => (x.Severity.ToString() == "High")).Count() + mIssues.Where(x => (x.Severity.ToString() == "Critical")).Count());
                                    CriticalAndHighIssuesLabelCounter.Foreground = new SolidColorBrush(Colors.Red);
                                    CriticalAndHighIssuesLabel.Visibility = Visibility.Visible;
                                }
                                if ((mIssues.Where(x => (x.CanAutoFix.ToString() == "Yes")).Count()) > 0)
                                {
                                    CanAutoFixLable.Content = "Can Auto Fix: ";
                                    CanAutoFixLableCounter.Content = mIssues.Where(x => (x.CanAutoFix.ToString() == "Yes")).Count();
                                    CanAutoFixLable.Visibility = Visibility.Visible;
                                }

                            }

                        }
            ));
        }

        private void SetAnalyzerItemsGridView()
        {
            //# Default View

            GridViewDef view = new GridViewDef(GridViewDef.DefaultViewName);
            view.GridColsView = new ObservableList<GridColView>();

            //TODO: add image
            // view.GridColsView.Add(new GridColView() { Field = "Image", Header = " ", BindImageCol = "Image", WidthWeight = 2.5, MaxWidth = 20 });
            //view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.Selected, Header = "Selected", WidthWeight = 2, StyleType = GridColView.eGridColStyleType.CheckBox });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.SeverityIcon, Header = " ", StyleType = GridColView.eGridColStyleType.Image, WidthWeight = 2.5, AllowSorting = true, MaxWidth = 20 });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.Selected, Header = "Selected", WidthWeight = 3, MaxWidth = 50, StyleType = GridColView.eGridColStyleType.Template, CellTemplate = (DataTemplate)this.AnalyzerItems.Resources["FieldActive"] });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.ItemClass, Header = "Item Type", WidthWeight = 3 ,AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.ItemName, Header = "Item Name", WidthWeight = 10, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.ItemParent, Header= "Item Parent", WidthWeight = 10, AllowSorting = true });                       
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.Description, Header = "Issue", WidthWeight = 15, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.Severity, Header = "Severity", WidthWeight = 3, MaxWidth = 50, AllowSorting = true });
            //view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.Details, Header = "Details", WidthWeight = 10 });
            //view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.Impact, Header = "Impact", WidthWeight = 5 });
            // view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.HowToFix, Header = "How To Fix", WidthWeight = 10, AllowSorting=true });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.CanAutoFix, Header = "Can Auto Fix", WidthWeight = 3, AllowSorting = true });
            view.GridColsView.Add(new GridColView() { Field = AnalyzerItemBase.Fields.Status, Header = "Status", WidthWeight = 3, AllowSorting = true });

            AnalyzerItemsGrid.AddToolbarTool("@UnCheckAllColumn_16x16.png", "Select/UnSelect all issues which can be auto fixed", new RoutedEventHandler(MarkUnMark));

            AnalyzerItemsGrid.RowDoubleClick += AnalyzerItemsGrid_RowDoubleClick;
            AnalyzerItemsGrid.SetAllColumnsDefaultView(view);
            AnalyzerItemsGrid.InitViewItems();
            AnalyzerItemsGrid.SetTitleLightStyle = true;
        }

        public void ShowAsWindow(eWindowShowStyle windowStyle = eWindowShowStyle.Free)
        {
            string title = string.Empty;
            switch (mAnalyzedObject)
            {
                case AnalyzedObject.BusinessFlow:
                    title = GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " Analyzer Results";
                    break;
                case AnalyzedObject.RunSetConfig:
                    title = GingerDicser.GetTermResValue(eTermResKey.RunSet) + " Analyzer Results";
                    break;
                case AnalyzedObject.Solution:
                    title = "Solution Analyzer Results";
                    break;
            }
            ObservableList<Button> winButtons = new ObservableList<Button>();

            Button SaveAllButton = new Button();
            SaveAllButton.Content = "Save Fixed Items";
            SaveAllButton.Margin = new Thickness(0, 0, 60, 0);
            //SaveAllButton.Click += new RoutedEventHandler(SaveAllButton_Click);
            SaveAllButton.Click += new RoutedEventHandler(SaveAllFixedItems_Click);
            winButtons.Add(SaveAllButton);

            Button FixSelectedButton = new Button();
            FixSelectedButton.Content = "Fix Selected";
            //FixSelectedButton.Click += new RoutedEventHandler(FixSelectedButton_Click);
            FixSelectedButton.Click += async (o, e) =>
            {
                if (mIssues.Where(x=> x.Selected==true).ToList().Count == 0)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticWarnMessage, "Please select issue to fix.");
                    return;
                }

                if (BusyInProcess)
                {
                    Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Please wait for current process to end.");
                    return;
                }
                BusyInProcess = true;
                SetStatus("Starting to Fix Selected Items...");
                StatusLabel.Visibility = Visibility.Visible;
                try
                { 
                    await Task.Run(() => FixSelectedItems());
                }
                finally
                {
                    BusyInProcess = false;
                }
                SetStatus("Finished to Fix Selected Items.");
                StatusLabel.Visibility = Visibility.Collapsed;               
            };
            winButtons.Add(FixSelectedButton);

            Button RerunButton = new Button();
            RerunButton.Content = "Re-Analyze";
            RerunButton.Margin = new Thickness(0, 0, 60, 0);
            RerunButton.Click += new RoutedEventHandler(RerunButton_Click);
            winButtons.Add(RerunButton);

            GingerCore.General.LoadGenericWindow(ref _pageGenericWin, App.MainWindow, windowStyle, title, this, winButtons);

            if (mAnalyzeDoneOnce)
                SetUIAfterAnalyzerCompleted();
        }

        private void MarkUnMark(object sender, RoutedEventArgs e)
        {
                List<AnalyzerItemBase> autoFixItemsList = mIssues.Where(x => x.CanAutoFix == AnalyzerItemBase.eCanFix.Yes).ToList();
                if (autoFixItemsList!= null && autoFixItemsList.Count>0)
                {
                    bool flagToset = (!autoFixItemsList[0].Selected);
                    foreach (AnalyzerItemBase item in autoFixItemsList)
                    {
                        item.Selected = flagToset;
                    }
                }
        }

        private async void SaveAllFixedItems_Click(object sender, RoutedEventArgs e)
        {
            if (BusyInProcess)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "Please wait for current process to end.");
                return;
            }
            // TODO: click/use the same code on solution which will save all changed items...
            // Meanwhile the below is good start 
            if (Reporter.ToUser(eUserMsgKeys.SaveAnalyzerItemWarning) == MessageBoxResult.Yes)
            {
                BusyInProcess = true;
                SetStatus("Starting to Save Fixed Items...");
                StatusLabel.Visibility = Visibility.Visible;
                try
                { 
                    await Task.Run(() => SaveAllFixedItems());
                }
                finally
                {
                    BusyInProcess = false;
                }
                SetStatus("Finished to Save Selected Items.");
                StatusLabel.Visibility = Visibility.Collapsed;                
            }
        }

        private void SaveAllFixedItems()
        {
            Dictionary<BusinessFlow, List<AnalyzerItemBase>> itemsWhichWereSaved = new Dictionary<BusinessFlow, List<AnalyzerItemBase>>();
            foreach (AnalyzerItemBase AI in mIssues)
            {
                if (AI.Status == AnalyzerItemBase.eStatus.Fixed)
                {
                    BusinessFlow bs = null;
                    if (AI.GetType() == typeof(AnalyzeBusinessFlow))
                    {
                        bs = ((AnalyzeBusinessFlow)AI).mBusinessFlow;
                    }
                    else if (AI.GetType() == typeof(AnalyzeActivity))
                    {
                        bs = ((AnalyzeActivity)AI).mBusinessFlow;
                    }
                    else if (AI.GetType() == typeof(AnalyzeAction))
                    {
                        bs = ((AnalyzeAction)AI).mBusinessFlow;
                    }
                    //TODO: add support for Run Set save

                    //using Dic so each BF will be saved only once
                    if (itemsWhichWereSaved.ContainsKey(bs) == false)
                        itemsWhichWereSaved.Add(bs, new List<AnalyzerItemBase>() { AI });
                    else
                        itemsWhichWereSaved[bs].Add(AI);
                }
            }

            //do Bf's Save
            foreach (KeyValuePair<BusinessFlow, List<AnalyzerItemBase>> bfToSave in itemsWhichWereSaved)
            {
                if (bfToSave.Key != null)
                {
                    SetStatus("Saving " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow, suffixString: ":")  + bfToSave.Key.Name);
                    bfToSave.Key.Save();
                    foreach (AnalyzerItemBase ai in bfToSave.Value)
                        ai.Status = AnalyzerItemBase.eStatus.FixedSaved;
                }
            }
        }

        private void FixSelectedItems()
        {            
            foreach (AnalyzerItemBase AI in mIssues)
            {
                if (AI.Selected && AI.Status == AnalyzerItemBase.eStatus.NeedFix)
                {
                    if (AI.FixItHandler != null)
                    {
                        //Reporter.ToGingerHelper(eGingerHelperMsgKey.AnalyzerFixingIssues, null, AI.ItemName);
                        SetStatus("Fixing: " + AI.ItemName);                      
                        AI.FixItHandler.Invoke(AI, null);
                        //Reporter.CloseGingerHelper();                        
                    }
                    else
                    {
                        AI.Status = AnalyzerItemBase.eStatus.MissingFixHandler;
                    }
                }
            }
        }

        private void AnalyzerItemsGrid_RowDoubleClick(object sender, EventArgs e)
        {
            //show the item edit page 
            if (AnalyzerItemsGrid.CurrentItem is AnalyzeAction)
            {
                AnalyzeAction currentAnalyzeAction = (AnalyzeAction)AnalyzerItemsGrid.CurrentItem;
                Act actionIssue = currentAnalyzeAction.mAction;
                actionIssue.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
                ActionEditPage actedit = new ActionEditPage(actionIssue, General.RepositoryItemPageViewMode.Child, currentAnalyzeAction.mBusinessFlow, currentAnalyzeAction.mActivity);
                //setting the BusinessFlow on the Action in Order to save 
                //actedit.mActParentBusinessFlow = ((AnalyzeAction)AnalyzerItemsGrid.CurrentItem).mBusinessFlow;
                //actedit.ap = null;
                actedit.ShowAsWindow(eWindowShowStyle.Dialog);
            }

            if (AnalyzerItemsGrid.CurrentItem is AnalyzeActivity)
            {
                AnalyzeActivity currentAnalyzeActivity = (AnalyzeActivity)AnalyzerItemsGrid.CurrentItem;
                Activity ActivityIssue = currentAnalyzeActivity.mActivity;
                //ActivityIssue.SolutionFolder = App.UserProfile.Solution.Folder.ToUpper();
                ActivityEditPage ActivityEdit = new ActivityEditPage(ActivityIssue, General.RepositoryItemPageViewMode.Child, currentAnalyzeActivity.mBusinessFlow);
                //setting the BusinessFlow on the Activity in Order to save
                //ActivityEdit.mBusinessFlow = ((AnalyzeActivity)AnalyzerItemsGrid.CurrentItem).mBusinessFlow;
                //ActivityEdit.ap = null;
                ActivityEdit.ShowAsWindow(eWindowShowStyle.Dialog);

            }

        }
        private void AnalyzerItemsGrid_RowChangedEvent(object sender, EventArgs e)
        {
            txtBlkAnalyzerIssue.Text = string.Empty;
            TextBlockHelper TBH = new TextBlockHelper(txtBlkAnalyzerIssue);

            AnalyzerItemBase a = (AnalyzerItemBase)AnalyzerItemsGrid.CurrentItem;

            if (a != null)
            {                
                if (a.ItemClass != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddHeader1("Item Type :");
                    TBH.AddLineBreak();
                    TBH.AddText(a.ItemClass.ToString());
                    TBH.AddLineBreak();
                }

                if (a.ItemName != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddHeader1("Item Name :");
                    TBH.AddLineBreak();
                    TBH.AddText(a.ItemName.ToString());
                    TBH.AddLineBreak();
                }

                if (a.ItemParent != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddHeader1("Item Parent :");
                    TBH.AddLineBreak();
                    TBH.AddText(a.ItemParent.ToString());
                    TBH.AddLineBreak();
                }

                if (a.Description != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddHeader1("Issue :");
                    TBH.AddLineBreak();
                    TBH.AddText(a.Description.ToString());
                    TBH.AddLineBreak();
                }

                if (a.Details != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddHeader1("Issue Details :");
                    TBH.AddLineBreak();
                    TBH.AddText(a.Details.ToString());
                    TBH.AddLineBreak();
                }

                if (a.Impact != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("Issue Impact :");
                    TBH.AddLineBreak();
                    TBH.AddText(a.Impact.ToString());
                    TBH.AddLineBreak();
                }

                if (a.HowToFix != null)
                {
                    TBH.AddLineBreak();
                    TBH.AddBoldText("How To Fix :");
                    TBH.AddLineBreak();
                    TBH.AddText(a.HowToFix.ToString());
                    TBH.AddLineBreak();
                }

                TBH.AddLineBreak();
                TBH.AddBoldText("Can Auto Fix :");
                TBH.AddLineBreak();
                TBH.AddText(a.CanAutoFix.ToString());
                TBH.AddLineBreak();
            }
        }
    }

}
