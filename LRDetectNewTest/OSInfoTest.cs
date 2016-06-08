using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.IO;

namespace LRDetectNewTest
{
  /// <summary>
  /// Summary description for UnitTest1
  /// </summary>
  [TestClass]
  public class OSInfoTest
  {
    public OSInfoTest()
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

    #region UAC
    [TestMethod]
    public void IsUACEnabledOnOldWindows_ReturnsNotSupported()
    {
      // Create a key that doesn't exist as in Windows XP and 2003
      OSCollectorHelper.uacKeyName = "EnableLUA_";
      string expected = "UAC is not supported for this OS";
      string actual = OSCollectorHelper.UACInfo();

      Assert.AreEqual(expected, actual);
    }
    #endregion

    #region Kerberos
    /// TESTS
    /// 1. %KRB5_CONFIG% not set C:\windows\krb5.ini exists RETURNS content
    /// 2. %KRB5_CONFIG% not set C:\windows\krb5.ini missing RETURNS Not detected
    /// 3. %KRB5_CONFIG% set C:\windows\krb5.ini exists RETURNS content
    /// 4. %KRB5_CONFIG% set C:\windows\krb5.ini missing RETURNS File not found


    // Get KRB configuration if krb5_config variable doesn't exists
    // but the the file c:\windows\krb5.ini exists
    [TestMethod]
    public void GetKRBContentFromFile_ReturnsContent()
    {
      // Create the file c:\windows\krb5.ini
      string path = @"c:\windows\krb5.ini";
      //Write some content in file
      CreateKrbFile(path);

      string content = OSCollectorHelper.GetKerberosConfiguration();
      bool actual = content.Contains("[libdefaults]");

      // Delete the file if it exists. 
      DeleteKrbFile(path);

      Assert.AreEqual(true, actual);
    }

    // Get KRB configuration from a c:\windows\krb5.ini file if krb5_config variable doesn't exists
    // and if c:\windows\krb5.ini doesn't exist
    [TestMethod]
    public void GetKRBContentFromFile_ReturnsNothing()
    {
      string path = @"c:\windows\krb5.ini";
      string newPath = @"c:\windows\krb5_.ini";

      // If the krb5.ini file exists, rename it so we don't find it 
      if (File.Exists(path))
        File.Move(path, newPath);
      
      string content = OSCollectorHelper.GetKerberosConfiguration();
      bool actual = content.Contains("Not detected");

      // If the file was renamed, revert back to the original name
      if (File.Exists(newPath))
        File.Move(newPath, path);

      Assert.AreEqual(true, actual);
    }

    // Get KRB configuration if krb5_config variable exists
    [TestMethod]
    public void GetKRBContentFromEnvVar_ReturnsContent()
    {
      // Create the file c:\windows\krb5.ini
      string path = @"c:\windows\krb5.ini";
      //Write some content in file
      CreateKrbFile(path);

      // Create the Env variable %krb5_config%
      Environment.SetEnvironmentVariable("KRB5_CONFIG", path);
      
      string content = OSCollectorHelper.GetKerberosConfiguration();
      bool actual = content.Contains("[libdefaults]");

      DeleteKrbFile(path);
      // Delete the environment variable
      Environment.SetEnvironmentVariable("KRB5_CONFIG", "");

      Assert.AreEqual(true, actual);
    }
    // Get KRB configuration if krb5_config variable exists
    // but points to a missing file
    [TestMethod]
    public void GetKRBContentFromEnvVar_ReturnsFileNotFound()
    {
      string path = @"c:\windows\krb5_stupid_name_that_dont_exist.ini";

      Environment.SetEnvironmentVariable("KRB5_CONFIG", path);

      string content = OSCollectorHelper.GetKerberosConfiguration();
      bool actual = content.Contains("File not found");

      Assert.AreEqual(true, actual);
    }


    private static void DeleteKrbFile(string path)
    {
      // Delete the file if it exists. 
      if (File.Exists(path))
      {
        File.Delete(path);
      }
    }

    private static void CreateKrbFile(string path)
    {
      using (FileStream fs = File.Create(path, 1024))
      {
        Byte[] info = new UTF8Encoding(true).GetBytes("[libdefaults]");
        // Add some information to the file.
        fs.Write(info, 0, info.Length);
      }
    }

    #endregion

    /*[TestMethod]
    public void IsOSDesktopEdition_Returns_True()
    {
      Assert.AreEqual(true, OSInfo.isOSDesktopEdition);
    }*/


  }
}
