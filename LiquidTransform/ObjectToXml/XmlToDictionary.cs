using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace BizTalk.Transforms.LiquidTransform
{
    public class XmlToDictionary
    {
        public static Dictionary<string, object> Parse(Stream xml)
        {
            var reader = XmlReader.Create(xml, new XmlReaderSettings
            {
                IgnoreWhitespace = true,
                IgnoreComments = true,
                IgnoreProcessingInstructions = true
            });

            return Parse(reader);
        }

        public static Dictionary<string, object> Parse(string xml)
        {
            
            var reader = XmlReader.Create(new StringReader(xml), new XmlReaderSettings { IgnoreWhitespace = true, 
                    IgnoreComments = true, IgnoreProcessingInstructions = true });

            return Parse(reader); 
        }

            public static Dictionary<string, object> Parse(XmlReader reader)
        {
            Dictionary<string, object> root = new Dictionary<string, object>(1);
            
            reader.Read();

            root.Add(reader.LocalName, GetChildNodes(reader));

            return root;
        }

        private static object GetChildNodes(XmlReader reader)
        {
            object returnObject = null;

            Dictionary<string, object> childNodes = null;

            string nodeName = null;
      
            
            if(reader.NodeType == XmlNodeType.None)
                reader.Read();

            if (reader.HasAttributes)
            {
                childNodes = InitDictionary(childNodes);


                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    childNodes.Add($"@{reader.LocalName}", reader.Value);

                }
            }


            while (reader.Read())
            {

                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        childNodes = InitDictionary(childNodes);

                        nodeName = reader.LocalName;

                        object childNode = null;//OrderLine

                        returnObject = GetChildNodes(reader.ReadSubtree());

                        if (childNodes.TryGetValue(nodeName, out childNode))
                        {
                            if (childNode is List<object>)
                            {
                                var list = (childNode as List<object>);
                                list.Add(returnObject);
                            }
                            else 
                            {

                                List<object> multiList = new List<object>();
                                multiList.Add(childNode);

                                multiList.Add(returnObject);

                                childNodes[nodeName] = multiList;

                            }
                           
                        }
                        else
                        {
                            childNodes.Add(nodeName, returnObject);
                        }

                        break;
                    case XmlNodeType.EndElement:

                        break;
                    case XmlNodeType.Text:
                        returnObject = reader.Value;
                        break;
                }
            }

            return childNodes == null ? returnObject : childNodes;
        }


        private static Dictionary<string, object> InitDictionary(Dictionary<string, object> dictionary)
        {
            if (dictionary == null)
            {
                dictionary = new Dictionary<string, object>(1);
            }


            return dictionary;

        }
    }
}
