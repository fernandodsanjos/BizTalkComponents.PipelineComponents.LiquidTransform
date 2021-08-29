using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DotLiquid;

namespace BizTalk.Transforms.LiquidTransform
{
    /// <summary>
    /// Generic liquid XML class. Thanks to Deane Barker, https://gist.github.com/deanebarker/408970e5cc2ebd3597a4d87a1cadc672
    /// </summary>
    public class XmlNode : Drop
    {
        private const string attributeMethodName = "attr";
        private const string xpathQueryMethodName = "xpath";
        private const string listQueryMethodName = "list";
        private const char shorthandDelimiter = '-';

        private XmlElement doc;

        public XmlNode(XmlDocument xml)
        {
            doc = xml.DocumentElement;
        }

        public XmlNode(XmlElement xml)
        {
            doc = xml;
        }

        public XmlNode(string xml)
        {
            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);
            }
            catch (XmlException e) when (e.Message.Contains("multiple root elements"))
            {
                xmlDoc.LoadXml($"<root>{xml}</root>");
            }
            doc = xmlDoc.DocumentElement;
        }

        private string GetPathToNode(string path)
        {
            // Yes, this just returns the input
            // It might change in the future, hence the abstraction
            return $"{path}";
        }

        private string GetPathToTextNode(string path)
        {
            return $"{path}/text()";
        }

        public override object BeforeMethod(string method)
        {
            // Raw Xpath
            // {{ data.xpath['/name/first/@type'] }}
            if (method == xpathQueryMethodName)
            {
                return new XPathQuery(doc);
            }

            // A list of nodes
            // {{ for person in data.list['//person'] }}
            if (method == listQueryMethodName)
            {
                return new ListQuery(doc);
            }

            // List shortcut
            // {% for person in data.list-person %}
            // Prepends "//", so it's the same as the prior example
            if (StartsWithMethodName(method, listQueryMethodName))
            {
                return ListQuery.GetDrops(doc, string.Concat("//", GetShorthandValue(method)));
            }

            // Attribute shortcut
            // {{ data.name.first.attr-type }}
            // Same as the XPath example above
            if (StartsWithMethodName(method, attributeMethodName))
            {
                return doc.Attributes[GetShorthandValue(method)]?.Value;
            }

            // We have nothing for this node, return an empty string
            if (doc.SelectSingleNode(GetPathToNode(method)) == null)
            {
                return string.Empty;
            }
            
            // We have inner text for this node, then this is what they actually want, so return it
            if (doc.SelectSingleNode(GetPathToTextNode(method)) != null)
            {
                return doc.SelectSingleNode(GetPathToTextNode(method)).InnerText;
            }

            // If there's no text, then assume they want to drill down into the inner XML of it
            return new XmlNode((XmlElement)doc.SelectSingleNode(GetPathToNode(method)));
        }

        private bool StartsWithMethodName(string method, string checkFor)
        {
            return method.StartsWith(string.Concat(checkFor, shorthandDelimiter));
        }

        private string GetShorthandValue(string method)
        {
            return method.Split(new char[] { shorthandDelimiter }).Last();
        }
    }


    public class XPathQuery : Drop, IIndexable
    {
        private XmlElement doc;

        public XPathQuery(XmlElement xml)
        {
            doc = xml;
        }

        public override object this[object key] => doc.SelectSingleNode(key.ToString())?.InnerText;
        public override bool ContainsKey(object name) => true;
    }

    public class ListQuery : Drop, IIndexable
    {
        private XmlElement doc;

        public ListQuery(XmlElement xml)
        {
            doc = xml;
        }

        public override object this[object key]
        {
            get
            {
                return GetDrops(doc, key.ToString());
            }
        }

        public static List<XmlNode> GetDrops(XmlElement xml, string xpath)
        {
            var returnDrops = new List<XmlNode>();
            var nodes = xml.SelectNodes(xpath);
            if (nodes != null && nodes.Count > 0)
            {
                foreach (XmlElement node in nodes)
                {
                    returnDrops.Add(new XmlNode(node));
                }
            }
            return returnDrops;
        }

        public override bool ContainsKey(object name) => true;
    }

}
