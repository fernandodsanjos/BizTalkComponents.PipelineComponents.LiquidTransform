using System;
using System.Collections.Generic;
using System.Resources;
using System.Drawing;
using System.Collections;
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
using Microsoft.XLANGs.RuntimeTypes;
using BizTalkComponents.Utils;

namespace BizTalkComponents.PipelineComponents
{

    public partial class LiquidTransform : Microsoft.BizTalk.Component.Interop.IComponent, IComponentUI, IPersistPropertyBag
    {
      

        #region IBaseComponent Members

        public string Description
        {
            get { return "Pipeline Component to apply BizTalk liquid map"; }
        }

        public string Name
        {
            get { return "Liquid Transformation"; }
        }

        public string Version
        {
            get { return "1.0.0"; }
        }

        #endregion

        #region IComponentUI Members

        public IntPtr Icon
        {
            get
            {
                return new IntPtr();
            }
        }

        public IEnumerator Validate(object projectSystem)
        {
            return ValidationHelper.Validate(this, false).GetEnumerator();
        }

        #endregion



        public void GetClassID(out Guid classID)
        {
            classID = new Guid("A8D45AF7-1235-4C3C-B8A6-1CB77A1B6727");
        }

        public void InitNew()
        {
            throw new Exception("The method or operation is not implemented.");
        }

       
    }

}
