using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  public class ClientsCollectorHelper
  {
    static DetectJava detectJava;
    static ClientsCollectorHelper()
    {
      try
      {
        detectJava = new DetectJava();
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
    }

    internal static string GetJavaProducts(string type)
    {
      try
      {
        StringBuilder sb = new StringBuilder();
        var products = (from p in detectJava.installedJavaProducts where p.type == type select p).ToList<JavaProduct>();
        foreach (var product in products)
          sb.Append(product.ToString() + Html.br);
        return sb.ToString();
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }

    internal static string GetVugenJREVersion()
    {
      return detectJava.GetJavaVersionFromCmd(ProductDetection.Vugen.JreFolder);
    }

    internal static string GetJavaDetails()
    {
      return detectJava.javaDetailsFromCMD.ToString();
    }
  }
}
