using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class SoftwareCollector : Collector
  {
    public override string Title { get { return "Installed software"; } }
    public override int Order { get { return 30; } }

    protected override void Collect()
    {
      string tabName = "Software Information";
      AddDataPair(tabName, "Default browser", OSCollectorHelper.GetDefaultBrowser());
      // Internet Explorer
      AddDataPair(tabName, "Internet Explorer version", OSCollectorHelper.GetIEVersion());
      AddDataPair(tabName, "Internet Explorer Extensions"
        , Html.AddLinkToHiddenContent(DetectOtherSoftware.GetIEExtensions()));
      // Google Chrome
      AddDataPair(tabName, "Is Google Chrome installed?", DetectOtherSoftware.GetGoogleChromeInfo());
      // Mozilla Firefox
      AddDataPair(tabName, "Is Mozilla Firefox installed?", DetectOtherSoftware.GetFirefoxInfo());
      AddDataPair(tabName, "Is anti-virus software detected?", DetectSecuritySoftware.GetAntiVirusProgramsInstalled());
      AddDataPair(tabName, "Is firewall software detected?", DetectSecuritySoftware.GetFirewallProgramsInstalled());
      
      // Add the list of Windows Add/Remove Programs only if the menu is selected
      if (FormArguments.menuInstalledPrograms || FormArguments.details >= 3)
      {
        var listOfPrograms = InstalledProgramsHelper.ToList();
        int count = InstalledProgramsHelper.installedProgramsList.Count;
        AddDataPair("Windows Add/Remove Programs", "Installed programs", count + " products found " + listOfPrograms);
      }
      if (FormArguments.menuWindowsUpdates || FormArguments.details >= 3)
        AddDataPair("Windows Add/Remove Programs", "Installed Windows updates", InstalledProgramsHelper.GetWindowsUpdatesInfo());

    }
  }
}
