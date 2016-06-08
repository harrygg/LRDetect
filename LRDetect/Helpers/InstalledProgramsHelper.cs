using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.ServiceProcess;

namespace LRDetect
{
  public static class InstalledProgramsHelper
  {
    private static int count = 0;
    /// <summary>
    /// Method to append the none-duplicated products from Uninstaller key
    /// We do this by iterating through the 64 and 32 bit Uninstall key @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall" 
    /// and @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall".
    /// Ignore all SystemComponent key names that have value of 1
    /// Then we iterate through all user keys HKEY_USERS\user-sid\Software\Microsoft\Windows\CurrentVersion\Uninstall
    /// Then we add all none-duplicated products from @"Software\Microsoft\Windows\CurrentVersion\Installer\UserData\user-sid\Products\product-code\InstallProperties"
    /// </summary>
    /// <param name="tempInstalledProducts"></param>
    /// <param name="inHive"></param>
    /// <param name="uninstallerKeyPath"></param>
    /// <param name="in64or32key"></param>
    /// <returns></returns>
    public static List<InstalledProgram> AddProductsFromUninstallerKey(this List<InstalledProgram> tempInstalledProducts, UIntPtr inHive, string uninstallerKeyPath, RegSAM in64or32key)
    {
      var regex = new Regex("KB[0-9]{5,7}$");
      int updatesCount = 0;

      List<string> subkeyNames = RegistryWrapper.GetSubKeyNames(inHive, uninstallerKeyPath, in64or32key);
      if (subkeyNames != null)
      {
        foreach (var subkeyName in subkeyNames)
        {
          // Check only keys like {CFEF48A8-BFB8-3EAC-8BA5-DE4F8AA267CE} 
          // but not {CFEF48A8-BFB8-3EAC-8BA5-DE4F8AA267CE}.KB2504637 which are updates
          if (!regex.Match(subkeyName).Success)
          {
            string SystemComponent = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "SystemComponent");
            if (SystemComponent != "1")
            {
              string Windowsinstaller = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "Windowsinstaller");
              if (Windowsinstaller != "1")
              {
                //Make sure we are not dealing with a patch or an update
                string releaseType = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "ReleaseType");
                string parentKeyName = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "ParentKeyName");

                if (parentKeyName != null)
                {
                  if (parentKeyName != "" || releaseType == "Security Update" || releaseType == "Update Rollup" || releaseType == "Hotfix" )
                    updatesCount++;
                }

                if (parentKeyName == null)
                {
                  string uninstallString = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "UninstallString");
                  if (uninstallString != null)
                  {
                    string displayName = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "DisplayName");
                    string displayVersion = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "DisplayVersion");
                    string installDate = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "InstallDate");

