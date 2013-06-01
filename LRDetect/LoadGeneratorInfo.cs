using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class LoadGeneratorInfo : ProductInfo
    {
        protected override string UpgradeCode
        {
            get { return "FE69D316C8C69434A99F832B897A4406"; }

        }

        protected override string ProductRegistryPath
        {
            get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; }
        }

        protected override string[] ExecutableFiles
        {
            get
            {
                return new string[] { @"bin\mdrv.exe", @"bin\mmdrv.exe" };
            }
        }

        protected override string[] environmentVarNames
        {
            get { return new string[] { "LR_ROOT", "LG_PATH" }; }
        }
        protected override string[,] importantRegistryKeys
        {
            get { return null; }
        }

        public override string[] LatestPatchNames 
        { 
            get 
            { 
                return new string[] { 
                    "HP LoadGenerator 9.52", 
                    "HP Load Generator Patch 4", 
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
                    "http://support.openview.hp.com/selfsolve/document/KM1338145", 
                    "" 
                };
            }
        }
    }
}
