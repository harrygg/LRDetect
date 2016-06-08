using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.IO;
using Microsoft.Win32;

namespace LRDetectNewTest
{
  /// <summary>
  /// Summary description for UnitTest1
  /// </summary>
  [TestClass]
  public class DetectOtherSoftwareTest
  {
    public DetectOtherSoftwareTest()
    {
      //
      // TODO: Add constructor logic here
      //
    }

    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext
    {
      get
      {
        return testContextInstance;
      }
      set
      {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    //
    // You can use the following additional attributes as you write your tests:
    //
    // Use ClassInitialize to run code before running the first test in the class
    // [ClassInitialize()]
    // public static void MyClassInitialize(TestContext testContext) { }
    //
    // Use ClassCleanup to run code after all tests in a class have run
    // [ClassCleanup()]
    // public static void MyClassCleanup() { }
    //
    // Use TestInitialize to run code before running each test 
    // [TestInitialize()]
    // public void MyTestInitialize() { }
    //
    // Use TestCleanup to run code after each test has run
    // [TestCleanup()]
    // public void MyTestCleanup() { }
    //
    #endregion
    /*
    #region SAPGUI Detection TEST

    /// <summary>
    /// Detect SAPGUI if saplogon.exe doesn't exist in SAPGUI installation folder
    /// </summary>
    [TestMethod]
    public void DetectSapGui_File_Not_Exist()
    {
      string file = @"C:\Program Files (x86)\SAP\FrontEnd\SAPgui\saplogon.exe";
      string renamed = @"C:\Program Files (x86)\SAP\FrontEnd\SAPgui\saplogon.exe_";

      try
      {
        if (File.Exists(file) && !File.Exists(renamed))
        File.Move(file, renamed);

        string result = DetectOtherSoftware.GetSapGuiClientInfo();
        bool actual = result.StartsWith("No");
        bool expected = true;

        Assert.AreEqual(expected, actual);
      }
      catch (Exception)
      {
      }
      finally
      {
        if (File.Exists(renamed) && !File.Exists(file))
          File.Move(renamed, file);
      }
    }
    
    /// <summary>
    /// Detect Sapgui if there is no sapgui registry key
    /// Sapgui is not installed
    /// </summary>
    [TestMethod]
    public void DetectSapGui_Key_Not_Exist()
    {
      //change the key to not existing one
      DetectOtherSoftware.sapguiKeyPath = @"SOFTWARE\SAP\SAP Shared1\";
      string result = DetectOtherSoftware.GetSapGuiClientInfo();
      bool actual = result.StartsWith("Not");
      bool expected = true;

      Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Detect when SAPGUI is installed
    /// Returns true
    /// </summary>
    [TestMethod]
    public void DetectSapGui_Existing()
    {
      DetectOtherSoftware.sapguiKeyPath = @"SOFTWARE\SAP\SAP Shared\";
      string result = DetectOtherSoftware.GetSapGuiClientInfo();
      bool actual = result.StartsWith("Yes");
      bool expected = true;

      Assert.AreEqual(expected, actual);
    }
    #endregion
    */
    
    /*
    #region Test Firefox Detection
    [TestMethod]
    public void Firefox_Installed()
    {
      DetectOtherSoftware.firefoxName = "Mozilla Firefox 9999 LRDETECT_TEST";
      //simulate firefox installation. Create registry key entry
      string uninstaller = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\";
      using(RegistryKey regKey = Registry.LocalMachine.OpenSubKey(uninstaller, true).CreateSubKey(DetectOtherSoftware.firefoxName))
      {
        regKey.SetValue("DisplayVersion", "99");
      }

      string result = DetectOtherSoftware.GetFirefoxInfo();
      bool actual = result.StartsWith("Yes");
      bool expected = true;

      Assert.AreEqual(expected, actual);

      //Delete registry key entry
      using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(uninstaller, true))
      {
        regKey.DeleteSubKey(DetectOtherSoftware.firefoxName);
      }
    }

    [TestMethod]
    public void Firefox_Not_Installed()
    {
      //change the key to not existing one
      DetectOtherSoftware.firefoxName = "Mozilla Firefox_";
      string result = DetectOtherSoftware.GetFirefoxInfo();
      bool actual = result.StartsWith("No");
      bool expected = true;

      Assert.AreEqual(expected, actual);
    }
    #endregion*/
  }
}
