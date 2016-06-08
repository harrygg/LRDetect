using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Microsoft.Win32;

namespace LRDetect
{
  /// <summary>
  /// A Class for getting a pogram properties if it was installed using the MS Installer
  /// </summary>
  public class MSIProgram
  {
    string upgradeCode = "";
    public string commonName = "";
    string productCode = "";
    public bool isInstalled = false;

    //string uninstallerKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
    string installerKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\{0}\InstallProperties";
    string installerProductKeyPath = "";

    string displayName = null;
    public string DisplayName 
    {
      get {
        if (displayName == null)
          displayName = RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "DisplayName");
        return displayName;
      }
      set { displayName = value; }
    }

    string displayVersion = null;
    public string DisplayVersion 
    {
      get {
        if (displayVersion == null)
          displayVersion = RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "DisplayVersion");
        return displayVersion;
      }
      set { displayVersion = value; }
    }

    string installLocation = null;
    public string InstallLocation 
    {
      get {
        if (installLocation == null)
          installLocation = RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "InstallLocation");
        return installLocation;
      }
      set { installLocation = value; }
    }

    string installDate = null;
    public string InstallDate 
    {
      get {
        if (installDate == null)
          installDate = Helper.ConvertInstallDate(RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "InstallDate"));
        return installDate;
      }
      set { installDate = value; }
    }

    public MSIProgram(string upgradeCode)
    {
      this.upgradeCode = upgradeCode;
      productCode = GetProductCode(upgradeCode);
      if (productCode != null)
      {
        isInstalled = true;
        installerProductKeyPath = String.Format(installerKeyPath, productCode);
      }
    }

    #region Find a product code by upgrade code
    /// <summary>
    /// Method to get the software product code by its upgrade code
    /// </summary>
    /// <returns></returns>
    public static string GetProductCode(string upgradeCode)
    {
      try
      {
        string value = RegistryWrapper.GetFirstValueName(RegHive.ClassesRoot, @"Installer\UpgradeCodes\" + upgradeCode);
        if (Logger.level > 3)
        {
          if (value != null)
            Logger.Debug("Product code " + value + " matched upgrade code " + upgradeCode);
          else
            Logger.Debug("No product code " + value + " matched upgrade code " + upgradeCode);
        }
        return value;
      }
      catch (Exception ex)
      {
        Logger.Warn("Product code not found for the following product upgrade code: " + upgradeCode + "\n" + ex.ToString());
        return null;
      }
    }
    #endregion


    /*
    #region Find if MSI product is installed in Add/Remove programs
    /// <summary>
    /// Method to find product in 64 and 32 bit Uninstaller
    /// If a product is found, we will set the isInstalled Property to true
    /// </summary>
    /// <param name="name">The name of the product</param>
    /// <param name="in32or64key">64 or 32bit registry i.e. RegSAM.WOW64_32Key</param>
    /// <returns>true if a product is found</returns>
    public bool FindMSIProduct(string name, RegSAM in32or64key)
    {
      try
      {
        List<string> subkeyNames = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, uninstallerKeyPath, in32or64key);
        foreach (String subkeyName in subkeyNames)
        {
          // Subkey might be the product name or
          // it might be a guid like this {FE74AC04-F248-4641-B3A9-89C6AA4339CD}
          if (!subkeyName.StartsWith("{"))
          {
            if (subkeyName.ToLower().Contains(name.ToLower()))
            {
              isInstalled = true;
              installerProductKeyPath = uninstallerKeyPath + "\\" + subkeyName;
              return true;
            }
          }
          else
          {
            string tempKey = uninstallerKeyPath + "\\" + subkeyName;
            string dName = RegistryWrapper.GetRegKey(RegHive.LocalMachine, tempKey, in32or64key, "DisplayName");

            if (dName != null)
            {
              if (dName.ToLower().Contains(name.ToLower()))
              {
                isInstalled = true;
                installerProductKeyPath = tempKey;
                return true;
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return false;
      }
      return false;
    }
    #endregion*/
/*
    #region Display
    public string ToHtml()
    {
      // if the product is not installed return an empty string
      return isInstalled ?
        String.Format("Yes, {0} installed on {1}Version: {2}{3}Install path: {4}{5}{6}"
        , displayName, Html.br, DisplayVersion, InstallDate, Html.br, installLocation, servicesInfo.ToString(), Html.br) : String.Empty;
    }
    #endregion

    #region Get information for service

    private void GetInfoForServices(List<string> serviceNames)
    {
      // Get services information
      if (serviceNames != null)
      {
        foreach (var serviceName in serviceNames)
        {
          servicesInfo.Append(GetInfoForService(serviceName));
        }
      }
    }
    

    private string GetInfoForService(string serviceName)
    {
      string message = null;
      try
      {
        ServiceController sc = new ServiceController(serviceName);
        var status = sc.Status.ToString();
        message = status == "Running" ? Html.Notice(status) : status;
      }
      catch (System.InvalidOperationException sioex)
      {
        Logger.Error(sioex.ToString());
        message = Html.Error("Not found on this computer");
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        message = Html.ErrorMsg();
      }
      return String.Format("{0}{1}{2} Status: {3}", Html.br, Html.tab, serviceName, message);
    }
    #endregion*/
  }
}
