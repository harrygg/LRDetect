using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Collections.Generic;
using Microsoft.Win32;

namespace LRDetectNewTest
{
  [TestClass]
  public class RegistryWrapperTest
  {
    #region Initializes all necessary keys
    public RegistryWrapperTest()
    {
      Create64Key();
      Create32Key();
    }
    void Create64Key()
    {
      //Create the key HKEY_LOCAL_MACHINE\SOFTWARE\Genev64
      using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64).OpenSubKey(@"Software\", true))
      {
        RegistryKey genev = key.CreateSubKey("Genev64");
        genev.SetValue("ID", "64");
        genev.SetValue("key2", "key2");
        genev.CreateSubKey("GenevSubKey");
      }
    }
    void Create32Key()
    {
      //Create the key HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Genev32
      using (RegistryKey key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"Software\", true))
      {
        key.CreateSubKey("Genev32").SetValue("ID", "32");
      }
    }

    #endregion

    string notExistingKeyPath = @"SOFTWARE\GenevTestKey";
    string Genev64KeyPath = @"SOFTWARE\Genev64";
    string Genev32KeyPath = @"SOFTWARE\Genev32";


    /// <summary>
    /// Get the value of HKEY_LOCAL_MACHINE\SOFTWARE\GenevTestKey\Name
    /// It should return "Hristo64"
    /// </summary>
    [TestMethod]
    public void GetRegKey_From64bitRegistry()
    {
      string actual = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, Genev64KeyPath, "ID");
      Assert.AreEqual("64", actual, "ID 64 expected but not found!");
    }
    /// <summary>
    /// Get the value of HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\GenevTestKey
    /// It should return "Hristo32"
    /// </summary>
    [TestMethod]
    public void GetRegKey_From32bitRegistry()
    {
      string actual = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, Genev32KeyPath, "ID");
      Assert.AreEqual("32", actual, "ID 32 expected but not found!");
    }

    /// <summary>
    /// Get the value of HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\GenevTestKey1
    /// There is no such key. 
    /// </summary>
    [TestMethod]
    public void GetRegKey_NotExistingRegistryKey()
    {
      string actual = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, notExistingKeyPath, "Name");
      Assert.AreEqual(null, actual, "Null expected but not returned!");
    }

    /// <summary>
    /// Enumerate keys from HKEY_LOCAL_MACHINE\SOFTWARE\Genev64
    /// There are more than 0 subkey 
    /// </summary>
    [TestMethod]
    public void GetValueNamesTest()
    {
      List<string> keys = RegistryWrapper.GetValueNames(RegHive.LocalMachine, Genev64KeyPath, RegSAM.WOW64_64Key);
      Assert.AreEqual(true, keys.Count == 2);
    }

    [TestMethod]
    public void GetFirstValueNameTest_Returns_Name()
    {
      string actual = RegistryWrapper.GetFirstValueName(RegHive.LocalMachine, Genev64KeyPath);
      Assert.AreEqual("ID", actual);
    }

    [TestMethod]
    public void GetFirstValueNameTest_Returns_Null()
    {
      string actual = RegistryWrapper.GetFirstValueName(RegHive.ClassesRoot, notExistingKeyPath);
      Assert.AreEqual(null, actual);
    }

    /// <summary>
    /// Gets the value HKEY_LOCAL_MACHINE\SOFTWARE\Genev64
    /// Should be 64
    /// </summary>
    [TestMethod]
    public void GetValue_Which_Exists_Only_In_64bit_Registry()
    {
      string actual = RegistryWrapper.GetValue(RegHive.LocalMachine, Genev64KeyPath, "ID");
      Assert.AreEqual("64", actual);
    }
    /// <summary>
    /// Gets the value HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\GenevTestKey\Key32
    /// Should be Value32
    /// </summary>
    [TestMethod]
    public void GetValue_Which_Exists_Only_In_32bit_Registry()
    {
      string actual = RegistryWrapper.GetValue(RegHive.LocalMachine, Genev32KeyPath, "ID");
      Assert.AreEqual("32", actual);
    }
    /// <summary>
    /// Get value of key which doesn't exists neither in 64 nor in 32 bit registry
    /// Returns null
    /// </summary>
    [TestMethod]
    public void GetValue_Which_Not_Exist_Returns_Null()
    {
      string actual = RegistryWrapper.GetValue(RegHive.LocalMachine, notExistingKeyPath, "Key32");
      Assert.AreEqual(null, actual);
    }

    [TestMethod]
    public void GetValue_Which_Not_Exist_Returns_Default()
    {
      string actual = RegistryWrapper.GetValue(RegHive.LocalMachine, notExistingKeyPath, "Key32", "DEFAULT");
      Assert.AreEqual("DEFAULT", actual);
    } 

    [TestMethod]
    public void GetSubKeyNames_Which_Not_Exist_ReturnsNull()
    {
      List<string> actual = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, notExistingKeyPath, RegSAM.WOW64_64Key);
      Assert.AreEqual(null, actual);
    }

    [TestMethod]
    public void GetSubKeyNames_Returns_1_result()
    {
      List<string> subKeys = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, Genev64KeyPath, RegSAM.WOW64_64Key);
      Assert.AreEqual(1, subKeys.Count);
    } 
  }
}
