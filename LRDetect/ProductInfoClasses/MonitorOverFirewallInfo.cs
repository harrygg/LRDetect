using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class MonitorOverFirewallInfo : ProductInfo
    {
        protected override string UpgradeCode { get { return "682E54D9CFBA4A24F91E7E9FD6F95FA8"; } }

        protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {   
              //Leaving the first argument empty as this is the only executable we check in all versions
              { "", new List<string> {  "MonitorClient.exe" } }
            };
          }
        }
    }
}