                    if (displayName != null && displayName != "")
                    {
                      var product = new InstalledProgram(displayName, displayVersion, uninstallString, installDate);
                      if (!tempInstalledProducts.Contains(product))
                        tempInstalledProducts.Add(product);
                    }
                  }
                }
              }
              else
              {
                string productId = GetInstallerKeyNameFromGuid(subkeyName);
                string productKey = @"Software\Classes\Installer\Products\" + productId;
                string productName = RegistryWrapper.GetRegKey(inHive, productKey, RegSAM.WOW64_64Key, "ProductName");
                if (productName != null)
                {
                  string displayVersion = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "DisplayVersion");
                  string uninstallString = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "UninstallString");
                  string installDate = RegistryWrapper.GetRegKey(inHive, uninstallerKeyPath + "\\" + subkeyName, in64or32key, "InstallDate");

                  var product = new InstalledProgram(productName, displayVersion, uninstallString, installDate);
                  if (!tempInstalledProducts.Contains(product))
                    tempInstalledProducts.Add(product);
                }
              }
            }
          }
        }
      }
      return tempInstalledProducts;
    }
    public static List<InstalledProgram> installedProgramsList = new List<InstalledProgram>();

    static InstalledProgramsHelper()
    {
      installedProgramsList = GetListOfInstalledPrograms();
    }

    /// <summary>
    /// Returns the program that has the specified name
    /// </summary>
    /// <param name="name">The program name (case insensitive)</param>
    /// <returns>The installed program object</returns>
    public static InstalledProgram GetInstalledProgramByName(string name)
    {
      try
      {
        foreach (var installedProgram in installedProgramsList)
        {
          if (installedProgram.DisplayName.ToLower() == name.ToLower())
            return installedProgram;
        }
        return null;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }

    /// <summary>
    /// Method to return the first program from a matching list
    /// </summary>
    /// <param name="pattern">The regular expression i.e. "^[V}v]isual [S|s]tudio"</param>
    /// <returns>the isntalled program</returns>
    public static InstalledProgram GetInstalledProgramByName(Regex pattern)
    {
      try
      {
        foreach (var installedProgram in installedProgramsList)
        {
          Logger.Debug("Searching for installed program: REGEX used: " + pattern);
          if (pattern.Match(installedProgram.DisplayName).Success)
            return installedProgram;
        }
        return null;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }

    /// <summary>
    /// Method to return a list of installed programs that match the provided name
    /// </summary>
    /// <param name="name">The name of the program. Accepts regular expressions</param>
    /// <returns>a list of installed programs</returns>
    public static List<InstalledProgram> GetInstalledProgramsByName(string name)
    {
      try
      {
        List<InstalledProgram> programs = new List<InstalledProgram>();
        var pattern = new Regex(name);
        if (installedProgramsList != null)
        {
          foreach (var installedProgram in installedProgramsList)
          {
            if (pattern.Match(installedProgram.DisplayName).Success)
              programs.Add(installedProgram);
          }
          return programs;
        }
        return null;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }

    /// <summary>
    /// Method to check if a list of programs is installed
    /// </summary>
    /// <param name="names">List of strings - names of the programs</param>
    /// <returns>Returns a List<InstalledProgram></returns>
    public static List<InstalledProgram> GetInstalledProgramsByNames(List<string> names)
    {
      try
      {
        List<InstalledProgram> programs = new List<InstalledProgram>();
        foreach (var programName in names)
        {
          var program = InstalledProgramsHelper.GetInstalledProgramByName(programName);
          if (program != null)
            programs.Add(program);
        }
        return programs;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }

    #region Get information for service

    public static string GetInfoForServices(List<string> serviceNames)
    {
      // Get services information
      StringBuilder servicesInfo = new StringBuilder();
      if (serviceNames != null)
        foreach (var serviceName in serviceNames)
          servicesInfo.Append(GetInfoForService(serviceName));

      return servicesInfo.ToString() + Html.br;
    }


    public static string GetInfoForService(string serviceName)
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
    #endregion 

    #region Get the list of all installed programs. Should match the programs in Windows Add/Remove programs
    public static List<InstalledProgram> GetListOfInstalledPrograms() 
    {
      string uninstallerKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

      //1. Get the products from 32bit Uninstaller @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
      installedProgramsList.AddProductsFromUninstallerKey(RegHive.LocalMachine, uninstallerKeyPath, RegSAM.WOW64_32Key);
      //count = installedProductsList.Count;
      //Logger.Debug(count + @" products found in HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");

      //2. Get the products from 64bit Uninstaller @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"
      installedProgramsList.AddProductsFromUninstallerKey(RegHive.LocalMachine, uninstallerKeyPath, RegSAM.WOW64_64Key);
      //count = installedProductsList.Count - count;
      //Logger.Debug(count + @" products found in HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall");
      //count = installedProductsList.Count;

      //3. Get the products from HKEY_USERS\<user-sid>\Software\Microsoft\Windows\CurrentVersion\Uninstall
      string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\ProfileList";
      List<string> userKeys = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, keyPath, RegSAM.WOW64_64Key);
      Logger.Debug(userKeys.Count + " user keys found in " + keyPath);

      if (userKeys != null)
      {
        foreach (var userKey in userKeys)
        {
          installedProgramsList.AddProductsFromUninstallerKey(RegHive.Users, userKey + "\\" + uninstallerKeyPath, RegSAM.WOW64_64Key);
          count = installedProgramsList.Count - count;
          Logger.Debug(count + " products found in HKEY_USERS\\" + userKey + "\\" + uninstallerKeyPath);
          count = installedProgramsList.Count;
        }
      }

      //4. Get the products from "Software\Microsoft\Windows\CurrentVersion\Installer\UserData\<user-sid>\Products\<product-code>\InstallProperties"
      keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData";
      userKeys = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, keyPath, RegSAM.WOW64_64Key);
      if (userKeys != null)
      {
        foreach (var userKey in userKeys)
        {
          if (userKey != "S-1-5-18")
          {
            //Get the products from i.e. "Software\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\<product-code>\InstallProperties"
            keyPath = @"Software\Microsoft\Windows\CurrentVersion\Installer\UserData\" + userKey + @"\Products\";
            List<string> productKeys = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, keyPath, RegSAM.WOW64_64Key);
            if (productKeys != null)
            {
              foreach (var productKey in productKeys)
              {
                string key = keyPath + productKey + @"\InstallProperties";
                string systemComponent = RegistryWrapper.GetRegKey(RegHive.LocalMachine, key, RegSAM.WOW64_64Key, "SystemComponent");
                if (systemComponent != "1")
                {
                  string uninstallString = RegistryWrapper.GetRegKey(RegHive.LocalMachine, key, RegSAM.WOW64_64Key, "UninstallString");
                  string displayName = RegistryWrapper.GetRegKey(RegHive.LocalMachine, key, RegSAM.WOW64_64Key, "DisplayName");
                  string displayVersion = RegistryWrapper.GetRegKey(RegHive.LocalMachine, key, RegSAM.WOW64_64Key, "DisplayVersion");
                  string installDate = RegistryWrapper.GetRegKey(RegHive.LocalMachine, key, RegSAM.WOW64_64Key, "InstallDate");

                  if (displayName != null && displayName != "")
                  {
                    var installedProduct = new InstalledProgram(displayName, displayVersion, uninstallString, installDate);
                    if (!installedProgramsList.Contains(installedProduct))
                      installedProgramsList.Add(installedProduct);
                  }
                }
              }
            }
          }
          count = installedProgramsList.Count - count;
          Logger.Debug(count + " products found in HKEY_LOCAL_MACHINE\\" + keyPath);
          count = installedProgramsList.Count;
        }
      }
      // Add some logs for debugging
      if (Logger.level == (int)Logger.Level.DEBUG)
      {
        Logger.Debug("Products after Sort");
        int i = 1;
        foreach (var p in installedProgramsList)
        {
          Logger.Debug(i + " " + p);
          i++;
        }
      }
      return installedProgramsList;
    }
    #endregion 

    public static string GetWindowsUpdatesInfo()
    {
      StringBuilder output = new StringBuilder(128);

      // split the output by \r\n
      string[] rows = OSCollectorHelper.SystemInfo.Split(new Char[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
      int startingRow = 0;
      int i = 0;

      foreach (string row in rows)
      {
        if (row.Contains("Hotfix(s)"))
        {
          startingRow = i + 1;
          output.Append(row.Replace("Hotfix(s): ", "") + " ");
          break;
        }
        i++;
      }

      StringBuilder updates = new StringBuilder();
      if (startingRow != 0)
      {
        for (i = startingRow; i < rows.Length; i++)
        {
          if (rows[i].Trim().StartsWith("["))
            updates.Append(rows[i].Trim() + Html.br);
          else
            i = rows.Length;
        }
        output.Append(Html.AddLinkToHiddenContent(updates.ToString()));
      }
      return output.ToString();
    }

    public static string ToList()
    {
      installedProgramsList.Sort();
      List<string> temp = installedProgramsList.ConvertAll(obj => obj.ToString());
      String output = String.Join(Html.br, temp.ToArray());

      return Html.AddLinkToHiddenContent(output);
    }

    #region GetInstallerKeyNameFromGuid
    public static string GetInstallerKeyNameFromGuid(string GuidName)
    {
      if (GuidName.StartsWith("{") && GuidName.Length == 38)
      {
        string[] ProductId = GuidName.Replace("{", "").Replace("}", "").Split('-');
        StringBuilder ProductGUID = new StringBuilder();
        //Just reverse the first 3 parts
        for (int i = 0; i <= 2; i++)
        {
          ProductGUID.Append(ReverseString(ProductId[i]));
        }
        //For the last 2 parts, reverse each character pair
        for (int j = 3; j <= 4; j++)
        {
          for (int i = 0; i <= ProductId[j].Length - 1; i++)
          {
            ProductGUID.Append(ProductId[j][i + 1]);
            ProductGUID.Append(ProductId[j][i]);
            i += 1;
          }
        }
        return ProductGUID.ToString();
      }
      return "";
    }

    private static string ReverseString(string input)
    {
      char[] Chars = input.ToCharArray();
      Array.Reverse(Chars);
      return new String(Chars);
    }
    #endregion

  }
}
