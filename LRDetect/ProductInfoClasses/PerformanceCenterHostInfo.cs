using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class PerformanceCenterHostInfo : ProductInfo
    {
        protected override string UpgradeCode { get { return "4C4B5A63B71605F4E93E5AF6EBDCB833"; } }

        protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {            
              { "12.0x", new List<string> { "Wlrun.exe", "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "AnalysisUI.exe", "lr_bridge.exe", "HP.Utt.StandaloneDebugger.exe", "HP.LR.ProxyRecorderStarter.exe", "TcWebIELauncher.exe" } },
              { "11.52", new List<string> { "Wlrun.exe", "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "AnalysisUI.exe", "lr_bridge.exe", "HP.Utt.Stan  daloneDebugger.exe", "HP.LR.ProxyRecorderStarter.exe" } },
              { "11.5x", new List<string> { "Wlrun.exe", "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "AnalysisUI.exe", "lr_bridge.exe", "HP.Utt.Stan  daloneDebugger.exe" } }, 
              { "11.0x", new List<string> { "Wlrun.exe", "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "AnalysisUI.exe", "lr_bridge.exe" } }, 
              { "9.5x", new List<string> { "Wlrun.exe", "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "AnalysisUI.exe", "lr_bridge.exe" } }
            };
          }
        }

        protected override string[] environmentVarNames { get { return new string[] { "ANALYSIS_PATH", "LG_PATH", "VUGEN_PATH" }; } }

        protected override string[,] importantRegistryKeys
        {
            get
            {
                return new string[,] 
                { 
                    { "RegHive.HKEY_LOCAL_MACHINE", @"SOFTWARE\Mercury Interactive\LoadRunner\Vugen\Thumbnails", "GenerateThumbs" },
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\HTTP\Analyzer", "AddNoCacheHeaderToHtml" }, 
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\HTTP\Analyzer", "DisableStaticCaching" },
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\HTTP\Analyzer", "DisableBrowserCaching" },
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\WPLUS\SSL\OpenSSL", "ReuseSSLSession" }
                };
            }
        }
    }
}
