using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace LRDetect
{
  class CitrixHelper
  {

    public class Server
    {
      public bool isInstalled = false;
      public string name;
      public string version;

      public Server()
      {
        var ctrxServer = new MSIProgram("140EC50CBD3676447B77F9BC0310C32A");
        //If code does not exist we'll search Add/Remove programs
        if (ctrxServer.isInstalled)
        {
          isInstalled = true;
          name = ctrxServer.DisplayName;
          version = ctrxServer.DisplayVersion;
        }
        else
        {
          var server = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("[x|X]en[A|a]pp"));
          if (server != null)
          {
            isInstalled = true;
            name = server.DisplayName;
            version = server.DisplayVersion;
          }
        }
      }

      public override string ToString()
      {
        return isInstalled ? name + " " + version : "Not detected";
      }

      string icaTcpRegPath = @"System\CurrentControlSet\Control\Terminal Server\WinStations\ICA-TCP";

      public string GetIcaMaxDisconnectionTime()
      {
        string value = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, icaTcpRegPath, "MaxDisconnectionTime");
        return (value == null) ? Html.ErrorMsg() : FormatSessionTimeout(value);
      }

      public string GetMaxConnectionTime()
      {
        string value = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, icaTcpRegPath, "MaxConnectionTime");
        return (value == null) ? Html.ErrorMsg() : FormatSessionTimeout(value);
      } 
    }

    public class Agent
    {
      public bool isInstalled = false;
      public string name;
      public string version;
      public string installDate;
      public string installLocation = "";
      public bool isTextTrappingDriverInstalled = false;
      public string textTrappingDriverState;
      public string textTrappingDriverVersion;
      public string driverSignedInfo;

      public Agent()
      {
        try
        {
          var agentProduct = new MSIProgram("25A070105E2588740B2ED37C28A66094");

          if (agentProduct.isInstalled)
          {
            isInstalled = true;
            name = agentProduct.DisplayName;
            version = agentProduct.DisplayVersion;
            installLocation = agentProduct.InstallLocation;
            installDate = agentProduct.InstallDate;
          }
          else //If the upgrade code does not exist or has changed we'll search Add/Remove programs
          {
            var server = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("HP Software Agent for Citrix"));
            if (server != null)
            {
              isInstalled = true;
              name = server.DisplayName;
              version = server.DisplayVersion;
            }
          }

          if (isInstalled)
          {
            GetTextDriverInfo();
          }

        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      #region Collect Text Trapping Driver Information
      /// <summary>
      /// 1. Check whether the driver is installed. Could be verified by running "sc query paldrv"
      /// 2. If installed check the state of the driver
      /// 3. Check if the driver is signed.
      /// </summary>
      void GetTextDriverInfo()
      {
        var driver = new AgentDriver();
        isTextTrappingDriverInstalled = driver.isInstalled;
        textTrappingDriverState = driver.state;
        textTrappingDriverVersion = driver.version;
        driverSignedInfo = driver.SignedInfo;
      }
      #endregion

      public string GetIniContentInfo()
      {
        string filePath = Path.Combine(installLocation, "bin\\CtrxAgent.ini");
        return IniParser.ToHtml(filePath);
      }

      /// <summary>
      ///Logon Script Only for Terminal Server Users
      ///On Server OS (http://support.microsoft.com/en-us/kb/195461)
      ///HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\AppSetup
      ///On Desktop OS (https://msdn.microsoft.com/en-us/library/windows/desktop/aa376977(v=vs.85).aspx)
      ///HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Run\HP Agent for Citrix
      ///
      /// </summary>
      public string GetImptKeyContent()
      {
        StringBuilder info = new StringBuilder();
        string value; 

        try
        {
          if (OSCollectorHelper.IsWindowsServer)
          {
            value = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon", "AppSetup");
            if (value == null) value = Html.ErrorMsg();
            info.Append(String.Format("{0}={1}{2}{3}", "AppSetup", Html.br, value, Html.br));
          }
          else
          {
            value = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "HP Agent for Citrix");
            if (value == null) value = Html.ErrorMsg();
            info.Append(String.Format("{0}={1}{2}{3}", "HP Agent for Citrix", Html.br, value, Html.br));
          }

          info.Append(Html.hr);
          value = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Citrix\wfshell\TWI", "LogoffCheckSysModules");
          if (value == null) value = Html.ErrorMsg();
          info.Append(String.Format("{0}={1}{2}", "LogoffCheckSysModules", value, Html.br));


        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          info.Append(ex.Message);
        }
        return info.ToString();
      }

      public override string ToString()
      {
        return isInstalled ? String.Format("{0} {1} {2}", name, version, installDate) : "Not detected";
      }
    }

    #region Collect Text Trapping Driver Information
    /// <summary>
    /// 1. Check whether the driver is installed. Could be verified by running "sc query paldrv"
    /// 2. If installed check the state of the driver
    /// 3. Check if the driver is signed.
    /// </summary>
    public class AgentDriver
    {
      public bool isInstalled = false;
      string displayName = "paldrv";
      string path = "";
      public string state = Html.Error("Unknown");
      public string version = Html.ErrorMsg();
      public string SignedInfo;

      public AgentDriver()
      {
        var wmiObject = Helper.GetWMIObject("root\\CIMV2", "Win32_SystemDriver", "WHERE DisplayName = 'paldrv'");
        Logger.Debug(String.Format("wmiObject:: DisplayName: {0} State: {1} PathName: {2}", wmiObject["DisplayName"], wmiObject["State"], wmiObject["PathName"]));

        //If HP Citrix Agent is installed, check if text trapping driver pal_drv.sys is installed
        isInstalled = wmiObject["DisplayName"].ToString().Equals(displayName);
        if (isInstalled)
        {
          state = wmiObject["State"].ToString();
          if (state == "Running" || state == "RUNNING")
            state = Html.Notice(state);

          path = wmiObject["PathName"].ToString();
          Logger.Info("WinTrust validating file " + path);
          SignedInfo = WinTrust.IsFileSignedInfo(path);

          GetFileVersion();
        }
      }

      void GetFileVersion()
      {
        Logger.Info("Executing AgentDriver.GetFileVersion");
        //Disable WOW64 redirection
        Helper.Wow64DisableWow64FsRedirection();
        try { version = ", version " + FileVersionInfo.GetVersionInfo(path).ProductVersion; }
        catch (Exception ex) { Logger.Error(ex.ToString()); }
        //Revert WOW64 redirection
        Helper.Wow64RevertWow64FsRedirection();
      }
    }
    #endregion

    public class Client
    {
      public string version = "";
      public string name = "";
      public bool isInstalled = false;
      //public bool IsSupported = false;

      public Client()
      {
        try
        {
          // check if the new Citrix client exists (> 11.2)
          var ctrxClient = new MSIProgram("9B123F490B54521479D0EDD389BCACC1");
          if (ctrxClient.isInstalled)
          {
            isInstalled = true;
            name = ctrxClient.DisplayName;
            version = ctrxClient.DisplayVersion;
          }
          else // check if the old Citrix client exists (<11.2)
          {
            ctrxClient = new MSIProgram("6D0FA3AFBC48DDC4897D9845832107CE");
            // If still not found, try searching in Add/Remove programs
            if (ctrxClient.isInstalled)
            {
              name = ctrxClient.DisplayName;
              isInstalled = true;
              version = ctrxClient.DisplayVersion;
            }
            else
            {
              var client = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("citrix +receiver", RegexOptions.IgnoreCase));
              if (client != null)
              {
                isInstalled = true;
                name = client.DisplayName;
                version = client.DisplayVersion;
              }
            }
          }
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      public bool IsRegistryPatchInstalled()
      {
        string virtualChannels = "not null";
        string allowSimulationAPI;
        string keyPath = @"SOFTWARE\Citrix\ICA Client\Engine\Lockdown Profiles\All Regions\Lockdown\Virtual Channels\Third Party\CustomVC";

        virtualChannels = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, keyPath, "VirtualChannels");
        Logger.Info(@"SOFTWARE\Citrix\ICA Client\Engine\Lockdown Profiles\All Regions\Lockdown\Virtual Channels\Third Party\CustomVC\VirtualChannels = " + virtualChannels);

        keyPath = @"SOFTWARE\Citrix\ICA Client\CCM";
        allowSimulationAPI = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, keyPath, "AllowSimulationAPI");
        Logger.Info(@"Key: " + keyPath + "\\AllowSimulationAPI = " + allowSimulationAPI);

        return (virtualChannels == "" && allowSimulationAPI == "1");
      }


      public string GetCitrixRegistryPatchInfo()
      {
        try
        {
          var not = IsRegistryPatchInstalled() ? "" : "not ";
          return "Citrix registry patch is " + not + "installed in 32bit registry";
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      internal string GetCitrixClientInfo()
      {
        return isInstalled ? "Yes, " + name + " " + version + " was detected" : "Not detected";
      }
    }


    /// <summary>
    /// Converts milliseconds to string
    /// Citrix session timeout has predified values and it can't be altered unless directly in registry
    /// </summary>
    /// <param name="timeout">the timeout value in milliseconds</param>
    /// <returns>One of the following: Never, 1 Minute, 5 Minutes, 10 Minutes, 15 Minutes, 30 Minutes, 1 Hour, 2 Hours, 3 Hours, 6 Hours, 8 Hours, 12 Hours, 16 Hours, 18 Hours, 1 Day, 2 Days, 3 Days, 4 Days, 5 Days</returns>
    public static string FormatSessionTimeout(string timeout)
    {
      string output = Html.ErrorMsg();

      try
      {
        TimeSpan ts = TimeSpan.FromMilliseconds(Convert.ToDouble(timeout));

        int minutes = ts.Minutes;
        int hours = ts.Hours;
        int days = ts.Days;
        int seconds = ts.Seconds;
        int mils = ts.Milliseconds;

        if (minutes == 0 && hours == 0 && days == 0)
          return "Never";
        //The msec and seconds value cannot be modified from GUI. 
        //If it was, then return Never plus the modified seconds.
        if (mils !=0 || seconds != 0)
          return "Never (Incorrect value detected " + ts.ToString() + ")";

        if (minutes > 0 && hours == 0 && days == 0)
        {
          output = minutes + " Minute";
          if (minutes > 1)
            output += "s";
        }
        else if (hours > 0 && minutes == 0 && days == 0)
        {
          output = hours + " Hour";
          if (hours > 1)
            output += "s";
        }
        else if (days > 0 && minutes == 0 && hours == 0)
        {
          output = days + " Day";
          if (days > 1)
            output += "s";
        }
        else
          output = "Never (Incorrect value detected " + ts.ToString() + ")";

      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
      return output;
    }
  }
}
