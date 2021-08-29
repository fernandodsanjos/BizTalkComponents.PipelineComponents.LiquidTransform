using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;
using Microsoft.XLANGs.BaseTypes;
using System.Xml.Serialization;
using System.IO;
using System.Reflection;
using BizTalk.Transforms.LiquidTransform;

namespace BizTalkComponents.PipelineComponents
{
    public class TemplateInstance
    {
        private string schemaStrongName = null;

        private string messageType = null;
        public TemplateInstance(TransformBase map)
        {
            Parameters = RegisterExtension(map);

            Template = Template.Parse(map.XmlContent);

            Microsoft.XLANGs.RuntimeTypes.SchemaMetadata targetSchema = Microsoft.XLANGs.RuntimeTypes.SchemaMetadata.For(SchemaBase.FindReferencedSchemaType(map.GetType(), map.TargetSchemas[0]));

            schemaStrongName = targetSchema.ReflectedType.AssemblyQualifiedName;
            messageType = targetSchema.SchemaName;

        }

        public string SchemaStrongName { get { return schemaStrongName; } }
        public string MessageType { get { return messageType; } }

        public RenderParameters Parameters { get; set; }

        public Template Template { get; set; }
        private RenderParameters RegisterExtension(TransformBase map)
        {
            RenderParameters parameters = new RenderParameters(System.Globalization.CultureInfo.CurrentCulture);
            
            XmlSerializer serilizer = new XmlSerializer(typeof(ExtensionObjects));
            ExtensionObjects extensionObjects = (ExtensionObjects)serilizer.Deserialize(new StringReader(map.XsltArgumentListContent));


            foreach (var extensionObject in extensionObjects.Extensions)
            {
                if(extensionObject.Namespace == "BizTalk.Transforms.LiquidTransform.ILiquidRegister")
                {
                    dynamic extension = null;

                    try
                    {
                        Assembly assembly = Assembly.Load(extensionObject.AssemblyName);

                        extension = assembly.CreateInstance(extensionObject.ClassName);
                    }
                    catch (Exception)
                    {

                        throw new Exception($"RegisterExtension: Could not load  {extensionObject.AssemblyName} {extensionObject.ClassName}");
                    }

                   
                    extension.Register(parameters);
                    
                }
               
            }

            return parameters;


        }

    }
}
