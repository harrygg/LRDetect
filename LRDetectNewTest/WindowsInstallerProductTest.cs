using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;

namespace LRDetectNewTest
{
  /// <summary>
  /// Most of the tests are done on Visual Studio Proffesional 2012
  /// </summary>
  [TestClass]
  public class WindowsInstallerProductTest
  {

    string visualStudioUID = "822F50511C8BA103EAEE1A9F3C6B5002";

    [TestMethod]
    public void DisplayNameForExistingProduct_ReturnsName()
    {
      var wip = new WindowsInstallerProduct(visualStudioUID);
      Assert.AreEqual(true, wip.DisplayName.Contains("Visual Studio"));
    }

    /// <summary>
    /// Test non-existing products
    /// </summary>
    [TestMethod]
    public void DisplayNameForNonExistingProduct_ReturnsEmptyString()
    {
      var wip = new WindowsInstallerProduct("NonexistingUid");
      Assert.AreEqual(true, wip.DisplayName.Equals(""));
    }

    [TestMethod]
    public void IsNonExistingProductInstalled_ReturnsFalse()
    {
      var wip = new WindowsInstallerProduct("NonexistingUid");
      Assert.AreEqual(false, wip.isInstalled);
    }
  }
}
