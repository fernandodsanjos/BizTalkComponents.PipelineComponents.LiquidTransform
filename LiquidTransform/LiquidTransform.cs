using System;
using System.Collections.Generic;
using System.Resources;
using System.Drawing;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.ComponentModel;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.IO;
using Microsoft.BizTalk.Streaming;
using Microsoft.BizTalk.Message.Interop;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.ScalableTransformation;
using Microsoft.XLANGs.BaseTypes;
//using Microsoft.XLANGs.RuntimeTypes;
using System.ComponentModel.DataAnnotations;
using BizTalkComponents.Utils;
using Microsoft.BizTalk.Component.Utilities;
using DotLiquid;
using Microsoft.XLANGs.RuntimeTypes;
using System.Xml.Serialization;


namespace BizTalkComponents.PipelineComponents
{
    /// <summary>
    ///  Transforms original message stream using streaming scalable transformation via provided map specification.
    /// </summary>
 
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Encoder)]
    [ComponentCategory(CategoryTypes.CATID_Decoder)]
    [System.Runtime.InteropServices.Guid("A8D45AF7-1235-4C3C-B8A6-1CB77A1B6727")]
    public partial class LiquidTransform : IBaseComponent
    {
        /// <summary>
        /// StrongMapType, Template
        /// </summary>
        public static ConcurrentDictionary<string, TemplateInstance> transforms = null;

        /// <summary>
        /// MessageType or StrongMessageType,StrongMapType , StrongMapType
        /// </summary>
        public static ConcurrentDictionary<Tuple<string,string>, string> schemaMappings = null;

      

        // private PortDirection m_portDirection;

       
        

        private const string _systemPropertiesNamespace = "http://schemas.microsoft.com/BizTalk/2003/system-properties";


        /// <summary>
        /// MessageType, StrongMessageType      /// </summary>
        private ConcurrentDictionary<Tuple<string, string>, string> SchemaMappings
        {

            get
            {
                if (schemaMappings == null)
                    schemaMappings = new ConcurrentDictionary<Tuple<string, string>, string>();

                return schemaMappings;
            }
        }

        /// <summary>
        /// StrongMapType, Template
        /// </summary>
        private ConcurrentDictionary<string, TemplateInstance> Transforms
        {

            get
            {
                if (transforms == null)
                    transforms = new ConcurrentDictionary<string, TemplateInstance>();

                return transforms;
            }
        }

        /// <summary>
        /// One or more piped Map specification to be applied to original message.
        /// </summary>	
        [RequiredRuntime]
        [Description("One or more piped fully qualified BizTalk map name(s).")]
        public string MapName
        {
            get;
            set;
        }

        [Description("One or more static parameters in the format [name]=[value], use pipe to specify multiple parameters")]
        public string Parameters
        {
            get;
            set;
        }

        [DisplayName("Map Required")]
        [Description("At least one map must match")]
        [DefaultValue(true)]
        public bool MapRequired
        {
            get;
            set;
        } = false;

        #region IComponent Members

        public IBaseMessage Execute(IPipelineContext pContext, IBaseMessage pInMsg)
        {

            try
            {
                TemplateInstance template = null;


                MapName = MapName.Trim();

                string map = GetContextTransform(pInMsg.Context);

                if (string.IsNullOrEmpty(map) == false)
                {
                    CacheTemplate(map);

                    if (string.IsNullOrEmpty(MapName))
                    {
                        MapName = map;
                    }
                    else
                    {
                        MapName = String.Format("{0}|{1}", MapName, map);//Add the map last
                    }
                }

                if (string.IsNullOrEmpty(MapName))
                {
                    if (MapRequired)
                    {
                        throw new ArgumentNullException("MapName");
                    }
                    else
                    {
                        return pInMsg;
                    }

                }

                string messageType = null;

                string[] maps = MapName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                messageType = DeriveMessageType(pInMsg);

                string stageID = pContext.StageID.ToString("D");

                if (stageID == CategoryTypes.CATID_Decoder && messageType == null)
                {
                    template = transforms[maps[0]];  
                }
                
                if(template == null)
                { 

                    template = FindFirstMatch(messageType);

                }


                if (template == null)
                {
                    System.Diagnostics.Debug.WriteLine("No match for map could be made for message type: " + messageType);
                }
                else
                {
                    var parameters = AddParameters();

                    Stream result = Processor.Transform(template, pInMsg.BodyPart.GetOriginalDataStream(), parameters);

                    pInMsg.BodyPart.Data = result;

                    pInMsg.Context.Promote("MessageType", _systemPropertiesNamespace, template.MessageType);

                    pInMsg.Context.Write("SchemaStrongName", _systemPropertiesNamespace, template.SchemaStrongName);



                    pContext.ResourceTracker.AddResource(result);
                }

            }
            catch (Exception ex)
            {

                throw new ApplicationException($"Error while trying to transform using MapType specification: {MapName}\n used\nError {ex.Message}", ex);
            }
            


            return pInMsg;
        }

        
        private string DeriveMessageType(IBaseMessage pInMsg)
        {

            string schemaStrongName = null;
            string messageType = null;
            

            if ((schemaStrongName = (string)pInMsg.Context.Read("SchemaStrongName", _systemPropertiesNamespace)) != null)
            {
                if (schemaStrongName.StartsWith("Microsoft.XLANGs.BaseTypes.Any") == false)
                {
                   
                    messageType = schemaStrongName;
                }

            }

            if (messageType == null)
            {
                messageType = (string)pInMsg.Context.Read("MessageType", _systemPropertiesNamespace);  

            }

            return messageType;

        }
        private string GetContextTransform(IBaseMessageContext context)
        {
            for (int i = 0; i < context.CountProperties; i++)
            {
                string name;
                string ns;
               
                object obj = context.ReadAt(i, out name, out ns);

                if(name == "LiquidTransform")
                {
                    context.Write(name, ns, null);//Remove context as we do not want it to interfere
                    return (string)obj;
                    
                }
            }

            return null;
        }

        #endregion

        private Dictionary<string,object> AddParameters()
        {
            Dictionary<string, object> parms = null;

            if (Parameters == null)
                return parms;

            string[] parametersArray = Parameters.Split(new char[] { '|' }, StringSplitOptions.None);

            if(parametersArray.Length > 0)
            {
                parms = new Dictionary<string, object>();

                foreach (var parameter in parametersArray)
                {
                    string[] parameterArray = parameter.Split(new char[] { '=' }, StringSplitOptions.None);

                    if (parameterArray.Length == 2)
                    {
                        parms.Add(parameterArray[0], parameterArray[1]);

                    }
                }
            }

            return parms;

        }


        private TemplateInstance FindFirstMatch(string messageType)
        {
            string[] maps = MapName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            TemplateInstance template = null;

            string mapName = null;

            for (int i = 0; i < maps.Length; i++)
            {
                var key = Tuple.Create(messageType, maps[i]);

                if (SchemaMappings.TryGetValue(key, out mapName))
                    return Transforms[mapName];

            }

            return template;

        }

        private void CacheTemplates()
        {
            string[] maps = MapName.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < maps.Length; i++)
            {
                CacheTemplate(maps[i]);
            }
        }

        private void CacheTemplate(string mapname)
        {
            if (Transforms.ContainsKey(mapname) == false)
            {

                Type mapType = Type.GetType(mapname, true);
                Object mapObj = Activator.CreateInstance(mapType);
                TransformBase map = mapObj as TransformBase;

                SchemaMetadata sourceSchema = Microsoft.XLANGs.RuntimeTypes.SchemaMetadata.For(SchemaBase.FindReferencedSchemaType(mapType, map.SourceSchemas[0]));

                SchemaMappings.TryAdd(Tuple.Create(sourceSchema.ReflectedType.AssemblyQualifiedName, mapname), mapname);
                SchemaMappings.TryAdd(Tuple.Create(sourceSchema.SchemaName, mapname), mapname);

                var template = new TemplateInstance(map);

                Transforms.TryAdd(mapname, template);
            }
        }

       

        /// <summary>
        /// Loads configuration property for component.
        /// </summary>
        /// <param name="pb">Configuration property bag.</param>
        /// <param name="errlog">Error status (not used in this code).</param>
        public void Load(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, Int32 errlog)
        {
           
            Parameters = PropertyBagHelper.ReadPropertyBag(pb, "Parameters", Parameters);
            MapRequired = PropertyBagHelper.ReadPropertyBag(pb, "MapRequired", MapRequired);
            MapName = PropertyBagHelper.ReadPropertyBag(pb, "MapName", MapName);

            CacheTemplates();
        }

        /// <summary>
        /// Saves current component configuration into the property bag.
        /// </summary>
        /// <param name="pb">Configuration property bag.</param>
        /// <param name="fClearDirty">Not used.</param>
        /// <param name="fSaveAllProperties">Not used.</param>
        public void Save(Microsoft.BizTalk.Component.Interop.IPropertyBag pb, bool fClearDirty, bool fSaveAllProperties)
        {
            PropertyBagHelper.WritePropertyBag(pb, "MapName", MapName);
            PropertyBagHelper.WritePropertyBag(pb, "Parameters",Parameters);
            PropertyBagHelper.WritePropertyBag(pb, "MapRequired", MapRequired);

        }
       
    }


    
}
