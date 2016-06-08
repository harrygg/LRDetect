using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.IO;

namespace LRDetectNewTest
{
  [TestClass]
  public class CorrelationRulesTest
  {
    public CorrelationRulesTest()
    {
      CorrelationRules.FilePath = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\" + CorrelationRules.settingsFileName);
    }


    /// <summary>
    /// Check if enabled group returns Yes
    /// </summary>
    [TestMethod]
    public void IsGroupEnabled_Enabled_Returns_True()
    {
      Assert.AreEqual("Yes", CorrelationRules.IsGroupEnabledText("Citrix_XenApp"));
    }


    /// <summary>
    /// Check if disabled group returns No
    /// AribaBuyer is disabled should return No
    /// </summary>
    [TestMethod]
    public void IsGroupEnabled_Disabled_Returns_False()
    {
      Assert.AreEqual("No", CorrelationRules.IsGroupEnabledText("AribaBuyer"));
    }

    [TestMethod]
    public void IsGroupEnabled_Unexisting_GroupName_Returns_Exception()
    {
      string expected = CorrelationRules.IsGroupEnabledText("Not_Existing_Group");
      Assert.AreEqual(true, expected.Contains("No correlation group with name"));
    }

    /// <summary>
    /// Get group by name
    /// </summary>
    [TestMethod]
    public void GetCorrelationGroupByName_Returns_CorrelationGroupObject()
    {
      string name = "Citrix_XenApp";
      var group = CorrelationRules.GetGroupByName(name);
      Assert.AreEqual(group.name, name);
    }

    /// <summary>
    /// Get group if it doesn't exist
    /// </summary>
    [TestMethod]
    public void GetCorrelationGroupByName_NoExistingGroup_Returns_Null()
    {
      var group = CorrelationRules.GetGroupByName("Not_Existing_Group");
      Assert.AreEqual(null, group);
    }

    /// <summary>
    /// Get group if it name is empty
    /// </summary>
    [TestMethod]
    public void GetCorrelationGroupByName_NameIsEmpty_Returns_Null()
    {
      var group = CorrelationRules.GetGroupByName("");
      Assert.AreEqual(null, group);
    }
    /// <summary>
    /// Get group if it name is null
    /// </summary>
    [TestMethod]
    public void GetCorrelationGroupByName_NameIsNull_Returns_Null()
    {
      var group = CorrelationRules.GetGroupByName(null);
      Assert.AreEqual(null, group);
    }
    /// <summary>
    /// Get rule by name if such group does not exist
    /// </summary>
    [TestMethod]
    public void GetCorrelationRuleByName_NoGroupExists_Returns_Null()
    {
      var rule = CorrelationRules.GetRuleByName("AutoDetect_AribaBuyer2", "Not_Existing_Group");
      Assert.AreEqual(null, rule);
    }

    /// <summary>
    /// Get rule by name if the group name is empty
    /// </summary>
    [TestMethod]
    public void GetCorrelationRuleByName_GroupNameEmpty_Returns_Null()
    {
      var rule = CorrelationRules.GetRuleByName("AutoDetect_AribaBuyer2", "");
      Assert.AreEqual(null, rule);
    }
    /// <summary>
    /// Get rule by name if the rule name is empty
    /// </summary>
    [TestMethod]
    public void GetCorrelationRuleByName_RullNameEmpty_Returns_Null()
    {
      var rule = CorrelationRules.GetRuleByName("", "AribaBuyer");
      Assert.AreEqual(null, rule);
    }
    /// <summary>
    /// Get rule by name if the group name is null
    /// </summary>
    [TestMethod]
    public void GetCorrelationRuleByName_GroupIsNull_Returns_Null()
    {
      var rule = CorrelationRules.GetRuleByName("AutoDetect_AribaBuyer2", null);
      Assert.AreEqual(null, rule);
    }
    /// <summary>
    /// Get rule by name if group exists but rule does not exist
    /// </summary>
    [TestMethod]
    public void GetCorrelationRuleByName_NoSuchRule_Returns_Null()
    {
      var rule = CorrelationRules.GetRuleByName("No_Such_Rule", "AribaBuyer");
      Assert.AreEqual(null, rule);
    }

    /// <summary>
    /// Get rule by name if group exists and rule exists
    /// </summary>
    [TestMethod]
    public void GetCorrelationRuleByName_Returns_RuleObject()
    {
      var rule = CorrelationRules.GetRuleByName("AutoDetect_AribaBuyer2", "AribaBuyer");
      Assert.AreEqual("AutoDetect_AribaBuyer2", rule.name);
    }


    /// <summary>
    /// Get callbackName property of existing rule
    /// </summary>
    [TestMethod]
    public void GetCorrelationRuleProperty_Returns_RuleObject()
    {
      var rule = CorrelationRules.GetRuleByName("AutoDetect_Siebel_Parse_Page", "Siebel");
      Assert.AreEqual("flCorrelationCallbackParseWebPage", rule.callbackName);
    }

  }
}
