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
using Amdocs.Ginger.Common.Enums;
using Amdocs.Ginger.Repository;
using Ginger;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GingerWPF.UserControlsLib.UCTreeView
{
    public class SelectionTreeEventArgs : EventArgs
    {
        List<object> mSelectedItems;
        public List<object> SelectedItems
        {
            get { return mSelectedItems; }
            set { this.mSelectedItems = value; }
        }

        // Constructor.
        public SelectionTreeEventArgs(List<object> selectedItems)
        {
            this.mSelectedItems = selectedItems;
        }       
    }

    public delegate void SelectionTreeEventHandler(object sender, SelectionTreeEventArgs e);

    /// <summary>
    /// Interaction logic for SingleItemTreeViewSelectionPage.xaml
    /// </summary>
    public partial class SingleItemTreeViewSelectionPage : Page
    {
        public enum eItemSelectionType
        {
            Single, Multi, MultiStayOpenOnDoubleClick
        }

        eItemSelectionType mItemSelectionType;
        GenericWindow mPageGenericWin = null;
        List<object> mSelectedItems = null;
        string mitemTypeName;
        public event SelectionTreeEventHandler SelectionDone;
        protected virtual void OnSelectionDone(SelectionTreeEventArgs e)
        {
            if (SelectionDone != null)
                SelectionDone(this, e);
        }

        public TreeView1 TreeView
        {
            get { return xTreeView; }
        }

        public SingleItemTreeViewSelectionPage(string itemTypeName, eImageType itemTypeIcon, ITreeViewItem itemTypeRootNode, eItemSelectionType itemSelectionType = eItemSelectionType.Single, bool allowTreeTools = false)
        {
            InitializeComponent();

            xTreeView.AllowTreeTools = allowTreeTools;

            TreeViewItem r = xTreeView.Tree.AddItem(itemTypeRootNode);
            r.IsExpanded = true;

            xTreeView.Tree.ItemDoubleClick += Tree_ItemDoubleClick;

            mitemTypeName = itemTypeName;
            xTreeView.TreeTitle = itemTypeName;
            xTreeView.TreeIcon = itemTypeIcon;

            mItemSelectionType = itemSelectionType;
            if (mItemSelectionType == eItemSelectionType.MultiStayOpenOnDoubleClick)
                xTipLabel.Visibility = Visibility.Visible;
            else
                xTipLabel.Visibility = Visibility.Collapsed;
        }

        public List<object> ShowAsWindow(string windowTitle="", eWindowShowStyle windowStyle = eWindowShowStyle.Dialog, bool startupLocationWithOffset = false)
        {
            ObservableList<Button> winButtons = new ObservableList<Button>();

            Button selectBtn = new Button();
            selectBtn.Content = "Select";
            selectBtn.Click += new RoutedEventHandler(selectBtn_Click);
            winButtons.Add(selectBtn);

            if (windowTitle == string.Empty)            
                windowTitle = mitemTypeName + " Selection";
            GenericWindow.LoadGenericWindow(ref mPageGenericWin, App.MainWindow, windowStyle, windowTitle, this, winButtons, true, "Close", CloseWinClicked, startupLocationWithOffset: startupLocationWithOffset);

            return mSelectedItems;
        }

        private void CloseWinClicked(object sender, EventArgs e)
        {
            mSelectedItems = null;
            mPageGenericWin.Close();
        }

        private bool SelectCurrentItem()
        {
            ITreeViewItem itvItem = xTreeView.Tree.CurrentSelectedTreeViewItem;

            if (itvItem != null)
            {
                mSelectedItems = new List<object>();
                if (itvItem.IsExpandable())
                {
                    if (mItemSelectionType == eItemSelectionType.Single)
                    {
                        MessageBox.Show("Please select single node item (not a folder).", "Item Selection", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                        return false;
                    }

                    //get all childerans objects of direct and sub folders
                    foreach (ITreeViewItem subItvItem in xTreeView.Tree.GetTreeNodeChildsIncludingSubChilds(itvItem))                      
                        if (subItvItem.NodeObject() != null && subItvItem.NodeObject().GetType().BaseType != typeof(RepositoryFolderBase))
                            mSelectedItems.Add(subItvItem.NodeObject());
                }
                else
                {
                    mSelectedItems.Add(itvItem.NodeObject());
                }
            }


            if (mSelectedItems == null || mSelectedItems.Count == 0)
            {
                //TODO: Fix with New Reporter (on GingerWPF)
                MessageBox.Show("No item was selected.", "Item Selection", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                return false;
            }

            return true;
        }

        private void selectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (SelectCurrentItem())
            {
                if (mPageGenericWin != null)
                {
                    mPageGenericWin.Close();
                }
            }
        }

        private void Tree_ItemDoubleClick(object sender, EventArgs e)
        {
            if (SelectCurrentItem())
            {
                if (mItemSelectionType == eItemSelectionType.MultiStayOpenOnDoubleClick)
                {
                    OnSelectionDone(new SelectionTreeEventArgs(mSelectedItems));
                    mSelectedItems.Clear();
                }
                else if (mPageGenericWin != null)
                {
                    mPageGenericWin.Close();
                }
            }
        }
    }
}