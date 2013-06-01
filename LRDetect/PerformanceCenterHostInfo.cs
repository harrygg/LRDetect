using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class PerformanceCenterHostInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "4C4B5A63B71605F4E93E5AF6EBDCB833"; }
        }

        //protected override string productCode = "";

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

        public override string[] LatestPatchNames { get { return new string[] { "", "HP Performance Center 11.00 Patch 5", "" }; } }

        public override string[] LatestPatchURLs
        {
            get
            {
                return new string[] { 
                    "", 
                    "", 
                    "" 
                };
            }
        }
    }
}
