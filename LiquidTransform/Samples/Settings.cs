using DotLiquid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizTalk.Transforms.LiquidTransform
{
    public class Settings : ILiquidRegister
    {
        public void Register(RenderParameters parameters)
        {
            string category = "DEV01";

            if(Environment.MachineName.Contains("TEST"))
            {
                category = "EST001";
            }
            else if (Environment.MachineName.Contains("UAT"))
            {
                category = "HST020";
            }
            else if (Environment.MachineName.Contains("PROD"))
            {
                category = "BDT033";
            }

            if (parameters.LocalVariables == null)
                parameters.LocalVariables = new Hash();

            parameters.LocalVariables.Add(new KeyValuePair<string, object>("category", category));



        }
    }
}
