using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Diagnostics;
using System.Collections.Generic;

namespace LRDetectNewTest
{
  [TestClass]
  public class VTSTest
  {  
    TestProduct t;
    ProductInfo VTS = null;
    public VTSTest()
    {
      t = new TestProduct { uid = "E86DE06E377BEEC4F8B5E243A797A42B", pCode = "00000000000000000_GENEV_PCODE_VTS", displayName = "HP VTS", displayVersion = "11.52", installDate = "20140319", installLocation = @"C:\Program Files\HP\VTS", exesLocation = "VTS_11.52", removeExesOnUninstall = true };
      if (t.InstallTestProduct())
      {
        Debug.WriteLine("Test product installed: " + t.displayName);

        t.exes = new List<string> { @"client\SharedParameter.dll", @"install\srvany.exe", @"web\node.exe" };
        if (t.copyExesToInstallDir())
          VTS = new VirtualTableServer();

      }
    }

    #region Analysis test
    [TestMethod]
    public void VTS_ProductName_ReturnsName()
    {
      Assert.AreEqual(true, VTS.ProductName.Contains("HP VTS"));
    }

    [TestMethod]
    public void VTS_GetProductNameVersionDateFormatted_ReturnsString()
    {
      var v = VTS.GetProductNameVersionDateFormatted();
      Assert.AreEqual(true, v.Contains("Yes " + Html.B(t.displayName) + " " + t.displayVersion + Helper.ConvertInstallDate(t.installDate)));
    }
    #endregion
  }
}
