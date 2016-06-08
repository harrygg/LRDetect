using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Text.RegularExpressions;

namespace LRDetectNewTest
{
  [TestClass]
  public class AddRemoveProgramsTest
  {
    //Input {00EC8ABC-3C5A-40F8-A8CB-E7DCD5ABFA05}
    //Output CBA8CE00A5C38F048ABC7ECD5DBAAF50
    [TestMethod]
    public void TestMSIProductCodeConversion()
    {
      string uninstallerCode = "{00EC8ABC-3C5A-40F8-A8CB-E7DCD5ABFA05}";
      string expected = "CBA8CE00A5C38F048ABC7ECD5DBAAF50";
      string actual = InstalledProgramsHelper.GetInstallerKeyNameFromGuid(uninstallerCode);

      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestMSIProductCodeConversion_Shorter_Name()
    {
      string uninstallerCode = "{00EC8ABC";
      string expected = "";
      string actual = InstalledProgramsHelper.GetInstallerKeyNameFromGuid(uninstallerCode);

      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestMSIProductCodeConversion_No_Such_Package_Name()
    {
      string uninstallerCode = "NO_SUCH_CODE";
      string expected = "";
      string actual = InstalledProgramsHelper.GetInstallerKeyNameFromGuid(uninstallerCode);

      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void TestGetInstalledProgramByName_Chrome()
    {
      string name = "Google Chrome";
      InstalledProgram product = InstalledProgramsHelper.GetInstalledProgramByName(name);
      bool actual = product.DisplayName.Contains(name);

      Assert.AreEqual(true, actual);
    }


    /// <summary>
    /// Test if Visual Studio is installed, of course it will always be
    /// </summary>
    [TestMethod]
    public void TestGetInstalledProgramByName_Regex_VisualStudio()
    {

      Regex name = new Regex("[M|m]icrosoft [V|v]isual [S|s]tudio");
      
      InstalledProgram product = InstalledProgramsHelper.GetInstalledProgramByName(name);
      bool actual = product != null;

      Assert.AreEqual(true, actual);
    }


    [TestMethod]
    public void Test_GetWindowsUpdates()
    {
      string updates = InstalledProgramsHelper.GetWindowsUpdatesInfo();
      Assert.AreEqual(true, updates.Contains("Hotfix(s) Installed"));    
    }
  }
}
