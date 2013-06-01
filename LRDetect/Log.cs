using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// File
using System.IO;
// Messagebox
using System.Windows.Forms;
// Fileinfo
using System.Diagnostics;
// assembly
using System.Reflection;

namespace LRDetect
{
    class Log
    {
        // 0 : disabled, 1 : Erros only, 2 : Warnings, 3 : All
        private static int level = 3;
        private static string logName = System.Environment.GetEnvironmentVariable("TEMP") + @"\LR.Detect.log";


        public Log()
        {
            try
            {
                //MainForm.args[0] = "loglevel0";
                //check if the log level is changed from CMD argument
                foreach (string arg in MainForm.args)
                {
                    if (arg.Contains("loglevel:"))
                    {
                        int loglevel = Int32.Parse(arg.Substring(10));
                        Log.level = loglevel;
                        //Log.Info("Log level changed to " + Helper.logLevel + " as per CMD arguments");
                    }
                }
                // if Logging is enabled delete the old log file and create a new one
                if (Log.level > 0)
                {
                    if (File.Exists(logName))
                        File.Delete(logName);
                    FileStream fs = File.Create(logName);
                    fs.Close();
                }

                Assembly assembly = Assembly.GetExecutingAssembly();
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
                 string version = fvi.FileVersion;

                Log.Raw("LR Detect Tool " + version + "\r\nby Hristo Genev");
                string cmdArgs = "none";
                if (MainForm.args.Length > 1)
                    cmdArgs = String.Join(" ", MainForm.args);
                Log.Raw("Command line args: " + cmdArgs);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        #region Log Raw messages
        /// <summary>
        /// Method to add a raw message to the log without a timestamp
        /// </summary>
        /// <param name="message"></param>
        public static void Raw(string message)
        {
            if (Log.level == 3)
                Log.WriteMessage(message);
        }
        #endregion

        #region Log Info messages
        /// <summary>
        /// Method to add an INFO message to the log
        /// check if log level allows logging messages of type INFO
        /// </summary>
        /// <param name="message"></param>
        public static void Info(string message)
        {
            if (Log.level == 3)
                Log.WriteMessage(message, 0);
        }
        #endregion

        #region Log Warning messages
        /// <summary>
        /// Method to add a Warning message to the log
        /// check if log level allows logging messages of type WARN if level >= 2
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(string message)
        {

            if (Log.level >= 2)
                Log.WriteMessage(message, 1);
        }
        #endregion

        #region Log Errors
        /// <summary>
        /// Method to add an Error message to the log
        /// messages of type ERROR are logged ALWAYS unless level == 0
        /// </summary>
        /// <param name="message"></param>
        public static void Error(string message)
        {
            if (Log.level > 0)
                Log.WriteMessage(message, 2);
        }
        #endregion

        #region Write log message
        private static void WriteMessage(string message, int type = -1)
        {
            try
            {
                //check if logging is enabled
                if (Log.level > 0)
                {
                    string[] logType = new string[] { "INFO", "WARN", "ERROR" };

                    using (StreamWriter w = File.AppendText(Log.logName))
                    {
                        if (type >= 0)
                            w.Write("{0:MM/dd/yyy hh:mm:ss.fff} - {1}: {2}\r\n", DateTime.Now, logType[type], message);
                        // if type == -1 we are logging a RAW message without a time stamp
                        else
                            w.WriteLine("{0}", message);
                        // Update the underlying file.
                        w.Flush();
                        // Close the writer and underlying file.
                        w.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion
    }
}
