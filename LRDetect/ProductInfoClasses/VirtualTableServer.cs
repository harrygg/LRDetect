using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.Diagnostics;
using System.IO;

namespace LRDetect
{
    class VirtualTableServer : ProductInfo
    {
        protected override string UpgradeCode { get { return "E86DE06E377BEEC4F8B5E243A797A42B"; } }

        protected override string ProductRegistryPath { get { return null; } } 

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {   
              //Leaving the first argument empty as this is the only executable we check in all versions
              //It's important to have the SharedParameter.dll as a first entry as we later check it's version
              { "", new List<string> {  @"client\SharedParameter.dll", @"install\srvany.exe", @"web\node.exe"  } }
            };
          }
        }

        protected override string[] environmentVarNames { get { return null; } }

        public string vtsAgentCaption = "VTS Service";

        public string GetVTSServiceInfo()
        {
          try
          {
            var vtsServiceStatus = Helper.GetServiceStatus(vtsAgentCaption);
            var vtsPort = Helper.GetOpenedPortsForProcessString("node.exe");
            return Helper.FormatServiceNameStatus(vtsAgentCaption, vtsServiceStatus) + vtsPort;
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return Html.ErrorMsg();
          }
        }
    }
}
