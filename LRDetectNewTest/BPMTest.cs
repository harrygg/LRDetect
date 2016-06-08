using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Diagnostics;
using System.Collections.Generic;

namespace LRDetectNewTest
{
  [TestClass]
  public class BPMTest
  {
    TestProduct t;
    ProductInfo BPM = null;
    public BPMTest()
    {
      t = new TestProduct { uid = "F3770988A38F9A74AABC8781784C173D", pCode = "00000000000000000_GENEV_PCODE_BPM", displayName = "HP BPM", displayVersion = "9.23", installDate = "20140419", installLocation = @"C:\HP\BPM", exesLocation = "BPM_11.52", removeExesOnUninstall = true };
      if (t.InstallTestProduct())
      {
        Debug.WriteLine("Test product installed: " + t.displayName);

        t.exes = new List<string> { @"client\SharedParameter.dll", @"install\srvany.exe", @"web\node.exe" };
        if (t.copyExesToInstallDir())
          BPM = new VirtualTableServer();

      }
    }
  }
}
