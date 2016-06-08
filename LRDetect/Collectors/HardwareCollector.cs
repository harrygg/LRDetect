using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class HardwareCollector : Collector
  {
    public override string Title
    {
      get { return FormArguments.network ? "Hardware & Network" : "Hardware";  }
    }
    public override int Order { get { return 20; } }


    protected override void Collect()
    {
      var dh = new DataHolder("Hardware Information");
      dh.dataPairs.Add("CPU", OSCollectorHelper.GetProcessorNameString());
      dh.dataPairs.Add("Processor Count", Environment.ProcessorCount.ToString());
      dh.dataPairs.Add("Total Memory", OSCollectorHelper.GetMemoryInfo());
      dh.dataPairs.Add("Hard Drives", OSCollectorHelper.GetHardDrivesInformation());
      dh.dataPairs.Add("Monitor information", OSCollectorHelper.GetMonitorsInfo());
      //IPCONFIG /ALL
      if (FormArguments.network || FormArguments.details >= 3)
      {
        dh.dataPairs.Add("Network cards & IPs", OSCollectorHelper.GetNetworkCardsInfo());
        dh.dataPairs.Add("Output of 'ipconfig /all' command", Html.AddLinkToHiddenContent(Html.Pre(OSCollectorHelper.IpConfig())));
      }

      dataHolders.Add(dh);
    }
  }
}
