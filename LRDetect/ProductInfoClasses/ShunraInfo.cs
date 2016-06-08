using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  public class ShunraForLG : ProductInfo
  {
    protected override string UpgradeCode { get { return "6D125BB6CB71DBE4C9CF669B33EB7AC5"; } }
    protected override string ProductRegistryPath { get { return @"SOFTWARE\Shunra\Bootstrapper\"; } }
    public override Dictionary<string, List<string>> Executables
    {
      get
      {
        return new Dictionary<string, List<string>> 
        {   
          //Leaving the first argument empty as this is the only executable we check in all versions
          { "", new List<string> {  "ShunraWatchDogService.exe" } }
        };
      }
    }
  }

  public class ShunraForPCServer : ProductInfo
  {
    protected override string UpgradeCode { get { return "AEA7A9DC383F5AE459F779CEEE66A298"; } }
    protected override string ProductRegistryPath { get { return @"SOFTWARE\Shunra\Bootstrapper\"; } }
    public override Dictionary<string, List<string>> Executables
    {
      get
      {
        return new Dictionary<string, List<string>> 
        {   
          //Leaving the first argument empty as this is the only executable we check in all versions
          { "", new List<string> {  "ShunraWatchDogService.exe" } }
        };
      }
    }
  }

  public class ShunraForController : ProductInfo
  {
    protected override string UpgradeCode { get { return "AEA7A9DC383F5AE459F779CEEE66A298"; } }
    protected override string ProductRegistryPath { get { return @"SOFTWARE\Shunra\Bootstrapper\"; } }
    public override Dictionary<string, List<string>> Executables
    {
      get
      {
        return new Dictionary<string, List<string>> 
        {   
          //Leaving the first argument empty as this is the only executable we check in all versions
          { "", new List<string> {  "ShunraWatchDogService.exe" } }
        };
      }
    }
  }
}
