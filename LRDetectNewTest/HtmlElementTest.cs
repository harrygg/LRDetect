using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;

namespace LRDetectNewTest
{
  [TestClass]
  public class HtmlElementTest
  {
    [TestMethod]
    public void HtmlElementCreate_A()
    {
      var element = new HtmlElement("a");
      element.Attributes.Add("id", "aaa");
      element.Attributes.Add("class", "ShowMore");
      element.Attributes.Add("href", "http");
      element.Attributes.Add("title", "Click me please!!!");
      element.text = "Click here";

      var expected = String.Format("<a id=\"aaa\" class=\"ShowMore\" href=\"http\" title=\"Click me please!!!\">Click here</a>");
      var actual = element.ToString();
      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void HtmlElementCreate_Div()
    {
      var element = new HtmlElement("div");
      element.Attributes.Add("id", "divId");
      element.Attributes.Add("class", "divClass");
      element.text = "This is a DIV";

      var expected = String.Format("<div id=\"divId\" class=\"divClass\">This is a DIV</div>");
      var actual = element.ToString();
      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void HtmlElementCreate_CheckBox()
    {
      var element = new HtmlElement("input");
      element.Attributes.Add("id", "checkBoxId");
      element.Attributes.Add("type", "checkbox");
      element.Attributes.Add("checked", "checked");
      element.Attributes.Add("value", "This is a CheckBox");

      var expected = "<input id=\"checkBoxId\" type=\"checkbox\" checked value=\"This is a CheckBox\" />";
      var actual = element.ToString();
      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void HtmlElementCreate_RadioBox()
    {
      var element = new HtmlElement("input");
      element.Attributes.Add("type", "radio");
      element.Attributes.Add("class", "");
      element.Attributes.Add("name", "");
      element.Attributes.Add("checked", "checked");
      element.Attributes.Add("disabled", "disabled");
      element.Attributes.Add("value", "This is a radio box");

      var expected = String.Format("<input type=\"radio\" class=\"\" name=\"\" checked disabled value=\"This is a radio box\" />");
      var actual = element.ToString();
      Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public void HtmlElementCreate_Span()
    {
      var element = new HtmlElement("span");
      element.Attributes.Add("id", "");
      element.Attributes.Add("style", "text-decoration:underline;");
      element.text = "This is a span";

      var expected = String.Format("<span id=\"\" style=\"text-decoration:underline;\">This is a span</span>");
      var actual = element.ToString();
      Assert.AreEqual(expected, actual);
    }


    [TestMethod]
    public void HtmlElementCreate_Pre()
    {
      var element = new HtmlElement("pre");
      element.text = "This is a PRE";

      var expected = String.Format("<pre>This is a PRE</pre>");
      var actual = element.ToString();
      Assert.AreEqual(expected, actual);
    }
  }
}
