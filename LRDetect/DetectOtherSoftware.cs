using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// registries
using Microsoft.Win32;
// file 
using System.IO;
// fileinfo
using System.Diagnostics;
// wmi info
using System.Management;
using System.Text.RegularExpressions;
using System.ServiceProcess;

namespace LRDetect
{
  public class DetectOtherSoftware
  {
    #region Citrix Client Detection
    /// <summary>
    /// CITRIX DETECTION
    /// </summary>
    /// <returns></returns>
    /*public static string GetCitrixClientInfo()
    {
        try
        {
          Version v = GetCitrixClientVersion();
          if (v != null)
            return "Yes, " + clientName + " " + v;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return null;
        }
    }*/

    /*public static Version GetCitrixClientVersion()
    {
      try
      {
        // check if the new Citrix client exists (>11.2)
        string citrixClientCode = ProductInfo.GetProductCode("9B123F490B54521479D0EDD389BCACC1");
        if (citrixClientCode == null)
        {
          // check if the old Citrix client exists (<11.2)
          citrixClientCode = ProductInfo.GetProductCode("6D0FA3AFBC48DDC4897D9845832107CE");
          if (citrixClientCode == null)
          {
            // Try searching in Add/Remove programs
            var client = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("Citrix Receiver", RegexOptions.IgnoreCase));
            if (client != null)
              return new Version(client.DisplayVersion);
            else
              return null;
          }
        }

        string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + citrixClientCode + @"\InstallProperties";
        string clientName = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayName");
        string clientVersion = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayVersion");

        return new Version(clientVersion);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }


    public static string GetCitrixRegistryPatchInfo(string productVersion = "11.50")
    {
      try
      {
        string virtualChannels = "not null";
        string allowSimulationAPI = String.Empty;
            
        string keyPath = @"SOFTWARE\Citrix\ICA Client\Engine\Lockdown Profiles\All Regions\Lockdown\Virtual Channels\Third Party\CustomVC";
        virtualChannels = RegistryWrapper.GetValue(RegHive.LocalMachine, keyPath, "VirtualChannels");
        Logger.Info(@"SOFTWARE\Citrix\ICA Client\Engine\Lockdown Profiles\All Regions\Lockdown\Virtual Channels\Third Party\CustomVC\VirtualChannels = " + virtualChannels);

        keyPath = @"SOFTWARE\Citrix\ICA Client\CCM";
        allowSimulationAPI = RegistryWrapper.GetValue(RegHive.LocalMachine, keyPath, "AllowSimulationAPI");
        Logger.Info(@"Key: " + keyPath + "\\AllowSimulationAPI = " + allowSimulationAPI);

        // The below key is available only in LR 11 citrix reg patch
        string preApproved = "Default";
        keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Ext\PreApproved\{238F6F83-B8B4-11CF-8771-00A024541EE3}";
        preApproved = RegistryWrapper.GetValue(RegHive.LocalMachine, keyPath, "(Default)");
        string not = (preApproved == null) ? "not " : "";
        Logger.Warn("The key does " + not + "exist: " + keyPath);
                
        string warning = String.Empty;
        if (productVersion.StartsWith("11.0") && preApproved == null)
            warning = "<br />" + Html.Error("The plugin was not found in the list of pre-approved plugins for IE");

        not = (virtualChannels == "" && allowSimulationAPI == "1") ? "" : "not ";
        return "Citrix registry patch is " + not + "installed" + warning;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }

    */
    #endregion

    #region RDP Client Detection
    /// <summary>
    /// RDP DETECTION
    /// </summary>
    /// <returns></returns>   
    public static string GetRDPClientVersion()
    {
        try
        {
            string path = Environment.GetEnvironmentVariable("WINDIR").ToString() + @"\system32\";
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path + "mstsc.exe");
            if(fvi != null) 
                return fvi.ProductVersion.ToString();
            return "No RDP client detected";
        }
        catch(Exception ex)
        {
            Logger.Error(ex.ToString());
            return Html.ErrorMsg();
        }
    }
    #endregion

