using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class AnalysisInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "CE53A34494FAD3C4BB524D4EA62DB6FF"; }

        }

        protected override string ProductRegistryPath
        {
            get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; }
        }

        protected override string[] ExecutableFiles
        {
            get
            {
                return new string[] { @"bin\AnalysisUI.exe" };
            }
        }

        protected override string[] environmentVarNames
        {
            get { return new string[] { "ANALYSIS_PATH" }; }
        }

        //protected override string[,] importantRegistryKeys
        //{
        //    get 
        //    {
        //        return new string[,] 
        //        { 
        //            { @"RegHive.HKEY_LOCAL_MACHINE", @"SOFTWARE\Mercury Interactive\Analysis\VuGen\Thumbnails", "GenerateThumbs" }
        //        };
        //    }
        //}

        public override string[] LatestPatchNames { get { return new string[] { "", "HP Analysis 11.00 Patch 4", "" }; } }
        public override string[] LatestPatchURLs
        {
            get
            {
                return new string[] 
                { 
                    "", 
                    "http://support.openview.hp.com/selfsolve/document/KM1338155", 
                    "" 
                };
            }
        }
    }
}
