using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class AnalysisInfo : ProductInfo
    {
        protected override string UpgradeCode { get { return "CE53A34494FAD3C4BB524D4EA62DB6FF"; } }

        protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

        protected override string[] environmentVarNames { get { return new string[] { "ANALYSIS_PATH" }; } }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {   
              //Leaving the first argument empty as this is the only executable we check in all versions
              { "", new List<string> {  "AnalysisUI.exe" } }
            };
          }
        }
    }
}
