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
using Amdocs.Ginger.Common.Repository;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Amdocs.Ginger.Repository
{

    public class GuidMapper
    {
        public Guid Original { get; set; }
        public Guid newGuid { get; set; } // pointer to obj
    }

    public abstract class RepositoryItemBase : INotifyPropertyChanged, ISearchFilter 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public RepositoryItemHeader RepositoryItemHeader { get; set; }

        [IsSerializedForLocalRepository]
        public Guid ParentGuid { get; set; }

        [IsSerializedForLocalRepository]
        public string ExternalID { get; set; }

        public string ObjFolderName { get { return FolderName(this.GetType()); } }

        //DO Not save
        protected Dictionary<string, object> mBackupDic;

        public virtual string ObjFileExt
        {
            get
            {
                // We can override if we want differnet extension for example un sub class 
                // like APIModel - SOAP/REST we want both file name to be with same extension - ApplicationAPIModel
                return RepositorySerializer.FileExt(this.GetType());
            }
        }

        public static string FolderName(Type T)
        {
            // string s =  GetClassShortName(T) + "s";
            string s = T.Name + "s";


            if (s.EndsWith("ys"))
            {
                s = s.Replace("ys", "ies");
            }

            //Special handling for Shared Repository item to be in sub folder
            if (s == "ActivitiesGroups" || s == "Activities" || s == "Actions" || s == "Variables" || s == "Validations")
            {
                s = @"SharedRepository\" + s;
            }


            return s;
        }

        

        // TypeName cache
        private static ConcurrentDictionary<string, string> ShortNameDictionary = new ConcurrentDictionary<string, string>();

        

        public static string GetClassShortName(Type t)
        {
            string ClassName = t.FullName;

            //TODO: make it generic using RS classes dic
            // For speed and in order to to waste mem by creating everytime obj to get name we cache it

            string ShortName = null;
            ShortNameDictionary.TryGetValue(ClassName, out ShortName);
            if (ShortName == null)
            {
                RepositoryItemBase obj = (RepositoryItemBase)(t.Assembly.CreateInstance(ClassName));
                if (obj != null)
                {
                    ShortName = obj.ObjFileExt;
                    ShortNameDictionary.TryAdd(ClassName, ShortName);
                }
                else
                {
                    throw new Exception("GetClassShortName - Unable to create class - " + ClassName);
                }
            }
            return ShortName;
        }

        // TODO: temp by default all repository items use the old Serializer, unless flagged to use the new, overrride in sub class
        public virtual bool UseNewRepositorySerializer { get { return false; } }

        static NewRepositorySerializer NewRepositorySerializer2;
        static IRepositorySerializer OldRepositorySerializer;

        // TODO: temp until we move all items to new RS
        public static void InitSerializers(IRepositorySerializer oldRepositorySerializer)
        {
            OldRepositorySerializer = oldRepositorySerializer;
            NewRepositorySerializer2 = new NewRepositorySerializer();


        }

        public IRepositorySerializer RepositorySerializer
        {
            get
            {
                if (UseNewRepositorySerializer)
                {
                    return NewRepositorySerializer2;
                }
                else
                {
                    return OldRepositorySerializer;
                }
            }
        }


        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
                DirtyCheck(name);
            }
        }

        

        Guid mGuid = Guid.Empty;
        [IsSerializedForLocalRepository]
        public Guid Guid
        {
            get
            {
                if (mGuid == Guid.Empty)
                    mGuid = Guid.NewGuid();
                return mGuid;
            }
            set
            {
                mGuid = value;
            }
        }

        public bool IsDirty
        {
            get
            {
                if (mBackupDic == null)
                    return false;
                else
                    return true;
            }
        }
               
        public void SaveToFile(string FilePath)
        {
            RepositorySerializer.SaveToFile(this, FilePath);
        }

        public void SaveBackup()
        {
            if (IsDirty == false)
            {
                CreateBackup();
                OnPropertyChanged(nameof(IsDirty));
            }
        }

        // Deep backup keep obj ref and all prop, restore to real original situation
        private void CreateBackup()
        {
            mBackupDic = new Dictionary<string, object>();
            var properties = this.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                if (mi.Name == nameof(mBackupDic)) continue; // since we are running on repo item which contain the dic we need to ignore trying to save it...

                dynamic v = null;
                if (mi.MemberType == MemberTypes.Property)
                {
                    //Make sure we can do set - not all props have set, so do not save if there is only get
                    PropertyInfo PI = this.GetType().GetProperty(mi.Name);
                    if (PI.CanWrite)
                    {
                        //TODO: mark with no backup
                        //TODO: find better way, make it generic
                        if (mi.Name != nameof(FileName) && mi.Name != nameof(FilePath) && mi.Name != nameof(ObjFolderName) && mi.Name != nameof(ObjFileExt) && mi.Name != nameof(ContainingFolder) && mi.Name != nameof(ContainingFolderFullPath)) // Will cause err to get filename on each repo item
                        {
                            v = PI.GetValue(this);
                        }
                    }
                }
                else if (mi.MemberType == MemberTypes.Field)
                {
                    v = this.GetType().GetField(mi.Name).GetValue(this);
                }

                if (v is IObservableList)
                {
                    BackupList(mi.Name, v);
                }
                else
                {
                    // TODO: add List<string>
                }
                // Save it
                mBackupDic.Add(mi.Name, v);
            }
        }


        private void BackupList(string Name, IObservableList v)
        {
            //TODO: if v is Lazy bak the text without drill down
            List<dynamic> list = new List<dynamic>();
            foreach (object o in v)
            {
                // Run back on each item, so will drill down the hierarchy
                if (o is RepositoryItemBase)
                {
                    ((RepositoryItemBase)o).CreateBackup();
                }
                list.Add(o);
            }
            // we keep the original list of items in special name like: Activities~List
            mBackupDic.Add(Name + "~List", list);
        }
        
        // Item which will not be saved to the XML - for example dynamic activities or temp output values - no expected or store to
        // Only when in Observable list 
        public virtual bool IsTempItem
        {
            get
            {
                return false;
            }
        }


        public void ClearBackup()
        {
            mBackupDic = null;
            OnPropertyChanged(nameof(IsDirty));
        }

        private void RestoreBackup()
        {
            if (mBackupDic == null || mBackupDic.Count == 0)
            {
                return;
            }

            var properties = this.GetType().GetMembers().Where(x => x.MemberType == MemberTypes.Property || x.MemberType == MemberTypes.Field);
            foreach (MemberInfo mi in properties)
            {
                // Console.WriteLine(this.ToString() +  " - mi:" + mi.Name + " - " + mi.ToString());               
                if (mi.Name == nameof(mBackupDic) || mi.Name == nameof(FileName) || mi.Name == nameof(FilePath) || mi.Name == nameof(ObjFolderName) || mi.Name == nameof(ObjFileExt) || mi.Name == nameof(ContainingFolder) || mi.Name == nameof(ContainingFolderFullPath))
                        continue;
               
                dynamic v;
                bool b = mBackupDic.TryGetValue(mi.Name, out v);
                if (!b)
                {
                    //TODO: handle Error
                }
                if (mi.MemberType == MemberTypes.Property)
                {
                    // check that we have set method, TODO: do not save it in first place
                    try
                    {
                        //Make sure we can do set - not all props have set
                        PropertyInfo PI = this.GetType().GetProperty(mi.Name);

                        if (PI.CanWrite)
                        {
                            PI.SetValue(this, v);
                        }
                    }
                    catch (Exception)
                    {// temp fix me 
                    }
                }
                else if (mi.MemberType == MemberTypes.Field)
                {
                    // Do reverse + resotre each obj
                    // Do set only if we can really do set, some attrs are get only
                    // FieldInfo fi = this.GetType().GetField(mi.Name, BindingFlags.SetProperty);
                    FieldInfo fi = this.GetType().GetField(mi.Name);
                    if (fi != null)
                    {
                        fi.SetValue(this, v);
                    }
                }

                if (v is IObservableList)
                {
                    // Console.WriteLine("Restoring IObservableList: - " + mi.Name);
                    RestoreList(mi.Name, v);
                }
                else
                {
                    // TODO: add List<string>
                    // else err
                    //temp below until we have it
                }

                mBackupDic.Remove(mi.Name);
                // Console.WriteLine(mi.MemberType + " : " + mi.ToString() + " " + mi.Name + "=" + v);
            }
            // make sure we cleared all bak items = full restore
            if (mBackupDic.Count() != 0)
            {
                // TODO: err handler
            }
        }

        private void RestoreList(string Name, IObservableList v)
        {
            v.Clear();

            object Backuplist;
            bool b = mBackupDic.TryGetValue(Name + "~List", out Backuplist);
            if (!b)
            {
                // TODO: handle err 
            }

            foreach (object o in (List<dynamic>)Backuplist)
            {
                v.Add(o);

                if (o is RepositoryItemBase)
                {
                    ((RepositoryItemBase)o).RestoreBackup();   // Drill down the restore
                }
            }
            mBackupDic.Remove(Name + "~List");
        }

        public void RestoreFromBackup()
        {
            RestoreBackup();
            ClearBackup();
        }

        private string mFileName = null;        
        public string FileName
        {
            get
            {
                if (mFileName == null)
                {
                    return GetNameForFileName();
                }
                else
                {
                    return mFileName;
                }
            }
            set { mFileName = value; }
        }
        
        public virtual string GetNameForFileName()
        {
            //Only the Repository items which are stored as XML should override this.
            //For other we just return null instead of exception
            return null;
            // In case no override impl then throw
            //throw new Exception("Please override this method in class - " + this.GetType().ToString());
        }
        
        public abstract string ItemName
        {
            get;
            set;
        }

        public void InitHeader()
        {
            RepositoryItemHeader = new RepositoryItemHeader()
            {
                Created = GetUTCDateTime(),
                CreatedBy = Environment.UserName,
                GingerVersion = GingerVersion.GetCurrentVersion(),
                LastUpdate = GetUTCDateTime()

                //TODO: other fields
            };
        }

        private DateTime GetUTCDateTime()
        {
            // We remove the seconds and millis as we don't save them and we want the load date time to match exectly when parsed          
            DateTime dt = DateTime.UtcNow;
            DateTime dt2 = new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, 0);
            return dt2;
        }
        
        public RepositoryItemBase CreateCopy(bool setNewGUID = true)
        {
            // Create a copy by serailized and load from the text, it will not copy all atrs only the one which are saved to XML
            string s = RepositorySerializer.SerializeToString(this);
            // TODO: fixme not good practice and not safe, add param to handle in function or another solution...
            RepositoryItemBase duplicatedItem = (RepositoryItemBase)RepositorySerializer.DeserializeFromText(this.GetType(), s, filePath:this.FilePath);

            //change the GUID of duplicated item
            if (setNewGUID && duplicatedItem != null)
            {
                duplicatedItem.ParentGuid = Guid.Empty;
                duplicatedItem.ExternalID = string.Empty;
                duplicatedItem.Guid = Guid.NewGuid();


                List<GuidMapper> guidMappingList = new List<GuidMapper>();

                //set new GUID also to child items
                UpdateRepoItemGuids(duplicatedItem,  guidMappingList);
                duplicatedItem= duplicatedItem.GetUpdatedRepoItem(guidMappingList);

            }

          
            
            return duplicatedItem;
        }





               
        private void UpdateRepoItemGuids(RepositoryItemBase item, List<GuidMapper> guidMappingList)
        {            
            
            foreach (FieldInfo PI in item.GetType().GetFields())
            {
                var token = PI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token == null) continue;

                // we drill down to ObservableList
                if (typeof(IObservableList).IsAssignableFrom(PI.FieldType))
                {
                    IObservableList obj = (IObservableList)PI.GetValue(item);
                    if (obj == null) return;
                    List<object> items = ((IObservableList)obj).ListItems;

                    if ((items != null) && (items.Count > 0) && (items[0].GetType().IsSubclassOf(typeof(RepositoryItemBase))))
                    {
                        foreach (RepositoryItemBase ri in items.Cast<RepositoryItemBase>())
                        {
                            GuidMapper mapping = new GuidMapper();
                            mapping.Original = ri.Guid;
                            ri.Guid = Guid.NewGuid();
                            mapping.newGuid = ri.Guid;

                            guidMappingList.Add(mapping);

                            UpdateRepoItemGuids(ri, guidMappingList);
                        }
                    }
                }          
         
            }

        }

        private RepositoryItemBase GetUpdatedRepoItem(List<GuidMapper> list)
        {
            string s = RepositorySerializer.SerializeToString(this);

            foreach (GuidMapper mapper in list)
            {
                s = s.Replace(mapper.Original.ToString(), mapper.newGuid.ToString());
            }          

            return (RepositoryItemBase)RepositorySerializer.DeserializeFromText(this.GetType(), s, filePath: this.FilePath);
        }
        

        public static string GetOldRepositoryItemFileExt(Type T)
        {
            return OldRepositorySerializer.GetShortType(T);
        }

        public string FileExt(Type T)
        {
            if (UseNewRepositorySerializer)
            {
                return NewRepositorySerializer2.GetShortType(T);
            }
            else
            {
                return OldRepositorySerializer.GetShortType(T);
            }
            
            
            //TODO: use below after we move to GingerCoreCommon
        }

        private RepositoryItemKey mRepositoryItemKey;
        
        public RepositoryItemKey Key
        {
            get
            {
                if (mRepositoryItemKey == null)
                {
                    mRepositoryItemKey = new RepositoryItemKey();
                }
                // we keep it updated just in case, to get the latest and greatest key any time requested
                mRepositoryItemKey.Guid = this.Guid;
                mRepositoryItemKey.ItemName = this.ItemName;
                return mRepositoryItemKey;
            }
        }

        public virtual string RelativeFilePath { get; set; }

        internal void UpdateBeforeSave()
        {
            {
                if (RepositoryItemHeader == null)
                {
                    InitHeader();
                }
                RepositoryItemHeader.Version++;
                RepositoryItemHeader.LastUpdate = DateTime.UtcNow;
                RepositoryItemHeader.LastUpdateBy = Environment.UserName;
                this.ClearBackup();
            }
        }

        public string GetContainingFolder()
        {
            string containingFolder = string.Empty;
            try
            {
                int startIndx = this.FileName.ToUpper().IndexOf(this.ObjFolderName.ToUpper());
                int endIndx = this.FileName.LastIndexOf('\\');
                if (endIndx > startIndx)
                    containingFolder = this.FileName.Substring(startIndx, endIndx - startIndx) + "\\";
                return containingFolder;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return containingFolder;
            }
        }

        public virtual bool FilterBy(eFilterBy filterType, object obj)
        {
            return false;
        }

        private string mContainingFolder = null;
        public string ContainingFolder
        {
            get
            {
                if (mContainingFolder == null)
                {
                    mContainingFolder = GetContainingFolder();
                    return mContainingFolder;
                }
                else
                {
                    return mContainingFolder;
                }
            }
            set { mContainingFolder = value; }
        }

        private string mContainingFolderFullPath = null;
        public string ContainingFolderFullPath
        {
            get
            {
                if (mContainingFolderFullPath == null)
                {
                    if (!string.IsNullOrEmpty(mFileName))
                    {
                        //TODO: cleanup later - use only below
                        mContainingFolderFullPath = System.IO.Path.GetDirectoryName(mFileName);
                    }
                    else
                    {
                        mContainingFolderFullPath = System.IO.Path.GetDirectoryName(mFilePath);                        
                    }
                }
                return mContainingFolderFullPath;
            }
            set { mContainingFolderFullPath = value; }
        }
        
        private string mFilePath = null;
        public string FilePath
        {
            get
            {
                if (mFilePath == null)
                {
                    return GetNameForFileName();
                }
                else
                {
                    return mFilePath;
                }
            }
            set { mFilePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        #region SourceControl

        public static ISourceControl SourceControl = null;
        public eImageType? SourceControlStatus
        {             
            get
            {
                if (SourceControl != null)
                {
                    eImageType st = SourceControl.GetFileStatusForRepositoryItemPath(mFilePath);
                    return st;
                }
                else
                {
                    return null;
                }
            }
        }

        public void RefreshSourceControlStatus()
        {
            OnPropertyChanged(nameof(SourceControlStatus));            
        }

        #endregion SourceControl

        #region Dirty


        eDirtyStatus mDirtyStatus;
        public eDirtyStatus DirtyStatus
        {
            get
            {
                return mDirtyStatus;
            }
            set
            {
                if (mDirtyStatus != value)
                {
                    mDirtyStatus = value;
                    RaiseDirtyChangedEvent();
                    OnPropertyChanged(nameof(DirtyStatus));
                    OnPropertyChanged(nameof(DirtyStatusImage));                    
                }
            }
        }

        public eImageType? DirtyStatusImage
        {
            get
            {

                if (mDirtyStatus == eDirtyStatus.Modified)
                {
                    return eImageType.ItemModified;
                }
                else
                {
                    return eImageType.Empty;
                }                   
            }            
        }


        public event EventHandler OnDirtyStatusChanged;
    
        void RaiseDirtyChangedEvent()
        {
            if (OnDirtyStatusChanged != null)
            {
                OnDirtyStatusChanged(this, new EventArgs());
            }
        }

        
        private void DirtyCheck(string name)
        {
            if (DirtyStatus != eDirtyStatus.NoTracked && DirtyTrackingFields.Contains(name))
            {
                DirtyStatus = eDirtyStatus.Modified;
                // RaiseDirtyChangedEvent();
            }            
        }


        internal void RaiseDirtyChanged(object sender, EventArgs e)
        {            
            DirtyStatus = eDirtyStatus.Modified;
            // RaiseDirtyChangedEvent();
        }

        

        internal void ChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // each change in Observavle will mark the item modified - all NotifyCollectionChangedAction.*
            DirtyStatus = eDirtyStatus.Modified;
            // RaiseDirtyChangedEvent();
        }

        List<string> DirtyTrackingFields;
        public void StartDirtyTracking()
        {            
            if (DirtyStatus != eDirtyStatus.NoTracked)
            {     
                // Nothing to do
                return;
            }

            DirtyTrackingFields = new List<string>();
            DirtyStatus = eDirtyStatus.NoChange;
            //first track self item changes            
            PropertyChanged += ItmePropertyChanged;

            // now track all children which are marked with isSerizalized...
            // throw err if item is serialized but dindn't impl IsDirty

            // Properties
            foreach (PropertyInfo PI in this.GetType().GetProperties())
            {
                var token = PI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token == null) continue;

                DirtyTrackingFields.Add(PI.Name);
                
                // We track observable list which are seriazlized - drill down recursivley in obj tree
                if (typeof(IObservableList).IsAssignableFrom(PI.PropertyType))
                {                    
                    IObservableList obj = (IObservableList)PI.GetValue(this);
                    if (obj == null) return;
                    TrackObservableList((IObservableList)obj);
                    
                }
            }

            // Fields
            foreach (FieldInfo FI in this.GetType().GetFields())
            {
                var token = FI.GetCustomAttribute(typeof(IsSerializedForLocalRepositoryAttribute));
                if (token == null) continue;

                DirtyTrackingFields.Add(FI.Name);

                // We track observable list which are seriazlized - drill down recursivley in obj tree
                if (typeof(IObservableList).IsAssignableFrom(FI.FieldType))
                {
                    IObservableList obj = (IObservableList)FI.GetValue(this);
                    if (obj == null) return;
                    TrackObservableList((IObservableList)obj);

                }
            }

        }

        private void TrackObservableList(IObservableList obj)
        {
            List<object> items = ((IObservableList)obj).ListItems;

            ((INotifyCollectionChanged)obj).CollectionChanged += ((RepositoryItemBase)this).ChildCollectionChanged;

            foreach (object item in items)
            {
                if (item is RepositoryItemBase)
                {
                    RepositoryItemBase RI = ((RepositoryItemBase)item);
                    // Do start tracking only for item which are not already tracked
                    if (RI.DirtyStatus == eDirtyStatus.NoTracked)
                    {
                        RI.StartDirtyTracking();
                    }
                    RI.OnDirtyStatusChanged += this.RaiseDirtyChanged;
                }
                else
                {

                    // for now we ignore list of Guids - like Agents.Tags as user cannot change the value, but if he add/remove it will be tracked
                    if (item is Guid) continue;  
                    throw new Exception("Error: trying to track object which is Serialzied in a list but is not RepositoryItemBase " + this.GetType().FullName + " " + item.ToString() );
                }
            }
        }

        private void ItmePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DirtyTrackingFields.Contains(e.PropertyName))
            {
                if (((RepositoryItemBase)sender).DirtyStatus != eDirtyStatus.Modified)
                {
                    ((RepositoryItemBase)sender).DirtyStatus = eDirtyStatus.Modified;
                    // ((RepositoryItemBase)sender).OnPropertyChanged(nameof(DirtyStatus));
                }
            }
        }

        // test after save dirt should be reset to no change
        // undo shoudl reset to - restpre from bak

        #endregion Dirty

    }
}
