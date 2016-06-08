using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LRDetect
{
  class RelatedProductsCollector : Collector
  {
    public override string Title { get { return "Other HP related products"; } }
    public override int Order { get { return 130; } }

    protected override void Collect()
    {


      var title = "HP Citrix Agent Information";


      var ctrxAgent = new CitrixHelper.Agent();


      AddDataPair(title, "Citrix Agent", ctrxAgent.ToString());

      if (ctrxAgent.isInstalled)
      {
        AddDataPair(title, "Agent configuration", Html.AddLinkToHiddenContent(ctrxAgent.GetIniContentInfo()));
        AddDataPair(title, "Citrix registry keys", ctrxAgent.GetImptKeyContent());
        AddDataPair(title, "Is text trapping driver installed?", Html.BoolToYesNo(ctrxAgent.isTextTrappingDriverInstalled) + ctrxAgent.textTrappingDriverVersion);
        if (ctrxAgent.isTextTrappingDriverInstalled)
        {
          AddDataPair(title, "Is text trapping driver signed?", ctrxAgent.driverSignedInfo);
          AddDataPair(title, "Text trapping driver state", ctrxAgent.textTrappingDriverState);
        }
      }
      


      //################
      //RDP Role Detection
      //################
      title = "HP RDP Agent Information";
      //AddDataPair(title, "Is RDP access allowed?", DetectOtherSoftware.IsRDPAccessAllowedInfo());
      AddDataPair(title, "Is RDP Role installed?", DetectOtherSoftware.IsRDPRollInstalledInfo());
      AddDataPair(title, "HP RDP Agent", DetectOtherSoftware.RDPAgentInfo());




      //#############
      //VTS DETECTION
      //#############
      Logger.Info("Collecting VTS information");
      var vts = new VirtualTableServer();
      title = "HP Virtual Table Server";

      if (vts.IsInstalled)
      {
        AddDataPair(title, "Is VTS installed?", vts.GetProductNameVersionDateFormatted());
        AddDataPair(title, "Details", vts.GetExecutableFilesInfo());
        AddDataPair(title, "Service status", vts.GetVTSServiceInfo());
      }
      else
      {
        var vts2 = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("[L|l]oad[R|r]unner - VTS"));
        var info = (vts2 != null) ? vts2.ToString() : "No";
        AddDataPair(title, "Is VTS installed?", info);

      }
      


      //#############
      //LoadRunner Eclipse Add-in for Developers DETECTION
      //#############
      Logger.Info("Collecting LoadRunner Eclipse information");
      title = "LoadRunner Eclipse Add-in for Developers";
      var eclipsePlugin = InstalledProgramsHelper.GetInstalledProgramByName(new System.Text.RegularExpressions.Regex("LoadRunner Eclipse.*Developers"));
      if (eclipsePlugin != null)
      {
        AddDataPair(title, "Is Eclipse Add-in installed?", "Yes " + eclipsePlugin.DisplayName);
        AddDataPair(title, "Version", eclipsePlugin.DisplayVersion);
        AddDataPair(title, "Eclipse path", VuGenProperties.GetAttributeValue("Java.EclipseIdePath", "value", "Not found!"));
      }
      else
        AddDataPair(title, "Is Add-in installed?", "No");



      //#############
      //LoadRunner Visual Studio 2012 Add-in for Developers DETECTION
      //#############
      Logger.Info("Collecting LoadRunner Eclipse information");
      title = "LoadRunner Visual Studio Add-in for Developers";
      var vsPlugin = InstalledProgramsHelper.GetInstalledProgramByName(new System.Text.RegularExpressions.Regex("LoadRunner Visual Studio 20[0-9]{2} Add"));
      string installed = vsPlugin != null ? Html.Yes + " " + vsPlugin.DisplayName + " version " + vsPlugin.DisplayVersion : Html.No;
      AddDataPair(title, "Is Add-in installed?", installed);




      //#############
      //BPM DETECTION
      //#############
      Logger.Info("Detecting BPM");
      title = "HP Business Process Monitor";
      var bpm = new BusinessProcessMonitorInfo();
      //Logger.Info("BPM installation " + Html.Bool2Text(bpm.IsInstalled));
      var bpmInstallInfo = (bpm.IsInstalled == true) ? bpm.GetProductNameVersionDateFormatted() : BusinessProcessMonitorInfo.GetBPMInstallationInfoFromUninstaller();
      AddDataPair(title, "Is BPM installed?", bpmInstallInfo);

      if (bpm.IsInstalled == true || !bpmInstallInfo.Contains("No"))
      {
        AddDataPair(title, "Service status", BusinessProcessMonitorInfo.GetBPMServiceInfo());
        AddDataPair(title, "BPM processes", Html.AddLinkToHiddenContent(BusinessProcessMonitorInfo.GetBPMProcessesInfo()));
      }



      //#############
      //ALM Platform Loader
      //#############
      if (FormArguments.details >= 2)
      {
        Logger.Info("Detecting ALM PL information");
        AddDataPair("ALM Platform Loader", "Is ALM Platform Loader installed?", DetectOtherSoftware.GetALMPlatformLoaderInfo());
      }



      //#############
      //QTP DETECTION
      //#############
      Logger.Info("Detecting UFT");
      AddDataPair("HP Unified Functional Testing", "Is QTP/UFT installed?", DetectOtherSoftware.GetUFTInstallationInfo());




      //#############
      //SITESCOPE DETECTION
      //#############
      if (FormArguments.details >= 2)
      {
        Logger.Info("Collecting SiteScope information");
        AddDataPair("HP SiteScope", "Is SiteScope installed?", DetectOtherSoftware.GetSiteScopeInfo());
      }
    }
  }
}
