using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;



namespace LRDetectNewTest
{
  [TestClass]
  public class DetectJavaTest
  {
    DetectJava dj = new DetectJava();

    [TestMethod]
    public void ExecuteJavaVersionCommand_Returns_Java_Output()
    {
      var cmd = dj.GetJavaVersionFromCmd(@"C:\Program Files (x86)\Java\jre7\");
      Assert.AreEqual(true, cmd.StartsWith("1.7"));
    }

    [TestMethod]
    public void FormatJavaVersionFromCmdstring_Returns_Java_Version()
    {
      var output = "java version \"1.7.0_51\"\r\nJava(TM) SE Runtime Environment (build 1.7.0_51-b13)\r\nJava HotSpot(TM) Client VM (build 24.51-b03, mixed mode, sharing)";

      var actual = dj.FormatJavaVersionFromCmdString(output);

      Assert.AreEqual(true, actual.Equals("1.7.0_51"));
    }
  }
}
