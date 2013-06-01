using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceProcess;
using System.IO;
using System.Management;
using System.Diagnostics;

namespace LRDetect
{

    /// <summary>
    /// Class to collect information about LR/PC process/service. 
    /// It checks if agent is installed as service or process or both. 
    /// Then it checks if the process is running. It detects if more than one process is running.
    /// Ouputs the ports opened for the running process
    /// </summary>
    
    class MagentInfo
    {
        #region Properties
        private ProductInfo installedLoadGen = null;
        private string lrAgentServiceCaption = "LoadRunner Agent Service";
        private string pcAgentServiceCaption = "Performance Center Agent Service";
        //private string agentServiceName = "magentservice";
        private bool isAgentInstalledAsService = false;
        private bool isAgentServiceRunning = false;
        private string AgentServiceOpenedPorts = String.Empty;

        private string lrAgentProcessCaption = "LoadRunner Agent Process";
        //private string lrAgentProcessCaption = "Agent Process";
        private string pcAgentProcessCaption = "Performance Center Agent Process";
        // will be defined if any of the above is installed
        private string installedAgentName = String.Empty;
        private bool isAgentInstalledAsProcess = false;
        private string agentProcessName = "magentproc";
        private bool isAgentProcessRunning = false;
        private string AgentProcessOpenedPorts = String.Empty;

        private int agentProcessId = 0;
        private List<int> agentProcessIds = new List<int>();
        private string agentProcessPath = String.Empty;
        private List<string> agentProcessPaths = new List<string>();
        private string agentProcessOwnder = String.Empty;
        private List<string> agentProcessOwnders = new List<string>();
        private static StringBuilder openedPortDetailsFromCMD = new StringBuilder();

        // will be defined if any of the above is true
        //private string runningAgentName = String.Empty;
        // will be defined if the number of running agents is more than 1 (i.e. we have 2 magentservice.exe)
        private int numberOfRunningAgents = 0;

        #endregion
        
