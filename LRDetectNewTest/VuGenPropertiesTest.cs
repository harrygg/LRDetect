using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;

namespace LRDetectNewTest
{
  [TestClass]
  public class VuGenPropertiesTest
  {
    [TestMethod]
    public void GetDefaultStartPort_Returns_8080()
    {
      VuGenProperties vp = new VuGenProperties();
      Assert.AreEqual(8080, vp.startPort);
    }

    [TestMethod]
    public void GetDefaultStartPort_MissingPropertiesFile_Returns_8080()
    {
      VuGenProperties vp = new VuGenProperties("Not_Existing_file.xml");
      Assert.AreEqual(8080, vp.startPort);
    }


  }
}
