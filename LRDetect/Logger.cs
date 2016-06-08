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
    class Logger
    {
        // 0 : disabled, 1 : Erros only, 2 : Warnings, 3 : Info, 4 : Debug
      static int _level = 3;
      public static int level 
      {
        get { return _level; }
        set { _level = value; }
      }
        static string logName = Path.Combine(Path.GetTempPath(), "LR.Detect.log");
        static readonly TextWriter tw;
        public enum Level { RAW, DEBUG, INFO, WARN, ERROR } //TODO make bitwise checks
        static Logger()
        {
          try
          {
            // if Logging is enabled delete the old log file and create a new one
            if (Logger.level > 0)
            {
                if (File.Exists(logName))
                    File.Delete(logName);
                FileStream fs = File.Create(logName);
                fs.Close();
            }
            tw = TextWriter.Synchronized(File.AppendText(logName));
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
              string version = fvi.FileVersion;

            Logger.Raw("LR Detect Tool " + version + "\r\nby Hristo Genev");
            String cmdArgs = (FormArguments.cmdArgs != "") ? FormArguments.cmdArgs : "none";
            Logger.Raw("Command line args: " + cmdArgs);
          }
          catch (Exception ex)
          {
            MessageBox.Show(ex.ToString());
          }
        }


        #region Log Raw messages
        /// <summary>
        /// Method to add a raw message to the log without a timestamp
        /// </summary>
        /// <param name="message"></param>
        public static void Raw(String message)
        {
            if (Logger.level >= 3)
                Logger.WriteMessage(message, Level.RAW);
        }
        #endregion

        #region Log Debug messages
        /// <summary>
        /// Method to add an INFO message to the log
        /// check if log level allows logging messages of type INFO
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(String message)
        {
          if (Logger.level == 4)
            Logger.WriteMessage(message, Level.DEBUG);
        }
        #endregion

        #region Log Info messages
        /// <summary>
        /// Method to add an INFO message to the log
        /// check if log level allows logging messages of type INFO
        /// </summary>
        /// <param name="message"></param>
        public static void Info(String message)
        {
            if (Logger.level >= 3)
                Logger.WriteMessage(message, Level.INFO);
        }
        #endregion

        #region Log Warning messages
        /// <summary>
        /// Method to add a Warning message to the log
        /// check if log level allows logging messages of type WARN if level >= 2
        /// </summary>
        /// <param name="message"></param>
        public static void Warn(String message)
        {
            if (Logger.level >= 2)
                Logger.WriteMessage(message, Level.WARN);
        }
        #endregion

        #region Log Errors
        /// <summary>
        /// Method to add an Error message to the log
        /// messages of type ERROR are logged ALWAYS unless level == 0
        /// </summary>
        /// <param name="message"></param>
        public static void Error(String message)
        {
            if (Logger.level > 0)
                Logger.WriteMessage(message, Level.ERROR);
        }
        #endregion

        #region Write log message
        private static void WriteMessage(String message, Enum logLevel)
        {
            try
            {
                //check if logging is enabled
                if (Logger.level > 0)
                {
                  //String[] logType = new String[] {"RAW", "DEBUG", "INFO", "WARN", "ERROR" };
                  {
                    if (Convert.ToInt16(logLevel) > 0)
                      tw.Write("{0:MM/dd/yyy hh:mm:ss.fff} - {1}: {2}\r\n", DateTime.Now, logLevel, message);
                    // if type == -1 we are logging a RAW message without a time stamp
                    else
                      tw.WriteLine("{0}", message);
                    // Update the underlying file.
                    tw.Flush();
                  }
                }
            }
            catch (Exception)
            {
              tw.Close();
            }
        }
        #endregion
    }
}