        #region Default constructor
        public MagentInfo(ProductInfo loadgen)
        {
            try
            {

                //installedLoadGen should contain a ProductInfo object which we'll need later for the log files
                //Could be PC Host, LR or LG stand alone
                installedLoadGen = loadgen;
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
                Log.Error(ex.ToString());
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
        private bool IsAgentServiceInstalledAndRunning(string agentServiceCaption)
        {
            try
            {
                // If the service is not installed an Exception will be raised when trying to get its status
                ServiceController sc = new ServiceController(agentServiceCaption);
                //ServiceControllerStatus status = new ServiceController(agentServiceCaption).Status;

                switch (sc.Status)
                {
                    case ServiceControllerStatus.Running:
                    case ServiceControllerStatus.StartPending:
                    case ServiceControllerStatus.StopPending:
                        isAgentServiceRunning = true;
                        string processId = Helper.QueryWMI("ProcessId", "root\\CIMV2", "Win32_Service", "WHERE Caption='" + agentServiceCaption + "'");
                        if (!processId.Contains("Error"))
                        {
                            agentProcessId = Convert.ToInt32(processId);
                            agentProcessOwnder = Helper.GetProcessOwner(agentProcessId);
                        }
                        //runningAgentName = agentServiceName;
                        agentProcessPath = Helper.QueryWMI("PathName", "root\\CIMV2", "Win32_Service", "WHERE Caption='" + agentServiceCaption + "'");

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
                Log.Warn(agentServiceCaption + " is not installed as a service");
                return false;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return false;
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
        private bool IsAgentProcessInstalled(string agentProcessCaption)
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_StartupCommand");
                foreach (ManagementObject queryObj in searcher.Get())
                {
                    string caption = queryObj["Caption"].ToString();
                    string command = queryObj["Command"].ToString();
                    if ((caption.Contains("Agent Process") || caption.Contains("magent") || caption.Contains("MAGENT"))
                        &&
                        (command.Contains("MAGENT") || command.Contains("magent")))
                    {
                        Log.Info("Query for Agent installation: Agent caption: " + caption + ", command: " + command);
                        installedAgentName = agentProcessCaption;
                        return true;
                    }
                }
                return false;
            }
            catch (ManagementException ex)
            {
                Log.Warn(ex.ToString());
                return false;
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
                Log.Warn("No process with name " + processName + " was detected");
                return false;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                Log.Warn("No process " + processName + ".exe found");
                return false;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return false;
            }
        }
        #endregion 
        
        #region Method to get a list of all opened ports for a given process id
        private string GetOpenedPortsInfo()
        {
            try
            {
                StringBuilder info = new StringBuilder();

                if (numberOfRunningAgents == 1)
                {
                    info.Append(Html.U("PID: " + agentProcessId + " " + agentProcessOwnder) + Html.br
                        + "Path: " + agentProcessPath + GetOpenedPortsForProcessId(agentProcessId));
                }
                if (numberOfRunningAgents > 1)
                {

                    for (int i = 0; i < numberOfRunningAgents; i++)
                    {
                        info.Append(Html.U("PID: " + agentProcessIds[i] + " " + agentProcessOwnders[i])
                            + "Path " + Html.I(agentProcessPaths[i]) + GetOpenedPortsForProcessId(agentProcessIds[i]));
                    }
                }
                // add the raw output to the DOM in case for debug purposes
                if (numberOfRunningAgents > 0)
                {
                    //info.Append(Html.br + "Raw output " + Html.LinkShowContent("ports") + Html.br);
                    info.Append("\n\t<div id=\"ports\" class=\"dontShow\">"
                        + openedPortDetailsFromCMD.ToString() + "\n\t</div>");
                }
                return info.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
        }
        #endregion 

        #region Get all opened ports for given process id
        /// <summary>
        /// Method to execute netstat command to list the ports opened for given process id.
        /// 1. Execute the netstat -ano | findstr /e process_id (matches all lines ending with process_id
        /// 2. Create an array by splitting the output by \r, \n, \t, ' ', ':' 
        /// 3. Loop through the array values and if valuee==process_id get the necessary information
        /// </summary>
        /// <param name="processId"></param>
        /// <returns></returns>
        public static string GetOpenedPortsForProcessId(int processId)
        {
            try
            {
                StringBuilder portInfo = new StringBuilder(128);
                if (processId > 0)
                {
                    string netstatOutput = Helper.ExecuteCMDCommand("netstat -anop tcp | findstr /e " + processId);
                    openedPortDetailsFromCMD.Append(netstatOutput);

                    if (netstatOutput.Contains(processId.ToString()))
                    {
                        Log.Info("Ports for process ID " + processId + ":\r\n" + netstatOutput);
                        // split the output by \r\n
                        char[] delimiter = new Char[] { ' ', '\t', '\r', ':' };
                        string[] parts = netstatOutput.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (parts[i] == processId.ToString())
                            {
                                string status = String.Empty;
                                switch (parts[i - 1])
                                {
                                    case "LISTENING": //english
                                    case "ABH™REN":   //german
                                        status = Html.Notice("LISTENING");
                                        break;
                                    case "ESTABLISHED": //english
                                    case "HERGESTELLT": //german
                                        status = Html.B("ESTABLISHED");
                                        break;
                                    default:
                                        status = parts[i - 1];
                                        break;
                                }
                                portInfo.Append(Html.br + "Port: " + parts[i - 4] + " " + status);
                            }
                        }
                    }
                }
                return portInfo.ToString() + Html.br;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return null;
            }
        }
        #endregion


        private string GetLastLinesFromAgentLog()
        {
            try
            {
                string pathToAgentStartUpLog = Environment.GetEnvironmentVariable("TEMP") + @"\";
                string nameOfAgentFile = "LoadRunner";


                // if we have a load generator product installed
                if (this.installedLoadGen.IsProductInstalled)
                {
                    Log.Info(this.installedLoadGen.ProductName + this.installedLoadGen.ProductVersion + " detected.");
                    if (this.installedLoadGen.ProductName.Contains("Performance"))
                    {
                        nameOfAgentFile = "Performance Center";
                    }

                    if (this.installedLoadGen.ProductName.Contains("LoadRunner") || installedLoadGen.ProductName.Contains("Generator"))
                    {
                        pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
                        Log.Info("Searching for Magent log file in: " + pathToAgentStartUpLog);
                    }

                    if (this.installedLoadGen.ProductName.Contains("Performance"))
                    {
                        //if the agent is installed as a service, usually for 11.04 and below version the log goes to C:\tmp else it goes to pc_folder\temp
                        if (isAgentInstalledAsService)
                        {
                            ////if agent is not earlier than 11.50 (if yes CompareTo returns -1)
                            if (this.installedLoadGen.version.CompareTo(new Version("11.50")) != -1)
                            {
                                Log.Info("Agent installed as service");
                                pathToAgentStartUpLog = this.installedLoadGen.InstallLocation + @"Temp\" + nameOfAgentFile + "_agent_service.log";
                                Log.Info("Searching for Magent log file in: " + this.installedLoadGen.InstallLocation + @"\Temp\" + nameOfAgentFile + "_agent_service.log");
                            }
                            else
                            {
                                //I will hardcode the c:\tmp as there is no such env. variable 
                                pathToAgentStartUpLog = @"C:\tmp\" + nameOfAgentFile + "_agent_service.log";
                                Log.Info(this.installedLoadGen.ProductName + this.installedLoadGen.ProductVersion + " detected.");
                                Log.Info("Searching for Magent log file in: " + this.installedLoadGen.InstallLocation + pathToAgentStartUpLog);
                            }
                        }
                        else if (isAgentInstalledAsProcess)
                        {
                            //if installed as process the log goes to user %temp% folder
                            Log.Info("Agent installed as process");
                            pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
                        }
                        else
                        {
                            pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
                        }
                    }
                }
                else
                {
                    pathToAgentStartUpLog += nameOfAgentFile + "_agent_startup.log";
                }
                StringBuilder output = new StringBuilder();

                output.Append(Html.br + "Last 10 lines of the agent start up log: " + Html.LinkShowContent("agentLog"));
                output.Append(Html.br + "\n\t<div id=\"agentLog\" class=\"dontShow\">\n\t" + Helper.GetLastLinesFromFile(1024 * 1024, pathToAgentStartUpLog, 10) + "\n\t</div>");

                return output.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }


        #region Method to output the collected information in the necessarry format
        public override string ToString()
        {
            try
            {

                StringBuilder info = new StringBuilder(1024);
                // check if agent is installed as service/process
                if (isAgentInstalledAsProcess && isAgentInstalledAsService)
                {
                    info.Append(Html.Error("The agent is installed both as a process and a service!" + Html.br));
                }
                else
                {
                    if (isAgentInstalledAsService || isAgentInstalledAsProcess)
                    {
                        info.Append("Yes, " + Html.B(installedAgentName) + " is installed. ");
                    }
                    else
                    {
                        info.Append("The agent is not installed! ");
                    }
                }
                // check if the process is currently running
                if (isAgentProcessRunning && isAgentServiceRunning)
                {
                    info.Append(Html.Error("The agent is running both as a process and a service!")
                        + AgentServiceOpenedPorts + AgentProcessOpenedPorts);
                }
                else
                {
                    if (isAgentProcessRunning || isAgentServiceRunning)
                    {
                        info.Append(/*Html.B(runningAgentName) + */"It is currently running."
                            + AgentServiceOpenedPorts + AgentProcessOpenedPorts);
                    }
                    else
                    {
                        info.Append("Not running!");
                    }
                }

                info.Append(GetLastLinesFromAgentLog());

                return info.ToString();
            }
            catch (Exception ex)
            {

                Log.Error(ex.ToString());
                return ex.Message;
            }
        }
        #endregion 
    }
}
