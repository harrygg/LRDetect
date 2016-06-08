using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class MIListener : ProductInfo
  {
    protected override string UpgradeCode { get { return "85CBC6E82880C974F9913D497EC0FB9C"; } }

    protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

    public override Dictionary<string, List<string>> Executables
    {
      get 
      {
        return new Dictionary<string, List<string>> 
            {   
              //Leaving the first argument empty as this is the only executable we check in all versions
              { "", new List<string> {  "magentproc.exe", @"magentservice.exe" } }
            };
      }
    }
  }
}
