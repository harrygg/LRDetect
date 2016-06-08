using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class PerformanceCenterServerInfo : ProductInfo
    {
        protected override string UpgradeCode { get { return "DBC0B694AF01C15448B57B6BF896DE16"; } }

        protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {   
              //Leaving the first argument empty as this is the only executable we check in all versions
              { "", new List<string> {   @"HostSetup.exe", @"KAdminUI.exe", @"HP.PC.PCS.Configurator.exe"} }
            };
          }
        }

    }
}
