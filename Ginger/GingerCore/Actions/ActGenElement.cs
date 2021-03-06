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
using Amdocs.Ginger.Common.UIElement;
using Amdocs.Ginger.Repository;
using GingerCore.Actions.Common;
using GingerCore.Helpers;
using GingerCore.Properties;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System;
using System.Collections.Generic;

namespace GingerCore.Actions
{
    // This class is for UI link elemnet
    public class ActGenElement : Act, IObsoleteAction
    {
        // --------------------------------------------------------------------------------------------------------------
        // This Action is OBSOLETE and should be converted to new ActUIElement !!!!!!!!!!!!!!!!!!!!
        // --------------------------------------------------------------------------------------------------------------
        public new static partial class Fields
        {
            public static string Xoffset = "Xoffset";
            public static string Yoffset = "Yoffset";
        }

        bool IObsoleteAction.IsObsoleteForPlatform(ePlatformType platfrom)
        {
            if (platfrom == ePlatformType.AndroidDevice)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        ePlatformType IObsoleteAction.GetTargetPlatform()
        {
            return ePlatformType.NA;
        }
        Type IObsoleteAction.TargetAction()
        {
            return typeof(ActUIElement);
        }
        String IObsoleteAction.TargetActionTypeName()
        {
            ActUIElement actUIElement = new ActUIElement();
            return actUIElement.ActionDescription;
        }
        Act IObsoleteAction.GetNewAction()
        {
            ActUIElement NewAct = new ActUIElement();
            NewAct.CopyInfoFrom(this);
            NewAct.Description = "[New] " + Description;
            NewAct.ElementType = eElementType.Unknown;

            switch (GenElementAction)
            {
                case eGenElementAction.Click:
                    NewAct.ElementAction = ActUIElement.eElementAction.Click;
                    break;
                case eGenElementAction.GetValue:
                    NewAct.ElementAction = ActUIElement.eElementAction.GetValue;
                    break;
                case eGenElementAction.SetValue:
                    NewAct.ElementAction = ActUIElement.eElementAction.SetValue;
                    break;
                default:
                    throw new Exception("Converter error, missing Action translator for - " + GenElementAction);
            }            

            //TODO: move to Act.cs so can be used by other converter
            switch (LocateBy)
            {
                case eLocateBy.ByID:
                    if (Platform == ePlatformType.AndroidDevice)
                    {
                        NewAct.LocateBy = eLocateBy.ByResourceID;
                    }
                    else
                    {
                        NewAct.LocateBy = eLocateBy.ByID;
                    }                    
                    break;
                case eLocateBy.ByXPath:
                    NewAct.LocateBy = eLocateBy.ByXPath;
                    break;
                case eLocateBy.ByCSS:
                    NewAct.LocateBy = eLocateBy.ByCSS;
                    break;
                case eLocateBy.ByXY:
                    NewAct.LocateBy = eLocateBy.ByXY;
                    break;
                default:
                    throw new Exception("Converter error, missing LocateBy translator for - " + LocateBy);
                //TODO: add all the rest
            }

            NewAct.ElementLocateValue = this.LocateValue;

            return NewAct;
        }
        // --------------------------------------------------------------------------------------------------------------
        // This Action is OBSOLETE and should be converted to new ActUIElement !!!!!!!!!!!!!!!!!!!!
        // --------------------------------------------------------------------------------------------------------------
        
        public override string ActionDescription { get { return "Generic Element"; } }
        public override string ActionUserDescription { get { return "Click on a generic control object"; } }

        public override void ActionUserRecommendedUseCase(TextBlockHelper TBH)
        {
            TBH.AddText("Use this action in case you need to automate a click on an object from type generic control.");
            TBH.AddLineBreak();
            TBH.AddLineBreak();
            TBH.AddText("To use this action,select property type of label from Locate By drop down and then enter label property value and then select an action type.");
            TBH.AddLineBreak();
            TBH.AddText("For Using Set Attribute using javascript the format should be :  Attribute||Value ");
        }        

        public override string ActionEditPage { get { return "ActGenElementEditPage"; } }
        public override bool ObjectLocatorConfigsNeeded { get { return true; } }
        public override bool ValueConfigsNeeded { get { return true; } }  // This depends on the action...


        // return the list of platforms this action is supported on
        public override List<ePlatformType> Platforms
        {
            get
            {
                if (mPlatforms.Count == 0)
                {
                    mPlatforms.Add(ePlatformType.Web);
                    mPlatforms.Add(ePlatformType.Mobile);
                    mPlatforms.Add(ePlatformType.Java);
                    mPlatforms.Add(ePlatformType.AndroidDevice);
                    mPlatforms.Add(ePlatformType.PowerBuilder);
                    mPlatforms.Add(ePlatformType.Windows);
                }
                return mPlatforms;
            }
        }

        public enum eGenElementAction
        {
            Click = 1,
            Hover = 2, //This is needed for hovering to expand menus.
            [EnumValueDescription("Get Value")]
            GetValue = 3, //for validation
            [EnumValueDescription("Is Visible")]
            Visible = 4, //for validation
            [EnumValueDescription("Set Value")]
            SetValue = 5,
            [EnumValueDescription("Mouse Click")]
            MouseClick = 6,
            [EnumValueDescription("Keyboard Input")]
            KeyboardInput = 7, 
            //SwipeUp= 6, //for mobile
            //SwipeDown = 7, //for mobile
            //SwipeRight = 8, //for mobile
            //SwipeLeft = 9, //for mobile
            //Press = 10, //for mobile
            Wait = 11, //for mobile
            Back= 12, //for Browser
            [EnumValueDescription("Select From List Scr")]
            SelectFromListScr = 13, //for Browser
            [EnumValueDescription("Key Type")]
            KeyType = 14, //for Browser
            [EnumValueDescription("GoTo URL")]
            GotoURL = 15, //for Browser
            [EnumValueDescription("Select From Drop Down")]
            SelectFromDropDown = 16, //for Browser
            [EnumValueDescription("Switch Frame")]
            SwitchFrame = 17, //for Browser
            [EnumValueDescription("Get Inner Text")]
            GetInnerText = 18, //for Browser
            [EnumValueDescription("Close Browser")]
            CloseBrowser = 19, //for Browser
            [EnumValueDescription("Dismiss Message Box")]
            DismissMessageBox = 20, //for Browser
            [EnumValueDescription("Switch Window")]
            SwitchWindow=21,
            [EnumValueDescription("Get Width")]
            GetWidth=22,
            [EnumValueDescription("Get Height")]
            GetHeight=23,
            [EnumValueDescription("Get Style")]
            GetStyle=24,
            [EnumValueDescription("Message Box")]
            MsgBox = 25,
            [EnumValueDescription("Get Custom Attribute")]
            GetCustomAttribute=26,
            [EnumValueDescription("Start Browser")]
            StartBrowser=28,
            [EnumValueDescription("Async Click")]
            AsyncClick = 27, //used for clicking on elemnts which opening Dialog window- because if using regular click then the driver get stuck till the dialog been closed
            [EnumValueDescription("Scroll to Element")]
            ScrollToElement= 29,
            [EnumValueDescription("Simple Click")]
            SimpleClick=30,
            [EnumValueDescription("Switch To Default Frame")]
            SwitchToDefaultFrame=31,
            [EnumValueDescription("Delete All Cookies")]
            DeleteAllCookies=32,
            Refresh = 33,
            [EnumValueDescription("Get Contexts")]
            GetContexts = 34,
            [EnumValueDescription("Set Contexts")]
            SetContext = 35,
            [EnumValueDescription("Select From Dijit List")]
            SelectFromDijitList = 36,//used for selecting from DropDown/Combobox which his values loaded using dijit (Dojo Toolkit)
            [EnumValueDescription("Run Java Script")]
            RunJavaScript= 37,
            [EnumValueDescription("Double Click")]
            DoubleClick= 38,
            [EnumValueDescription("Right Click")]
            RightClick= 39,
            [EnumValueDescription("Switch To Parent Frame")]
            SwitchToParentFrame=40,
            [EnumValueDescription("Async Select From Drop Down (By Index)")]
            AsyncSelectFromDropDownByIndex = 41,
            [EnumValueDescription("Accept Message Box")]
            AcceptMessageBox = 42,
            [EnumValueDescription("Get Element Attribute Value")]
            GetElementAttributeValue, 
			//Dummy = 43,
            [EnumValueDescription("Batch Clicks")]
            BatchClicks = 44,
            [EnumValueDescription("Batch Set Values")]
            BatchSetValues = 45,
            [EnumValueDescription("Get Window Title")]
			GetWindowTitle = 46,
			Disabled=47,
            [EnumValueDescription("Set Attribute using JavaScript")]
            SetAttributeUsingJs=48,
            [EnumValueDescription("Get Number of Elements")]
            GetNumberOfElements = 49,
            [EnumValueDescription("Send Keys")]
            SendKeys=50,
            [EnumValueDescription("Click At")]
            ClickAt = 51,
            [EnumValueDescription("Is Enabled")]
            Enabled = 52,
            [EnumValueDescription("Tap Element")]
            TapElement = 53,
            [EnumValueDescription("Select From Drop Down (By Index)")]
            SelectFromDropDownByIndex = 54,
            XYClick = 55,
            XYDoubleClick = 56,
            XYSendKeys = 57,
            //XYSendkeys2=58,
            [EnumValueDescription("Is Focused")]
            Focus = 59,
            [EnumValueDescription("Simple Double Click")]
            Doubleclick2 = 61,
            HighLightElement=60,
            [EnumValueDescription("Fire Mouse Event")]
            FireMouseEvent = 62,
            [EnumValueDescription("Fire Special Event")]
            FireSpecialEvent = 63,
            [EnumValueDescription("Scroll Down")]
            ScrollDown = 64,
            [EnumValueDescription("Scroll Up")]
            ScrollUp = 65
        }
        
        [IsSerializedForLocalRepository]
        public eGenElementAction GenElementAction { get; set; }

        string mXoffset = string.Empty;
        public string Xoffset
        {
            get
            {
                //nd for backworth support
                if( GetInputParamValue(Fields.Xoffset) == null && mXoffset != string.Empty)
                    AddOrUpdateInputParamValue(Fields.Xoffset, mXoffset);

                return GetInputParamCalculatedValue(Fields.Xoffset);
            }
            set
            {
                mXoffset = value;
                AddOrUpdateInputParamValue(Fields.Xoffset, value);
            }
        }

        string mYoffset = string.Empty;

        public string Yoffset
        {
            get
            {
                //nd for backworth support
                if (GetInputParamValue(Fields.Yoffset) == null && mYoffset != string.Empty)
                    AddOrUpdateInputParamValue(Fields.Yoffset, mYoffset);

                return GetInputParamCalculatedValue(Fields.Yoffset);
            }
            set
            {
                mYoffset = value;
                AddOrUpdateInputParamValue(Fields.Yoffset, value);
            }
        }
        
        public override String ToString()
        {
            return "Generic Web Element: " + GetInputParamValue("Value");
        }

        public override String ActionType
        {
            get
            {
                return "Generic Web Element: " + GenElementAction.ToString();
            }
        }

        public override System.Drawing.Image Image { get { return Resources.ActLink; } }  // TODO: make me dynamic based on elem type
        
        public override ActionDetails Details
        {            
            get
            {

                // We create a customized user friendly action details for actions grid and report
                ActionDetails d = base.Details;

                // return params order by priority
                d.Params.Clear();

                d.Params.Add(new ActionParamInfo() { Param = "Action", Value = GenElementAction.ToString() });   

                if (!string.IsNullOrEmpty(this.Value))
                {                    
                    d.Params.Add(new ActionParamInfo() { Param = "Value", Value = this.Value });   
                }
                return d;
            }
        }      
    }
}
