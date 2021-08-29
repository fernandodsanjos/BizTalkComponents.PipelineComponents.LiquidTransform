using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace BizTalk.Transforms.LiquidTransform
{
    
    public  interface ILiquidRegister
    {
        //Template.RegisterTag<Random>("random");  TAG|BLOCK, These can only be added to Global and must be unique for the running host instance
        //parameters.Filters.Append(typeof(LiquidRegister)); FILTER Unique per map

        //variables.Add(KeyValuePair)  if we want to add other objects available to the map or simple value dictionaries
        void Register(RenderParameters parameters);
       
       
    }
}
