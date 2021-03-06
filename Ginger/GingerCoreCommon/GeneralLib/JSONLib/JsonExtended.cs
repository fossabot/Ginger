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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Amdocs.Ginger.Common.GeneralLib
{
   public class JsonExtended
    {

        private JToken JT;
        JsonExtended ParentNode;

        #region Properties
        public string JsonString
        {
            get
            {
               return JT.ToString();
            }
        }
        public string Path
        {
            get
            {
                return JT.Path;
            }
        }
        public string Name
        {
            get
            {
                return JT.Path.Split('.').Last();
            }
        }
        public string XPathWithoutNamspaces;
      /*  {
            get
            {
                string tempPath = "";
                string[] parts = XPath.Split('/');

                string part1 = parts[2];

                if (part1.StartsWith(@"*[name()='"))
                {
                    string a = part1.Replace(@"//*[name()='", "");
                    string b = a.Replace("']", "");
                    part1 = "//*[local-name()='" + b.Split(':').ElementAt(1) + "']";


                }

                for (int i = 3; i < parts.Length; i++)
                {

                    string[] currentParts = parts[i].Split(':');
                    if (currentParts.Length > 1)
                    {
                        tempPath += "/" + currentParts[1];
                    }
                    else
                    {
                        tempPath += "/" + parts[i];
                    }

                }

                return part1 + tempPath;
            }*/



    

  
        #endregion

        #region Constructors
        private JsonExtended(JsonExtended Parent, JToken Node)
        {
            ParentNode = Parent;

            JT = Node;
           
            BuildThisNode(JT.Children());

        }
        public JsonExtended(string json)
        {
            JToken XD = JToken.Parse(json);
            ParentNode = null;
            //   XPath = Node.Name;
            JT = XD;
          
            BuildThisNode(JT.Children());
        }

 
        #endregion

        #region PublicFunctions
 
   
        public List<JsonExtended> GetChildNodes()
        {
            return ChildNodes;
        }





      

        public JToken GetToken()
        {
            return JT;
        }


        public List<JsonExtended> GetAllNodes(List<JsonExtended> XDL = null)
        {

            if (XDL == null)
            {
                XDL = new List<JsonExtended>();
            }


            foreach (JsonExtended XDN in this.GetChildNodes())
            {
                int currentSize =XDL.Count;
                XDN.GetAllNodes(XDL);

             if(XDL.Where(x=>x.GetToken().Parent==XDN.GetToken()).Count()==0)
                    XDL.Add(XDN);
                
            }

            return XDL;
        }

        public IEnumerable<JsonExtended> GetEndingNodes(bool IncludeSelfClosingTags = true)
        {
            List<JsonExtended> mEndingnodes=new List<JsonExtended>();

            var Jpropertytype = typeof(JProperty);
          //  mEndingnodes=   this.GetAllNodes().Where(x=> x.GetToken().GetType()== Jpropertytype & (x.GetToken().Children().Count()>0)).ToList();
            /* if (IncludeSelfClosingTags)
             {
                 mEndingnodes = this.GetAllNodes().Where(x => x.ChildNodes.Count == 0 && x.GetXmlNode().NodeType != XmlNodeType.EndElement && x.GetXmlNode().NodeType != XmlNodeType.Comment).ToList();
             }
             else
             {
                 mEndingnodes = this.GetAllNodes().Where(x => x.ChildNodes.Count == 0 && !x.XMLString.EndsWith("/>") && x.GetXmlNode().NodeType != XmlNodeType.EndElement && x.GetXmlNode().NodeType != XmlNodeType.Comment).ToList();

             }*/

            foreach( var nodes in this.GetAllNodes())
            {
           if(nodes.GetToken().GetType() == Jpropertytype)
                    {

                    mEndingnodes.Add(nodes);
                }
            }
            return this.GetAllNodes().Where(x=>x.GetToken().Children().Count()==0);
        }


        #endregion

        #region Private
  

        private List<JsonExtended> ChildNodes = new List<JsonExtended>();

       

 



        private void BuildThisNode(JEnumerable<JToken> JTl)
        {
            foreach (JToken XN in JTl)
            {
            
                    ChildNodes.Add(new JsonExtended(this, XN));
                
            }
        }


        #endregion

    }
}