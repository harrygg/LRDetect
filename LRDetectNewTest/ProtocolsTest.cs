using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

namespace LRDetectNewTest
{
  [TestClass]
  public class ProtocolsTest
  {



   [TestMethod]
    public void ConvertNetshCommandOutput()
    {
      var output = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\Test Resources\netsh output.txt"));
      var expected = VugenProtocolsCollectorHelper.TruClientFF.FilterPorts(output);
     
      Assert.AreEqual(true, expected.Contains("127.0.0.1:8080"));
    }
    
    
    /*
    [TestMethod]
    public void GetJavaIniOption_GetExistingBoolOption_Returns_No()
    {
      CreateVuGenIni();

      VugenProtocols.Java.ini = new IniParser("vugen.ini");
      var saveParams = VugenProtocols.Java.GetJavaIniBoolOption("Java_Prepend_Classpath");
      Assert.AreEqual("No", saveParams);

    }


    private void CreateVuGenIni()
    {
      string content = "[JavaVM:Options]\r\nJava_Env_ClassPath=C:\\spring-remoting-1.2.1.jar;C:\\CRSDTO.jar;\r\nJava_Prepend_Classpath=1\r\nJava_VM_Params=-Xmx=256Mb\r\nJava_VM_Params_Choice_1= -Xmx=256Mb\r\nJava_SaveParams=1\r\nJava_Prepend_Classpath=0";
      File.WriteAllText("vugen.ini", content);
    }*/
  }
}
