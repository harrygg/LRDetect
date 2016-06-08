using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class QuickTestProInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "99F9FE8FCA23AC3488DB13B6F1837C99"; }
        }

        protected override string ProductRegistryPath
        {
            get { return @"SOFTWARE\Mercury Interactive\QuickTest Professional\"; }
        }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {   
              //Leaving the first argument empty as this is the only executable we check in all versions
              { "", new List<string> {  "QTPro.exe" } }
            };
          }
        }
    }
}
