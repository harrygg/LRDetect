using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using LRDetect;

namespace LRDetectNewTest
{
  [TestClass]
  public class LRControllerTest
  {
    [TestMethod]
    public void FormatOutput_ExpectedOutput_ReturnsFormattedResult()
    {
      var output = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Test Resources\netstat wlrun output.txt"));
      var expected = ControllerSettingsCollectorHelper.FormatOutput(output, "1696");
      Assert.AreEqual(true, expected.Contains("4 connections found"));
    }
  }
}
