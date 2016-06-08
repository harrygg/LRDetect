using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;

namespace LRDetectNewTest
{
  [TestClass]
  public class CitrixHelperTest
  {
    [TestMethod]
    public void FormatSessionTimeout_Return_Never()
    {
      var actual = CitrixHelper.FormatSessionTimeout("0"); 
      Assert.AreEqual("Never", actual); 
    }


    [TestMethod]
    public void FormatSessionTimeout_Return_Days()
    {
      string actual = CitrixHelper.FormatSessionTimeout("432000000"); //Should return 5 days
      Assert.AreEqual("5 Days", actual);
    }

    [TestMethod]
    public void FormatSessionTimeout_Return_Hours()
    {
      string actual = CitrixHelper.FormatSessionTimeout("64800000"); //Should return 18 hours
      Assert.AreEqual("18 Hours", actual);
    }

    [TestMethod]
    public void FormatSessionTimeout_Return_Minutes()
    {
      string actual = CitrixHelper.FormatSessionTimeout("300000"); //Should return 5 Minutes
      Assert.AreEqual("5 Minutes", actual);
    }

    [TestMethod]
    public void FormatSessionTimeout_Return_1Minute()
    {
      string actual = CitrixHelper.FormatSessionTimeout("60000"); //Should return 1 Minute
      Assert.AreEqual("1 Minute", actual);
    }


    [TestMethod]
    public void FormatSessionTimeout_Return_Undefined()
    {
      string actual = CitrixHelper.FormatSessionTimeout("300001"); //Should return Never + the time
      Assert.AreEqual("Never (Incorrect value detected 00:05:00.0010000)", actual);
    }


  }
}
