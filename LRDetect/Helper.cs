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

namespace LRDetect
{
    public class Helper
    {
        private string temp = Environment.GetEnvironmentVariable("TEMP");
        //private static bool verboseOutput = MainForm.args.Any(s => s.Contains("verbose"));
        //private static bool dlls = MainForm.menuDllsInfo || MainForm.args.Any(s => s.Contains("dlls"));
        private static bool appendLog = false;//MainForm.args.Any(s => s.Contains("appendlog"));
        private static string logFile = System.Environment.GetEnvironmentVariable("TEMP") + @"\LR.Detect.log";
 
        #region Save HTML file
        public static void SaveHtmlFile(string html, Action<int, int> reportProgress)
        {
            try
            {
                File.WriteAllText(MainForm.OutputFileName, html);
                if (reportProgress != null)
                    reportProgress(7, 7);
            }
            catch (UnauthorizedAccessException e)
            {
                //if we encounter UnauthorizedAccessException we'll write the file into  user %TEMP% directory
                // set the new path to the output file name
                MainForm.OutputFileName = Environment.GetEnvironmentVariable("TEMP") + @"\" + MainForm.OutputFileName;
                File.WriteAllText(MainForm.OutputFileName, html);
                Log.Error("Unable to write in program directory. File saved in " + Environment.GetEnvironmentVariable("TEMP") + "\n" + e.ToString());
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            finally
            {
                Log.Info("Output file name: " + MainForm.OutputFileName);
            }
        }
        #endregion
 
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
                Log.Info(ex.ToString());
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
            DateTime dt = new DateTime(Convert.ToInt32(date.Substring(0, 4)), Convert.ToInt32(date.Substring(4, 2)), Convert.ToInt32(date.Substring(6, 2)));
            return " installed on " + dt.ToLocalTime().ToShortDateString().ToString();
        }
        #endregion

