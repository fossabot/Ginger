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

using Ginger.Agents;
using GingerWPF.UserControlsLib.UCTreeView;
using GingerCore;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using GingerWPF.TreeViewItemsLib;
using Amdocs.Ginger.UserControls;
using amdocs.ginger.GingerCoreNET;
using Amdocs.Ginger.Common.Enums;

namespace Ginger.SolutionWindows.TreeViewItems
{
    class AgentTreeItem : NewTreeViewItemBase, ITreeViewItem
    {
        private AgentEditPage mAgentEditPage;
        public Agent Agent { get; set; }        

        Object ITreeViewItem.NodeObject()
        {
            return Agent;
        }
        override public string NodePath()
        {
            return Agent.FileName;
        }
        override public Type NodeObjectType()
        {
            return typeof(Agent);
        }

        StackPanel ITreeViewItem.Header()
        {            
            // TODO: have eimagetype on RIBase
            return NewTVItemStyle(Agent, eImageType.Agent, nameof(Agent.Name));
        }
       
        List<ITreeViewItem> ITreeViewItem.Childrens()
        {
            return null;
        }

        bool ITreeViewItem.IsExpandable()
        {
            return false;
        }

        Page ITreeViewItem.EditPage()
        {
            if (mAgentEditPage == null)
            {
                mAgentEditPage = new AgentEditPage(Agent);
            }
            return mAgentEditPage;
        }

        ContextMenu ITreeViewItem.Menu()
        {
            return mContextMenu;
        }

        private void Rename(object sender, RoutedEventArgs e)
        {
            RenameItem("Agent Name:", Agent, Agent.Fields.Name);
        }

        void ITreeViewItem.SetTools(ITreeView TV)
        {
            mTreeView = TV;
            mContextMenu = new ContextMenu();

            AddItemNodeBasicManipulationsOptions(mContextMenu);
            AddSourceControlOptions(mContextMenu);
        }

        public override void PostSaveTreeItemHandler()
        {
            App.UpdateApplicationsAgentsMapping();
        }

        public override void PostDeleteTreeItemHandler()
        {
            App.UpdateApplicationsAgentsMapping();
        }
    }
}