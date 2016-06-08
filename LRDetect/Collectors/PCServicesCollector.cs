using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class PCServicesCollector : Collector
  {
    public override string Title { get { return "Performance Center services"; } }
    public override int Order { get { return 60; } }
    protected override bool Enabled
    {
      get { return ProductDetection.pcServer.IsInstalled || ProductDetection.isPCHostInstalled; }
    }

    protected override void Collect()
    {
      var title = "Performance Center Server";
      if (ProductDetection.pcServer.IsInstalled)
      {
        AddDataPair(title, "Data Collection Agent service", PCServicesCollectorHelper.GetServiceInfo("DataCollectionAgent"));
        AddDataPair(title, "Remote Management Agent service", PCServicesCollectorHelper.GetServiceInfo("RemoteManagementAgent"));
      }
      if (ProductDetection.isPCHostInstalled)
      {
        AddDataPair(title, "Data Collection Agent service", PCServicesCollectorHelper.GetServiceInfo("DataCollectionAgent"));
        AddDataPair(title, "Remote Management Agent service", PCServicesCollectorHelper.GetServiceInfo("RemoteManagementAgent"));
        AddDataPair(title, "Performance Center Agent service", PCServicesCollectorHelper.GetServiceInfo("Performance CenterAgent"));
        AddDataPair(title, "Performance Center Load Testing service", PCServicesCollectorHelper.GetServiceInfo("Performance Center Load Testing Service"));
      }
    }
  }
}
