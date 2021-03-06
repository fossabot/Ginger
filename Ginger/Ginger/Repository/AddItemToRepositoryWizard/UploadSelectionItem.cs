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
using System.ComponentModel;
using GingerCore;

namespace Ginger.Repository.ItemToRepositoryWizard
{
    public class UploadItemSelection : RepositoryItem, INotifyPropertyChanged
    {
        public static new class Fields
        {
            public static string Selected = "Selected";
            public static string ItemGUID = "ItemGUID";
            public static string ItemName = "ItemName";
            public static string ExistingItemName = "ExistingItemName";
            public static string ItemUploadType = "ItemUploadType";
            public static string UploadTypeList = "UploadTypeList";
            public static string PartToUpload = "PartToUpload";
            public static string Comment = "Comment";
            public static string SelectedItemPart = "SelectedItemPart";
            public static string IsOverrite = "IsOverrite";
            public static string IsExistingItemParent = "IsExistingItemParent";
            public static string ItemUploadStatus = "ItemUploadStatus"; 
        }

        public enum eItemUploadStatus
        {
            Uploaded,
            FailedToUpload,
            Skipped
        }

        public eItemUploadStatus ItemUploadStatus { get; set; }

        public static ObservableList<UploadItemSelection> mSelectedItems = new ObservableList<UploadItemSelection>();
     
        public enum eItemUploadType
        {
            New,
            Overwrite
        }
        private eItemUploadType mItemUploadType;
        public eItemUploadType ItemUploadType
        { get { return mItemUploadType; }
            set {
                if (mItemUploadType != value)
                {
                    mItemUploadType = value;
                    OnPropertyChanged(Fields.ItemUploadType);
                    OnPropertyChanged(Fields.IsOverrite);
                }
            }
        }
        public bool IsOverrite
        {
            get
            {
                if (ItemUploadType == eItemUploadType.Overwrite)
                    return true;
                else
                    return false;
            }
        }

        public bool IsExistingItemParent
        {
            get
            {
                if (ExistingItemType == eExistingItemType.ExistingItemIsParent)
                    return true;
                else
                    return false;
            }
        }

        public RepositoryItem UsageItem { get; set; }
        public RepositoryItem ExistingItem { get; set; }
    
        public enum eExistingItemType
        {
            NA,
            ExistingItemIsParent,
            ExistingItemIsExternalID,
            ExistingItemIsDuplicate,            
        }

        public eExistingItemType ExistingItemType { get; set; }

        bool mSelected;
        public bool Selected
        {
            get
            {
                return mSelected;
            }
            set
            {
                mSelected = value;
                OnPropertyChanged(Fields.Selected);
            }
        }
        
        public string Comment { get; set; }

        string mSelectedItemPart;
        public string SelectedItemPart
        {
            get
            {
                return mSelectedItemPart;
            }
            set
            {
                mSelectedItemPart = value;
                OnPropertyChanged(Fields.SelectedItemPart);
            }
        }

        Array mUploadTypeList { get; set; }

        public Array UploadTypeList
        {
            get
             {
                return Enum.GetValues(typeof(eItemUploadType));
            }
            set
            {
                mUploadTypeList = value;
                OnPropertyChanged(Fields.UploadTypeList);
            }
        }

        ObservableList<string> mPartToUpload = new ObservableList<string>();
        public ObservableList<string> PartToUpload
        {
            get
            {
                return mPartToUpload;
            }
            set
            {
                mPartToUpload = value;
                OnPropertyChanged(Fields.PartToUpload);
            }
        }

         public override string ItemName { get ; set; }

        public string ExistingItemName
        {
            get
            {
                if (ExistingItem != null)
                    return ExistingItem.ItemName;
                else
                    return "NA";
            }
            set
            {
                ExistingItemName = value;
            }
        }

        public Guid ItemGUID { get; set; }
       

        public void SetItemPartesFromEnum(Type enumType)
        {
            PartToUpload.Clear();
            if (enumType != null)
            {
                // Get all possible enum vals
                foreach (object item in Enum.GetValues(enumType))
                {
                    PartToUpload.Add(item.ToString());
                }
            }
            if (PartToUpload.Count > 0)
                SelectedItemPart = PartToUpload[0];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
