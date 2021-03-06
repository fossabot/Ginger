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
using Ginger.Repository;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using GingerCore.GeneralLib;
using GingerCore.SourceControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class SharedActivitiesFolderTreeItem : TreeViewItemBase, ITreeViewItem
    {
        private ActivitiesRepositoryPage mActivitiesRepositoryPage;
        public string Folder { get; set; }
        public string Path { get; set; }
        public enum eActivitiesItemsShowMode { ReadWrite, ReadOnly }
        private eActivitiesItemsShowMode mShowMode;

        public SharedActivitiesFolderTreeItem(eActivitiesItemsShowMode showMode = eActivitiesItemsShowMode.ReadWrite)
        {
            mShowMode = showMode;
        }

        Object ITreeViewItem.NodeObject()
        {
            return null;
        }
        override public string NodePath()
        {
            return Path + @"\";
        }
        override public Type NodeObjectType()
        {
            return typeof(Activity);
        }

        StackPanel ITreeViewItem.Header()
        {
            string ImageFile;
            if (IsGingerDefualtFolder)
            {
                ImageFile = "@Activities_16x16.png";
            }
            else
            {
                ImageFile = "@Folder2_16x16.png";
            }

            return TreeViewUtils.CreateItemHeader(Folder, ImageFile, Ginger.SourceControl.SourceControlIntegration.GetItemSourceControlImage(Path, ref ItemSourceControlStatus));
        }

        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            List<ITreeViewItem> Childrens = new List<ITreeViewItem>();

            AddsubFolders(Path, Childrens);

            //Add Activities
            ObservableList<Activity> Activities = App.LocalRepository.GetSolutionRepoActivities(specificFolderPath: Path);
            foreach (Activity activity in Activities)
            {
                SharedActivityTreeItem SATI = new SharedActivityTreeItem(mShowMode);
                SATI.Activity = activity;
                Childrens.Add(SATI);
            }
            return Childrens;
        }

        private void AddsubFolders(string sDir, List<ITreeViewItem> Childrens)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(Path))
                {
                    SharedActivitiesFolderTreeItem FolderItem = new SharedActivitiesFolderTreeItem(mShowMode);
                    string FolderName = System.IO.Path.GetFileName(d);

                    FolderItem.Folder = FolderName;
                    FolderItem.Path = d;

                    Childrens.Add(FolderItem);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }
        bool ITreeViewItem.IsExpandable()
        {
            return true;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mActivitiesRepositoryPage == null)
            {
                mActivitiesRepositoryPage = new ActivitiesRepositoryPage(Path);
            }
            return mActivitiesRepositoryPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            if (mShowMode == eActivitiesItemsShowMode.ReadWrite)
            {
                if (IsGingerDefualtFolder)
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Activity), allowAddNew: false, allowRenameFolder: false, allowDeleteFolder: false);
                else
                    AddFolderNodeBasicManipulationsOptions(mContextMenu, nodeItemTypeName: GingerDicser.GetTermResValue(eTermResKey.Activity), allowAddNew: false);
                
                AddSourceControlOptions(mContextMenu, false, false);
            }
            else
            {
                AddFolderNodeBasicManipulationsOptions(mContextMenu, GingerDicser.GetTermResValue(eTermResKey.ActivitiesGroup), true, false, false, false, false, false, false, false, false,false);
            }
        }               
    }
}
