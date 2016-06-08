using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using Microsoft.Win32;
using System.Diagnostics;

namespace LRDetectNewTest
{
  [TestClass]
  public class ProductInfoTest
  {
  
    ProductInfo FullLR = null;
    //ProductInfo Analysis = null;
    public ProductInfoTest()
    {
      FullLR = new LoadRunnerInfo();
    }


    #region GetProductCode Tests
    [TestMethod]
    public void GetProductCode_ExistingUpgradeCode_Returns_ProductCode()
    {
      string uid = "00000000000000000000_GENEV_UCODE";
      string productCode = "00000000000000000000_GENEV_PCODE";

      //Create the key HKEY_LOCAL_MACHINE\SOFTWARE\Genev64
      using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Registry64).OpenSubKey(@"Installer\UpgradeCodes", true))
      {
        RegistryKey genev = key.CreateSubKey(uid);
        genev.SetValue(productCode, "");
        string actual = ProductInfo.GetProductCode(uid);
        Assert.AreEqual(productCode, actual);

        key.DeleteSubKey(uid);
      } 
    }
    [TestMethod]
    public void GetProductCode_NotExistingUpgradeCode_Returns_Null()
    {
      string actual = ProductInfo.GetProductCode("00000000000000000000_GENEV_UCODE_");
      Assert.AreEqual(null, actual);
    }
    #endregion 

    #region LoadRunner Installation Tests
    [TestMethod]
    public void LR_Product_Name_Returns_Name()
    {
      string actual = FullLR.ProductName;
      Assert.AreEqual(true, actual.Contains("HP LoadRunner"));
    }

    [TestMethod]
    public void LR_ProductVersion_Returns_ProductVersion()
    {
      string actual = FullLR.ProductVersion;
      using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"Software\Mercury Interactive\LoadRunner\CurrentVersion", true))
      {
        Assert.AreEqual(true, actual.Equals(String.Format("{0}.{1}", key.GetValue("Major"), key.GetValue("Minor"))));
      }
    }

    [TestMethod]
    public void LR_Version_Returns_Version()
    {
      Version actual = FullLR.version;
      using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"Software\Mercury Interactive\LoadRunner\CurrentVersion", true))
      {
        Version v = new Version(Convert.ToInt32(key.GetValue("Major")), Convert.ToInt32(key.GetValue("Minor")));
        Assert.AreEqual(true, actual.Equals(v));
      }
    }


    //Test to check if we get the version correctly if the keys Major and Minor are empty in 
    //HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mercury Interactive\LoadRunner\CurrentVersion
    [TestMethod]
    public void LR_Version_If_MajorMinor_Empty_Returns_Version()
    {
      using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"Software\Mercury Interactive\LoadRunner\CurrentVersion", true))
      {
        // Save and then delete the 2 keys in HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mercury Interactive\LoadRunner\CurrentVersion
        // so we force the detection from Installer key
        string tempMajor = key.GetValue("Major").ToString();
        key.SetValue("Major", "");
        string tempMinor = key.GetValue("Minor").ToString();
        key.SetValue("Minor", "");

        var lr = new LoadRunnerInfo();

        Version actual = lr.version;
        Version current = new Version(Convert.ToInt32(tempMajor), Convert.ToInt32(tempMinor));

        // restore the keys
        key.SetValue("Major", tempMajor);
        key.SetValue("Minor", tempMinor);

        Assert.AreEqual(true, actual.Equals(current));
      }
    }

    //Test to check if we get the version correctly if the keys Major and Minor are missing in 
    //HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mercury Interactive\LoadRunner\CurrentVersion
    [TestMethod]
    public void LR_Version_If_MajorMinor_NotExist_Returns_Version()
    {
      using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"Software\Mercury Interactive\LoadRunner\CurrentVersion", true))
      {
        // Save and then delete the 2 keys in HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Mercury Interactive\LoadRunner\CurrentVersion
        // so we force the detection from Installer key
        string tempMajor = key.GetValue("Major").ToString();
        key.DeleteValue("Major");
        string tempMinor = key.GetValue("Minor").ToString();
        key.DeleteValue("Minor");

        var lr = new LoadRunnerInfo();

        Version actual = lr.version;
        Version current = new Version(Convert.ToInt32(tempMajor), Convert.ToInt32(tempMinor));

        // restore the keys
        key.SetValue("Major", tempMajor);
        key.SetValue("Minor", tempMinor);

        Assert.AreEqual(true, actual.Equals(current));
      }
    }


    [TestMethod]
    public void LR_Product_InstallLocation()
    {
      Assert.AreEqual(true, FullLR.InstallLocation.Equals(@"C:\Program Files (x86)\HP\LoadRunner\"));
    }

    [TestMethod]
    public void LR_Product_BinLocation()
    {
      Assert.AreEqual(true, FullLR.BinFolder.Equals(@"C:\Program Files (x86)\HP\LoadRunner\bin"));
    }
    #endregion
  }

  
}
