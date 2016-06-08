using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class NVShunraCollector : Collector
  {
    public override string Title { get { return "Network Virtualization"; } }
    public override int Order { get { return 110; } }
    protected override bool Enabled
    {
      get { return !(ProductDetection.Vugen.IsInstalled && ProductDetection.Vugen.version >= new Version(12,50)
        || !(ProductDetection.pcServer.IsInstalled && ProductDetection.pcServer.version >= new Version(12,50)));
      }
    }
    protected override void Collect()
    {

      var info = NVShunraCollectorHelper.IsShunraInstalledFor("Controller");
      AddDataPair("Shunra for HP Controller", "Is installed?", info);
      if (info.Contains("Yes"))
        AddDataPair("Shunra for HP Controller", "Details", NVShunraCollectorHelper.ProductDetails);
      
      info = NVShunraCollectorHelper.IsShunraInstalledFor("PC Server");
      AddDataPair("Shunra for HP PC Server", "Is installed?", info);
      if (info.Contains("Yes"))
        AddDataPair("Shunra for HP PC Server", "Details", NVShunraCollectorHelper.ProductDetails);

      info = NVShunraCollectorHelper.IsShunraInstalledFor("Load Generator");
      AddDataPair("Shunra for HP LG", "Is installed?", info);
      if (info.Contains("Yes"))
        AddDataPair("Shunra for HP LG", "Details", NVShunraCollectorHelper.ProductDetails);
        
    }
  }
}
