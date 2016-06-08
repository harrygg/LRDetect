using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LRDetect
{
  public class NVShunraCollectorHelper
  {
    internal static string IsShunraInstalledFor(string productName)
    {
      var output = Html.ErrorMsg();
      try
      {
        // Prepend Shunra NV for HP to the product name and convert to regex
        var regexString = @"Shunra\s*NV.*HP\s*" + productName.Replace(" ", @"\s*");
        var regex = new Regex(regexString, RegexOptions.IgnoreCase);
        var p = InstalledProgramsHelper.GetInstalledProgramByName(regex);

        if (p != null)
        {
          output = Html.Yes;
          GetShunraProductDetails(p);
        }
        else
          output = Html.No;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
      return output;
    }

    static void GetShunraProductDetails(InstalledProgram p)
    {
      string currentVersion = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, @"SOFTWARE\Shunra\Bootstrapper", "CurrentVersion");
      string buildVersion = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, @"SOFTWARE\Shunra\Bootstrapper", "BuildVersion");
      string installedPath = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, @"SOFTWARE\Shunra\Bootstrapper", "InstalledPath");

      productDetails = new StringBuilder();
      productDetails.Append(Html.B(p.DisplayName) + Helper.ConvertInstallDate(p.InstallDate) + Html.br);
      productDetails.Append("Version: " + currentVersion + Html.br);
      productDetails.Append("Build: " + buildVersion + Html.br);
      productDetails.Append("Location: " + installedPath + Html.br + Html.br);

      productDetails.Append("Services: " + Html.br);
      productDetails.Append("Shunra WatchDog Service: " + PCServicesCollectorHelper.GetServiceInfo("ShunraWatchDogService") + Html.br);
      productDetails.Append("Shunra Performance Counters Service: " + PCServicesCollectorHelper.GetServiceInfo("ShunraPerformanceCountersService") + Html.br);
    }

    static StringBuilder productDetails;
    public static string ProductDetails { get { return productDetails.ToString(); } }
  }
}
