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
using Ginger.Actions.UserControls;
using GingerCore.Platforms;
using GingerCoreNET.SolutionRepositoryLib.RepositoryObjectsLib.PlatformsLib;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

namespace Ginger.ApplicationModelsLib.POMModels
{
    /// <summary>
    /// Interaction logic for POMEditPage.xaml
    /// </summary>
    public partial class POMEditPage : Page
    {
        ApplicationPOMModel mApplicationPOM;
        
        ScreenShotViewPage pd;
        public POMEditPage(ApplicationPOMModel ApplicationPOM)
        {
            InitializeComponent();
            mApplicationPOM = ApplicationPOM;
            PageNameTextBox.BindControl(mApplicationPOM, nameof(ApplicationPOM.Name));

            ObservableList<ApplicationPlatform> Apps = App.UserProfile.Solution.ApplicationPlatforms;
            //ApplicationPlatform AP = (from x in Apps where x.Guid == mApplicationPOM.TargetAppplicationKey.Guid select x).FirstOrDefault();
            //mApplicationPOM.TargetAppplicationKey = AP.Key;  // Create new binding and get updates if exist as key app name might changed
            
            //Yuval Uncomment
            //TargetApplicationComboBox.BindControl<ApplicationPlatform>(mApplicationPOM, nameof(mApplicationPOM.TargetAppplicationKey), Apps, nameof(ApplicationPlatform.AppName), nameof(ApplicationPlatform.Key));            
            
            ////TODO: lazy load based on tabs with cache
            //if (mPom.ScreenShot == null)
            //{
            //    string filename = mPom.FileName.Replace("xml", "Screenshot.bmp");  // TODO: use same const
            //    //mPom.ScreenShot = General.LoadBitmapFromFile(filename);
            //}
            //ScreenShotViewPage p = new ScreenShotViewPage(mPom.Name, mPom.ScreenShot);
            //ScreenShotFrame.Content = p;

            //POMSimulatorFrame.Content = new POMSimulatorPage(mPom);


            //LearnWizardPage p2 = new LearnWizardPage(mPom);
            //UIElementsFrame.Content = p2;

            //MappingFrame.Content = new MapUIElementsWizardPage(mPom);

            //NaviagtionsFrame.Content = new NavigationsPage(mPom);

            ////TODO: create a page and move code to design page
            //pd = new ScreenShotViewPage(mPom.Name, mPom.ScreenShot);
            //pd.MouseClickOnScreenshot += MouseClickOnScreenshot;
            //pd.MouseUpOnScreenshot += MouseUpOnScreenshot;
            //pd.MouseMoveOnScreenshot += MouseMoveOnScreenshot;
            //DesignFrame.Content = pd;

            //InitControlPropertiesGrid();
        }

        private void InitControlPropertiesGrid()
        {
            string[] list = new string[] {"Name=ABC", "Mandatory=No", "FontColor=Black" };
            ControlPropertiesGrid.ItemsSource = list;
        }

        double MX = -1;
        double MY = -1;

        private void MouseClickOnScreenshot(object arg1, MouseclickonScrenshot arg2)
        {
            MX = arg2.X;
            MY = arg2.Y;            
        }

        private void MouseMoveOnScreenshot(object arg1, MouseMoveonScrenshot arg2)
        {
            if (MX != -1 && MY != -1)
            {
                Rectangle r = GetMouseRectangle(MX, MY, arg2.X, arg2.Y);
                pd.HighLight(r);
            }
        }


        // Return correct rectangle when x, Y is always top left, user can draw rectangle in different direction, below function avoid get width/height in minus
        Rectangle GetMouseRectangle(double X1, double Y1, double X2, double Y2)
        {
            Rectangle r = new Rectangle();

            if (X1 < X2)
            {
                r.X = (int)X1;
            }
            else
            {
                r.X = (int)X2 + 1;  // + 1 to make sure our highlighert is not under mouse and we will not get th mouse events, help when drawing back or up
            }

            if (Y1 < Y2)
            {
                r.Y = (int)Y1;
            }
            else
            {
                r.Y = (int)Y2 + 1; // + 1 to make sure our highlighert is not under mouse and we will not get th mouse events, help when drawing back or up
            }

            r.Width = (int)System.Math.Abs(X1 - X2);
            r.Height = (int)System.Math.Abs(Y1 - Y2);

            return r;
        }


        private void MouseUpOnScreenshot(object arg1, MouseUponScrenshot arg2)
        {
            Rectangle r = GetMouseRectangle(MX, MY, arg2.X, arg2.Y);
            MX = -1;
            MY = -1;
            CreateControl(r);
        }

        private void CreateControl(Rectangle r)
        {
            // ******************************************************************************
            //DO NOT  DELETE Temp commented for moving to GingerCoreCommon
            // ******************************************************************************
            //if (ControlsListBox.SelectedItem == null) return;
            //var c = ((ListBoxItem)ControlsListBox.SelectedItem).Content;

            //Control control = null;

            //if (c is Label)
            //{
            //    Label l = new Label();                
            //    l.Content = "Label";
            //    l.MouseDown += Label_MouseDown;                
            //    control = l;
                
            //    //TODO: create at the end for all the same
            //    ElementInfo EI = new ElementInfo();
            //    EI.ElementName = "New Label";
            //    EI.X = r.X;
            //    EI.Y = r.Y;
            //    EI.Width = (int)l.Width;
            //    EI.Height = (int)l.Height;
            //    EI.ElementType = "Label";
            //    EI.Active = true;
            //    mApplicationPOM.UIElements.Add(EI);
            //    l.Tag = EI;
            //}

            //if (c is Button)
            //{
            //    Button b = new Button();                
            //    b.Content = "Button";
            //    b.Click += B_Click;
            //    control = b;
            //}

            //if (c is TextBox)
            //{
            //    TextBox TB = new TextBox();
            //    control = TB;
            //}

            //if (c is ComboBox)
            //{
            //    ComboBox CB = new ComboBox();                
            //    CB.ItemsSource = new string[] { "A", "B", "C" };
            //    control = CB;
            //}

            //if (control != null)
            //{
            //    control.Width = r.Width;
            //    control.Height = r.Height;
            //    pd.AddControl(control, r.X, r.Y);
            //}

            ////TODO: common Setcontrol location for all C
        }

        private void B_Click(object sender, System.Windows.RoutedEventArgs e)
        {
        }

        private void Label_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void ClearButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            pd.ClearControls();
        }
    }
}
