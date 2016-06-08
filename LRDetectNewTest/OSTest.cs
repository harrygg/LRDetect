using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Collections.Generic;

namespace LRDetectNewTest
{
  [TestClass]
  public class OSTest
  {
    /// <summary>
    /// Windows XP, Service Pack 3	5.1.2600 
    /// Windows Server 2003, Service Pack 1	5.2.3790
    /// Windows Vista, Service Pack 2	6.0.6002
    /// Windows Server 2008	6.0.6001
    /// Windows 7	6.1.7601
    /// Windows Server 2008 R2, SP1	6.1.7601
    /// Windows Server 2012	6.2.9200
    /// Windows 8	6.2.9200
    /// Windows Server 2012 R2	6.3.9200 
    /// Windows 8.1	6.3.9200
    /// Windows 8.1, Update 1	6.3.9600
    /// </summary>
    /// 
    #region 12.01 Check Operating systems
    [TestMethod]
    public void OSWin7x64SP1_SupportedOn_1201_Returns_True()
    {
      var hostOS = new OS("6.1.7601", 1, 64); //Windows 7 SP1 x64

      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      bool supported = os != null;
      Assert.AreEqual(true, supported && os.recommended);
    }

    [TestMethod]
    public void OSWin2K8x64R2_SupportedOn_1201_Returns_True()
    {
      var hostOS = new OS("6.1.7601", 1, 64); //Windows Server 2008 R2 x64

      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      bool supported = os != null;
      Assert.AreEqual(true, supported && os.recommended);
    }

    [TestMethod]
    public void OSWin81x64_SupportedOn_1201_Returns_True()
    {
      var hostOS = new OS("6.3.9200", 0, 64); //Windows 8.1 x64

      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      bool supported = os != null;
      Assert.AreEqual(true, supported && os.recommended == false);
    }

    [TestMethod]
    public void OSWin7x64_SupportedOn_1201_Returns_False()
    {
      var hostOS = new OS("6.1.7601", 0, 64); //Windows 7 x64 No SPs
      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      Assert.AreEqual(true, os == null);
    }

    [TestMethod]
    public void OSWin7x32SP1_SupportedOn_1201_Returns_True()
    {
      var hostOS = new OS("6.1.7601", 1, 32); //Windows 7 SP1 x32

      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      bool supported = os != null;

      Assert.AreEqual(true, supported && os.recommended == false);
    }

    [TestMethod]
    public void OSWinVistax32SP2_SupportedOn_1201_Returns_False()
    {
      var hostOS = new OS("6.0.6002", 2, 32); //Windows XP SP3 x32

      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      bool supported = os != null;

      Assert.AreEqual(false, supported);
    }

    [TestMethod]
    public void OSWin2K3x32SP2_SupportedOn_1201_Returns_False()
    {
      var hostOS = new OS("5.2.3790", 2, 32); //Windows XP SP3 x32

      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      bool supported = os != null;

      Assert.AreEqual(false, supported);
    }

    [TestMethod]
    public void OSWinXPx32SP3_SupportedOn_1201_Returns_False()
    {
      var hostOS = new OS("5.1.2600", 3, 32); //Windows XP SP3 x32

      OS os = OS.FindSupportedOs(hostOS, OS.GetSupportedOSForProductVersion("12.01"));
      bool supported = os != null;

      Assert.AreEqual(false, supported);
    }
    #endregion

  }
}
