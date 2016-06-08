using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace LRDetect
{
  public partial class MainForm : Form
  {
    #region Members and properties
    ReportBuilder reportBuilder = new ReportBuilder();
    Stopwatch stopWatch;
    public Dictionary<string, string> statuses = new Dictionary<string, string>();
    #endregion

    #region MainForm contructor
    public MainForm()
    {
      try
      {
        InitializeComponent();
        Collector.RaiseProgressUpdate += report_OnProgressUpdate;
        Collector.CollectionStarted += report_OnCollectionStarted;
        Collector.CollectionEnded += report_OnCollectionEnded;
        if (FormArguments.help)
          ShowHelpMenu();
        if (FormArguments.startCollection)
          btn_CollectInfo_Click(this, null);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
    }

    #endregion

    #region ShowHelpMenu
    private void ShowHelpMenu()
    {
      MessageBox.Show("Command Line Arguments to start LRDetect.exe:"
                    + "\n\n-help or /?      - Both display the help screen."
                    + "\n-details N       - Sets the report level of details (N could be 1, 2 - the default or 3 - selects all itesm from the Include menu)"
                    + "\n-collect           - Start collection immediately after launching the GUI"
                    + "\n-hideReport   - Don't open the report in browser when done"
                    + "\n-file NAME     - Sets a new name for report file (-file:\"C:\\my example.html\")"
                    + "\n-dlls dll1,dll2, etc   - Diplays the version of dlls in LR bin folder. Use dlls:all or comma separated list of dlls (time consuming)"
                    + "\n-ipconfig        - Executes 'ipconfig /all' and adds it to the report"
                    + "\n-logLevel N     - Sets the log level (N could be 1, 2, 3 or 4)"
                    + "\n-appendlogs  - Creates a ZIP archive with the report file and all log files"
                    + "\n\nUsage:"
                    + "\nThe following command makes the tool start collecting data automatically, save the report into C:\\output.html without opening the browser."
                    + "\nLRDetect.exe -collect -file:C:\\output.html -hideReport"
                    , "LR Detect Help");
    }
    #endregion

    #region Click on Collect Info button
    void btn_CollectInfo_Click(object sender, EventArgs e)
    {
      Logger.Debug("btn_CollectInfo_Click executed");
      //start the timer
      stopWatch = Stopwatch.StartNew();
      progressBar1.Value = 0;
      btn_CollectInfo.Enabled = false;
      if(btn_SendInfo.Enabled)
        btn_SendInfo.Enabled = false;

      //do the data collection in the background
      bgWorker.RunWorkerAsync();
    
    }
    #endregion

    #region Click on Send Info button
    void btn_SendInfo_Click(object sender, EventArgs e)
    {
      try
      {
          Mapi mapi = new Mapi();
          mapi.SendMail(ReportBuilder.ReportFileName);
      }
      catch (Win32Exception w32e)
      {
          Logger.Error(w32e.ToString());
          MessageBox.Show("There is no e-mail client. Please send the file manually.");
      }
      catch (InvalidOperationException iox)
      {
          Logger.Error(iox.ToString());
          MessageBox.Show(String.Format(
            "Error during opening default client!\nPlease send the file\n {0} {1} \nmanually."
            , Path.Combine(Directory.GetCurrentDirectory(), ReportBuilder.ReportFileName)));
      }
      catch (Exception ex)
      {
          Logger.Error(ex.ToString());
          MessageBox.Show(ex.ToString());
      }
    }
    #endregion

    #region Method to update the Progress Label
    public void UpdateProgressLabel(string text)
    {
        progressStatus.Text = text;
    }
    #endregion

    #region Handle the Menu click
    void DetailsToolStripMenuItem_Click(object sender, EventArgs e)
    {
      FormArguments.SetMenuItems((ToolStripMenuItem)sender);
    }
    #endregion

    #region Handle the ProgressChanged Event
    void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
    {
      progressBar1.Value = e.ProgressPercentage;
      UpdateProgressLabel(e.UserState as String);
    }
    #endregion

    #region Handle the DoWork Event
    void bgWorker_DoWork(object sender, DoWorkEventArgs e)
    {
      bgWorker = sender as BackgroundWorker;

      // collect the necessarry information and save it in a buffer
      bgWorker.ReportProgress(1, "Starting detection...");
      //report.AddActiveCollectors();
      reportBuilder.GenerateHtmlContent();

      bgWorker.ReportProgress(90, "Saving HTML file");
      // save the collected information into an html file
      Logger.Info("Saving HTML file");
      reportBuilder.SaveHtmlFile();
      Logger.Info("HTML file saved successfully.");
      
      bgWorker.ReportProgress(92, "HTML file saved successfully");
      if (FormArguments.appendLogs)
      {
        Logger.Debug("ZIP file creation started");
        var zipper = new LRDetectZipper();
        zipper.filesToBeZipped.Add(new LogFile { name = ReportBuilder.ReportFileName, folder = Directory.GetCurrentDirectory() });
        bgWorker.ReportProgress(94, "Creating zip archive");

        if (zipper.ZipFiles())
        {
          Process.Start("explorer.exe", "/select," + zipper.ZipFileName);
        }
        else
        {
          if (!FormArguments.hideReport)
            Process.Start(ReportBuilder.ReportFileName);
        }
      }
      else
      {
        bgWorker.ReportProgress(99, "Opening report file");
        if (!FormArguments.hideReport)
          Process.Start(ReportBuilder.ReportFileName);
      }
      bgWorker.ReportProgress(100, "All done!");
    }
    #endregion

    #region Handle the RunWorkerCompleted Event
    void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
      // Check to see if an error occurred in the background process.
      if (e.Error != null)
      {
        if (e.Error.InnerException is UnauthorizedAccessException)
        {
          Logger.Error(e.Error.ToString());
          MessageBox.Show("You don't have enough privileges to run this tool! Are you running a Guest account?");
        }
        else if (e.Error.InnerException is Win32Exception)
        {
          Logger.Error(ReportBuilder.ReportFileName + "\n" + e.Error.ToString());
          MessageBox.Show(e.Error.ToString());
        }
        else
        {
          Logger.Error(e.Error.ToString());
          MessageBox.Show(e.Error.ToString());
        }
        UpdateProgressLabel("Critical error. See log file for details!");
      }

      //stop the timer
      TimeSpan ts = stopWatch.Elapsed;
      stopWatch.Stop();
      Logger.Raw("Program execution time: " + ts.ToString());


      // Close the form if we called it from CMD
      if (FormArguments.startCollection)
        this.Close();

      btn_SendInfo.Enabled = true;
      btn_CollectInfo.Enabled = true;
    }
    #endregion

    #region Hendle the OnProgressUpdate Event comming from the Collector class
    void report_OnProgressUpdate()
    {
      // Its another thread so invoke back to UI thread
      base.Invoke((Action)delegate
      {
        UpdateProgressBar();
        //UpdateProgressLabel(e.status);
      });
    }
    #endregion

    #region Update Progress bar
    int pBarMaxValue = 90;
    int percentStep = 1;
    void UpdateProgressBar()
    {
      if (progressBar1.Value < pBarMaxValue - percentStep)
        progressBar1.Value += percentStep;
    } 
    #endregion

    #region Update Status Message
    public void UpdateStatus()
    {
      if (statuses.Count > 0)
        progressStatus.Text = statuses.Last().Value.ToString();
      else
        progressStatus.Text = "";
    }
    #endregion

    #region Event OnCollectionStarted
    void report_OnCollectionStarted(object sender, CollectorStatusEventArgs e)
    {
      lock (statuses)
      {
        statuses.Add(e.name, e.status);
        base.Invoke((Action)delegate
        {
          UpdateStatus();
        });
      }
    }
    #endregion

    #region Event report_OnCollectionEnded
    void report_OnCollectionEnded(object sender, CollectorStatusEventArgs e)
    {
      statuses.Remove(e.name);
      base.Invoke((Action)delegate
      {
        UpdateStatus();
      });
    }
    #endregion
  }

  #region CollectorStatusEventArgs
  public class CollectorStatusEventArgs : EventArgs
  {
    public CollectorStatusEventArgs(string collectorName, string statusMsg)
    {
      status = statusMsg;
      name = collectorName;
    }
    public string name;
    public string status;
  }
  #endregion

}