    #region Oracle DB Client detection
    public static string GetOracleClientInfo()
    {
      var OraClientsInfo = new OracleClientsHelper();
      return OraClientsInfo.ToString();
    }

    #endregion

    #region SAP GUI Detection 
    public static string sapguiKeyPath = @"SOFTWARE\SAP\SAP Shared\";   
    /// <summary>
    /// SAPGUI DETECTION
    /// </summary>
    /// <returns></returns> 
    public static string GetSapGuiClientInfo()
    {
      string output = "Not detected";
      try
      {
        string SAPsysdir = RegistryWrapper.GetValue(RegHive.LocalMachine, sapguiKeyPath, "SAPsysdir");
        if (SAPsysdir != null)
        {
          string saplogonexe = Path.Combine(SAPsysdir, "saplogon.exe");

          FileVersionInfo fvi;
          if (File.Exists(saplogonexe))
          {
            fvi = FileVersionInfo.GetVersionInfo(Path.Combine(SAPsysdir, "saplogon.exe"));
            output = "Yes, version " + fvi.FileVersion;
          }
        }
      }
      catch (Exception ex)
      {
        return ex.ToString();
      }
        return output;
    }

    //public static string SapGuiScriptingEnabledInfo()
    //{
    //    try
    //    {
    //    }
    //    catch (Exception ex)
    //    {
    //        return ex.ToString();
    //    }
    //    return "Not implemented yet";
    //}
    #endregion

