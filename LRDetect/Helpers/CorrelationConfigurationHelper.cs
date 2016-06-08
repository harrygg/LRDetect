using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LRDetect
{
  class CorrelationConfigurationHelper
  {
    static XmlDocument doc = new XmlDocument();
    public static Hashtable Settings = new Hashtable()
    {
      {"ScanAfterCodeGeneration", "False"}
      , {"IsIgnoreCases", "True"}
      , {"AutomaticApplyRules", "True"}
      , {"WarnOnLargeDifferences", "False"}
      , {"MinCorrelationLength", "4"}
      , {"MaxCorrelationLength", "4096"}
      , {"RuleHeuristicLevel", "Medium"}
      , {"TypeOfScanDiff", "1"}
      , {"UseRegExpCorrelation", "True"}
      , {"ReplayScanEnabled", "True"}
      , {"RuleScanEnabled", "True"}
      , {"RecordScanEnabled", "True"}
    };

    static CorrelationConfigurationHelper()
    {
      string file = Path.Combine(ProductDetection.Vugen.ConfigFolder, "CorrelationConfiguration.xml");
      if (File.Exists(file))
      {
        try
        {
          doc.Load(file);
          Hashtable temp = new Hashtable();
          foreach (string key in Settings.Keys)
            temp[key] = GetValue(key);
          Settings = temp;
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }
    }

    public static string GetValue(string optionName)
    {
      string nodePath = "/CorrelationOptions/Correlation." + optionName;
      try
      {
        var node = doc.SelectSingleNode(nodePath);
        return node.InnerText;
      }
      catch (Exception)
      {
        return Settings[optionName].ToString();
      }
    }
  }
}
