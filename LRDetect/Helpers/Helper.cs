using System;
using System.Collections.Generic;
// Hashtable
using System.Collections;
using System.Linq;
using System.Text;
// File
using System.IO;
// mapi
using System.Runtime.InteropServices;
// process
using System.Diagnostics;
// registries
using Microsoft.Win32;
//wmi
using System.Management;
using System.Windows.Forms;
// assembly
using System.Reflection;
using System.Threading;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Web.UI;


namespace LRDetect
{
    public static class Helper
    {
      #region Exucute CMD command
      public static string ExecuteCMDCommand(string command)
      {
          string output = String.Empty;
          try
          {
              // create the ProcessStartInfo using "cmd" as the program to be run,
              // and "/c " as the parameters. /c tells cmd that we want it to execute the command that follows, and then exit.
              ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);
              // The following commands are needed to redirect the standard output.
              // This means that it will be redirected to the Process.StandardOutput StreamReader.
              procStartInfo.RedirectStandardOutput = true;
              procStartInfo.RedirectStandardError = true;
              procStartInfo.UseShellExecute = false;
              // Do not create the black window.
              procStartInfo.CreateNoWindow = true;
              // Now we create a process, assign its ProcessStartInfo and start it
              Process proc = new Process();
              proc.StartInfo = procStartInfo;
              proc.Start();
              // Get the output into a string
              output = proc.StandardOutput.ReadToEnd();
              // Next lines are needed because when 'java -version' command is executed
              // the output is returned in StandardError. I don't know why. If someone can explain!!!
              if (output == "" || output == null)
                  output = proc.StandardError.ReadToEnd();
              return output;
          }
          catch (Exception ex)
          {
              Logger.Info(ex.ToString());
              return null;
          }
      }
      #endregion

      #region Get the name of the process owner
      public static string GetProcessOwner(int processId)
      {
          string query = "Select * From Win32_Process Where ProcessID = " + processId;
          ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
          ManagementObjectCollection processList = searcher.Get();

          foreach (ManagementObject obj in processList)
          {
              string[] argList = new string[] { string.Empty, string.Empty };
              int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
              if (returnVal == 0)
              {
                  // return DOMAIN\user
                  return argList[1] + "\\" + argList[0];
              }
          }

          return "NO OWNER";
      }
      #endregion

      #region Convert install date to local time
      /// <summary>
      /// Method to covert the install date for the software to readable format in local time
      /// </summary>
      /// <returns></returns>
      public static string ConvertInstallDate(string date)
      {
        try
        {
          if (date == null || date == "")
            return "";
          DateTime dt = new DateTime(Convert.ToInt32(date.Substring(0, 4)), Convert.ToInt32(date.Substring(4, 2)), Convert.ToInt32(date.Substring(6, 2)));
          return " installed on " + dt.ToLocalTime().ToShortDateString().ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return "";
        }
      }
      #endregion

