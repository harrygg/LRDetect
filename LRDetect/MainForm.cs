using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
//using Microsoft.Win32;
//Stopwatch
using System.Diagnostics;
namespace LRDetect
{
    public partial class MainForm : Form
    {
        #region Members and properties
        private static string outputFileName = "LR_Detect_Report.html";
        public static string[] args;
        public static bool menuNetorkInfo = false;
        public static bool menuDllsInfo = false;
        public static bool menuSystemProcesses = true;

        // property for setting the richness of the output document
        // 1, 2 or 3. Adjustable from CMD argument outputlevel:N
        public static int reportDetailsLevel = 2;
        // property to provide access to outputFileName field
        public static string OutputFileName
        {
            get { return outputFileName; }
            set { outputFileName = value;}
        }
        private Html htmlBuffer = new Html();

        #endregion

        #region MainForm contructor
        public MainForm()
        {
            try
            {
                InitializeComponent();

                args = Environment.GetCommandLineArgs();
                bool help = MainForm.args.Any(s => s.Contains("help")) || MainForm.args.Any(h => h.Contains("?"));
                if (help)
                {
                    MessageBox.Show("Command Line Arguments to start LRDetect.exe:"
                        + "\n\n-help or /?    - Both display the help screen."
                        + "\n-details:N     - Sets the report level of details (N could be 1, 2 or 3)"
                        + "\n-file:NAME   - Sets a new name for report file (-file:\"C:\\my example.html\")"
                        + "\n-dlls:dll1,dll2, etc - Diplays the version of dlls in LR bin folder. Use dlls:all or comma separated list of dlls (time consuming)"
                        + "\n-ipconfig      - Executes 'ipconfig /all' and adds it to the report"
                        + "\n-loglevel:N   - Sets the log level (N could be 1, 2 or 3)"
                        + "\n-appendlog  - Appends the log to the report html."
                        + "\n\nUsage:"
                        + "\nLRDetect.exe -details:3 -file:C:\\ouput.html -appendlog -loglevel:3"
                        , "LR Detect Help");
                }
                // Initialize the log 
                //Helper.LogInit(true);
                Log log = new Log();
                //set the output level of details from CMD arg
                foreach (string arg in MainForm.args)
                {
                    if (arg.Contains("details:"))
                    {
                        reportDetailsLevel = Int32.Parse(arg.Substring(9));
                        Log.Info("Level of details changed to " + reportDetailsLevel + " as per CMD argument");
                    }
                    if (arg.Contains("file:"))
                    {
                        outputFileName = arg.Substring(6);
                        Log.Info("Report file name changed to " + outputFileName + " as per CMD argument");
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region Click on Collect Info button
        private void btn_CollectInfo_Click(object sender, EventArgs e)
        {

            //start the timer
            Stopwatch stopWatch = Stopwatch.StartNew();
            btn_CollectInfo.Enabled = false;

            try
            {
                // reset the progress bar
                progressBar1.Value = 0;

                // collect the necessarry information
                // and save it in a buffer
                Helper.GenerateHtmlContent(htmlBuffer, UpdateProgressLabel, UpdateProgressBar);
                btn_SendInfo.Enabled = true;
                // 9
                progressBar1.PerformStep();

                // save the collected information into an html file
                Log.Info("Saving HTML file");

                Helper.SaveHtmlFile(htmlBuffer.ToString(), UpdateProgressBar);
                Log.Info("HTML file saved successfully.");
                UpdateProgressLabel("HTML file saved successfully");
                // clear the content of the body
                htmlBuffer.Clear();
                btn_SendInfo.Enabled = true;
                progressBar1.PerformStep();

                // open the document in the default browser
                progressBar1.PerformStep();
                UpdateProgressLabel("Opening report file");
                Process.Start(OutputFileName);
                UpdateProgressLabel("All done!");
                // 10
            }
            catch (UnauthorizedAccessException uae)
            {
                Log.Error(uae.ToString());
                progressBar1.Value = 0;
                MessageBox.Show("You don't have enough privileges to run this tool! Are you running a Guest account?");
            }
            catch (Win32Exception ex) 
            {
                Log.Error(OutputFileName + " " + ex.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical error. See log file for details!");
                Log.Error(ex.ToString());
            }
            finally
            {
                btn_CollectInfo.Enabled = true;
                //stop the timer
                TimeSpan ts = stopWatch.Elapsed;
                stopWatch.Stop();
                Log.Raw("Program execution time: " + ts.ToString());
                //Debug.WriteLine("Program execution time: " + ts.ToString());
            }
        }
        #endregion

        #region Click on Send Info button
        private void btn_SendInfo_Click(object sender, EventArgs e)
        {
            try
            {
                Mapi mapi = new Mapi();
                mapi.SendMail(OutputFileName);
            }
            catch (Win32Exception w32e)
            {
                Log.Error(w32e.ToString());
                MessageBox.Show("There is no mail client. Please send the file manually.");
            }
            catch (InvalidOperationException iox)
            {
                Log.Error(iox.ToString());
                MessageBox.Show("Error during opening default client!\nPlease send the file\n" + Directory.GetCurrentDirectory() + @"\" + OutputFileName + "\nmanually.");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        private void UpdateProgressBar(int count, int max)
        {
            progressBar1.PerformStep();
        }

        public void UpdateProgressLabel(string text)
        {
           progressLabel.Text = text;
           Application.DoEvents();
        }

        private void DetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsm = (ToolStripMenuItem ) sender;
            if (tsm.Name.Contains("network"))
                MainForm.menuNetorkInfo = tsm.Checked ? true : false;
            if (tsm.Name.Contains("dlls"))
                MainForm.menuDllsInfo = tsm.Checked ? true : false;
            if (tsm.Name.Contains("system"))
                MainForm.menuSystemProcesses = tsm.Checked ? true : false;
        }

        /*private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 aboutModal = new Form2();
            aboutModal.Show();
        }*/
    }
}
