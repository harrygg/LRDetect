using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.IO;
using System.Management;
using System.Diagnostics;
using System.Reflection;

namespace LRDetect
{

    /// <summary>
    /// Class to collect information about LR/PC process/service. 
    /// It checks if agent is installed as service or process or both. 
    /// Then it checks if the process is running. It detects if more than one process is running.
    /// Ouputs the ports opened for the running process
    /// </summary>
    
    class MagentCollectorHelper
    {
        #region Properties
        string lrAgentServiceCaption = "LoadRunner Agent Service";
        string pcAgentServiceCaption = "Performance Center Agent Service";
        // string agentServiceName = "magentservice";
        public bool isAgentInstalledAsService = false;
        bool isAgentServiceRunning = false;
        string AgentServiceOpenedPorts = String.Empty;

        string lrAgentProcessCaption = "LoadRunner Agent Process";
        //private string lrAgentProcessCaption = "Agent Process";
        string pcAgentProcessCaption = "Performance Center Agent Process";
        // will be defined if any of the above is installed
        public string installedAgentName = String.Empty;
        public bool isAgentInstalledAsProcess = false;
        string agentProcessName = "magentproc";
        bool isAgentProcessRunning = false;
        string AgentProcessOpenedPorts = String.Empty;
        public bool isInstalled { get { return isAgentInstalledAsService || isAgentInstalledAsProcess; } }
        int agentProcessId = 0;
        List<int> agentProcessIds = new List<int>();
        string agentProcessPath = String.Empty;
        List<string> agentProcessPaths = new List<string>();
        string agentProcessOwnder = String.Empty;
        List<string> agentProcessOwnders = new List<string>();
        //private static StringBuilder openedPortDetailsFromCMD = new StringBuilder();

        // will be defined if any of the above is true
        //private string runningAgentName = String.Empty;
        // will be defined if the number of running agents is more than 1 (i.e. we have 2 magentservice.exe)
        private int numberOfRunningAgents = 0;

        #endregion
        
        #region Default constructor
        public MagentCollectorHelper()
        {
          try
          {
            // check if LR/PC agent is installed as a service
            isAgentInstalledAsService = IsAgentServiceInstalledAndRunning(lrAgentServiceCaption) || IsAgentServiceInstalledAndRunning(pcAgentServiceCaption);
            // check if LR/PC agent is installed as a process
            isAgentInstalledAsProcess = IsAgentProcessInstalled(lrAgentProcessCaption) || IsAgentProcessInstalled(pcAgentProcessCaption);
            //check if LR/PC agent process is running
            isAgentProcessRunning = IsAgentRunning(agentProcessName);
            //if the procses is running get the ports it is using
            if (isAgentProcessRunning)
              AgentProcessOpenedPorts = Html.br + Html.B("Details:") + Html.br + GetOpenedPortsInfo();
            //check if agent is running as a service only if it is installed as a service
            //to avoid confusion with diagnostics agent which has the same name
            if (isAgentInstalledAsService)
            {
              if (isAgentServiceRunning)
              {
                numberOfRunningAgents = 1;
                AgentServiceOpenedPorts = Html.br + Html.B("Details:") + Html.br + GetOpenedPortsInfo();
              }
            }
          }
          catch (Exception ex)
          {
              Logger.Error(ex.ToString());
          }
        }
        #endregion

        #region Method to check if LR/PC agent service is installed
        /// <summary>
        /// Check if Agent is installed as service. To do that we create an instance of the service
        /// if the service is missing Exception will be raised false will be returned. Else an instance will be created
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns>Returns true or false</returns>
        bool IsAgentServiceInstalledAndRunning(string agentServiceCaption)
        {
          //TODO test tasklist /svc /fi "SERVICES eq mcafeeframework"
          //Logger.Debug("Starting IsAgentServiceInstalledAndRunning()");
          Logger.Debug("Executing " + GetType().Name + "." + MethodBase.GetCurrentMethod().Name);
          try
          {
            // If the service is not installed an Exception will be raised when trying to get its status
            ServiceController sc = new ServiceController(agentServiceCaption);

            switch (sc.Status)
            {
              case ServiceControllerStatus.Running:
              case ServiceControllerStatus.StartPending:
              case ServiceControllerStatus.StopPending:
                isAgentServiceRunning = true;

                var magentObj = Helper.GetWMIObject("root\\CIMV2", "Win32_Service", "WHERE Caption='" + agentServiceCaption + "'");

                string processId = magentObj["ProcessId"].ToString(); 
              
                if (!processId.Contains("Error"))
                {
                  agentProcessId = Convert.ToInt32(processId);
                  agentProcessOwnder = Helper.GetProcessOwner(agentProcessId);
                }
                //runningAgentName = agentServiceName;
                agentProcessPath = magentObj["PathName"].ToString();
                break;
              case ServiceControllerStatus.Stopped:
              case ServiceControllerStatus.Paused:
                isAgentServiceRunning = false;
                break;
            }

            installedAgentName = agentServiceCaption;
            return true;
          }
          catch (InvalidOperationException)
          {
            Logger.Warn(agentServiceCaption + " is not installed as a service");
            return false;
          }
          catch (Exception ex)
          {
            Logger.Warn(ex.ToString());
            return false;
          }
          finally
          {
            Logger.Debug("Ended IsAgentServiceInstalledAndRunning()");
          }
        }
        #endregion

