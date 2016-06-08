using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class VugenSAInfo : ProductInfo
    {
        protected override string UpgradeCode { get { return "8B408CD7247E07943A21B9C23B75ACEB"; } }

        protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

        protected override string[] environmentVarNames { get { return new string[] { "VUGEN_PATH" }; } }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {            
              { "12.0x", new List<string> { "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "HP.Utt.StandaloneDebugger.exe", "HP.LR.ProxyRecorderStarter.exe", "TcWebIELauncher.exe" } },
              { "11.52", new List<string> { "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "HP.Utt.StandaloneDebugger.exe", "HP.LR.ProxyRecorderStarter.exe" } },
              { "11.5x", new List<string> { "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe", "HP.Utt.StandaloneDebugger.exe" } }, 
              { "11.0x", new List<string> { "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe" } }, 
              { "9.5x", new List<string> {  "vugen.exe", "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe" } }
            };
          }
        }

        
        protected override string[,] importantRegistryKeys
        {
            get
            {
                return new string[,] 
                { 
                    { "RegHive.HKEY_LOCAL_MACHINE", @"SOFTWARE\Mercury Interactive\LoadRunner\VuGen\Thumbnails", "GenerateThumbs" },
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\HTTP\Analyzer", "AddNoCacheHeaderToHtml" }, 
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\HTTP\Analyzer", "DisableStaticCaching" },
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\HTTP\Analyzer", "DisableBrowserCaching" },
                    { "RegHive.HKEY_CURRENT_USER", @"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\WPLUS\SSL\OpenSSL", "ReuseSSLSession" }
                };
            }
        }
    }
}
