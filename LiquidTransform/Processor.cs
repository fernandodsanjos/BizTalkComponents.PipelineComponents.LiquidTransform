using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;
using System.IO;
using Microsoft.BizTalk.Streaming;
using Newtonsoft.Json;
using System.Dynamic;
using BizTalk.Transforms.LiquidTransform;

namespace BizTalkComponents.PipelineComponents
{
    public class Processor
    {
        
        public static  Stream Transform(TemplateInstance template,Stream input,Dictionary<string,object> parameters)
        {
            Stream results = null;
            const int jsonObject = 123;
            const int jsonArray = 91;
            const int xmlStart = 60;

            byte[] startBytes = new byte[10];
            input.Read(startBytes, 0, startBytes.Length);

            input.Position = 0;

            if(parameters != null)
            {
                if (template.Parameters.LocalVariables == null)
                    template.Parameters.LocalVariables = new Hash();

                template.Parameters.LocalVariables.Add("pipelineparms", parameters);
            }
            


            switch (ObjectType(startBytes))
            {
                case xmlStart:
                    results = DeserializeXml(input, template);
                    break;
                case jsonArray:
                    results = DeserializeArray(input, template);
                    break;
                case jsonObject:
                    results = DeserializeObject(input, template);
                    break;
            }

            return results;
        }
        private static Stream Render(dynamic input, TemplateInstance template)
        {
            VirtualStream results = new VirtualStream();
            if (template.Parameters.LocalVariables == null)
            {
                template.Parameters.LocalVariables = Hash.FromDictionary(input);
            }
            else
            {
                Hash hash = template.Parameters.LocalVariables;
                hash.Merge(input);

            }


            //Add pipeline parameters

            template.Template.Render(results,template.Parameters);

            results.Position = 0;

            return results;
        }

        private static Stream DeserializeXml(Stream xml, TemplateInstance template)
        {
            VirtualStream results = new VirtualStream();
            var xmlDictionary = XmlToDictionary.Parse(xml);

            return Render(xmlDictionary, template);


        }
        private static Stream DeserializeObject(Stream json, TemplateInstance template)
        {
            VirtualStream results = new VirtualStream();
            StreamReader reader = new StreamReader(json);
            dynamic expandoObj = JsonConvert.DeserializeObject<ExpandoObject>(reader.ReadToEnd());

            return Render(expandoObj, template);

        }

        private static Stream DeserializeArray(Stream json, TemplateInstance template)
        {
            StreamReader reader = new StreamReader(json);
            dynamic expandoObj = JsonConvert.DeserializeObject<dynamic[]>(reader.ReadToEnd());

            Dictionary<string, object> items = new Dictionary<string, object>();
            items.Add("items", expandoObj);

            return Render(items, template);

        }

        
        private static int ObjectType(byte[] startBytes)
        {
            int res = 0;
            for (int i = 0; i < startBytes.Length; i++)
            {
                if (startBytes[i] == (byte)123 || startBytes[i] == (byte)91 || startBytes[i] == (byte)60)//123 = {, 91 = [, 60 = <
                {
                    res = (int)startBytes[i];
                    break;
                }

            }

            return res;
        }


    }
}