    #region DotNet detection
    public static string GetGetDotNetVersionFromRegistry()
    {
      StringBuilder output = new StringBuilder();
      try
      {
        RegistryKey ndpKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\");
        foreach (string versionKeyName in ndpKey.GetSubKeyNames())
        {
          if (versionKeyName.StartsWith("v"))
          {
            RegistryKey versionKey = ndpKey.OpenSubKey(versionKeyName);
            string name = (string)versionKey.GetValue("Version", "");
            string sp = versionKey.GetValue("SP", "").ToString();
            string install = versionKey.GetValue("Install", "").ToString();
            if (install == "") //no install info, must be later.
              output.Append(Html.B(versionKeyName) + Html.tab + name + Html.br);
            else
            {
              if (sp != "" && install == "1")
                output.Append(Html.B(versionKeyName) + Html.tab + name + "  SP" + sp + Html.br);
            }
            if (name != "")
              continue;
            foreach (string subKeyName in versionKey.GetSubKeyNames())
            {
              RegistryKey subKey = versionKey.OpenSubKey(subKeyName);
              name = (string)subKey.GetValue("Version", "");
              if (name != "")
                sp = subKey.GetValue("SP", "").ToString();
              install = subKey.GetValue("Install", "").ToString();
              if (install == "") //no install info, must be later.
                output.Append(versionKeyName + "  " + name + Html.br);
              else
              {
                if (sp != "" && install == "1")
                  output.Append(Html.tab + subKeyName + Html.tab + name + "  SP" + sp + Html.br);
                else if (install == "1")
                  output.Append(Html.tab + subKeyName + Html.tab + name + Html.br);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
      return output.ToString();
    }
    /*
    /// <summary>
    /// Check if certain .net version is installed
    /// </summary>
    /// <param name="version">v3.5 | v4.0</param>
    /// <returns></returns>
    public bool IsDotNetVersionInstalled(string version)
    {
        try
        {
            string path = (version == "v4") ? @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" : @"SOFTWARE\Microsoft\NET Framework Setup\NDP\" + version;
            string key = Registry.LocalMachine.OpenSubKey(path).GetValue("Install").ToString();
            return key == "1" ? true : false;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return false;
        }
            
    }*/

    #endregion

    #region SiteScope detection
    /// <summary>
    /// SiteScope DETECTION
    /// </summary>
    /// <returns></returns>  
    public static string GetSiteScopeInfo()
    {
        try
        {
          Logger.Debug("GetSiteScopeInfo() started.");
            // check for installations of
            String[] siteScopeUpgradeCodes = new String[] 
            {
                "7C9790C8356D6A142B1E73F0AE4F22AB", //HP SiteScope for LoadTesting
                "55D444412FEC9B349A920326911A26F1", //HP SiteScope Integrations
                "F60D57E75AF65A84E888D007E2799EE9", //HP SiteScope Monitoring
                "350F46B85A30F7240911CD2A1C293400", //HP SiteScope Server
                "54509F95E83F79C43A15FDAFB2FF3CA3", //HP SiteScope User Interface
                "CFCBC45A6F7CE934286FF54746AF7708"  //HP SiteScope Tools
            };

            // Get product codes for installed SiteScope features
            StringBuilder productInfo = new StringBuilder(1024);
            String productCode = String.Empty;

            Stopwatch stopWatch = Stopwatch.StartNew();

            //if Parallel execution is enabled (default)
            if (FormArguments.async)
            {
              Helper.EachParallel(siteScopeUpgradeCodes, upgradeCode =>
              {
                //foreach (string upgradeCode in siteScopeUpgradeCodes)
                //{
                productCode = ProductInfo.GetProductCode(upgradeCode);
                if (productCode != null)
                {
                  string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCode + @"\InstallProperties";
                  //Logger.Log("Searching for SiteScope in registry path " + registryPath);
                  string productName = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayName");
                  Logger.Info("SiteScope product name " + productName);
                  // Product name found now find the product version
                  string productVersion = String.Empty;
                  if (productName != null)
                    productVersion = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayVersion");

                  string intallDate = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "InstallDate");
                  productInfo.Append(productName + ", version " + productVersion + " " + Helper.ConvertInstallDate(intallDate) + Html.br);
                }
              });
            }
            else
            {
              foreach (string upgradeCode in siteScopeUpgradeCodes)
              {
                productCode = ProductInfo.GetProductCode(upgradeCode);
                if (productCode != null)
                {
                  string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCode + @"\InstallProperties";
                  //Helper.Log("Searching for SiteScope in registry path " + registryPath);
                  string productName = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayName");
                  Logger.Info("SiteScope product name " + productName);
                  // Product name found now find the product version
                  string productVersion = String.Empty;
                  if (productName != null)
                    productVersion = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayVersion");

                  string intallDate = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "InstallDate");
                  productInfo.Append(productName + ", version " + productVersion + " " + Helper.ConvertInstallDate(intallDate) + Html.br);
                }
              }
            }

            TimeSpan ts = stopWatch.Elapsed;
            stopWatch.Stop();
            Logger.Debug("DetectOtherSoftware.GetProductCode finished. Execution time: " + ts.ToString());
            Debug.WriteLine(ts.ToString());

            return (productInfo.Length != 0) ? productInfo.ToString() : "No";
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return Html.ErrorMsg(); 
        }
    }
    #endregion

    #region Detect Google Chrome installation
    public static string GetGoogleChromeInfo()
    {
      try
      {
        InstalledProgram GoogleChrome = InstalledProgramsHelper.GetInstalledProgramByName("Google Chrome");
        return GoogleChrome == null ? "No" : "Yes, version " + GoogleChrome.DisplayVersion;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }

    #endregion 

    #region Detect Mozilla Firefox installation
    public static string GetFirefoxInfo()
    {
      try
      {
        InstalledProgram firefox = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("Firefox"));
        return firefox == null ? "No" : "Yes, version " + firefox.DisplayVersion;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }

    #endregion 

    #region Get IE Extensions
      public static string GetIEExtensions()
      {
        try
        {
          //HKEY_CURRENT_USER\Software\Microsoft\Internet Explorer\Approved Extensions
          string keyPath = @"Software\Microsoft\Internet Explorer\Approved Extensions";
          RegistryKey regKey = Registry.CurrentUser;
          regKey = regKey.OpenSubKey(keyPath);
          String[] subkeyNames = regKey.GetValueNames();
          StringBuilder output = new StringBuilder();
          foreach (string subKeyName in subkeyNames)
          {
            //HKEY_CLASSES_ROOT\Wow6432Node\CLSID\
            string extName = RegistryWrapper.GetValue(RegHive.ClassesRoot, @"\CLSID\" + subKeyName, null);
            if (extName != null)
              output.Append(extName + Html.br);

            extName = RegistryWrapper.GetValue(RegHive.ClassesRoot, @"\CLSID\" + subKeyName + "\\InprocServer32", null);
            if (extName != null)
              output.Append("&nbsp;&nbsp;&nbsp;&nbsp;" + extName + Html.br);

          }
          return output.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }
    #endregion

    #region ALM Platform Loader
      public static string GetALMPlatformLoaderInfo()
      {
        try
        {
          var almpl = InstalledProgramsHelper.GetInstalledProgramByName("ALM Platform Loader");
          return (almpl != null) ? almpl.ToString() : "No";
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }
    #endregion 

    #region Detect QTP/UFT
      public static string GetUFTInstallationInfo()
      {
        try
        {
          var qtpi = new QuickTestProInfo();
          string output = "No";

          if (qtpi.IsInstalled)
          {
            if (qtpi.ProductName.Contains("QC Integration"))
            {
              var qtp = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("HP Unified Functional Testing"));
              if (qtp != null)
                output = "Yes, " + qtp.DisplayName + " " + qtp.DisplayVersion;
            }
            else
              output = Html.BoolToYesNo(qtpi.IsInstalled) + " " + qtpi.ProductName + " " + qtpi.ProductVersion + Helper.ConvertInstallDate(qtpi.InstallDate);
          }
          return output;
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }
    #endregion

      internal static string GetJenkinsInfo()
      {
        StringBuilder output = new StringBuilder();
        try
        {
          var jenkins = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("Jenkins"));
          if (jenkins != null)
            output.Append(Html.Yes + ", " + jenkins.DisplayName + " " + jenkins.DisplayVersion);
          else
            output.Append(Html.No);
          return output.ToString(); 
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      internal static string GetJenkinsPluginInfo()
      {
        var pluginFile = "";
        try
        {
          var executable = RegistryWrapper.GetValue(RegHive.LocalMachine, @"SYSTEM\CurrentControlSet\Services\Jenkins", "ImagePath");
          if (executable != null)
          {
            var path = Path.GetDirectoryName(executable.Replace("\"", ""));
            pluginFile = Path.Combine(path, @"plugins\hp-application-automation-tools-plugin.jpi");
          }
          //ServiceController sc = new ServiceController("Jenkins");
          //output.Append("Status: " + sc.Status); 
          return "HP Application Automation Tools plugin is " + (File.Exists(pluginFile) ? "" : "not ") + "installed";
        }
        catch (Exception ex) 
        { 
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      internal static string IsRDPAccessAllowedInfo()
      {
        //HKLM\System\CurrentControlSet\Control\Terminal Server\fDenyTSConnections = 1
        //HKLM\System\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp\UserAuthentication = 0
        //HKLM\System\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp\SecurityLayer = 1

        var fDenyTSConnections = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, @"System\CurrentControlSet\Control\Terminal Server", "fDenyTSConnections");
        var UserAuthentication = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, @"System\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp", "UserAuthentication");
        var SecurityLayer = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, @"System\CurrentControlSet\Control\Terminal Server\WinStations\RDP-Tcp", "SecurityLayer");

        if (fDenyTSConnections == null || UserAuthentication == null || SecurityLayer == null)
          return Html.ErrorMsg();

        if (fDenyTSConnections == "1")
          return "Connections to this computer are not allowed";
        else
        {
          if (UserAuthentication == "0")
            return "Connections are allowed from computers running any version of Remote Desktop";
          else
            return "Connections are allowed only from computers running Remote Desktop with Network Level Authentication";
        }
      }
  
    
      /// <summary>
      /// Detects if RDP Role is installed
      /// https://msdn.microsoft.com/en-us/library/cc644951(v=vs.85).aspx
      /// </summary>
      /// <returns></returns>
      internal static string IsRDPRollInstalledInfo()
      {
        var serverFeature = Helper.GetWMIObject("root\\CIMV2", "Win32_ServerFeature", "WHERE ID = '14'");
        return serverFeature != null ? Html.Yes : Html.No;
      }



      public static string RDPAgentInfo()
      {
        StringBuilder output = new StringBuilder();
        var agent = new MSIProgram("59638050AABC6184181BF2015E0E24B6");
        if (agent.isInstalled)
        {
          output.Append(String.Format("{0} {1} {2}", agent.DisplayName, agent.DisplayVersion, agent.InstallDate));

        }
        else
          output.Append("Not detected");

        return output.ToString();
      }


  }
  /*
  public class CitrixClient
  {
    public Version version = null;
    public string name = "";
    public bool isInstalled = false;
    //public bool IsSupported = false;

    public CitrixClient()
    {
      try
      {
        // check if the new Citrix client exists (>11.2)
        string citrixClientCode = ProductInfo.GetProductCode("9B123F490B54521479D0EDD389BCACC1");
        if (citrixClientCode == null)
        {
          // check if the old Citrix client exists (<11.2)
          citrixClientCode = ProductInfo.GetProductCode("6D0FA3AFBC48DDC4897D9845832107CE");
          if (citrixClientCode == null)
          {
            // Try searching in Add/Remove programs
            var client = InstalledProgramsHelper.GetInstalledProgramByName(new Regex("Citrix Receiver", RegexOptions.IgnoreCase));
            if (client != null)
            {
              isInstalled = true;
              name = client.DisplayName;
              version = new Version(client.DisplayVersion);
            }
            else // product is not installed
            { 
            }
          }
        }

        // if product code was found
        if (citrixClientCode != null)
        {
          // Find product details based on product code from above
          string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + citrixClientCode + @"\InstallProperties";
          name = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayName");
          if (name != null)
          {
            isInstalled = true;
            string clientVersion = RegistryWrapper.GetValue(RegHive.LocalMachine, registryPath, "DisplayVersion");
            if (clientVersion != null)
              version = new Version(clientVersion);
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

      virtualChannels = RegistryWrapper.GetValue(RegHive.LocalMachine, keyPath, "VirtualChannels");
      Logger.Info(@"SOFTWARE\Citrix\ICA Client\Engine\Lockdown Profiles\All Regions\Lockdown\Virtual Channels\Third Party\CustomVC\VirtualChannels = " + virtualChannels);

      keyPath = @"SOFTWARE\Citrix\ICA Client\CCM";
      allowSimulationAPI = RegistryWrapper.GetValue(RegHive.LocalMachine, keyPath, "AllowSimulationAPI");
      Logger.Info(@"Key: " + keyPath + "\\AllowSimulationAPI = " + allowSimulationAPI);

      return (virtualChannels == "" && allowSimulationAPI == "1");
    }


    public string GetCitrixRegistryPatchInfo()
    {
      try
      {
        var not = IsRegistryPatchInstalled() ? "" : "not ";
        return "Citrix registry patch is " + not + "installed";
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

    internal string GetClientVersionSupportedInfo()
    {
      try
      {
        if (ProductDetection.Vugen.IsInstalled && this.isInstalled)
        {
          Version vugenVersion = ProductDetection.Vugen.version;

          // Vugen > 11.52 supports Citrix clients 10.x to 14.x
          if (vugenVersion >= new Version(11, 52))
            return (this.version.Major >= 10 && this.version.Major <= 14) ? "Yes" : Html.cNo;
          //Vugen 11.50 -> 11.51 supports Citrix clients 10.x to 13.x
          else if (vugenVersion.Major == 11 && vugenVersion.Minor >= 50)
            return (this.version.Major >= 10 && this.version.Major <= 13) ? "Yes" : Html.cNo;
          //Vugen 11.04 supports Citrix clients 10.x to 12.x
          else if (vugenVersion.Major == 11 && vugenVersion.Minor >= 0)
            return (this.version.Major >= 9 && this.version.Major <= 12) ? "Yes" : Html.cNo;
          //Vugen 9.x supports Citrix clients 8.x to 10.x
          else if (vugenVersion.Major == 11 && vugenVersion.Minor >= 0)
            return (this.version.Major >= 8 && this.version.Major <= 10) ? "Yes" : Html.cNo;
          else
            return "No information found for this version of Vugen/Citrix";
        }
        return "Vurtual user generator or Citrix client installation not detected"; 
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }
  }*/
}
