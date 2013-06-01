using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class VugenSAInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "8B408CD7247E07943A21B9C23B75ACEB"; }
        }

        protected override string ProductRegistryPath
        {
            get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; }
        }
        
        protected override string[] ExecutableFiles
        {
            get
            {
                return new string[] { @"bin\vugen.exe", @"bin\mdrv.exe", @"bin\mmdrv.exe", @"bin\firefox\firefox.exe" };
            }
        }
        
        protected override string[] environmentVarNames
        {
            get { return new string[] { "LR_ROOT", "VUGEN_PATH" }; }
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

        public override string[] LatestPatchNames 
        { 
            get 
            { 
                return new string[] { 
                    "HP Virtual User Generator 9.52", 
                    "HP Virtual User Generator 11.00 Patch 4", 
                    "" 
                }; 
            } 
        }
        public override string[] LatestPatchURLs 
        { 
            get 
            { 
                return new string[] { 
                    "", 
                    "http://support.openview.hp.com/selfsolve/document/KM1338152", 
                    "" 
                }; 
            } 
        }
    }
}
