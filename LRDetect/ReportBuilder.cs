using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace LRDetect
{
  class ReportBuilder
  {
    Assembly assembly = Assembly.GetExecutingAssembly();
    public static string reportFileName = "LRDetect_Report.htm";
    // property to provide access to outputFileName field
    public static string ReportFileName
    {
      get
      {
        return (FormArguments.fileName == "") ? reportFileName : FormArguments.fileName;
      }
      set { reportFileName = value; }
    }

    List<Collector> collectorsData;
    IEnumerable<Type> collectors = Assembly.GetAssembly(typeof(Collector)).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Collector)));
    HtmlReport htmlReport = new HtmlReport();

    #region Collect information and generate HTML content
    /// <summary>
    ///  Collects the necessary information and returns HTML content string
    /// </summary>
    public void GenerateHtmlContent()
    {
      Logger.Debug("Started " + MethodBase.GetCurrentMethod());
      try
      {
        // Start the timer
        Stopwatch stopWatch = Stopwatch.StartNew();
        // Collect the information
        RunAllCollectors();
        htmlReport.AddRawContent(SortCollectorsData());

        string LRDetectVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
        //stop the timer
        TimeSpan ts = stopWatch.Elapsed;
        stopWatch.Stop();

        var time = String.Format("{0}Report generation time: {1}{2} LR Detect version: {3}", Html.br + Html.br, ts.ToString(), Html.br, LRDetectVersion);
        htmlReport.AddRawContent(time);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        MessageBox.Show("Critical error.\n" + ex.ToString() + "\n See log file for details.\r\nReport not completed!!!");
      }
      finally
      {
        Logger.Debug("Finished " + MethodBase.GetCurrentMethod());
      }
    }
    #endregion
    
    #region Sort Data based on Collector order number
    string SortCollectorsData()
    {
      StringBuilder output = new StringBuilder();
      // Sort the dictionary by key which is the order number
      var sortedDict = from entry in collectorsData orderby entry.Order ascending select entry;
      foreach (var entry in sortedDict)
        output.Append(entry.ToString());

      return output.ToString();
    }
    #endregion

    #region Run collectors
    void RunAllCollectors()
    {
      Logger.Debug("Started " + MethodBase.GetCurrentMethod());
      collectorsData = new List<Collector>();

      // If Async is selected we will run the collectors concurrently
      // Start the collectors in separate threads
      // .NET3.5 simulation of ForEach.Parallel
      if (FormArguments.async)
        Helper.EachParallel(collectors, collector => { RunCollector(collector); });
      else
        foreach (var collector in collectors)
          RunCollector(collector);

      Logger.Debug("Finished " + MethodBase.GetCurrentMethod());
    }

    internal void RunCollector(Type collector)
    {
      // Initialize the collector
      //object collector = Activator.CreateInstance(collector);
      Collector c = (Collector)Activator.CreateInstance(collector);
      // Add the information to dictionary
      collectorsData.Add(c);
    }

    #endregion

    #region Save HTML file
    public void SaveHtmlFile()
    {
      Logger.Debug("Started " + MethodBase.GetCurrentMethod());
      try
      {
        File.WriteAllText(ReportFileName, htmlReport.Render());
      }
      catch (UnauthorizedAccessException e)
      {
        //if we encounter UnauthorizedAccessException we'll write the file into  user %TEMP% directory
        // set the new path to the output file name
        ReportFileName = Path.Combine(Path.GetTempPath(), ReportFileName);
        File.WriteAllText(ReportFileName, htmlReport.Render());
        Logger.Error(String.Format("Unable to write in program directory. File saved in {0} \n {1}"
          ,Path.GetTempPath(), e.ToString()));
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
      finally
      {
        // clear the content of the body
        htmlReport.Clear();
        Logger.Info("Output file name: " + ReportFileName);
        Logger.Debug("Finished " + MethodBase.GetCurrentMethod());
      }
    }
    #endregion
  }
}
