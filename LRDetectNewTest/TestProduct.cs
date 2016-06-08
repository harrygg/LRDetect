using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LRDetectNewTest
{
  class TestProduct
  {
    public string uid;
    public string pCode;
    public string installLocation;
    public string exesLocation = "";
    public List<string> exes = new List<string>();
    public string displayName;
    public string displayVersion;
    public string installDate;
    public string installerKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\";
    public string testProductInstallerKeyPath { get { return installerKey + @"\" + pCode; } }
    public bool removeExesOnUninstall = false;

    public TestProduct()
    {
    }

    //Destructor to remove the created keys
    ~TestProduct()
    {
      RemoveTestProduct();
    }

    public bool InstallTestProduct()
    {

      try
      {
        //Create the necessary registries
        RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64).OpenSubKey(@"Installer\UpgradeCodes", true);
        key = key.CreateSubKey(uid);
        key.SetValue(pCode, "");

        key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(installerKey, true);
        key.CreateSubKey(pCode);

        RegistryKey testProductInstallerKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(testProductInstallerKeyPath, true);
        key = testProductInstallerKey.CreateSubKey("InstallProperties");

        key.SetValue("InstallLocation", installLocation);
        key.SetValue("DisplayVersion", displayVersion);
        key.SetValue("InstallDate", installDate);
        key.SetValue("DisplayName", displayName);

        key = testProductInstallerKey.CreateSubKey("Patches");
        key.SetValue("AllPatches", "");

        return true;
      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex.ToString());
        return false;
      }
    }

    public bool RemoveTestProduct()
    {
      try
      {
        RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64).OpenSubKey(@"Installer\UpgradeCodes", true);
        key.DeleteSubKey(uid);

        key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(installerKey, true);
        key.DeleteSubKeyTree(pCode);

        if (removeExesOnUninstall)
        {
          foreach (var exe in exes)
          {
            var file = Path.Combine(installLocation, exe);
            try { File.Delete(file); }
            catch (Exception) { Debug.Write(file + " was not deleted!"); };
          } 
          Directory.Delete(installLocation);
        }
        return true;
      }
      catch (Exception ex)
      {
        Debug.Write(ex.ToString());
        return false;
      }
    }

    public bool copyExesToInstallDir()
    {
      foreach (var exe in exes)
      {
        var copyFromPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "..\\..\\Executables\\" + exesLocation), exe);
        var copyToPath = Path.Combine(installLocation, exe);
        try
        {
          var dirName = Path.GetDirectoryName(copyToPath);
          if (!Directory.Exists(dirName))
            Directory.CreateDirectory(dirName);
          File.Copy(copyFromPath, copyToPath, true);
        }
        catch (Exception ex)
        {
          Debug.Write(ex.ToString());
          return false;
        }
      }
      return true;
    }
  }
}
