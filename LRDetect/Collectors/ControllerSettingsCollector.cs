using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class ControllerSettingsCollector : Collector
  {
    public override string Title { get { return "Controller settings";  } }
    public override int Order { get { return 90; } }

    protected override bool Enabled
    {
      get { return ProductDetection.FullLR.IsInstalled; }
    }

    protected override void Collect()
    {
      var filePath = Path.Combine(ProductDetection.FullLR.ConfigFolder, "wlrun7.ini");
      AddDataPair("Configuration files", "wlrun7.ini file content", Html.AddLinkToHiddenContent(IniParser.ToHtml(filePath)));
      AddDataPair("Connections", "Connected load generators", ControllerSettingsCollectorHelper.GetConnectedLGs());

      //filePath = Path.Combine(ProductDetection.FullLR.WebControllerFolder, @"webapp\server\lib\config\configuration.json");
      //AddDataPair("Web Controller", "Settings", Html.AddLinkToHiddenContent(ControllerSettingsCollectorHelper.WebControllerPorts()));
    }
  }
}
