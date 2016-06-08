using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;

namespace LRDetectNewTest
{
  [TestClass]
  public class HelperTest
  {
    [TestMethod]
    public void ConvertInstallDate_GetEmptyInput_Returns_EmptyString()
    {
      string actual = Helper.ConvertInstallDate("");
      Assert.AreEqual("", actual);
    }

    [TestMethod]
    public void ConvertInstallDate_Get_NULL_Returns_EmptyString()
    {
      string actual = Helper.ConvertInstallDate(null);
      Assert.AreEqual("", actual);
    }

    [TestMethod]
    public void ConvertInstallDate_Returns_Success()
    {
      string actual = Helper.ConvertInstallDate("20140319");
      Assert.AreEqual(" installed on 3/19/2014", actual);
    }

    [TestMethod]
    public void ConvertInstallDate_Get_ShorterString_Returns_Success()
    {
      string actual = Helper.ConvertInstallDate("333");
      Assert.AreEqual("", actual);
    }
  }
}
