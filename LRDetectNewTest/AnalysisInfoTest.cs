using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Diagnostics;
using Microsoft.Win32;

namespace LRDetectNewTest
{
  [TestClass]
  public class AnalysisInfoTest
  {
    #region Analysis test Initializations

    TestProduct t;
    ProductInfo Analysis = null;
    public AnalysisInfoTest()
    {
      t = new TestProduct { uid = "CE53A34494FAD3C4BB524D4EA62DB6FF", pCode = "00000000000_GENEV_PCODE_ANALYSIS", displayName = "HP Analysis", displayVersion = "12.00", installDate = "20140319", installLocation = @"C:\Program Files (x86)\HP\LoadRunner\" };
      if (t.InstallTestProduct())
      {
        Debug.WriteLine("Test product installed: " + t.displayName);
        Analysis = new AnalysisInfo(); 
      }
    }
    #endregion

    #region Analysis test
    [TestMethod]
    public void Analysis_BinLocation_ReturnsBinFolder()
    {
      Assert.AreEqual(true, Analysis.BinFolder.Equals(@"C:\Program Files (x86)\HP\LoadRunner\bin"));
    }
    #endregion
  }
}
