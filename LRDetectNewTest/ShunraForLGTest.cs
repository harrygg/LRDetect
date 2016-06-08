using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Win32;
using LRDetect;

namespace LRDetectNewTest
{
  [TestClass]
  public class ShunraForLGTest
  {
    string uid = "6D125BB6CB71DBE4C9CF669B33EB7AC5";
    string pCode = "D3E0B7B7F382AC748917CDB1FC2F8CE1";

    public ShunraForLGTest()
    {
      InstallProduct();
    }

    ~ShunraForLGTest()
    {
      //UninstallProduct();
    }

    void InstallProduct()
    {
      RegistryKey key;

      //Install Shunra for LG
      ////HKEY_CLASSES_ROOT\Installer\Products\6D125BB6CB71DBE4C9CF669B33EB7AC5
      key = Registry.ClassesRoot.CreateSubKey(@"Installer\UpgradeCodes\" + uid, RegistryKeyPermissionCheck.ReadWriteSubTree);
      key.SetValue(pCode, "");
      key.Close();

      //HKEY_CLASSES_ROOT\Installer\Products\D3E0B7B7F382AC748917CDB1FC2F8CE1
      key = Registry.ClassesRoot.CreateSubKey(@"Installer\Products\" + pCode, RegistryKeyPermissionCheck.ReadWriteSubTree);
      key.SetValue("PackageCode", "8D6CBBF351704F84EA3DB08D11B197D4");
      key.SetValue("ProductName", "Shunra NV for HP Load Generator");
      key.Close();

      RegistryKey localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
      //HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\D3E0B7B7F382AC748917CDB1FC2GENEV
      key = localKey.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + pCode + @"\InstallProperties");
      key.SetValue("InstallLocation", @"C:\Program Files (x86)\Shunra\");
      key.SetValue("InstallDate", "20140618");
      key.SetValue("DisplayName", "Shunra NV for HP Load Generator");
      key.SetValue("DisplayVersion", "8.61.0.146");
      key.SetValue("WindowsInstaller", "1", RegistryValueKind.DWord);
      key.Close();

      //Switch to Wow6432Node
      localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
      //HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Shunra\Bootstrapper
      key = localKey.CreateSubKey(@"SOFTWARE\Shunra\Bootstrapper");
      key.SetValue("BuildVersion", "2013.0711.1424.29");
      key.SetValue("CurrentVersion", "8.61.0.146");
      key.SetValue("InstalledPath", @"C:\Program Files (x86)\Shunra\");
      key.Close();

      //HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Shunra\ShunraAPI
      key = localKey.CreateSubKey(@"SOFTWARE\Shunra\ShunraAPI");
      key.SetValue("TraceFolder", @"C:\Program Files (x86)\Shunra\logs");
      key.SetValue("InstalledPath", @"C:\Program Files (x86)\Shunra\");
      key.Close();

      //HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Shunra\vCat
      key = localKey.CreateSubKey(@"SOFTWARE\Shunra\vCat");
      key.SetValue("BuildVersion", "2013.0711.1424.29");
      key.SetValue("CurrentVersion", "8.61.0.146");
      key.SetValue("DataPath", @"C:\ProgramData\Shunra\");
      key.SetValue("ProductName", "Shunra NV for HP Load Generator");
      key.SetValue("InstalledPath", @"C:\Program Files (x86)\Shunra\");
      key.Close();

      //Add product to Uninstaller
      //HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_{7B7B0E3D-283F-47CA-9871-DC1BCFF2C81E}
      key = localKey.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_{7B7B0E3D-283F-47CA-9871-DC1BCFF2C81E}");
      key.SetValue("DisplayName", "Shunra NV for HP Load Generator v8.61.0.146");
      key.SetValue("DisplayVersion", "8.61.0.146");
      key.SetValue("InstallDate", "20140618");
      key.SetValue("InstallLocation", @"C:\Program Files (x86)\Shunra\");
      key.SetValue("InstallSource", @"C:\Windows\Downloaded Installations\{3FBBC6D8-0715-48F4-AED3-0BD8111B794D}\");
      key.SetValue("UninstallString", "C:\\Program Files (x86)\\InstallShield Installation Information\\{7B7B0E3D-283F-47CA-9871-DC1BCFF2C81E}\\setup.exe\" -runfromtemp -l0x0409  -removeonly");
      key.Close();
    }


    void UninstallProduct()
    {
      //Remove keys
      RegistryKey localKey;
      localKey = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64);
      localKey.DeleteSubKey(@"Installer\UpgradeCodes\" + uid, false);
      localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
      localKey.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + pCode, false);

      localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
      localKey.DeleteSubKeyTree(@"SOFTWARE\Shunra", false);
      localKey.DeleteSubKeyTree(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\InstallShield_{7B7B0E3D-283F-47CA-9871-DC1BCFF2C81E}", false);
    }


    [TestMethod]
    public void ShunraOnLG_IsInstalled_True()
    {
      var p = new ShunraForLG();
      Assert.AreEqual(true, p.IsInstalled);
    }
  }
}
