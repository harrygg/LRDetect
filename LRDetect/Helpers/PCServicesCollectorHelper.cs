using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace LRDetect
{
  class PCServicesCollectorHelper
  {
    public static string GetServiceInfo(string serviceName, string processName = "")
    {
      try
      {
        if (processName == "")
          processName = serviceName;

        int agentProcessId = 0;
        string agentProcessOwnder = string.Empty;
        string agentProcessPath = string.Empty;


        ServiceController sc = new ServiceController(serviceName);

        Logger.Info("status " + serviceName + sc.Status);
        switch (sc.Status)
        {
          case ServiceControllerStatus.Running:
          case ServiceControllerStatus.StartPending:
          case ServiceControllerStatus.StopPending:
            string processId = Helper.QueryWMI("ProcessId", "root\\CIMV2", "Win32_Service", "WHERE Name='" + serviceName + "'");
            Logger.Info("ProcessId for  " + processName + " is " + processId);
            if (!processId.Contains("Error") && !processId.Contains("Not detected"))
            {
              agentProcessId = Convert.ToInt32(processId);
              agentProcessOwnder = Helper.GetProcessOwner(agentProcessId);
            }
            agentProcessPath = Helper.QueryWMI("PathName", "root\\CIMV2", "Win32_Service", "WHERE Name='" + serviceName + "'");
            Logger.Info("agentProcessPath for  " + processName + " is " + agentProcessPath);
            break;
          case ServiceControllerStatus.Stopped:
          case ServiceControllerStatus.Paused:
            break;
        }


        var status = sc.Status.ToString().Contains("Running") ? Html.Notice(sc.Status.ToString()) : sc.Status.ToString();
        status = status.Contains("Stopped") || status.Contains("Paused") ? Html.Error(status) : status;

        var openedPorts = agentProcessId != 0 ? Helper.GetOpenedPortsForProcessId(agentProcessId) : "";

        return "Status: " + status + Html.br
          + Html.B("Details: ") + Html.br
          + Html.U("PID: " + agentProcessId + " " + agentProcessOwnder) + Html.br
          + "Path: " + agentProcessPath + openedPorts;
      }
      catch (InvalidOperationException)
      {
        return Html.Warning("Service not installed on this computer!");
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return ex.ToString();
      }
    }
  }
}
