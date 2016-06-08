using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class LoadGeneratorInfo : ProductInfo
    {
        protected override string UpgradeCode { get { return "FE69D316C8C69434A99F832B897A4406"; } }
        protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

        public override Dictionary<string, List<string>> Executables
        {
          get
          {
            return new Dictionary<string, List<string>> 
            {   
              { "", new List<string> {  "mdrv.exe", "mmdrv.exe", @"bin\firefox\firefox.exe"  } }
            };
          }
        }

        protected override string[] environmentVarNames
        {
            get { return new string[] { "LG_PATH"}; }
        }

        protected override string[,] importantRegistryKeys
        {
            get { return null; }
        }
    }
}
