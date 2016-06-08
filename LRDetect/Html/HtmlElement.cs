using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class HtmlElement
  {
    public string text;
    public string tagName;
    public Dictionary<string, string> Attributes = new Dictionary<string, string>();
    bool hasClosingTag = true; //img, input and some others do not need a closing tag

    public HtmlElement(string tagName)
    {
      this.tagName = tagName;
      if (tagName == "input" || tagName == "img")
        hasClosingTag = false;
    }
    
    public override string ToString()
    {
      return OpenTag() + text + CloseTag();
    }

    string OpenTag()
    {
      var closeOpeningTag = hasClosingTag ? ">" : "";
      return String.Format("<{0}{1}{2}", tagName, GetAttributes(), closeOpeningTag);
    }

    string CloseTag()
    {
      return hasClosingTag ? "</" + tagName + ">" : " />";
    }

    string GetAttributes()
    {
      StringBuilder output = new StringBuilder();
      foreach (var a in Attributes)
      {
        output.Append(" ");
        if (a.Key == "checked" || a.Key == "disabled")
          output.Append(a.Value);
        else
          output.Append(String.Format("{0}=\"{1}\"", a.Key, a.Value));
      }
      return output.ToString();
    }
  }
}
