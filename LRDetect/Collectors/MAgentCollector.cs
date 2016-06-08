using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class MAgentCollector : Collector
  {
    public override int Order { get { return 50; } }

    protected override void Collect()
    {
      MagentCollectorHelper magentInfo = new MagentCollectorHelper();
      string title = "HP Load Testing Agent";

      AddDataPair(title, "Name of installed agent", magentInfo.IsAgentInstalledInfo());
      AddDataPair(title, "Status", magentInfo.GetAgentStatus());
      AddDataPair(title, "Last 10 lines of agent's log:", magentInfo.GetLastLinesFromAgentLog());
      if (magentInfo.isInstalled)
      {
        AddDataPair(title, "Enabled firewall agent?", magentInfo.IsFirewallAgentEnabledInfo());
        AddDataPair(title, "Enabled terminal services?", magentInfo.IsTerminalServicesEnabledInfo());
      }
      if (magentInfo.isAgentInstalledAsService)
      {
        AddDataPair(title, "Remote Management Agent service", PCServicesCollectorHelper.GetServiceInfo("RemoteManagementAgent"));
      }

    }
    public override string Title
    {
      get { return "LoadRunner Agent"; }
    }
  }
}
