using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.Diagnostics;
using System.Collections.Generic;

namespace LRDetectNewTest
{
  [TestClass]
  public class CollectorsTest
  {
    [TestMethod]
    public void Titles_In_Generated_Report_From_OSCollector()
    {
      //ReportBuilder report = new ReportBuilder();
      //Html htmlBuffer = new Html();
      // Start the timer
      Stopwatch stopWatch = Stopwatch.StartNew();

      var collector = new OSCollector();
      collector.ToString();
      //htmlBuffer.AddRawContent(collector.ToString());

      //stop the timer
      TimeSpan ts = stopWatch.Elapsed;
      stopWatch.Stop();

      var titles = new List<string> { "Machine name", "Full name", "Locale", "decimal separator is", "Is OS Virtualized?", "Is 3GB switch enabled?", "Data Execution Prevention", "User Account Control", "Is user Admin?", "Is user connected remotely?", "Is Windows firewall enabled?", "Environment information", "System environment variables", "Kerberos configuration", "Layered Service Providers", "entries found", "AppInit_DLLs registry value", "LoadAppInit_DLLs registry value", "x64 entry", "x32 entry"};

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    


      //var time = String.Format("{0}Report generation time: {1}{2}", Html.br + Html.br, ts.ToString(), Html.br);
      //htmlBuffer.AddRawContent(time);

      //ReportBuilder.ReportFileName = this.GetType().Name + ".html";
      //report.SaveHtmlFile(htmlBuffer);
    }


    bool CheckForItems(string report, List<string> items)
    {
      foreach (var i in items)
      {
        if (!report.Contains(i))
          return false;
      }
      return true;
    }
  }
}
