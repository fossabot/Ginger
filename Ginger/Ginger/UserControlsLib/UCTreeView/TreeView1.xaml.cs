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

using Amdocs.Ginger.Common.Enums;
using GingerCoreNET.SolutionRepositoryLib;
using GingerWPF.TreeViewItemsLib;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.UserControlsLib.UCTreeView
{
    /// <summary>
    /// Interaction logic for TreeView1.xaml
    /// </summary>
    public partial class TreeView1 : UserControl, ITreeView
    {
        public UCTreeView Tree => xTreeViewTree;

        public string TreeTitle
        {
            get { return xTreeTitle.Content.ToString(); }
            set { xTreeTitle.Content = value; }
        }

        public eImageType TreeIcon
        {
            get { return xTreeIcon.ImageType; }
            set { xTreeIcon.ImageType = value; }
        }
        
        public object TreeTooltip
        {
            get { return xTreeViewTree.ToolTip; }
            set { xTreeViewTree.ToolTip = value; xTreeTitle.ToolTip = value; }
        }

        public Style TreeTitleStyle
        {
            get { return xTreeTitle.Style; }
            set { xTreeTitle.Style = value; }
        }
        
        bool mAllowTreeTools = true;
        public bool AllowTreeTools { get { return mAllowTreeTools; } set { mAllowTreeTools = value; } }
        
        public TreeView1()
        {
            InitializeComponent();

            //Tree Style
            xTreeViewTree.Tree.BorderThickness = new Thickness(0);            

            xTreeViewTree.ItemSelected += xTreeViewTree_ItemSelected;
        }

        public void SetTopToolBarTools(RoutedEventHandler saveAllHandler=null, RoutedEventHandler addHandler = null)
        {
            if (saveAllHandler != null)
            {
                xSaveAllButton.Visibility = Visibility.Visible;
                xSaveAllButton.Click += saveAllHandler;
            }
            else
            {
                xSaveAllButton.Visibility = Visibility.Collapsed;
            }

            if (addHandler != null)
            {
                xAddButton.Visibility = Visibility.Visible;
                xAddButton.Click += addHandler;
            }
            else
            {
                xAddButton.Visibility = Visibility.Collapsed;
            }
        }

        private void xTreeViewTree_ItemSelected(object sender, EventArgs e)
        {
            if (xTreeViewTree.CurrentSelectedTreeViewItem != null & AllowTreeTools == true)
            {
                xTreeViewTree.CurrentSelectedTreeViewItem.SetTools(this);

                //mark as dirty                
                if (xTreeViewTree.CurrentSelectedTreeViewItem is TreeViewItemGenericBase)
                {
                    TreeViewItemGenericBase itemAsBase = (TreeViewItemGenericBase)xTreeViewTree.CurrentSelectedTreeViewItem;
                    itemAsBase.SaveBackup(xTreeViewTree.CurrentSelectedTreeViewItem.NodeObject());
                }
            }
        }

        private void xSearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (xSearchTextBox.Text.Length > 0)
            {
                xSearchNullText.Visibility= Visibility.Collapsed;
                xSearchClearBtn.Visibility = Visibility.Visible;
            }
            else
            {
                xSearchNullText.Visibility = Visibility.Visible;
                xSearchClearBtn.Visibility = Visibility.Collapsed;
            }

            string txt = xSearchTextBox.Text;
            xTreeViewTree.FilterItemsByText(xTreeViewTree.TreeItemsCollection, txt);
        }

        private void xSearchClearBtn_Click(object sender, RoutedEventArgs e)
        {
            xSearchTextBox.Text = "";
        }

        public void SearchTree(string txt)
        {
            xSearchTextBox.Text = txt;
        }

        private void xGroupBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        public void AddToolbarTool(string toolImage, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = Visibility.Visible, object CommandParameter = null)
        {
           //no tool bar to add to in this View type
        }
        public void AddToolbarTool(eImageType imageType, string toolTip = "", RoutedEventHandler clickHandler = null, Visibility toolVisibility = System.Windows.Visibility.Visible, object CommandParameter = null)
        {
            //no tool bar to add to in this View type
        }
    }
}
