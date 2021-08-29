using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;

namespace BizTalk.Transforms.LiquidTransform
{
    public class LookupFilter: ILiquidRegister
    {
        private static IDictionary<string, string> dictionary;

        private static IDictionary<string, string> Dictionary
        {
            get
            {
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, string>();
                    dictionary.Add("SE", "Sweden");
                    dictionary.Add("NO", "Norway");
                }


                return dictionary;
            }
            set
            {
                dictionary = value;
            }
        }
        public static string Lookup(string input)
        {
            {
                string output = String.Empty;

                if(Dictionary.TryGetValue(input,out output) == false)
                    output =  input;

                return output;
            }
        }


        public void Register(RenderParameters liquidParameters)
        {
            if (liquidParameters.Filters == null)
            {
                liquidParameters.Filters = new Type[] { typeof(LookupFilter) };
            }
            else
            {
                liquidParameters.Filters.Append(typeof(LookupFilter));//Or Template.RegisterFilter(typeof(LookupFilter)); to add it globally
            }
          

           
        }
    }
    
}
