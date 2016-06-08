using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class ApplicationLifecycleManagementInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "165D5913C3F237342A81ADB9BEF434A9"; }
        }

        protected override string ProductRegistryPath
        {
            get { return @""; }
        }

        protected override string[] environmentVarNames
        {
            get { return new string[] { "LG_PATH" }; }
        }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {   
              //Leaving the first argument empty as this is the only executable we check in all versions
              { "", new List<string> {  "ALM.exe", "SA.exe"  } }
            };
          }
        }
    }
}
