using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class VugenProtocolsCollector : Collector
  {
    public override string Title { get { return "Various protocols settings"; } }
    public override int Order { get { return 80; } }

    protected override bool Enabled
    {
      get { return ProductDetection.Vugen.IsInstalled; }
    }

    protected override void Collect()
    {
      var ro = new DetectRecordingOptions();

      base.OnRaiseProgressUpdate();

      var title = "WEB (HTTP/HTML)";
      AddDataPair(title, "Last used recording options", "User " + Html.B(Environment.UserName) + "  " + Html.AddLinkToHiddenContent(ro.ToString()));

      if (ProductDetection.Vugen.isNew)
      {
        title = "TruClient IE";
        AddDataPair(title, "Is enabled?", VugenProtocolsCollectorHelper.TruClientIE.IsEnabled());
        AddDataPair(title, "RRE version", VugenProtocolsCollectorHelper.TruClientIE.GetTCIEVersion());
        AddDataPair(title, "General > Browser settings", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.TruClientIE.GetGeneralBrowserSettings()));
        AddDataPair(title, "General > Interactive options", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.TruClientIE.GetInteractiveOptions()));
      }

      if (ProductDetection.Vugen.version > new Version("11.00"))
      {
        title = "TruClient Firefox";
        AddDataPair(title, "Firefox version", VugenProtocolsCollectorHelper.TruClientFF.GetFirefoxVersion());
        AddDataPair(title, "General > Browser settings", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.TruClientFF.GetBrowserSettings()));
        AddDataPair(title, "General > Interactive options", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.TruClientFF.GetInteractiveOptions()));
        if (ProductDetection.Vugen.version >= new Version("11.50"))
          AddDataPair(title, "Lists DACLs", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.TruClientFF.GetUrlAclInfo()));
      }

      //TODO test on 11
      title = "Java protocols";
      AddDataPair(title, "Classpath", VugenProtocolsCollectorHelper.Java.GetJavaIniOption("Java_Env_ClassPath"));
      AddDataPair(title, "VM Params", VugenProtocolsCollectorHelper.Java.GetJavaIniOption("Java_VM_Params"));
      AddDataPair(title, "Use VM params during replay?", VugenProtocolsCollectorHelper.Java.GetJavaIniBoolOption("Java_SaveParams"));
      AddDataPair(title, "Use classic Java", VugenProtocolsCollectorHelper.Java.GetJavaIniBoolOption("Java_Classic"));
      AddDataPair(title, "Prepend classpath to -Xbootclasspath", VugenProtocolsCollectorHelper.Java.GetJavaIniBoolOption("Java_Prepend_Classpath"));

      title = "Citrix";
      var ctrxClient = new CitrixHelper.Client();
      AddDataPair(title, "Is Citrix client installed?", ctrxClient.GetCitrixClientInfo());
      //if (citrixClient.isInstalled)
      //  AddDataPair(title, "Is client version supported?", citrixClient.GetClientVersionSupportedInfo());
      AddDataPair(title, "Is Citrix registry patch installed?", ctrxClient.GetCitrixRegistryPatchInfo());
      AddDataPair(title, "Recording options", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.Citrix.GetCitrixRecOptions()));
      AddDataPair(title, "Citrix_XenApp correlation rules enabled?", CorrelationRules.IsGroupEnabledText("Citrix_XenApp"));

      title = "Dot Net";
      AddDataPair(title, "Recording options", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.DotNet.GetDotNetRecOptions()));
      AddDataPair(title, "Filters", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.DotNet.GetDotNetFilters()));

      title = "RDP";
      AddDataPair(title, "RDP client version", DetectOtherSoftware.GetRDPClientVersion());
      AddDataPair(title, "RDP recording options", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.Rdp.GetRdpRecOptions()));

      title = "FLEX, AMF";
      AddDataPair(title, "Recording options", Html.AddLinkToHiddenContent(VugenProtocolsCollectorHelper.FlexAmf.GetFlexRoInfo()));
      AddDataPair(title, "Other settings", VugenProtocolsCollectorHelper.FlexAmf.GetFlexInfo());
      AddDataPair(title, "Flex correlation rules enabled?", CorrelationRules.IsGroupEnabledText("Flex"));

      title = "Siebel Web";
      AddDataPair(title, "Is Siebel correlation library used?", VugenProtocolsCollectorHelper.SiebelWeb.GetSiebelDllVersionInfo());
      AddDataPair(title, "Correlation rules", VugenProtocolsCollectorHelper.SiebelWeb.GetIsSiebelCorrelationEnabledInfo());
    }
  }
}