      #region Query with Windows Management Instrumentation
      /// <summary>
      /// Query the WMI
      /// </summary>
      /// <param name="propertyName">the name of the object property we want to get</param>
      /// <param name="scope">i.e. root\CIMV2</param>
      /// <param name="wmiClass">i.e. FirewallProduct or AntiVirusProduct</param>
      /// <param name="where">where clause, empty by default</param>
      /// <returns></returns>
      public static string QueryWMI(string propertyName, string scope, string wmiClass, string queryAppendix = "")
      {
        Logger.Debug("Executing Helper.QueryWMI()");
        string query = String.Format("SELECT * FROM {0} {1}", wmiClass, queryAppendix);

        try
        {
          ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
          Logger.Debug("ManagementObjectSearcher object created. Using query: " + query);

          List<String> results = new List<string>();

          foreach (ManagementObject queryObj in searcher.Get())
          {
            results.Add(queryObj[propertyName].ToString());
          }
          Logger.Debug("ManagementObjectSearcher object results : " + results.Count);

          string info = "Not detected";
          if (results.Count == 1)
            info = results[0].ToString();
          if (results.Count > 1)
            info = String.Join(", ", results.ToArray());
          return info;
        }
        catch (ManagementException ex)
        {
          Logger.Error("An error occurred while querying for WMI data\r\nQuery: " + query + "\r\n" + ex.ToString());
          return Html.ErrorMsg();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
        finally
        {
          Logger.Debug("Ended Helper.QueryWMI()");
        }
      }

      /// <summary>
      /// Query the WMI and returns the first object that is found
      /// </summary>
      /// <param name="scope">Example root\\CIMV2</param>
      /// <param name="wmiClass">Example Win32_SystemDriver</param>
      /// <param name="queryAppendix">Example "WHERE DisplayName =='Genev'"</param>
      /// <returns></returns>
      public static ManagementObject GetWMIObject(string scope, string wmiClass, string queryAppendix = "")
      {
        Logger.Debug("Executing Helper.GetWMIObject()");
        string query = String.Format("SELECT * FROM {0} {1}", wmiClass, queryAppendix);

        try
        {
          ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
          Logger.Debug("ManagementObjectSearcher object created. Using query: " + query);

          List<ManagementObject> results = new List<ManagementObject>();

          foreach (ManagementObject queryObj in searcher.Get())
            results.Add(queryObj);
          Logger.Debug("ManagementObjectSearcher object results : " + results.Count);

          return results[0];
        }
        catch (ManagementException ex)
        {
          Logger.Error("An error occurred while querying for WMI data\r\nQuery: " + query + "\r\n" + ex.ToString());
          return null;
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return null;
        }
        finally
        {
          Logger.Debug("Ended Helper.GetWMIObject()");
        }
      }
      #endregion

      #region ParseVuGenLog

      //public static string ParseVuGenLog()
      //{
      //    string output = Helper.GetLastLinesFromFile(1024 * 1024 * 1024, System.IO.Path.GetTempPath(), 30);
      //    return output;
      //}
      /// <summary>
      /// 
      /// </summary>
      /// <param name="maxFileSize">Max file size in bytes</param>
      /// <param name="pathToFile"></param>
      /// <param name="numberOfLines"></param>
      /// <returns></returns>
      public static string GetLastLinesFromFile(int maxFileSize, string pathToFile, int numberOfLines)
      {
          try
          {
              FileInfo fi = new FileInfo(pathToFile);
              if (fi.Length > maxFileSize)
                  return String.Format("The file {0} is too big to be parsed ({1} kb). Check manually. Parsing cancelled!"
                    , pathToFile, fi.Length / 1024);

              String line = String.Empty;
              String errorFileContext = String.Empty;

              using (var fs = new FileStream(pathToFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
              {
                  byte[] fileBytes = new byte[maxFileSize];
                  int amountOfBytes = fs.Read(fileBytes, 0, maxFileSize);
                  ASCIIEncoding ascii = new ASCIIEncoding();
                  errorFileContext = ascii.GetString(fileBytes, 0, amountOfBytes);
              }

              string[] separators = new string[] { "\r\n" };
              string[] lines = errorFileContext.Split(separators, StringSplitOptions.RemoveEmptyEntries);
              StringBuilder sb = new StringBuilder();
              // put the cursor to line where we want to start the parsing = allLines - numberOfLinesWeWant. 
              // if the file is less line than numberOfLines, put the cursor on the first line
              int cursor = (numberOfLines > lines.Length) ? 0 : lines.Length - numberOfLines;

              // Extract tje last numberOfLines lines from the file
              // starting from the cursor position incrementing to the end of the file
              while (cursor < lines.Length)
              {
                  if (lines[cursor] != "")
                  {
                      sb.Append(Html.UrlEncode(lines[cursor]) + Html.br);
                  }
                  cursor++;
              }
              return sb.ToString();
          }
          catch (FileNotFoundException)
          {
              Logger.Error("File not found: " + pathToFile);
              return "File not found: " + pathToFile;
          }
          catch (IOException)
          {
              Logger.Error("Can not parse " + pathToFile + ", it is locked by another process.");
              return "Can not parse " + pathToFile + ", it is locked by another process.";
          }
          catch (Exception ex)
          {
              Logger.Error(ex.ToString());
              return "The file was not parsed due to an error: " + ex.ToString();
          }
        
      }
      #endregion

      #region GetRegistraionFailures
      public static string GetRegistraionFailuresContent()
      {
          try
          {
            var regFailureFiles = ProductInfo.RegistrationFailureLogs;
              //if no files are found get out of here
            if (regFailureFiles.Count == 0)
              return "None, no RegistrationFailure log files found";

            StringBuilder output = new StringBuilder();

            foreach (var file in regFailureFiles)
            {
              string lastLines = GetLastLinesFromFile(1024, file.fullPath, 5);
              output.Append(String.Format("Last 5 lines in: {0} {1} {2}"
                , Html.I(Path.GetFileName(file.fullPath))
                , Html.br, lastLines));
            }

            return output.Length > 0 ? output.ToString() : "Not found!";
          }
          catch (Exception ex)
          {
              Logger.Error(ex.ToString());
              return ex.Message;
          }
      }
      #endregion

      #region EachParallel - DotNet 3.5 version of Parallel.ForEach
      /// <summary>
      /// EachParallel - DotNet 3.5 version of Parallel.ForEach
      /// Enumerates through each item in a list in parallel
      /// </summary>
      public static void EachParallel<T>(IEnumerable<T> list, Action<T> action)
      {
        try
        {
          // enumerate the list so it can't change during execution
          list = list.ToArray();
          var count = list.Count();

          if (count == 0)
          {
            return;
          }
          else if (count == 1)
          {
            // if there's only one element, just execute it
            action(list.First());
          }
          else
          {
            // Launch each method in it's own thread
            const int MaxHandles = 64;
            for (var offset = 0; offset <= count / MaxHandles; offset++)
            {
              // break up the list into 64-item chunks because of a limitiation in WaitHandle
              var chunk = list.Skip(offset * MaxHandles).Take(MaxHandles);

              // Initialize the reset event
              var resetEvent = new ManualResetEvent(false);

              // Queue action in thread pool for each item in the list
              long counter = count;
              // spawn a thread for each item in the chunk
              int i = 0;
              foreach (var item in chunk)
              {
                ThreadPool.QueueUserWorkItem(new WaitCallback((object data) =>
                {
                  int methodIndex = (int)((object[])data)[0];

                  // Execute the method and pass in the enumerated item
                  action((T)((object[])data)[1]);

                  // Decrements counter atomically
                  Interlocked.Decrement(ref counter);

                  // If we're at 0, then last action was executed
                  if (Interlocked.Read(ref counter) == 0)
                  {
                    // Tell the calling thread that we're done
                    resetEvent.Set();
                  }
                }), new object[] { i, item });
                i++;
              }

              // Wait for the single WaitHandle
              // which is only set when the last action executed
              resetEvent.WaitOne();
            }
          }
        }
        catch (Exception ex)
        {
          MessageBox.Show(ex.ToString());
        }
      }
    #endregion
      
      /// <summary>
      /// Method to return Service name and status
      /// </summary>
      /// <param name="AgentCaption"></param>
      /// <returns></returns>
      public static string FormatServiceNameStatus(string agentCaption, string status = null)
      {
        if (status == null || status != "Running")
          status = GetServiceStatus(agentCaption);
        if (status == "Running")
          status = Html.Notice(status);

        return agentCaption + " status: " + status;
      }

      public static string GetServiceStatus(string AgentCaption)
      {
        try
        {
          // If the service is not installed an Exception will be raised when trying to get its status
          ServiceController sc = new ServiceController(AgentCaption);
          return sc.Status.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.Message;
        }
      }

      #region Method to get a list of all opened ports for a given process id
      public static string GetOpenedPortsForProcessesString(string[] processNames)
      {
        StringBuilder output = new StringBuilder();
        try
        {
          foreach (var processName in processNames)
            output.Append(GetOpenedPortsForProcessString(processName));
          return output.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.Message;
        }
      }

      public static string GetOpenedPortsForProcessString(string processName)
      {
        StringBuilder output = new StringBuilder();
        try
        {
            Process[] processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
            if (processes.Length > 0)
            {
              foreach (Process p in processes)
              {
                var path = "";
                try { path = "Path " + Html.I(p.Modules[0].FileName); } catch (Exception) { }
                var processOwner = "";
                try { processOwner = Helper.GetProcessOwner(p.Id); } catch (Exception) { }
                output.Append(Html.U(Html.br + "PID: " + p.Id + " " + processName + " " + processOwner) + path + Helper.GetOpenedPortsForProcessId(p.Id));
              }
            }
          
          return output.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.Message;
        }
      }


      #endregion 


      #region Get all opened ports for given process id
      /// <summary>
      /// Method to execute netstat command to list the ports opened for given process id.
      /// 1. Execute the netstat -ano | findstr /e process_id (matches all lines ending with process_id
      /// 2. Create an array by splitting the output by \r, \n, \t, ' ', ':' 
      /// 3. Loop through the array values and if valuee==process_id get the necessary information
      /// </summary>
      /// <param name="processId"></param>
      /// <returns></returns>
      public static string GetOpenedPortsForProcessId(int processId)
      {
        try
        {
          StringBuilder portInfo = new StringBuilder(128);
          if (processId > 0)
          {
            string netstatOutput = Helper.ExecuteCMDCommand("netstat -anop tcp | findstr /e " + processId);

            if (netstatOutput.Contains(processId.ToString()))
            {
              Logger.Info("Ports for process ID " + processId + ":\r\n" + netstatOutput);
              // split the output by \r\n
              char[] delimiter = new Char[] { ' ', '\t', '\r', ':' };
              string[] parts = netstatOutput.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

              for (int i = 0; i < parts.Length; i++)
              {
                if (parts[i] == processId.ToString())
                {
                  string status = String.Empty;
                  switch (parts[i - 1])
                  {
                    case "LISTENING": //english
                    case "ABH™REN":   //german
                      status = Html.Notice("LISTENING");
                      break;
                    case "ESTABLISHED": //english
                    case "HERGESTELLT": //german
                      status = Html.B("ESTABLISHED");
                      break;
                    default:
                      status = parts[i - 1];
                      break;
                  }
                  portInfo.Append(Html.br + "Port: " + parts[i - 4] + " " + status);
                }
              }
            }
          }
          return portInfo.ToString() + Html.br;
        }
        catch (Exception ex)
        {
          Logger.Warn(ex.ToString());
          return null;
        }
      }
      #endregion

      #region Get installation file info
      /// <summary>
      /// Method to get a file info if a product is installed
      /// Exceptions will be captured on the next level
      /// </summary>
      /// <param name="fileName"></param>
      /// <returns></returns>
      public static Hashtable GetFileInfo(string fileName)
      {
        var fileInfo = new Hashtable();
        try
        {
          FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(fileName);
          FileInfo fi = new FileInfo(fileName);

          fileInfo.Add("Name", fi.Name);
          fileInfo.Add("Version", fvi.FileVersion == null ? "Unknown" : fvi.FileVersion);
          fileInfo.Add("ModifiedOn", fi.LastWriteTime.ToLocalTime());
          fileInfo.Add("Size", fi.Length);
          return fileInfo;
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          fileInfo.Add("Name", ex.Message);
          fileInfo.Add("Version", Html.ErrorMsg());
          fileInfo.Add("ModifiedOn", Html.ErrorMsg());
          fileInfo.Add("Size", Html.ErrorMsg());
        }
        return fileInfo;
      }

      #endregion

      public static bool TextToBool(string value)
      {
        switch (value.ToLower())
        {
          case "true":
            return true;
          case "t":
            return true;
          case "1":
            return true;
          case "0":
            return false;
          case "false":
            return false;
          case "f":
            return false;
          default:
            throw new InvalidCastException("You can't cast a weird value to a bool!");
        }
      }

      public static string FormatWith(this string format, object source)
      {
        return FormatWith(format, null, source);
      }

      public static string FormatWith(this string format, IFormatProvider provider, object source)
      {
        if (format == null)
          throw new ArgumentNullException("format");

        Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
          RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        List<object> values = new List<object>();
        string rewrittenFormat = r.Replace(format, delegate(Match m)
        {
          Group startGroup = m.Groups["start"];
          Group propertyGroup = m.Groups["property"];
          Group formatGroup = m.Groups["format"];
          Group endGroup = m.Groups["end"];

          values.Add((propertyGroup.Value == "0")
            ? source
            : DataBinder.Eval(source, propertyGroup.Value));

          return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
            + new string('}', endGroup.Captures.Count);
        });

        return string.Format(provider, rewrittenFormat, values.ToArray());
      }



      #region Method to prevent WOW64 redirection

      [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      static extern bool Wow64DisableWow64FsRedirection(ref IntPtr oldValue);

      [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
      static extern bool Wow64RevertWow64FsRedirection(IntPtr oldValue);

      static IntPtr ptr = new IntPtr();
      static bool isWow64FsRedirectionDisabled = false;
      
      public static bool Wow64DisableWow64FsRedirection()
      {
        //Execute only on 64bit OS
        if (OSCollectorHelper.is64BitOperatingSystem)
        {
          try
          {
            //Disable redirection, so LRDetect (as 32bit process) is not redirected to C:\Windows\SystemWOW64 instead of C:\Windows\System32
            isWow64FsRedirectionDisabled = Wow64DisableWow64FsRedirection(ref ptr);
            Logger.Info("isWow64FsRedirectionDisabled: " + isWow64FsRedirectionDisabled);
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
          }
        }
        else // We are on 32bit machine so there is no redirection
          return true;
        return isWow64FsRedirectionDisabled;
      }

      public static bool Wow64RevertWow64FsRedirection()
      {
        bool result = false;
        //Execute only on 64bit OS
        if (OSCollectorHelper.is64BitOperatingSystem)
        {
          try
          {
            if (isWow64FsRedirectionDisabled)
              result = Wow64RevertWow64FsRedirection(ptr); //Restore file system redirection for the calling thread.
            Logger.Info("isWow64FsRedirectionDisabled: " + isWow64FsRedirectionDisabled);
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
          }
        }
        else // We are on 32bit machine so there is no redirection
          return true;
        return result;
      }

      #endregion
    }
}
