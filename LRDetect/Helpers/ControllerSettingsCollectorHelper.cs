using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class ControllerSettingsCollectorHelper
  {
    public static string GetConnectedLGs()
    {
      try
      {
        var commandOutput = "None";
        Process[] process = Process.GetProcessesByName("wlrun");
        if (process.Length > 0)
        {
          var command = String.Format("netstat -ano | findstr /e {0}", process[0].Id);
          //var agentProcessPath = process[0].Modules[0].FileName;
          //var agentProcessOwnder = Helper.GetProcessOwner(process[0].Id);
          commandOutput = Helper.ExecuteCMDCommand(command);
          Logger.Info(commandOutput);

          commandOutput = FormatOutput(commandOutput, process[0].Id.ToString());
        }
        return commandOutput;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }

    internal static string FormatOutput(string commandOutput, string pid)
    {
      StringBuilder output = new StringBuilder();

      if (commandOutput.Length > 0)
      {
        string[] lines = commandOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        output.Append(lines.Length - 1 + " connections found" + Html.br);
        //output.Append("TYPE CONTROLLER_IP:PORT LG_IP:PORT" + Html.br);
        foreach (var line in lines)
          output.Append(line.Replace(pid, Html.br).Replace("TCP", Html.B("TCP")).Replace("ESTABLISHED", Html.Notice("ESTABLISHED")).Replace("HERGESTELLT", Html.Notice("HERGESTELLT")));
      }
      
      return output.ToString();
    }

    internal static string WebControllerPorts()
    {
      return "";
    }
  }
}
