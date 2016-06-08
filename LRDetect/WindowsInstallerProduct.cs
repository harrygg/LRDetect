using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  /// <summary>
  /// A Class for getting a product properties if it was installed using the Windows Installer
  /// </summary>
  class WindowsInstallerProduct
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
      get
      {
        if (displayName == null)
          displayName = RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "DisplayName", "");
        return displayName;
      }
      set { displayName = value; }
    }

    string displayVersion = null;
    public string DisplayVersion
    {
      get
      {
        if (displayVersion == null)
          displayVersion = RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "DisplayVersion", "");
        return displayVersion;
      }
      set { displayVersion = value; }
    }

    string installLocation = null;
    public string InstallLocation
    {
      get
      {
        if (installLocation == null)
          installLocation = RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "InstallLocation", "");
        return installLocation;
      }
      set { installLocation = value; }
    }

    string installDate = null;
    public string InstallDate
    {
      get
      {
        if (installDate == null)
          installDate = Helper.ConvertInstallDate(RegistryWrapper.GetValue(RegHive.LocalMachine, installerProductKeyPath, "InstallDate", ""));
        return installDate;
      }
      set { installDate = value; }
    }

    
    public WindowsInstallerProduct(string upgCode)
    {
      upgradeCode = upgCode;
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
        return value;
      }
      catch (Exception)
      {
        return null;
      }
    }
    #endregion

  }
}
