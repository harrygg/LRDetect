using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LRDetect
{
  class OSCollector : Collector
  {
    public override string Title { get { return "Operating System"; } }
    public override int Order{ get { return 1; } }

    protected override void Collect()
    {
      var dh = new DataHolder("Operating System");
      dh.dataPairs.Add("Machine name", System.Environment.MachineName);
      dh.dataPairs.Add("Full name", OSCollectorHelper.GetOSFullNameFormatted());
      dh.dataPairs.Add("Root directory", OSCollectorHelper.GetOSRootDir());
      // If VuGen is not installed product version will be 0.0
      //if (ProductDetection.Vugen.version >= new Version(11, 04) && ProductDetection.Vugen.version <= new Version(12, 01))
      //  dh.dataPairs.Add("Is OS supported?", OSCollectorHelper.IsOSSupportedInfo());
      dh.dataPairs.Add("Language", OSCollectorHelper.language);
      dh.dataPairs.Add("Locale", OSCollectorHelper.GetOSLocaleInfo());
      dh.dataPairs.Add("Is OS Virtualized?", OSCollectorHelper.IsOSVirtualizedInfo());
      dh.dataPairs.Add("Is 3GB switch enabled?", OSCollectorHelper.Is3GBSwitchEnabled());
      dh.dataPairs.Add("Data Execution Prevention", OSCollectorHelper.DepInfo());
      dh.dataPairs.Add("User Account Control", OSCollectorHelper.UACInfo());
      dh.dataPairs.Add("Is user Admin?", Html.BoolToYesNo(OSCollectorHelper.IsUserInAdminGroup()));
      dh.dataPairs.Add("Is user connected remotely?", Html.BoolToYesNo(SystemInformation.TerminalServerSession));
      dh.dataPairs.Add("Is Windows firewall enabled?", OSCollectorHelper.IsWindowsFirewallEnabled());
      dh.dataPairs.Add("Is secondary logon enabled?", OSCollectorHelper.IsSecondaryLogonEnabledInfo());

      dataHolders.Add(dh);

      dh = new DataHolder("Environment information");
      dh.dataPairs.Add("System environment variables", Html.AddLinkToHiddenContent(OSCollectorHelper.GetEnvVariables()));
      dh.dataPairs.Add("User environment variables", Html.AddLinkToHiddenContent(OSCollectorHelper.GetUsrEnvVariables()));
      dh.dataPairs.Add("Kerberos configuration", OSCollectorHelper.GetKerberosConfiguration());
      var lsp = Html.B(OSCollectorHelper.GetNumberOfInstalledLSPs() + " entries found ") + Html.AddLinkToHiddenContent(OSCollectorHelper.GetInstalledLSPs());
      dh.dataPairs.Add("Layered Service Providers", lsp);
      dh.dataPairs.Add("AppInit_DLLs registry value", OSCollectorHelper.GetAppInitDLLsInfo());
      
      //LoadAppInit_DLLs registry is only availbale in Windows 7 and later
      if (Environment.OSVersion.Version >=  new Version(6, 1))
        dh.dataPairs.Add("LoadAppInit_DLLs registry value", OSCollectorHelper.GetLoadAppInitDLLsInfo());
      dataHolders.Add(dh);
    }
  }
}