        #region Query with Windows Management Instrumentation
        /// <summary>
        /// Query the WMI
        /// </summary>
        /// <param name="name">the name of the object property we want to get</param>
        /// <param name="scope">i.e. root\CIMV2</param>
        /// <param name="wmiClass">i.e. FirewallProduct or AntiVirusProduct</param>
        /// <param name="where">where clause, empty by default</param>
        /// <returns></returns>
        public static string QueryWMI(string name, string scope, string wmiClass, string where = "")
        {
            try
            {
                string query = "SELECT * FROM " + wmiClass + " " + where;
                Log.Info("Quering WMI = " + query);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);
                List<String> results = new List<string>();

                foreach (ManagementObject queryObj in searcher.Get())
                {
                    results.Add(queryObj[name].ToString());
                }

                string info = "Not detected";
                if (results.Count == 1)
                    info = results[0].ToString();
                if (results.Count > 1)
                    info = String.Join(", ", results.ToArray());
                return info;
            }
            catch (ManagementException ex)
            {
                Log.Error("An error occurred while querying for WMI data: " + ex.Message);
                return Html.ErrorMsg();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Collect information and generate HTML content
        /// <summary>
        ///  collects the necessary information and returns HTML content string
        /// </summary>
        public static void GenerateHtmlContent(Html htmlBuffer, Action<string> updateProgressLabel, Action<int, int> updateProgressBar)
        {
            try
            {

                bool displayIpConfig = MainForm.menuNetorkInfo || MainForm.args.Any(s => s.Contains("ipconfig")) || MainForm.reportDetailsLevel == 3;
                //bool dlls = MainForm.menuDllsInfo || MainForm.args.Any(s => s.Contains("dlls"));

                //start the timer
                Stopwatch stopWatch = Stopwatch.StartNew();
                // ##############
                // OS INFORMATION
                // ##############
                Log.Info("Starting collection of OS & Hardware information");
                updateProgressLabel("Collecting system information");
                if (updateProgressBar != null)
                    updateProgressBar(1, 7);
                htmlBuffer.AddRawContent("\n\n\t\t<h2>Operating System & Hardware Information</h2>\n");
                htmlBuffer.OpenTable();
                htmlBuffer.AddTableRow("Machine name", System.Environment.MachineName);
                htmlBuffer.AddTableRow("Operating System", OSInfo.GetOSFullName());
                htmlBuffer.AddTableRow("Locale", OSInfo.GetOSLocaleInfo());
                updateProgressLabel("Detecting if system is virtualized");
                htmlBuffer.AddTableRow("Is OS Virtualized?", OSInfo.IsOSVirtualizedInfo());
                htmlBuffer.AddTableRow("CPU", OSInfo.GetProcessorNameString());
                htmlBuffer.AddTableRow("Processor Count", Environment.ProcessorCount.ToString());
                htmlBuffer.AddTableRow("Total Memory", OSInfo.GetMemoryInfo());
                htmlBuffer.AddTableRow("Is 3GB switch enabled", OSInfo.Is3GBSwitchEnabled());
                updateProgressLabel("Detecting hard drives information");
                htmlBuffer.AddTableRow("Hard Drives", OSInfo.GetHardDrivesInformation());
                htmlBuffer.AddTableRow("Monitor information", OSInfo.GetMonitorsInfo());
                htmlBuffer.AddTableRow("Default browser", OSInfo.GetDefaultBrowser());
                htmlBuffer.AddTableRow("Internet Explorer version", OSInfo.GetIEVersion());
                updateProgressLabel("Detecting DEP");
                htmlBuffer.AddTableRow("Data Execution Prevention", OSInfo.IsDepDisabled());
                updateProgressLabel("Detecting UAC");
                htmlBuffer.AddTableRow("User Account Control", OSInfo.IsUACEnabled());
                updateProgressLabel("Detecting is user administrator");
                htmlBuffer.AddTableRow("Is user Admin?", Html.Bool2Text(OSInfo.IsUserInAdminGroup()));
                htmlBuffer.AddTableRow("Is user connected remotely?", Html.Bool2Text(SystemInformation.TerminalServerSession));
                updateProgressLabel("Detecting AV/Firewall details");
                htmlBuffer.AddTableRow("Is Windows firewall enabled?", OSInfo.IsWindowsFirewallEnabled());
                // the following 2 methods work only on XP, Vista and Windows 7
                if (OSInfo.isOSDesktopEdition)
                {
                    htmlBuffer.AddTableRow("Is anti-virus software detected?", DetectSecuritySoftware.AntiVirusOnDesktopOSInfo());
                    htmlBuffer.AddTableRow("Is firewall software detected?", DetectSecuritySoftware.FirewallOnDesktopOSInfo());
                }
                else
                {
                    Log.Info("Checking for AV on Server type OS");
                    htmlBuffer.AddTableRow("Anti-virus software information", DetectSecuritySoftware.GetAntiVirusOnServerOSInfo());
                }

                updateProgressLabel("Detecting AV/Firewall details");
                var ss = new DetectSecuritySoftware();
                if (ss.numberOfDetectedProducts > 0)
                    htmlBuffer.AddTableRow("Security software details", ss.ToString());

                updateProgressLabel("Detecting environment variables");
                // get the content of krb5.ini if the file exists
                string krb5File = OSInfo.GetEnvVariable("KRB5_CONFIG");
                string krb5Content = String.Empty;
                if (!krb5File.Contains("Not set"))
                {
                    Log.Info("Attempting to get the krb5.ini content from " + krb5File);
                    krb5Content = "\n\t" + Html.LinkShowContent("krb5_config") + "\n\t<div id=\"krb5_config\" class=\"dontShow\">"
                        //+ Helper.ShowIniContent(@"C:\Users\genevh\Documents\LoadRunner DC\Kerberos\krb5.ini")
                        + IniParser.ToString(krb5File)
                        + "\n\t</div>";
                }

                htmlBuffer.AddTableRow("Environment Variables", ""
                    //+ Html.b("COMPUTERNAME = ") + Environment.GetEnvironmentVariable("COMPUTERNAME") + Html.br
                    //+ Html.b("HOMEPATH = ") + Environment.GetEnvironmentVariable("HOMEPATH") + Html.br
                    + Html.B("PATH =") + Html.br + OSInfo.GetEnvVariable("PATH") + Html.br
                    + Html.B("User TEMP = ") + OSInfo.GetEnvVariable("TEMP") + Html.br
                    + Html.B("System TEMP = ") + OSInfo.GetEnvVariable("TEMP", true) + Html.br
                    + Html.B("TMP = ") + OSInfo.GetEnvVariable("TMP") + Html.br
                    + Html.B("KRB5_CONFIG = ") + krb5File + krb5Content);


                htmlBuffer.AddTableRow("Layered Service Providers", "\n\t\t\t\t\t"
                    + Html.B(OSInfo.GetNumberOfInstalledLSPs().ToString()
                    + " entries found") + "\n\t\t\t\t\t" + Html.LinkShowContent("lsp")
                    + Html.br + OSInfo.GetInstalledLSPs("lsp"));

                updateProgressLabel("Detecting AppInit_DLLs");
                htmlBuffer.AddTableRow("AppInit_DLLs registry value", OSInfo.GetAppInitDLLsInfo());
                //LoadAppInit_DLLs registry is only availbale in Windows 7 and later
                Version verWhereLoadAppDLL = new Version(6, 1);
                if (OSInfo.OSVersion >= verWhereLoadAppDLL)
                {
                    htmlBuffer.AddTableRow("LoadAppInit_DLLs registry value", OSInfo.GetLoadAppInitDLLsInfo());
                }


                //IPCONFIG /ALL
                if (displayIpConfig)
                {
                    htmlBuffer.AddTableRow("IP Addresses", OSInfo.LocalIPAddress());

                    string ipConfig = Html.LinkShowContent("ipconfig") + "\n\t<div id=\"ipconfig\" class=\"dontShow\">"
                        + Html.Pre(OSInfo.IpConfig())
                        + "\n\t</div>";
                    htmlBuffer.AddTableRow("Output of 'ipconfig /all' command", ipConfig);
                }

                htmlBuffer.CloseTable();
                if (updateProgressBar != null)
                    updateProgressBar(2, 7);
                // ######################
                // LOADRUNNER INFORMATION
                // ######################
                if (updateProgressBar != null)
                    updateProgressBar(3, 7);
                updateProgressLabel("Detecting installed products");

                htmlBuffer.AddRawContent("\n\n\t\t<h2>LoadRunner Information</h2>\n");
                htmlBuffer.OpenTable();

                // Products object initialization
                // If a product is not instlalled its methods and properties will return NULL
                Log.Info("Starting collection of HP products information");
                Log.Info("Detecting LoadRunner installation");
                LoadRunnerInfo lri = new LoadRunnerInfo();
                Log.Info("Detecting VuGen Stand Alone installation");
                VugenSAInfo vgi = new VugenSAInfo();
                Log.Info("Detecting Analysis Stand Alone installation");
                AnalysisInfo ani = new AnalysisInfo();
                Log.Info("Detecting Load Generator installation");
                LoadGeneratorInfo lgi = new LoadGeneratorInfo();
                Log.Info("Detecting Monitor Over Firewall installation");
                MonitorOverFirewallInfo mofi = new MonitorOverFirewallInfo();
                Log.Info("Detecting Performance Center Host installation");
                PerformanceCenterHostInfo pchi = new PerformanceCenterHostInfo();
                Log.Info("Detecting Performance Center Server installation");
                PerformanceCenterServerInfo pcsi = new PerformanceCenterServerInfo();

                //LINQ query to find the installed products
                List<ProductInfo> products = new List<ProductInfo>() { lri, vgi, ani, lgi, mofi, pchi, pcsi };
                List<ProductInfo> installedProducts = products.FindAll(x => x.IsProductInstalled == true);

                //If any of the products from VuGen family (VuGen SA, LR or PC Host) is installed, we will assign it to vugen object
                List<ProductInfo> vugenProducts = new List<ProductInfo>() { lri, vgi, pchi };
                
                var vugen = vugenProducts.Find(x => x.IsProductInstalled == true);
                // if there is no vugen installed on the system, vugen will be null, so I will assign it to vgi
                if (vugen == null)
                    vugen = vgi;
                
                List<ProductInfo> loadGeneratorProducts = new List<ProductInfo>() { lri, pchi, lgi };
                var loadgen = loadGeneratorProducts.Find(x => x.IsProductInstalled == true);
                // if there is no load generator installed on the system, lgi will be null, so I will assign it to loadgen
                if (loadgen == null)
                    loadgen = lgi;

                bool isFullProductInstalled = lri.IsProductInstalled || pchi.IsProductInstalled;

                //create an Analysis object. Both conditions are met in Full loadrunner product and analysis standalone
                var analysis = (ProductInfo)installedProducts.Find(x => x.isAnalysis == true && x.IsProductInstalled == true);
                //if we don't have neither full loadrunner nor analysis sa installed, we refer to ani object
                if (analysis == null)
                    analysis = ani;

                //bool isAnyProductInstalled = lri.IsProductInstalled || pchi.IsProductInstalled || vgi.IsProductInstalled || ani.IsProductInstalled || lgi.IsProductInstalled || mofi.IsProductInstalled || pcsi.IsProductInstalled;
                //var anyProduct = (ProductInfo) installedProducts.Find(x => x.IsProductInstalled == true);
                //bool isAnyProductInstalled = anyProduct.IsProductInstalled;
                //var anyProduct = products.Find(x => x.IsProductInstalled == true);
                //bool isAnyProductInstalled = anyProduct.IsProductInstalled;

                //string productName = lri.ProductName + pchi.ProductName;
                //string productVersion = lri.ProductVersion + pchi.ProductVersion;
                string installDate = lri.InstallDate + pchi.InstallDate;
                string readme = isFullProductInstalled ? Html.Link("file:///" + lri.InstallLocation + pchi.InstallLocation + "dat/Readme.htm", "View Readme") : "";
                htmlBuffer.AddTableRow("Is full LoadRunner installed?", Html.Bool2Text(isFullProductInstalled) + " " + lri.ProductName + pchi.ProductName + " " + lri.ProductVersion + pchi.ProductVersion + " " + installDate + " " + readme);

                //If Full LR or PC HOST are not installed:
                if (!isFullProductInstalled)
                {
                    string vugenSAInfo = vgi.IsProductInstalled ? "Yes " + vgi.GetProductNameVersionDate() + " " + Html.Link(vgi.InstallLocation + @"\dat\Readme.htm", "View Readme") : "No";
                    htmlBuffer.AddTableRow("Is VuGen stand-alone installed?", vugenSAInfo);
                    htmlBuffer.AddTableRow("Is Analysis stand-alone installed?", Html.Bool2Text(ani.IsProductInstalled) + " " + ani.GetProductNameVersionDate());
                    htmlBuffer.AddTableRow("Is Load Generator installed?", Html.Bool2Text(lgi.IsProductInstalled) + " " + lgi.GetProductNameVersionDate());
                    htmlBuffer.AddTableRow("Is Monitor Over Firewall installed?", Html.Bool2Text(mofi.IsProductInstalled) + " " + mofi.GetProductNameVersionDate());
                    htmlBuffer.AddTableRow("Is Performance Center Server installed?", Html.Bool2Text(pcsi.IsProductInstalled) + " " + pcsi.GetProductNameVersionDate());
                }

                // Display patches installed for the products. If a product is not install patchesInstalled will return empty string
                if (installedProducts.Count > 0)
                {
                    //updateProgressLabel("Detecting LR components");
                    StringBuilder customComponentsInstalled = new StringBuilder(1024);
                    StringBuilder mainExecutableFiles = new StringBuilder(1024);
                    StringBuilder importantRegKeys = new StringBuilder(1024);
                    StringBuilder environmentVariables = new StringBuilder(1024);
                    StringBuilder patchesInstalledInfo = new StringBuilder(1024);

                    foreach (var installedProduct in installedProducts)
                    {
                        customComponentsInstalled.Append(installedProduct.CustomComponentsInstalled);
                        mainExecutableFiles.Append(installedProduct.mainExecutableFilesInfo);
                        importantRegKeys.Append(installedProduct.ImportantRegKeyValues);
                        environmentVariables.Append(installedProduct.environmentVariables);
                        patchesInstalledInfo.Append(installedProduct.patchesInstalled);
                    }

                    // Collect the patches for the installed products
                    string patchesInstalled = (patchesInstalledInfo.ToString() != "") ? patchesInstalledInfo.ToString() : "None";
                    updateProgressLabel("Detecting LR patches");
                    htmlBuffer.AddTableRow("Patches installed", patchesInstalled);

                    // Display if the latest patch installed
                    // Valid only for LoadRunner or Performance Center Host
                    /*string latestPatchInfo = lri.latestPatchInfo + pchi.latestPatchInfo + vgi.latestPatchInfo;

                    if (isFullProductInstalled || vgi.IsProductInstalled)
                        htmlBuffer.AddTableRow("Is the latest patch installed?", latestPatchInfo);
                    if (ani.IsProductInstalled)
                        htmlBuffer.AddTableRow("Is Analysis latest patch installed?", ani.latestPatchInfo);
                    if (lgi.IsProductInstalled)
                        htmlBuffer.AddTableRow("Is Load Generator latest patch installed?", lgi.latestPatchInfo);
                    if (pcsi.IsProductInstalled)
                        htmlBuffer.AddTableRow("Is PC Server latest patch installed?", pcsi.latestPatchInfo);

                    //if (isFullProductInstalled)
                      //  htmlBuffer.AddTableRow("License information", "Detected license bundles - not implemented yet!");
                    */
                    htmlBuffer.AddTableRow("Custom components installed", customComponentsInstalled.ToString());

                    updateProgressLabel("Detecting main executables");
                    htmlBuffer.AddTableRow("Main executable files", mainExecutableFiles.ToString());
                    // check if we only have MOF installed
                    if (mofi.IsProductInstalled && (!vugen.IsProductInstalled || !lgi.IsProductInstalled))
                        htmlBuffer.AddTableRow("Related environment variables", mofi.environmentVariables);
                    else
                        htmlBuffer.AddTableRow("Related environment variables", environmentVariables.ToString());


                    MagentInfo magentInfo = new MagentInfo(loadgen);
                    htmlBuffer.AddTableRow("Is Agent installed?", magentInfo.ToString());

                    //######################
                    // VuGen configuration #
                    //######################

                    if (vugen.IsProductInstalled)
                    {
                        htmlBuffer.CloseTable();

                        //###################################
                        //# Checking product configuration  #
                        //###################################

                        htmlBuffer.AddRawContent("\n\n\t\t<h2>" + vugen.ProductName + " configuration</h2>\n");
                        htmlBuffer.OpenTable();
                        updateProgressLabel("Checking product settings.");
                        //htmlBuffer.AddTableRow("Correlation rules support", vgi.correlationRulesSupport + lri.correlationRulesSupport + pchi.correlationRulesSupport);
                        htmlBuffer.AddTableRow("Correlation rules support", vugen.correlationRulesSupport);


                        //check for ignored content only on versions > 11.50
                        if (vugen.version.CompareTo(new Version("11.50")) != -1)
                            htmlBuffer.AddTableRow("Correlation ignored content types", vugen.correlationIgnoredContent);
                        //BBHOOK vesrion
                        htmlBuffer.AddTableRow("Bbhook version", vugen.bbhookVersion);
                        htmlBuffer.AddTableRow("Miscellenious registry settings", importantRegKeys.ToString());

                        updateProgressLabel("Parsing vugen.ini");
                        htmlBuffer.AddTableRow("vugen.ini content", Html.LinkShowContent("vugenIni"));
                        htmlBuffer.CloseTable();

                        htmlBuffer.AddRawContent("\n\t<div id=\"vugenIni\" class=\"dontShow\">"
                            + IniParser.ToString(vugen.InstallLocation + @"config\vugen.ini")
                            + "\n\t</div>");

                        htmlBuffer.OpenTable();
                        updateProgressLabel("Checking for registration failures");
                        htmlBuffer.AddTableRow("Registration failures", Html.LinkShowContent("registrationFailures"));
                        htmlBuffer.CloseTable();
                        htmlBuffer.AddRawContent("\n\t<div id=\"registrationFailures\" class=\"dontShow\">"
                                + Helper.GetRegistraionFailures()
                                + "\n\t</div>");

                        //any of the below will cotain data if vugen is installed
                        //string vugenVersion = productVersion + vgi.ProductVersion;
                        //if vugen is not earlier than 11.50 (if yes CompareTo returns -1)
                        if (vugen.version.CompareTo(new Version("11.50")) != -1)
                        {
                            htmlBuffer.OpenTable();
                            updateProgressLabel("Checking product settings.");
                            htmlBuffer.AddTableRow("Last 30 lines of HP.LR.VuGen.Log", Html.LinkShowContent("vugenLog"));
                            htmlBuffer.CloseTable();

                            updateProgressLabel("Parsing VuGen log file.");
                            htmlBuffer.AddRawContent("\n\t<div id=\"vugenLog\" class=\"dontShow\">"
                                + Helper.GetLastLinesFromFile(1024 * 1024, Path.GetTempPath() + "HP.LR.VuGen.log", 30)
                                + "\n\t</div>");
                        }



                        #region DLLs check
                        // get info about LR dlls only if -noverbose and -nodlls CMD args are not used
                        bool addDllsCheckToReport = MainForm.menuDllsInfo || MainForm.args.Any(s => s.Contains("dlls"));
                        if (!addDllsCheckToReport)
                            Log.Info("Dlls check skipped");
                        if (MainForm.reportDetailsLevel == 3 || addDllsCheckToReport)
                        {
                            updateProgressLabel("Checking DLLs. Please wait.");
                            htmlBuffer.OpenTable();
                            htmlBuffer.AddTableRow("DLLs versions", Html.LinkShowContent("dlls"));
                            htmlBuffer.CloseTable();
                            htmlBuffer.AddRawContent("\n\t<div id=\"dlls\" class=\"dontShow\">");

                            htmlBuffer.OpenTable("lrdlls", "alternateColors");
                            htmlBuffer.OpenTableBody();
                            //we will scann all dlls if the argument does not contain list of dlls to be scanned
                            bool scanAllDlls = MainForm.menuDllsInfo || MainForm.args.Any(s => s.Equals("-dlls"));
                            if (scanAllDlls)
                            {
                                var dllsInBinFolder = Directory.GetFiles(vugen.InstallLocation + "bin").Where(name => name.EndsWith(".dll"));
                                foreach (string dll in dllsInBinFolder)
                                {
                                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(dll);
                                    htmlBuffer.AddTableRowPlain(Path.GetFileName(fvi.FileName), fvi.FileVersion, File.GetLastWriteTime(dll).ToLocalTime().ToString());
                                }
                            }
                            else
                            {
                                List<string> listOfSelectedDlls = new List<string>();

                                //check if the user has specified dlls:all or a list of dlls as dlls:dll1.dll,dll2.dll etc ... 
                                string argument = String.Empty;
                                foreach (string arg in MainForm.args)
                                {
                                    if (arg.StartsWith("-dlls:"))
                                    {
                                        argument = arg.Substring(6);
                                    }
                                }
                                //if the list contains more than one dll separated by commas create an array
                                if (argument.Contains(','))
                                    listOfSelectedDlls = argument.Split(',').ToList();
                                else
                                    listOfSelectedDlls.Add(argument);

                                foreach (string dll in listOfSelectedDlls)
                                {
                                    string dllPath = vugen.InstallLocation + @"bin\" + dll;
                                    if (File.Exists(dllPath))
                                    {
                                        FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(dllPath);
                                        htmlBuffer.AddTableRowPlain(dll, fvi.FileVersion, File.GetLastWriteTime(dllPath).ToLocalTime().ToString());
                                    }
                                    else
                                    {
                                        htmlBuffer.AddTableRowPlain(dll + " does not exist in bin folder", "");
                                    }
                                }
                            }
                            htmlBuffer.CloseTableBody();
                            htmlBuffer.CloseTable();
                            htmlBuffer.AddRawContent("\n\t</div>");
                        }
                        #endregion

                        //#########################
                        //PROTOCOLS SETTINGS
                        //#########################
                        htmlBuffer.AddRawContent("\n\n\t\t<h2>" + vugen.ProductName + " various protocols settings</h2>\n");
                        htmlBuffer.OpenTable();
                        htmlBuffer.AddTableRow("FLEX, AMF", vugen.GetProtocolFlexInfo());
                        htmlBuffer.CloseTable();
                    }

                    //#########################
                    //Analysis SETTINGS
                    //#########################

                    if (analysis.IsProductInstalled)
                    {
                        htmlBuffer.AddRawContent("\n\n\t\t<h2>" + analysis.ProductName + " settings</h2>\n");
                        htmlBuffer.OpenTable();
                        htmlBuffer.AddTableRow("Analysis configuration file", Html.LinkShowContent("analysis80"));
                        htmlBuffer.CloseTable();
                        updateProgressLabel("Parsing Analysis settings file.");
                        htmlBuffer.AddRawContent("\n\t<div id=\"analysis80\" class=\"dontShow\">"
                            + analysis.parseAnalysisIniFile()
                            + "\n\t</div>");
                        htmlBuffer.OpenTable();

                    }


                    htmlBuffer.CloseTable();
                    if (updateProgressBar != null)
                        updateProgressBar(4, 7);
                }
                else
                {
                    //#######################
                    // NO PRODUCTS INTALLED
                    // Check for prerequisites 
                    //#######################
                    //htmlBuffer.AddTableRow("LoadRunner installation was not detected", "Checking for OS compatibility and prerequisites - not implemented yet!");
                    Log.Raw("LoadRunner installation was not detected");
                    if (updateProgressBar != null)
                        updateProgressBar(4, 7);
                }

                // ##########################
                // OTHER SOFTWARE INFORMATION
                // ##########################
                if (updateProgressBar != null)
                    updateProgressBar(5, 7);


                // Collect this info only if the details level of report is 2 or 3
                if (MainForm.reportDetailsLevel > 1)
                {
                    Log.Info("Starting collection of other useful information");
                    htmlBuffer.AddRawContent("\n\n\t\t<h2>Other related information</h2>\n");
                    htmlBuffer.OpenTable();

                    updateProgressLabel("Collecting .net information");
                    Log.Info("Collecting .NET Information");
                    htmlBuffer.AddTableRow(".NET", 2);
                    htmlBuffer.AddTableRow(".NET versions installed", DetectOtherSoftware.GetDotNetVersion());

                    updateProgressLabel("Collecting java information");
                    Log.Info("Collecting JAVA Information");
                    htmlBuffer.AddTableRow("JAVA", 2);
                    DetectJava dj = new DetectJava();
                    htmlBuffer.AddTableRow("Java Information", dj.ToString());
                    if (vugen.IsProductInstalled)
                        htmlBuffer.AddTableRow("VuGen JRE vesrion", dj.GetJavaVersionFromCmd(vugen.InstallLocation + @"\jre"));

                    updateProgressLabel("Collecting Citrix information");
                    Log.Info("Collecting Citrix Information");
                    htmlBuffer.AddTableRow("Citrix", 2);
                    htmlBuffer.AddTableRow("Citrix client version", DetectOtherSoftware.GetCitrixClientInfo());
                    //check if Citrix registry patch is installed only on vugen family or load generators > 11 systems
                    if (vugen.IsProductInstalled)
                    {
                        if (vugen.version.CompareTo(new Version("11.0")) != -1)
                            htmlBuffer.AddTableRow("Is Citrix registry patch installed?", DetectOtherSoftware.GetCitrixRegistryPatchInfo(vugen.ProductVersion));
                    }
                    else if (loadgen.IsProductInstalled)
                    {
                        if (loadgen.version.CompareTo(new Version("11.0")) != -1)
                            htmlBuffer.AddTableRow("Is Citrix registry patch installed?", DetectOtherSoftware.GetCitrixRegistryPatchInfo(vugen.ProductVersion));
                    }

                    updateProgressLabel("Collecting RDP information");
                    Log.Info("Collecting RDP Information");
                    htmlBuffer.AddTableRow("RDP", 2);
                    htmlBuffer.AddTableRow("RDP client version", DetectOtherSoftware.GetRDPClientVersion());

                    updateProgressLabel("Collecting Oracle information");
                    Log.Info("Collecting Oracle client information");
                    htmlBuffer.AddTableRow("Oracle", 2);
                    htmlBuffer.AddTableRow("Oracle DB client information", DetectOtherSoftware.GetOracleClientInfo());

                    //buffer.AddTableRow("SapGui", 2);
                    //buffer.AddTableRow("SapGui client version", DetectOtherSoftware.getSapGuiClientVersion());
                    //buffer.AddTableRow("Is SapGui scripting enabled?", DetectOtherSoftware.isSapGuiScriptingEnabled()); 

                    htmlBuffer.CloseTable();

                    updateProgressLabel("Detecting QTP installation");
                    htmlBuffer.OpenTable();
                    Log.Info("Collecting QTP information");
                    htmlBuffer.AddRawContent("\n\n\t\t<h2>Other HP related products</h2>\n");
                    htmlBuffer.AddTableRow("HP QuickTest Professional", 2);
                    QuickTestProInfo qtpi = new QuickTestProInfo();
                    htmlBuffer.AddTableRow("Is QTP installed?", Html.Bool2Text(qtpi.IsProductInstalled) + " " + qtpi.ProductName + " " + qtpi.ProductVersion + qtpi.InstallDate);

                    updateProgressLabel("Detecting SiteScope installation");
                    Log.Info("Collecting SiteScope information");
                    htmlBuffer.AddTableRow("HP SiteScope", 2);
                    htmlBuffer.AddTableRow("Is SiteScope installed?", DetectOtherSoftware.GetSiteScopeInfo());

                    htmlBuffer.CloseTable();

                    if (updateProgressBar != null)
                        updateProgressBar(6, 7);



                    // ################################
                    // COLLECTING PROCESSES INFORMATION  #
                    // ################################
                    if (MainForm.menuSystemProcesses)
                    {
                        updateProgressLabel("Detecting running processes");
                        Log.Info("Collecting information about running processes");
                        htmlBuffer.AddRawContent("\n\n\t\t<h2>List of running processes (" + OSInfo.GetRunningProcesses().Count() + " processes are running)</h2>\n");

                        htmlBuffer.AddRawContent(Html.LinkShowContent("processes"));
                        htmlBuffer.AddRawContent(Html.br + Html.br);

                        htmlBuffer.OpenTable("processes", "alternateColors");
                        htmlBuffer.AddTableHead(Html.B(" Process Name "), Html.B(" Memory Usage "), Html.B(" Process ID "));
                        htmlBuffer.AddRawContent("<tbody>");
                        int i = 0;

                        foreach (Process p in OSInfo.GetRunningProcesses())
                        {
                            htmlBuffer.AddTableRow(p.ProcessName, (p.WorkingSet64 / 1024).ToString() + " Mb", p.Id.ToString());
                            i++;
                        }
                        htmlBuffer.AddRawContent("</tbody>");
                        htmlBuffer.CloseTable();
                        Log.Info("Information about running processes collected.");
                    }


                }
                else
                {
                    // if details level of report is 1 then finish
                    if (updateProgressBar != null)
                        updateProgressBar(6, 7);
                }


                //stop the timer
                TimeSpan ts = stopWatch.Elapsed;
                stopWatch.Stop();
                updateProgressLabel("Information collected!");
                Assembly assembly = Assembly.GetExecutingAssembly();
                String LRDetectVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
                
                htmlBuffer.AddRawContent(Html.br + "Report generation time: " + ts.ToString() + Html.br + "LR Detect version: " + LRDetectVersion);

                // Attach the created log for debuging purposes
                // only if -addLogToOutput CMD arg is used
                if (appendLog)
                {
                    Log.Info("Adding log to output as per CMD arguments");
                    if (File.Exists(Helper.logFile))
                    {
                        htmlBuffer.AddRawContent(Html.br + Html.br + "LR Detect log attached for debugging purposes " + Html.LinkShowContent("log") + Html.br);
                        htmlBuffer.AddRawContent("\n\t<div id=\"log\" class=\"dontShow\">"
                            + Html.Pre(File.ReadAllText(Helper.logFile)) + "\n\t</div>");
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Critical error. " + ex.Message + "\r\n See log file for details.\r\nReport not completed!!!");
                Log.Error(ex.ToString());
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
                    return "The file " + pathToFile + " is too big to be parsed (" + fi.Length / 1024 + " kb). Check manually. Parsing cancelled!";

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
                        sb.Append(Html.EscapeSpecialChars(lines[cursor]) + Html.br);
                    }
                    cursor++;
                }
                return sb.ToString();
            }
            catch (FileNotFoundException)
            {
                Log.Error("File not found: " + pathToFile);
                return "File not found: " + pathToFile;
            }
            catch (IOException)
            {
                Log.Error("Can not parse " + pathToFile + ", it is locked by another process.");
                return "Can not parse " + pathToFile + ", it is locked by another process.";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return "The file was not parsed due to an error: " + ex.ToString();
            }
        
        }
        #endregion

        public static string GetRegistraionFailures()
        {
            try
            {
                string windowsTempDir = Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine);
                string[] filePaths = Directory.GetFiles(windowsTempDir, "*RegistrationFailure.log");
                //if no files are found get out of here
                if (filePaths.Length == 0)
                    return "None, no RegistrationFailure.log files found";

                StringBuilder output = new StringBuilder();

                foreach (string filePath in filePaths)
                {
                    string lastLines = GetLastLinesFromFile(1024, filePath, 5);
                    output.Append("Last 5 lines in: " + Html.I(Path.GetFileName(filePath)) + Html.br + " " + lastLines);

                }

                return output.Length > 0 ? output.ToString() : "Not found!";
            }
            catch (ArgumentNullException ane)
            {
                Log.Error(ane.ToString());
                return Html.Error("System environment variable PATH not set!");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }
    }
}
