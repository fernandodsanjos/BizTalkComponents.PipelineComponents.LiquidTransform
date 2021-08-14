using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;

namespace BizTalkComponents.PipelineComponents
{
    public class Context
    {
        IBaseMessageContext context = null;
        string messageID = String.Empty;
        private const string systemPropertiesNamespace = "http://schemas.microsoft.com/BizTalk/2003/system-properties";
        //This signature does not work in the map => Read(string name,string ns = systemPropertiesNamespace)

        public Context(IBaseMessageContext MessageContext,string MessageID)
        {
            context = MessageContext;
            messageID = MessageID;
        }

        public string Read(string name)
        {
            if (name == "MessageID")
                return messageID;

            return this.Read(name, systemPropertiesNamespace);
        }

        public string Read(string name, string ns)
        {
            if (name == "MessageID" && ns == systemPropertiesNamespace)
                return messageID;

            object property = null;

            property = context.Read(name, ns);

            return property == null ? String.Empty : property.ToString();
        }

        public void Write(string name, string ns,object value)
        {
            if (value == null || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(ns) )
                return;

            context.Write(name, ns, value);
        }

        public void Promote(string name, string ns, object value)
        {
            if (value == null || String.IsNullOrEmpty(name) || String.IsNullOrEmpty(ns))
                return;

            context.Promote(name, ns, value);
          
        }
    }
}
