using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Collections.Generic;
using System.Collections;


namespace LRDetectNewTest
{
  [TestClass]
  public class JsonTest
  {
    string json = "{\"JsonDemo\": {\"ZeroValue\": {\"value\": 0	}, \"EmptyValue\": {\"value\": \"\" }, \"BoolValue\": {\"value\": true } } }";

    [TestMethod]
    public void ReadJsonFile_Returns_Dictionary()
    {
      Hashtable ht = (Hashtable)JSON.JsonDecode(json);
      ht = (Hashtable)ht["JsonDemo"];

      Hashtable ht1 = (Hashtable)ht["ZeroValue"];
      var ZeroValue = ht1["value"].ToString();

      ht1 = (Hashtable)ht["EmptyValue"];
      var EmptyValue = ht1["value"].ToString();

      ht1 = (Hashtable)ht["BoolValue"];
      var BoolValue = ht1["value"].ToString();

      Assert.AreEqual(true, (ZeroValue == "0" && EmptyValue == "" && BoolValue == "True"));
    }

    /*[TestMethod]
    public void ReadTCIEPrefContent()
    {
      Hashtable settings = null;
      settings = VugenProtocols.TruClientIE.GetPreferencesFromJson("lrwebIE_browser_master_prefs.json");

      Assert.AreEqual(true, settings != null);

    
    }*/
  }
}
