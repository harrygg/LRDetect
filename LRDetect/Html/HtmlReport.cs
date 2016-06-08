using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LRDetect
{
  public class HtmlReport
  {
    StringBuilder body = new StringBuilder(32000);

    public void AddRawContent(string content)
    {
      body.Append(content);
    }

    /// <summary>
    /// Method to clear the content of the body
    /// in case we run the tool twice
    /// TODO use Clear() in .NET 4
    /// </summary>
    public void Clear()
    {
      body.Length = 0;
      body.Capacity = 0;
    }

    string LoadResourcesContent(string fileName)
    {
      Assembly assembly = Assembly.GetExecutingAssembly();
      using (Stream stream = assembly.GetManifestResourceStream(fileName))
      using (StreamReader reader = new StreamReader(stream))
      {
        return reader.ReadToEnd();
      }
    }

    public string Render()
    {
      var template = LoadResourcesContent("LRDetect.Resources.Template.html");

      object htmlContent = new
      {
        CSS = LoadResourcesContent("LRDetect.Resources.Style.css"),
        SCRIPT1 = "var ShowMore = \"" + Html.ShowMore + "\"; var ShowLess = \"" + Html.ShowLess + "\";",
        SCRIPT2 = LoadResourcesContent("LRDetect.Resources.jquery-1.11.1.min.js"),
        SCRIPT3 = LoadResourcesContent("LRDetect.Resources.JavaScriptContent.js"),
        BODY = body.ToString(),
        SUBTITLE = String.Format("Generated on {0} for {1}\\{2}", DateTime.Now.ToLocalTime(), Html.B(Environment.MachineName), Html.B(Environment.UserName))
      };

      var output = template.FormatWith(htmlContent); 
      return output;
    }
  }
}
