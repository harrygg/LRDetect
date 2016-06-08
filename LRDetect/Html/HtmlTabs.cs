using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  public class HtmlTabs
  {
    public static string CreateTabs(Tab[] tabs)
    {
      StringBuilder output = new StringBuilder();

      /*foreach (var tab in tabs)
      {
        Html.A tabLink = new Html.A() { text = tab.title, aClass = "tabLinks" };
        string tabContent = Html.Div(tab.content);

        output.Append(tabLink.ToString());
      
      }*/
      //string div = "";
      
      //output.Append(Html.Div(div));

      return output.ToString();
    }


    public class Tab
    {
      public string title;
      public string content;
      public Tab(string title, string content)
      { 
        
      }
    }
  }
}
