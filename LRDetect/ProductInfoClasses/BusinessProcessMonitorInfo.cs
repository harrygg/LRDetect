using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class BusinessProcessMonitorInfo : ProductInfo
    {
      protected override string UpgradeCode { get { return "F3770988A38F9A74AABC8781784C173D"; } }

      protected override string ProductRegistryPath { get { return @"SOFTWARE\Mercury Interactive\LoadRunner\"; } }

      protected override string[] environmentVarNames { get { return new string[] {"LG_PATH" }; } }

      public static string AgentCaption = "HP Business Process Monitor";

      public override Dictionary<string, List<string>> Executables
      {
        get
        {
          return new Dictionary<string, List<string>> 
          {   
            //Leaving the first argument empty as this is the only executable we check in all versions
            { "", new List<string> {  "mdrv.exe", @"bin\firefox\firefox.exe"  } }
          };
        }
      }

      public static string GetBPMInstallationInfoFromUninstaller()
      {
        Logger.Info("GetBPMInstallationInfoFromUninstaller");
        // Check if BPM is installed
        var bpm = InstalledProgramsHelper.GetInstalledProgramByName(new System.Text.RegularExpressions.Regex("HP Business Process Monitor"));
        if (bpm != null)
          return String.Format("Yes, {0} {1}", Html.B(bpm.DisplayName), bpm.DisplayVersion);
        return "No";
      }

      public static string GetBPMServiceInfo()
      {
        try
        {
          var status = Helper.GetServiceStatus(AgentCaption);
          return Helper.FormatServiceNameStatus(AgentCaption, status);
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }
      public static string GetBPMProcessesInfo()
      {
        return Helper.GetOpenedPortsForProcessesString(new string[2] { "bpm_nanny.exe", "bpm.exe" });
      }
    }
}
