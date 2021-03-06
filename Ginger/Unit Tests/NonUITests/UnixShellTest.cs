//#region License
///*
//Copyright © 2014-2018 European Support Limited

//Licensed under the Apache License, Version 2.0 (the "License")
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at 

//http://www.apache.org/licenses/LICENSE-2.0 

//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS, 
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
//See the License for the specific language governing permissions and 
//limitations under the License. 
//*/
//#endregion

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using GingerCore;
//using GingerCore.Actions;
//using GingerCore.Environments;
//using GingerCore.Variables;
//using GingerCore.Drivers.ConsoleDriverLib;
//using System.Threading;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//namespace UnitTests.NonUITests
//{
//    [TestClass]
//    [Ignore]
//    public class UnixShellTest 
//    {
//        BusinessFlow bf = new BusinessFlow("Test");
        
//        [TestInitialize]
//        public void TestInitialize()
//        {
           
            
//        }
//        [TestMethod]
//        [Ignore]
//        public void LoginWithOutPrivateKey()
//        {
//            // TODO: Add AAA
//            UnixShellDriver uxsh = new UnixShellDriver(bf);

//            // Commented out due to email from security
//            //uxsh.Host = "hpitan4";
//            //uxsh.UserName="jackd";
//            //uxsh.Password = "Unix11!!";
//            uxsh.Port = 22;
//            uxsh.StartDriver();
//            Thread.Sleep(30);
//            Assert.IsTrue(uxsh.UnixClient.IsConnected);
//            uxsh.Disconnect();
//            uxsh.CloseDriver();
//        }

//        [TestMethod]
//        public void LoginWithPrivateKey()
//        {
//            // TODO: Add AAA
//            UnixShellDriver uxsh = new UnixShellDriver(bf);
//            // Commented out due to email from security
//            //uxsh.Host = "hpitan4";
//            //uxsh.UserName = "ksingara";
//            uxsh.Password = "";
//            uxsh.PrivateKey = Common.getGingerUnitTesterDocumentsFolder() + @"keys\Privatekey.txt";
//            uxsh.Port = 22;
//            uxsh.StartDriver();
//            Thread.Sleep(30);
//            Assert.IsTrue(uxsh.UnixClient.IsConnected);
//            uxsh.Disconnect();
//            uxsh.CloseDriver();
//        }

