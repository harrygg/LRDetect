using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LRDetect
{
  class CorrelationRules
  {
    //TODO check if this exists on 9.50
    internal static string settingsFileName = "CorrelationSettings.xml";
    internal static string defaultSettingsFileName = @"webrulesdefaultsettings\rulesdefaultsettings.xml";
    static List<CorrelationGroup> groups = new List<CorrelationGroup>();

    #region FilePath
    internal static string filePath;
    internal static string FilePath
    {
      get
      {
        if (filePath == null) //allows filePath to be set from CorrelationRulesTest.cs
        {
          if (ProductDetection.Vugen.IsInstalled)
          {
            filePath = Path.Combine(ProductDetection.Vugen.ConfigFolder, settingsFileName);
            // if the file doesn't exist, return the default settings
            if (!File.Exists(filePath))
              filePath = Path.Combine(ProductDetection.Vugen.DatFolder, defaultSettingsFileName);
          }
        }
        return filePath;
      }
      set
      { 
        //if the filePath is manually set from CorrelationRulesTest
        filePath = value;
      }
    }
    #endregion

    #region Constructor
    static CorrelationRules()
    {
      XmlDocument xmlSettings = new XmlDocument();
      if (!File.Exists(FilePath))
        return;
  
      xmlSettings.Load(FilePath);
      
      XmlNodeList groupNodes = xmlSettings.DocumentElement.GetElementsByTagName("Group");
      if (groupNodes != null && groupNodes.Count > 0)
      {
        foreach (XmlNode groupNode in groupNodes)
          groups.Add(new CorrelationGroup(groupNode));
      }  
    }
    #endregion

    /// <summary>
    /// Get Correlation group object by group name
    /// </summary>
    /// <param name="groupName"></param>
    /// <returns></returns>
    internal static CorrelationGroup GetGroupByName(string groupName)
    { 
      return String.IsNullOrEmpty(groupName) ? null : groups.Find(g => g.name.ToLower().Equals(groupName.ToLower()));
    }

    /// <summary>
    /// Searches and finds a correlation rule by given name and group.
    /// If no group name is supplied all groups are searched
    /// </summary>
    /// <param name="ruleName">The name of the rule</param>
    /// <param name="groupName">The rule group if known</param>
    /// <returns>CorrelationRule</returns>
    public static CorrelationRule GetRuleByName(string ruleName, string groupName)
    {
      if (String.IsNullOrEmpty(ruleName) || String.IsNullOrEmpty(groupName))
        return null;

      var group = GetGroupByName(groupName);
      return group == null ? null : group.rules.Find(r => r.name.ToLower().Equals(ruleName.ToLower()));
    }

    public static string IsGroupEnabledText(string groupName)
    {
      try
      {
        var group = GetGroupByName(groupName);
        return Html.BoolToYesNo(group.enabled);
      }
      catch (NullReferenceException)
      { 
        return "No correlation group with name " + groupName + " found!";
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return ex.Message;
      }
    }

    internal static string GetListOfRulesText()
    {
      try
      {
        StringBuilder output = new StringBuilder();
        if (groups.Count > 0)
        {
          foreach (CorrelationGroup group in groups)
            output.Append(group.ToString());
          return output.ToString();
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
      return Html.ErrorMsg();
    }

    internal static string GetRawContent()
    {
      try
      {
        string content = File.ReadAllText(CorrelationRules.FilePath);
        return Html.UrlEncode(content);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return ex.Message;
      }
    }
  }

  #region CorrelationGroup class
  public class CorrelationGroup
  {
    public string name;
    public bool enabled = false;
    public List<CorrelationRule> rules = new List<CorrelationRule>();

    public CorrelationGroup(XmlNode node)
    {
      name = node.Attributes["Name"].Value;
      enabled = node.Attributes["Enable"].Value == "1";

      if (node.HasChildNodes)
      {
        foreach (XmlNode ruleNode in node.ChildNodes)
        {
          CorrelationRule rule = new CorrelationRule(ruleNode);
          rules.Add(rule);
        }
      }
    }

    public override string ToString()
    {
      return Html.CheckBox(enabled) + name + " " + Html.AddLinkToHiddenContent(RulesToHtml()) + Html.br;
    }

    string RulesToHtml()
    { 
      StringBuilder output = new StringBuilder();
      foreach (CorrelationRule rule in rules)
      {
        output.Append(rule.ToString() + Html.br);
      }
      return output.ToString();
    }
  }
  #endregion

  #region CorrelationGroup rule
  public class CorrelationRule
  {
    public string name;
    public bool enabled = false;
    public string lb;
    public string rb;
    public bool isXpath;
    public bool isRegex;
    public string callbackName;
    public string callbackDllName;

    public CorrelationRule(XmlNode node)
    {
      var attribute = node.Attributes["Name"];
      name = attribute != null ? attribute.Value : "";

      attribute = node.Attributes["LeftBoundText"];
      lb = attribute != null ? Html.UrlEncode(attribute.Value) : "";

      attribute = node.Attributes["RightBoundText"];
      rb = attribute != null ? Html.UrlEncode(attribute.Value) : "";

      attribute = node.Attributes["CallbackName"];
      callbackName = attribute != null ? attribute.Value : "";

      attribute = node.Attributes["CallbackDLLName"];
      callbackDllName = attribute != null ? attribute.Value : "";

      int flags = Convert.ToInt32(node.Attributes["Flags"].Value);
      // Check if the rule is disabled by doing
      // bitwise AND and checking the 0x80 bit is up
      // 128 is decimal for 0000000010000000
      enabled = 0 == ((RuleDefFlags)flags & RuleDefFlags.Disabled);
      isXpath = RuleDefFlags.Xpath == ((RuleDefFlags)flags & RuleDefFlags.Xpath);
      isRegex = RuleDefFlags.Regex == ((RuleDefFlags)flags & RuleDefFlags.Regex);
    }

    public override string ToString()
    {
      StringBuilder output = new StringBuilder();
      //Create HTML checkbox
      var el = new HtmlElement("input");
      el.Attributes.Add("type", "checkbox");
      el.Attributes.Add("checked", enabled ? "checked" : "");

      string ruleText;
      if (isRegex)
        ruleText = " Regex rule: " + lb;
      else if (isXpath)
        ruleText = " Xpath rule: " + lb;
      else
        ruleText = " LB=\"" + lb + "\" RB=\"" + rb + "\"";

      output.Append(Html.IndentWithSpaces(8));
      output.Append(el.ToString() + Html.B(name) + ruleText);

      return output.ToString();
    }
  }
  #endregion

  #region Correlation RuleDefFlags
  [Flags]
  public enum RuleDefFlags
  {
    Default = 0x0,
    MatchCase = 0x1,
    ReverseSearch = 0x2,
    AlwaysCreateParam = 0x4,
    UsePrefix = 0x8,
    AlwaysReuseParam = 0x10,
    Disabled = 0x80, //0000000010000000 or 128
    CustomSearch = 0x100,
    UseNumeric = 0x200,
    ExactMatch = 0x400,
    SameParamName = 0x800,
    BoundsAreBinary = 0x1000,
    Xpath = 0x4000, //0100000000000000 16384
    Regex = 0x8000 //1000000000000000 32768
  }
  #endregion
}
