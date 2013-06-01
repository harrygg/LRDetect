using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class LoadRunnerInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "B497C0589D908C14C9713A936FCCB3C6"; }
        }

        //protected override string productCode = String.Empty;

        protected override string ProductRegistryPath
        {
            get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; }
        }
        protected override string[] ExecutableFiles
        {
            get 
            {
                return new string[] { @"bin\Wlrun.exe", @"bin\vugen.exe", @"bin\mdrv.exe", @"bin\mmdrv.exe", @"bin\firefox\firefox.exe", @"bin\AnalysisUI.exe", @"bin\lr_bridge.exe" }; 
            }
        }

        protected override string[] environmentVarNames
        {
            get { return new string[] { "ANALYSIS_PATH", "LG_PATH", "LR_ROOT", "VUGEN_PATH" }; }
        }

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

        //TODO
        // Change after each patch is released!!!
        // the array contains latest patch for 9.5, 11 and 11.5 - the order is important!
        public override string[] LatestPatchNames 
        { 
            get 
            {
                return new string[] 
                { 
                    "HP LoadRunner 9.52", 
                    "HP LoadRunner 11.00 Patch 4", 
                    "" 
                }; 
            } 
        }
        
        public override string[] LatestPatchURLs
        {
            get
            {
                return new string[] { 
                    "http://support.openview.hp.com/selfsolve/document/KM793318", 
                    "http://support.openview.hp.com/selfsolve/document/KM1338147", 
                    "" 
                };
            }
        }
    }
}