//        [TestMethod]
//        [Ignore]
//        public void LoginWithPrivateKeyandPassPhrase()
//        {
//            // TODO: Add AAA
//            UnixShellDriver uxsh = new UnixShellDriver(bf);
//            // Commented out due to email from security
//            //uxsh.Host = "hpitan4";
//            //uxsh.UserName = "jackd";
//            uxsh.Password = "";
//            uxsh.PrivateKey = Common.getGingerUnitTesterDocumentsFolder()+@"keys\PrivateKey_jack.txt";
//            uxsh.PrivateKeyPassPhrase="1122";
//            uxsh.Port = 22;
//            uxsh.StartDriver();
//            Thread.Sleep(30);
//            Assert.IsTrue(uxsh.UnixClient.IsConnected);
//            uxsh.Disconnect();
//            uxsh.CloseDriver();
//        }
//        [TestMethod]
//        public void RunScripts()
//        {
//            // TODO: Add AAA
//            UnixShellDriver uxsh = new UnixShellDriver(bf);
//            // Commented out due to email from security
//            //uxsh.Host = "hpitan4";
//            //uxsh.UserName = "jackd";
//            uxsh.Password = "";
//            uxsh.PrivateKey = Common.getGingerUnitTesterDocumentsFolder() + @"keys\PrivateKey_jack.txt";
//            uxsh.PrivateKeyPassPhrase = "1122";
//            uxsh.Port = 22;
//            uxsh.StartDriver();
//            Thread.Sleep(30);
//            //Assert.IsTrue(uxsh.UnixClient.IsConnected);
//            GingerCore.Actions.ActConsoleCommand act=new GingerCore.Actions.ActConsoleCommand();
//            act.ConsoleCommand=GingerCore.Actions.ActConsoleCommand.eConsoleCommand.Script;
//            uxsh.SetScriptsFolder(Common.getGingerUnitTesterDocumentsFolder() + @"sh\");
//            act.ScriptName="TextParamsEcho.sh";
//            act.AddOrUpdateInputParamValue("Ban","ABan");
//            act.AddOrUpdateInputParamValue("Num1", "Num1");
//            act.AddOrUpdateInputParamValue("Num2", "Num2");
//            act.AddOrUpdateInputParamValue("Num3", "Num3");
//            act.AddOrUpdateInputParamValue("Num4", "Num4");
//            act.AddOrUpdateInputParamValue("Num5", "Num5");
//            ValueExpression VE = new ValueExpression(new ProjEnvironment(), bf);
//            foreach (var fpa in act.InputValues)
//            {
//                if (fpa.Value != null)
//                {
//                    VE.Value = fpa.Value;
//                    fpa.ValueForDriver = VE.ValueCalculated;
//                }
//            }
//            uxsh.RunAction(act);
//            uxsh.Disconnect();
//            uxsh.CloseDriver();
//        }
//        [TestMethod]
//        public void RunScriptsAgainstLinux()
//        {
//            // TODO: Add AAA
//            UnixShellDriver uxsh = new UnixShellDriver(bf);
//            // Commented out due to email from security
//            //uxsh.Host = "indlin991";
//            //uxsh.UserName = "o2twrk8";
//            //uxsh.Password = "unix11";
           
//            uxsh.Port = 22;
//            uxsh.StartDriver();
//            Thread.Sleep(30);
//            //Assert.IsTrue(uxsh.UnixClient.IsConnected);
//            GingerCore.Actions.ActConsoleCommand act = new GingerCore.Actions.ActConsoleCommand();
//            act.ConsoleCommand = GingerCore.Actions.ActConsoleCommand.eConsoleCommand.FreeCommand;
//            act.AddOrUpdateInputParamValue("Free Command", "tcsh");
//            uxsh.RunAction(act);
//            act.ConsoleCommand = GingerCore.Actions.ActConsoleCommand.eConsoleCommand.Script;
//            uxsh.SetScriptsFolder(Common.getGingerUnitTesterDocumentsFolder() + @"sh\");
//            act.ScriptName = "TextParamsEcho.sh";
//            act.AddOrUpdateInputParamValue("Ban", "Ban");
//            act.AddOrUpdateInputParamValue("Num1", "Num1");
//            act.AddOrUpdateInputParamValue("Num2", "Num2");
//            act.AddOrUpdateInputParamValue("Num3", "Num3");
//            act.AddOrUpdateInputParamValue("Num4", "Num4");
//            act.AddOrUpdateInputParamValue("Num5", "Num5");
//            ValueExpression VE = new ValueExpression(new ProjEnvironment(), bf);
//            foreach (var fpa in act.InputValues)
//            {
//                if (fpa.Value != null)
//                {
//                    VE.Value = fpa.Value;
//                    fpa.ValueForDriver = VE.ValueCalculated;
//                }
//            }
//            uxsh.RunAction(act);
            
//        }
//        [TestMethod]
//        [Ignore]
//        public void RunScriptsAgainstFreeBSD()
//        {

//        }
//        [TestMethod]
//        [Ignore]
//        public void RunScriptsAgainstHPUX()
//        {

//        }
//        [TestMethod]
//        [Ignore]
//        public void RunScriptsAgainstSunOS()
//        {

//        }
//        [TestMethod]
//        [Ignore]
//        public void LongRunCommands()
//        {

//        }
//        [TestMethod]
//        public void MultiInput()
//        {

//        }
//        [TestMethod]
//        [Ignore]
//        public void MultiOuput()
//        {

//        }
//    }
//}
