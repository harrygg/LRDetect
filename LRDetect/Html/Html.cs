using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  public class Html
  {
    public static string br = "<br />\n";
    public static string hr = "<hr />\n";
    public static string tab = "&nbsp;&nbsp;&nbsp;&nbsp;";

    public static string B(string innerHTML)
    {
      return new HtmlElement("b") { text = innerHTML }.ToString();
    }

    public static string Small(string innerHTML)
    {
      return new HtmlElement("small") { text = innerHTML }.ToString();
    }

    public static string I(string innerHTML)
    {
      var i = new HtmlElement("span") { text = innerHTML };
      i.text = innerHTML;
      i.Attributes.Add("style", "font-style:italic;");
      return i.ToString();
    }

    public static String Heading(Html.HeadingSize h, String innerHTML)
    {
      return String.Format(@"<h{0}>{1}</h{0}>", (int)h, innerHTML);
    }

    public static String U(String innerHTML)
    {
      var el = new HtmlElement("span") { text = innerHTML };
      el.Attributes.Add("style", "text-decoration:underline;");
      return el.ToString();
    }

    public static String Pre(String innerHTML)
    {
      return new HtmlElement("pre") { text = innerHTML }.ToString();
    }

    public static string AddLinkToHiddenContent(string rawContent, string customName = ShowMore)
    {
      string divId = Guid.NewGuid().ToString().Substring(0, 8);
      var el = new HtmlElement("a") { text = customName };
      el.Attributes.Add("href", "#");
      el.Attributes.Add("class", "ShowMore");
      el.Attributes.Add("title", "Click here to show/hide additional content");

      var div = new HtmlElement("div") { text = rawContent };
      div.Attributes.Add("class", "dontShow");

      return el.ToString() + div.ToString();
    }


    public static string Notice(string innerHTML)
    {
      var props = new Dictionary<string, string> { { "class", "notice" } };
      return new HtmlElement("span") { Attributes = props, text = innerHTML }.ToString();
    }

    public static string Warning(string innerHTML)
    {
      var props = new Dictionary<string, string> { { "class", "warning" } };
      return new HtmlElement("span") { Attributes = props, text = innerHTML }.ToString();
    }

    public static string Error(string innerHTML)
    {
      var props = new Dictionary<string, string> { { "class", "error" } };
      return new HtmlElement("span") { Attributes = props, text = innerHTML }.ToString();
    }

    public static string Semicolon2br(string content)
    {
      return content.Replace(";", "<br />\n");
    }

    public static string BoolToYesNo(bool value)
    {
      return value ? Yes : No;
    }

    public static string StringToYesNo(string value)
    {
      if (value == "1")
        return BoolToYesNo(true);
      else if (value == "0")
        return BoolToYesNo(false);
      else
        return Html.Error("Unknown");
    }

    public static string Yes = "Yes";
    public static string No = "No";
    //colored in green Yes and red No
    public static string cYes = Html.Notice(Yes);
    public static string cNo = Html.Error(No);

    public static string Radio(bool isChecked, int leadingSpaces = 4, string value = "")
    {
      var el = new HtmlElement("input");
      el.Attributes.Add("type", "radio");
      el.Attributes.Add("checked", isChecked ? "checked" : "");
      el.Attributes.Add("value", value);
      return IndentWithSpaces(leadingSpaces) + el.ToString();
    }

    public static string CheckBox(bool isChecked, bool isDisabled = false, int leadingSpaces = 4, string value = "")
    {
      var el = new HtmlElement("input");
      el.Attributes.Add("type", "checkbox");
      el.Attributes.Add("checked", isChecked ? "checked" : "");
      el.Attributes.Add("disabled", isDisabled ? " disabled " : "");
      el.Attributes.Add("value", value);
      return IndentWithSpaces(leadingSpaces) + el.ToString();
    }

    public static string IndentWithSpaces(int n = 4)
    {
      StringBuilder output = new StringBuilder();
      for (int i = 0; i < n; i++)
        output.Append("&nbsp;");
      return output.ToString();
    }


    /// <summary>
    /// < becomes &lt;
    /// > becomes &gt;
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public static string UrlEncode(string input)
    {
      return input.Replace("<", "&lt;").Replace(">", "&gt;");
    }

    // TODO check if this is useless
    public static string ErrorMsg(int errorNumber = 0)
    {
      string[] errors = new String[] { "Unable to detect!", "Unknown error!", "Could not find settings for user: " + Html.B(Environment.UserName) };
      return Html.Error(errors[errorNumber]);
    }

    public const string ShowMore = "Show more";
    public const string ShowLess = "Show less";

    public static String Div(string rawContent, string divId = "", string className = "")
    {
      return String.Format("<div id=\"{0}\" class=\"{2}\">{1}</div>", divId, rawContent, className);
    }

    public enum HeadingSize
    {
      H1 = 1, H2 = 2, H3 = 3
    }
  }
}
