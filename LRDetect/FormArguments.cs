using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace LRDetect
{
  class FormArguments
  {
    public static string[] args = Environment.GetCommandLineArgs();
    public static string cmdArgs = String.Empty;
    public static bool help = false;

    // Start collection immideatelly
    public static bool startCollection = false;

    //Report level
    public static int details = 2;
    public static String fileName = String.Empty;
    
    //do we check dlls versions?
    public static bool dllsCheck = false;

    //do we check all dlls in lr bin folder?
    public static bool checkAllDlls = false;

    //array of dlls to be checked
    public static List<string> dlls = new List<string>();

    //do we collect network information?
    public static bool network = false;

    public static int loglevel { get { return Logger.level; } set { Logger.level = value; } }
    public static bool appendLogs = false;
    public static bool async = true;

    // Should I open the generated report when done?
    public static bool hideReport = false;

    public static bool system = false;

    public static bool menuNetorkInfo { get { return network; } set { network = value; } }

    public static bool menuDllsInfo { get { return checkAllDlls; } set { checkAllDlls = value; dllsCheck = value; } }
    public static bool processes = true;
    public static bool menuSystemLogs { get { return appendLogs; } set { appendLogs = value; } }
    public static bool menuSystemProcesses { get { return processes; } set { processes = value; } }
    private static bool installedPrograms = true;
    public static bool menuInstalledPrograms { get { return installedPrograms; } set { installedPrograms = value; } }
    private static bool includeWindowsUpdates = false;
    public static bool menuWindowsUpdates { get { return includeWindowsUpdates; } set { includeWindowsUpdates = value; } }

    private static bool hashCheck = true;
    public static bool menuHashCheck { get { return hashCheck; } set { hashCheck = value; } }

    static FormArguments()
    {
      if (args.Length > 0)
      {
        cmdArgs = String.Join(" ", args);
        for (int i = 0; i < args.Length; i++)
        {
	        // Convert all arguments to lower case
	        var Arg = args[i].ToLower();
	        var NextArg = i < args.Length - 1 ? args[i + 1] : "";
	
	        switch (Arg) 
	        {
		        case "-help":
		        case "/?":
			        help = true;
			        break;
		
		        case "-collect":
			        startCollection = true;
			        break;
			
		        case "-hidereport":
			        hideReport = true;
			        break;
		
		        case "-ipconfig":
			        network = true;
			        break;
			
		        case "-appendlogs":
			        appendLogs = true;
			        break;
			
		        case "-noasync":
			        async = false;
			        Logger.Info("No collector methods will be executed in parallel mode");
			        break;
			
		        case "-details":
			        details = Int32.Parse(NextArg);
			        Logger.Info("Level of details changed to " + details + " as per CMD argument");
			        break;
			
		        case "-file":
			        fileName = NextArg.Replace("\"",""); // remove all "
			        Logger.Info("Report file name changed to " + fileName + " as per CMD argument");
			        break;
			
		        case "-dlls":
			        // Check if next argument is the list of dlls something
			        if (NextArg.StartsWith("-"))
                menuDllsInfo = true; //There is no dlls list so we use check all dlls in bin folder
			        else
			        {
				        dlls = NextArg.Replace("\"","").Split(',').ToList<string>();
                dllsCheck = true; 
				        Logger.Info("Report will check the some selected dlls versions in %VUGEN_PATH%\\bin folder");
			        }
			        break;
		
		        case "-loglevel":
			        loglevel = Int32.Parse(NextArg);
			        break;
	        }
        }
      }
    }

    internal static void SetMenuItems(ToolStripMenuItem tsm)
    {
      if (tsm.Name.Contains("network"))
        menuNetorkInfo = tsm.Checked ? true : false;
      if (tsm.Name.Contains("dlls"))
        menuDllsInfo = tsm.Checked ? true : false;
      if (tsm.Name.Contains("system"))
        menuSystemProcesses = tsm.Checked ? true : false;
      if (tsm.Name.Contains("Logs"))
        menuSystemLogs = tsm.Checked ? true : false;
      if (tsm.Name.Contains("installedPrograms"))
        menuInstalledPrograms = tsm.Checked ? true : false;
      if (tsm.Name.Contains("windowsUpdates"))
        menuWindowsUpdates = tsm.Checked ? true : false;
    }
  }
}