        #region Method to check if LR/PC agent process is installed
        /// <summary>
        /// Check if Agent is installed as process. If it is it should be in the Windows Start up folder. 
        /// To do that we query the start up programs for the specific captions
        /// </summary>
        ///param name="agentProcessCaption" = either loadrunner or pc agent.
        /// <returns>Returns true or false</returns>
        bool IsAgentProcessInstalled(string agentProcessCaption)
        {
          Logger.Debug("Starting IsAgentProcessInstalled");

          try
          {
            string value = RegistryWrapper.GetValue(RegHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", lrAgentProcessCaption);
            if (value != null)
            {
              installedAgentName = agentProcessCaption;
              return true;
            }
            else
            {
              var queryObj = Helper.GetWMIObject("root\\CIMV2", "Win32_StartupCommand", "WHERE (Caption LIKE '%Agent Process%' OR Caption LIKE '%magent%')");
                if (queryObj != null)
                {
                  Logger.Info("Query for Agent installation: Agent caption: " + queryObj["Caption"] + ", command: " + queryObj["Command"]);
                  installedAgentName = agentProcessCaption;
                  return true;
                }
              return false;
            }
          }
          catch (Exception ex)
          {
            Logger.Warn(ex.ToString());
            return false;
          }
          finally
          {
            Logger.Debug("Ended IsAgentProcessInstalled");
          }
        }
        #endregion

        #region Method to determine if a process is running
        /// <summary>
        /// Check if we have a running process with the name processName. It might be magentproc.exe or magentservice.exe. 
        /// </summary>
        /// <returns>Returns true of false</returns>
        private bool IsAgentRunning(string processName)
        {
            try
            {
                Process[] process = Process.GetProcessesByName(processName);
                numberOfRunningAgents = process.Length;
                //if (numberOfRunningAgents != 0)
                //{
                //    runningAgentName = processName;
                //}
                if (numberOfRunningAgents == 1)
                {
                    agentProcessId = process[0].Id;
                    agentProcessPath = process[0].Modules[0].FileName;
                    agentProcessOwnder = Helper.GetProcessOwner(process[0].Id);

                    return true;
                }
                // if there are more than 1 agents running return their process ids as a list of int
                if (numberOfRunningAgents > 1)
                {
                    for (int i = 0; i < numberOfRunningAgents; i++)
                    {
                        agentProcessIds.Add(process[i].Id);
                        agentProcessPaths.Add(process[i].Modules[0].FileName);
                        agentProcessOwnders.Add(Helper.GetProcessOwner(process[i].Id));
                    }
                    return true;
                }
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                Logger.Warn("No process with name " + processName + " was detected");
                return false;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Logger.Warn("No process " + processName + ".exe found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
                return false;
            }
        }
        #endregion 
        
        #region Method to get a list of all opened ports for a given process id
        string GetOpenedPortsInfo()
        {
            try
            {
                StringBuilder info = new StringBuilder();

                if (numberOfRunningAgents == 1)
                {
                    info.Append(Html.U("PID: " + agentProcessId + " " + agentProcessOwnder) + Html.br
                        + "Path: " + agentProcessPath + Helper.GetOpenedPortsForProcessId(agentProcessId));
                }
                if (numberOfRunningAgents > 1)
                {

                    for (int i = 0; i < numberOfRunningAgents; i++)
                    {
                        info.Append(Html.U("PID: " + agentProcessIds[i] + " " + agentProcessOwnders[i])
                            + "Path " + Html.I(agentProcessPaths[i]) + Helper.GetOpenedPortsForProcessId(agentProcessIds[i]));
                    }
                }
                // add the raw output to the DOM in case for debug purposes
                //if (numberOfRunningAgents > 0)
                //{
                //    info.Append(Html.Div(openedPortDetailsFromCMD.ToString(), "ports", "dontShow"));
                //}
                return info.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }
        #endregion 


        #region Method to get the last lines from Agent log
      /// <summary>
        /// PC LG 12.02 installed as a service saves 2 logs: 
        /// C:\Program Files (x86)\HP\Load Generator\Temp\LoadRunner_agent_service.log
        /// C:\Program Files (x86)\HP\Load Generator\Temp\RemoteManagement_agent_service.log
        /// 
        /// LR LG 12.02 installed as a process saves log in 
        /// %Temp%\LoadRunner_agent_startup.log
      /// </summary>
      /// <returns></returns>
        public string GetLastLinesFromAgentLog()
        {
          try
          {
            string pathToAgentStartUpLog = Path.GetTempPath();
            string nameOfAgentFile = "LoadRunner";

            // if we have a load generator product installed which means PC Host and LoadRunner as well
            if (ProductDetection.Loadgen.IsInstalled)
            {
              if (ProductDetection.Loadgen.ProductName.Contains("Performance"))
                nameOfAgentFile = "Performance Center";

              if (ProductDetection.Loadgen.ProductName.Contains("LoadRunner") || ProductDetection.Loadgen.ProductName.Contains("Generator"))
              {
                pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
                Logger.Info("Searching for Magent log file in: " + pathToAgentStartUpLog);
              }

              if (ProductDetection.Loadgen.ProductName.Contains("Performance"))
              {
                //if the agent is installed as a service, usually for 11.04 and below version the log goes to C:\tmp else it goes to %lg_path%\Temp
                if (isAgentInstalledAsService)
                {
                  if (ProductDetection.Loadgen.isNew)
                  {
                    pathToAgentStartUpLog = ProductDetection.Loadgen.InstallLocation + @"Temp\" + nameOfAgentFile + "_agent_service.log";
                    Logger.Info("Searching for Magent log file in: " + pathToAgentStartUpLog);
                  }
                  else
                  {
                    pathToAgentStartUpLog = @"C:\tmp\" + nameOfAgentFile + "_agent_service.log";
                    Logger.Info("Searching for Magent log file in: " + ProductDetection.Loadgen.InstallLocation + pathToAgentStartUpLog);
                  }
                }
                else if (isAgentInstalledAsProcess)
                  pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
                else
                  pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
              }
            }
            else
            {
              pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
            }
            StringBuilder output = new StringBuilder();

            output.Append(Html.AddLinkToHiddenContent(
              Helper.GetLastLinesFromFile(1024 * 1024, pathToAgentStartUpLog, 10)));

            return output.ToString();
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return ex.Message;
          }
        }
        #endregion

        #region Method to output the collected information in the necessarry format
        /*public override string ToString()
        {
            try
            {

                StringBuilder output = new StringBuilder(1024);
                if (isAgentProcessRunning && isAgentServiceRunning)
                {
                    output.Append(Html.Error("The agent is running both as a process and a service!")
                        + AgentServiceOpenedPorts + AgentProcessOpenedPorts);
                }
                else
                {
                  if (isAgentProcessRunning || isAgentServiceRunning)
                    output.Append("Currently running." + AgentServiceOpenedPorts + AgentProcessOpenedPorts);
                  else
                    output.Append("Not running!");
                }

                return output.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return ex.Message;
            }
        }*/
        #endregion 
    
       #region Method to display the agent status information
        internal string GetAgentStatus()
        {
          try
          {

            StringBuilder output = new StringBuilder(1024);
            if (isAgentProcessRunning && isAgentServiceRunning)
            {
              output.Append(Html.Error("The agent is running both as a process and a service!")
                  + AgentServiceOpenedPorts + AgentProcessOpenedPorts);
            }
            else
            {
              if (isAgentProcessRunning || isAgentServiceRunning)
                output.Append("Status: " + Html.Notice("Running") + AgentServiceOpenedPorts + AgentProcessOpenedPorts);
              else
                output.Append("Not running!");
            }

            return output.ToString();
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return ex.Message;
          }
        }
      #endregion

        #region Is agent installed
        internal string IsAgentInstalledInfo()
        {
          StringBuilder info = new StringBuilder(1024);
          // check if agent is installed as service/process
          if (isAgentInstalledAsProcess && isAgentInstalledAsService)
            info.Append(Html.Error("The agent is installed both as a process and a service!" + Html.br));
          else
          {
            if (isInstalled)
              info.Append("Yes, " + Html.B(installedAgentName) + " is installed. ");
            else
              info.Append("The agent is not installed! ");
          }
          return info.ToString();
        }
      #endregion

        internal string IsFirewallAgentEnabledInfo()
        {
          string path = Path.Combine(ProductDetection.Loadgen.InstallLocation, @"launch_service\dat\br_lnch_server.cfg");
          try
          {
            var ini = new IniParser(path);
            bool res = ini.GetBoolSetting("Citrix", "CitrixIsActive", false);
            return Html.BoolToYesNo(res);
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return Html.Error("Configuration file not found!");
          }
        }

        internal string IsTerminalServicesEnabledInfo()
        {
          string path = Path.Combine(ProductDetection.Loadgen.InstallLocation, @"launch_service\dat\br_lnch_server.cfg");
          try
          {
            var ini = new IniParser(path);
            bool res = ini.GetBoolSetting("FireWall", "FireWallServiceActive", false);
            return Html.BoolToYesNo(res);
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return Html.Error("Configuration file not found!");
          }
        }
    }
}
