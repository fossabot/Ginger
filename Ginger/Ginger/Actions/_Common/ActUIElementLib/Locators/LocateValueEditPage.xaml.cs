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

using System.Windows.Controls;
using GingerCore.Actions.Common;

namespace Ginger.Actions._Common.ActUIElementLib
{
    /// <summary>
    /// Interaction logic for LocateValueEditPage.xaml
    /// </summary>
    public partial class LocateValueEditPage : Page
    {
        ActUIElement mAction;

        public LocateValueEditPage(ActUIElement Action)
        {
            InitializeComponent();

            mAction = Action;

            // Bind LocateValue and init VE
            App.ObjFieldBinding(txtLocateValue, TextBox.TextProperty, mAction, ActUIElement.Fields.ElementLocateValue);
            txtLocateValue.Init(mAction, ActUIElement.Fields.ElementLocateValue);
        }
    }
}
