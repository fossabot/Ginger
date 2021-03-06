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
using Ginger.ALM.Rally;
using Ginger.Repository;
using GingerCore;
using GingerCore.Activities;
using GingerCore.ALM;
using GingerCore.ALM.Rally;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace Ginger.ALM.Repository
{
    /// <summary>
    /// </summary>
    class RallyRepository : ALMRepository
    {
        const string RallyID = "RQMID";

        public override void ImportALMTests(String importDestinationFolderPath) //Import Test Plans
        {
            RallyPlansExplorerPage win = new RallyPlansExplorerPage(importDestinationFolderPath);
            win.ShowAsWindow();
        }

        public override bool ShowImportReviewPage(string importDestinationFolderPath, object selectedTestPlan = null)
        {
            if (importDestinationFolderPath == "")
                importDestinationFolderPath = App.UserProfile.Solution.BusinessFlowsMainFolder;

            // get activities groups
            RallyImportReviewPage win = new RallyImportReviewPage(RallyConnect.Instance.GetRallyTestPlanFullData(App.UserProfile.Solution.ALMServerURL, App.UserProfile.ALMUserName, App.UserProfile.ALMPassword, App.UserProfile.Solution.ALMProject, (RallyTestPlan)selectedTestPlan), importDestinationFolderPath);
            win.ShowAsWindow();

            return true;
        }

        public override bool ConnectALMServer(ALMIntegration.eALMConnectType userMsgStyle)
        {
            bool isConnectSucc = false;
            Reporter.ToLog(eLogLevel.INFO, "Connecting to Rally server");
            try
            {
                isConnectSucc = ALMIntegration.Instance.AlmCore.ConnectALMServer();
            }
            catch (Exception e)
            {
                Reporter.ToLog(eLogLevel.ERROR, "Error connecting to Rally server", e);
            }

            if (!isConnectSucc)
            {
                Reporter.ToLog(eLogLevel.INFO, "Could not connect to Rally server");
                if (userMsgStyle == ALMIntegration.eALMConnectType.Manual)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailure);
                else if (userMsgStyle == ALMIntegration.eALMConnectType.Auto)
                    Reporter.ToUser(eUserMsgKeys.ALMConnectFailureWithCurrSettings);
            }

            return isConnectSucc;
        }

        public override bool ImportSelectedTests(string importDestinationPath, IEnumerable<Object> testPlanList)
        {
            if (testPlanList != null)
            {
                foreach (RallyTestPlan testPlan in testPlanList)
                {
                    //Refresh Ginger repository and allow GingerRally to use it
                    ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = App.LocalRepository.GetSolutionRepoActivitiesGroups(false);
                    ALMIntegration.Instance.AlmCore.GingerActivitiesGroupsRepo = App.LocalRepository.GetSolutionRepoActivitiesGroups(false);
                    ALMIntegration.Instance.AlmCore.GingerActivitiesRepo = App.LocalRepository.GetSolutionRepoActivities(false);

                    try
                    {
                        BusinessFlow existedBF = App.LocalRepository.GetSolutionBusinessFlows().Where(x => x.ExternalID == RallyID + "=" + testPlan.RallyID).FirstOrDefault();
                        if (existedBF != null)
                        {
                            MessageBoxResult userSelection = Reporter.ToUser(eUserMsgKeys.TestSetExists, testPlan.Name);
                            if (userSelection == MessageBoxResult.Yes)
                            {
                                File.Delete(existedBF.FileName);
                            }
                        }

                        Reporter.ToGingerHelper(eGingerHelperMsgKey.ALMTestSetImport, null, testPlan.Name);

                        // convert test set into BF
                        BusinessFlow tsBusFlow = ALMIntegration.Instance.AlmCore.ConvertRallyTestPlanToBF(testPlan);

                        if (App.UserProfile.Solution.MainApplication != null)
                        {
                            //add the applications mapped to the Activities
                            foreach (Activity activ in tsBusFlow.Activities)
                                if (string.IsNullOrEmpty(activ.TargetApplication) == false)
                                    if (tsBusFlow.TargetApplications.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault() == null)
                                    {
                                        ApplicationPlatform appAgent = App.UserProfile.Solution.ApplicationPlatforms.Where(x => x.AppName == activ.TargetApplication).FirstOrDefault();
                                        if (appAgent != null)
                                            tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = appAgent.AppName });
                                    }
                            //handle non mapped Activities
                            if (tsBusFlow.TargetApplications.Count == 0)
                                tsBusFlow.TargetApplications.Add(new TargetApplication() { AppName = App.UserProfile.Solution.MainApplication });
                            foreach (Activity activ in tsBusFlow.Activities)
                                if (string.IsNullOrEmpty(activ.TargetApplication))
                                    activ.TargetApplication = tsBusFlow.MainApplication;
                        }
                        else
                        {
                            foreach (Activity activ in tsBusFlow.Activities)
                                activ.TargetApplication = null; // no app configured on solution level
                        }

                        //save bf
                        tsBusFlow.FileName = LocalRepository.GetRepoItemFileName(tsBusFlow, importDestinationPath);
                        tsBusFlow.SaveToFile(tsBusFlow.FileName);
                        //add to cach
                        App.LocalRepository.AddItemToCache(tsBusFlow);
                        Reporter.CloseGingerHelper();
                    }
                    catch (Exception ex)
                    {
                        Reporter.ToUser(eUserMsgKeys.ErrorInTestsetImport, testPlan.Name, ex.Message);
                    }

                    //Refresh the solution tree
                    App.MainWindow.RefreshSolutionPage();

                    Reporter.ToUser(eUserMsgKeys.TestSetsImportedSuccessfully);
                }
                return true;
            }

            return false;
        }

        public override void UpdateActivitiesGroup(ref BusinessFlow businessFlow, List<Tuple<string, string>> TCsIDs)
        {
            // TODO ... 
        }

        public override void UpdateBusinessFlow(ref BusinessFlow businessFlow)
        {
            // TODO ... 
        }

        public override void ExportBfActivitiesGroupsToALM(BusinessFlow businessFlow, ObservableList<ActivitiesGroup> grdActivitiesGroups)
        {
            if (businessFlow == null) return;

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.NoItemWasSelected);
                return;
            }

            bool exportRes;

            string res = string.Empty;
            Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportItemToALM, null, "Selected Activities Groups");
            exportRes = ((RallyCore)ALMIntegration.Instance.AlmCore).ExportBfActivitiesGroupsToALM(businessFlow, grdActivitiesGroups, ref res);

            if (exportRes)
            {
                //Check if we need to perform save
                Reporter.ToUser(eUserMsgKeys.ExportItemToALMSucceed);
            }
            else
            {
                Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            Reporter.CloseGingerHelper();
        }

        public override bool ExportBusinessFlowToALM(BusinessFlow businessFlow, bool performSaveAfterExport = false, ALMIntegration.eALMConnectType almConectStyle = ALMIntegration.eALMConnectType.Manual, string testPlanUploadPath = null, string testLabUploadPath = null)
        {
            if (businessFlow == null) return false;

            if (businessFlow.ActivitiesGroups.Count == 0)
            {
                Reporter.ToUser(eUserMsgKeys.StaticInfoMessage, "The " + GingerDicser.GetTermResValue(eTermResKey.BusinessFlow) + " do not include " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroups) + " which supposed to be mapped to ALM Test Cases, please add at least one " + GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup) + " before doing export.");
                return false;
            }

            bool exportRes;
            string res = string.Empty;
            Reporter.ToGingerHelper(eGingerHelperMsgKey.ExportItemToALM, null, businessFlow.Name);

            exportRes = ((RallyCore)ALMIntegration.Instance.AlmCore).ExportBusinessFlowToRally(businessFlow, App.UserProfile.Solution.ExternalItemsFields, ref res);

            if (exportRes)
            {
                if (performSaveAfterExport)
                {
                    Reporter.ToGingerHelper(eGingerHelperMsgKey.SaveItem, null, businessFlow.Name, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow));
                    businessFlow.Save();
                }
                if(almConectStyle != ALMIntegration.eALMConnectType.Auto && almConectStyle != ALMIntegration.eALMConnectType.Silence)
                    Reporter.ToUser(eUserMsgKeys.ExportItemToALMSucceed);
            }
            else
            {
                if(almConectStyle != ALMIntegration.eALMConnectType.Silence)
                    Reporter.ToUser(eUserMsgKeys.ExportItemToALMFailed, GingerDicser.GetTermResValue(eTermResKey.BusinessFlow), businessFlow.Name, res);
            }

            Reporter.CloseGingerHelper();
            return exportRes;
        }


        public override eUserMsgKeys GetDownloadPossibleValuesMessage()
        {
            return eUserMsgKeys.AskIfToDownloadPossibleValuesShortProcesss;
        }

        public override IEnumerable<Object> SelectALMTestSets()
        {
            throw new NotImplementedException();
        }

        public override bool ExportActivitiesGroupToALM(ActivitiesGroup activtiesGroup, string uploadPath = null, bool performSaveAfterExport = false)
        {
            throw new NotImplementedException();
        }

        public override string SelectALMTestPlanPath()
        {
            return "";
        }

        public override string SelectALMTestLabPath()
        {
            return "";
        }

        public override bool LoadALMConfigurations()
        {
            throw new NotImplementedException();
        }

        public override void ImportALMTestsById(string importDestinationFolderPath)
        {
            throw new NotImplementedException();
        }
    }
}
