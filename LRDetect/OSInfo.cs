#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// registry access
using Microsoft.Win32;
// dllimport
using System.Runtime.InteropServices;
// screen resolution
using System.Windows.Forms;
// hard drives info
using System.Management;
using System.IO;
// is dep disabled
using System.Diagnostics;
// is user admin
using System.DirectoryServices.AccountManagement;
// environment variables
using System.Collections;
//is user admin
using System.ComponentModel;
using System.Security.Principal;
using System.Security.Permissions;
using Microsoft.Win32.SafeHandles;
// locale
using System.Threading;
using System.Globalization;
// ip address host
using System.Net;
using System.Net.Sockets;

#endregion

namespace LRDetect
{
    class OSInfo
    {
        #region Members
        public static bool is64BitOperatingSystem = false;
        public static bool isOSDesktopEdition = false;
        // used in IsFirewallDetected and IsAntiVirusDetected
        public static Version OSVersion = new Version("9.0.0.0");
        #endregion

        #region Methods

        #region Get OS name
        /// <summary>
        /// Get Operating system information.
        /// </summary>
        /// <returns></returns>
        public static string GetOSFullName()
        {
            try
            {
                OperatingSystem os = Environment.OSVersion;
                OSInfo.OSVersion = os.Version;
                Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
                // !!!this is necessary for the WMI security center scope
                if (ci.OSFullName.Contains("XP") || ci.OSFullName.Contains("Vista") || ci.OSFullName.Contains("7"))
                    OSInfo.isOSDesktopEdition = true;

                return ci.OSFullName + " " + os.ServicePack + " " + GetOperatingSystemArchitecture() + " (" + ci.OSVersion + ") ";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }        
        #endregion

        #region Execute ipconfig /all
        public static string IpConfig()
        {
            string output = Helper.ExecuteCMDCommand("ipconfig /all");
            return output;
        }

        #endregion

        #region Get IP Addresses
        public static string LocalIPAddress()
        {
            try
            {
                IPHostEntry host;
                string localIP = "";
                host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6)
                    {
                        localIP += ip.ToString() + Html.br;
                    }
                }
                return localIP;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }
        #endregion

        #region Detect Operating System architecture
        /// <summary>
        /// method to identify OS bits by searching for Wow6432Node registry
        /// The function returns the system architecture 64bit or 32bit
        /// </summary>
        /// <returns>
        /// The function returns string "x64" or "x86"; 
        /// </returns>
        public static string GetOperatingSystemArchitecture()
        {
            if (IntPtr.Size == 8)  // 64-bit programs run only on Win64
            {
                is64BitOperatingSystem = true;
                return "x64";
            }
            else  // 32-bit programs run on both 32-bit and 64-bit Windows
            {
                // Detect whether the current process is a 32-bit process running on a 64-bit system.
                bool flag;
                if ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out flag)) && flag)
                {
                    is64BitOperatingSystem = true;
                    return "x64";
                }
                else
                {
                    is64BitOperatingSystem = false;
                    return "x86";
                }
                //return ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out flag)) && flag) ? "x64" : "x86";
            }
        }

        /// <summary>
        /// The function determins whether a method exists in the export table of a certain module.
        /// </summary>
        /// <param name="moduleName">The name of the module</param>
        /// <param name="methodName">The name of the method</param>
        /// <returns>
        /// The function returns true if the method specified by methodName 
        /// exists in the export table of the module specified by moduleName.
        /// </returns>
        static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = GetModuleHandle(moduleName);
            if (moduleHandle == IntPtr.Zero)
            {
                return false;
            }
            return (GetProcAddress(moduleHandle, methodName) != IntPtr.Zero);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule,
            [MarshalAs(UnmanagedType.LPStr)]string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        #endregion

        #region Get information about system locale
        public static string GetOSLocaleInfo()
        {
            try
            {
                string culture = Thread.CurrentThread.CurrentUICulture.Name;
                NumberFormatInfo nfi = Thread.CurrentThread.CurrentUICulture.NumberFormat;

                return culture + ", decimal separator is \"" + nfi.NumberDecimalSeparator + "\" "
                    + "group separator is \"" + nfi.NumberGroupSeparator + "\", list separator is \""
                    + Thread.CurrentThread.CurrentUICulture.TextInfo.ListSeparator + "\"";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }
        #endregion

        #region Detect processor name
        public static string GetProcessorNameString()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
                return rk.GetValue("ProcessorNameString").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.Error(ex.ToString());
            }
        }
        #endregion

        #region Get memory information
        private static int GetTotalMemory(string type = "Physical")
        {
            try
            {
                Microsoft.VisualBasic.Devices.ComputerInfo ci = new Microsoft.VisualBasic.Devices.ComputerInfo();
                double totalMemory = (type == "Physical") ? ci.TotalPhysicalMemory : ci.TotalVirtualMemory;
                double inMb = totalMemory / (1024 * 1024);
                return ((int)Math.Ceiling(inMb));
            }
            catch (Exception ex)
            {
                Html.Error(ex.ToString());
                return 0;
            }
        }

        public static string GetMemoryInfo()
        {
            int physicalMemory = GetTotalMemory();
            int virtualMemory = GetTotalMemory("Virtual");
            string virtualMemoryInfo = String.Empty;
            if (GetOperatingSystemArchitecture() == "x86")
                virtualMemoryInfo = "(Virtual memory for process: " + virtualMemory + " Mb)";
            return physicalMemory + " Mb " + virtualMemoryInfo;
        }
                #endregion

        #region Is 3GB switch enabled
        /// <summary>
        /// Check if the virtual memory available to a process is > 2048
        /// </summary>
        /// <returns></returns>
        public static string Is3GBSwitchEnabled()
        {
            if (OSInfo.is64BitOperatingSystem)
            {
                return "Not applicable to 64-bit Operating Systems";
            }
            try
            {
                int virtualMemory = OSInfo.GetTotalMemory("Virtual");
                return (virtualMemory > 2048 && GetOperatingSystemArchitecture() == "x86") ? "Yes" : "No";
            }
            catch (Exception ex)
            {
                Log.Info(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Get information about monitors setup
        /// <summary>
        /// Screen setup information, resolution
        /// </summary>
        /// <returns></returns>
        public static string GetMonitorsInfo()
        {
            try
            {
                StringBuilder info = new StringBuilder(128);
                string monitor = " monitor";
                if (Screen.AllScreens.Length > 1)
                    monitor = " monitors";
                info.Append(Screen.AllScreens.Length + monitor + " detected. " + Html.br
                    + "Primary screen resolution: " + Screen.PrimaryScreen.Bounds.Width + "x" + Screen.PrimaryScreen.Bounds.Height + " pixels");

                return info.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
                #endregion

        #region Get Internet Explorer version
        /// <summary>
        /// Get version for Ineternet Explorer
        /// </summary>
        /// <returns></returns>
        public static string GetIEVersion()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Internet Explorer");
                return rk.GetValue("Version").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Get hardware information
        public static string GetHardDrivesInformation()
        {
            try
            {
                DriveInfo[] allDrives = DriveInfo.GetDrives();
                string driveInfo = null;

                foreach (DriveInfo d in allDrives)
                {
                    // get only the fixed drives i.e. no network drives, no cdroms
                    if (d.DriveType.ToString() == "Fixed")
                    {
                        driveInfo += Html.B("Drive: " + d.Name) + Html.br;
                        //driveInfo += "File type: " + d.DriveType + "<br />";

                        if (d.IsReady == true)
                        {
                            //driveInfo += "Volume label: " + d.VolumeLabel + "<br />";
                            //driveInfo += "File system: " + d.DriveFormat + "<br />";
                            //driveInfo += "Free space: " + d.AvailableFreeSpace + " bytes <br />" ;
                            driveInfo += "Total size of drive: " + (Convert.ToInt64(d.TotalSize) / (1024 * 1024)).ToString() + " Mb" + Html.br;
                            driveInfo += "Total available space: " + (Convert.ToInt64(d.TotalFreeSpace) / (1024 * 1024)).ToString() + " Mb" + Html.br;
                        }
                    }
                }

                return driveInfo;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
                #endregion

        #region Is Data Execution Prevention enabled
        public static string IsDepDisabled()
        {
            string s = null;
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_OperatingSystem");

                foreach (ManagementObject mo in searcher.Get())
                {
                    s = mo["DataExecutionPrevention_SupportPolicy"].ToString();
                }
            }
            catch (ManagementException me)
            {
                Log.Error("An error occurred while querying for WMI data: \n" + me.Message);
                return Html.ErrorMsg();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        
            switch (s)
            {
                case "0":
                    s = "DEP is disabled for all processes.\n(AlwaysOff)";
                    break;
                case "1":
                    s = "DEP is " + Html.B(Html.Error("enabled")) + " for all processes.\n(AlwaysOn)";
                    break;
                case "2":
                    s = "DEP is " + Html.B(Html.Error("enabled")) + " for only Windows system components and services. Default setting.\n(OptIn)";
                    break;
                case "3":
                    s = "DEP is " + Html.B(Html.Error("enabled")) + " for all processes. Administrators can manually create a list of specific applications which do not have DEP applied.\n(OptOut)";
                    break;
                default:
                    s = Html.Warning("DEP status unknown\n");
                    break;
            }

            return s;
        }
        #endregion

        #region Get default browser
        public static string GetDefaultBrowser()
        {
            string browser = string.Empty;
            RegistryKey key = null;
            try
            {
                key = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command");

                //trim off quotes
                browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
                if (!browser.EndsWith("exe"))
                {
                    //get rid of everything after the ".exe"
                    browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
                }
                return browser;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
            return Html.Warning("Default browser could not be detected!");
        }
        #endregion

        #region Is user in Admin group
        /// <summary>
        /// The function checks whether the primary access token of the process belongs 
        /// to user account that is a member of the local Administrators group, even if 
        /// it currently is not elevated.
        /// </summary>
        /// <returns>
        /// Returns true if the primary access token of the process belongs to user 
        /// account that is a member of the local Administrators group. Returns false 
        /// if the token does not.
        /// </returns>
        /// <exception cref="System.ComponentModel.Win32Exception">
        /// When any native Windows API call fails, the function throws a Win32Exception 
        /// with the last error code.
        /// </exception>
        public static bool IsUserInAdminGroup()
        {
            bool fInAdminGroup = false;
            SafeTokenHandle hToken = null;
            SafeTokenHandle hTokenToCheck = null;
            IntPtr pElevationType = IntPtr.Zero;
            IntPtr pLinkedToken = IntPtr.Zero;
            int cbSize = 0;

            try
            {
                // Open the access token of the current process for query and duplicate.
                if (!NativeMethods.OpenProcessToken(Process.GetCurrentProcess().Handle,
                    NativeMethods.TOKEN_QUERY | NativeMethods.TOKEN_DUPLICATE, out hToken))
                {
                    throw new Win32Exception();
                }

                // Determine whether system is running Windows Vista or later operating 
                // systems (major version >= 6) because they support linked tokens, but 
                // previous versions (major version < 6) do not.
                if (Environment.OSVersion.Version.Major >= 6)
                {
                    // Running Windows Vista or later (major version >= 6). 
                    // Determine token type: limited, elevated, or default. 

                    // Allocate a buffer for the elevation type information.
                    cbSize = sizeof(TOKEN_ELEVATION_TYPE);
                    pElevationType = Marshal.AllocHGlobal(cbSize);
                    if (pElevationType == IntPtr.Zero)
                    {
                        throw new Win32Exception();
                    }

                    // Retrieve token elevation type information.
                    if (!NativeMethods.GetTokenInformation(hToken,
                        TOKEN_INFORMATION_CLASS.TokenElevationType, pElevationType,
                        cbSize, out cbSize))
                    {
                        throw new Win32Exception();
                    }

                    // Marshal the TOKEN_ELEVATION_TYPE enum from native to .NET.
                    TOKEN_ELEVATION_TYPE elevType = (TOKEN_ELEVATION_TYPE)
                        Marshal.ReadInt32(pElevationType);

                    // If limited, get the linked elevated token for further check.
                    if (elevType == TOKEN_ELEVATION_TYPE.TokenElevationTypeLimited)
                    {
                        // Allocate a buffer for the linked token.
                        cbSize = IntPtr.Size;
                        pLinkedToken = Marshal.AllocHGlobal(cbSize);
                        if (pLinkedToken == IntPtr.Zero)
                        {
                            throw new Win32Exception();
                        }

                        // Get the linked token.
                        if (!NativeMethods.GetTokenInformation(hToken,
                            TOKEN_INFORMATION_CLASS.TokenLinkedToken, pLinkedToken,
                            cbSize, out cbSize))
                        {
                            throw new Win32Exception();
                        }

                        // Marshal the linked token value from native to .NET.
                        IntPtr hLinkedToken = Marshal.ReadIntPtr(pLinkedToken);
                        hTokenToCheck = new SafeTokenHandle(hLinkedToken);
                    }
                }

                // CheckTokenMembership requires an impersonation token. If we just got 
                // a linked token, it already is an impersonation token.  If we did not 
                // get a linked token, duplicate the original into an impersonation 
                // token for CheckTokenMembership.
                if (hTokenToCheck == null)
                {
                    if (!NativeMethods.DuplicateToken(hToken,
                        SECURITY_IMPERSONATION_LEVEL.SecurityIdentification,
                        out hTokenToCheck))
                    {
                        throw new Win32Exception();
                    }
                }

                // Check if the token to be checked contains admin SID.
                WindowsIdentity id = new WindowsIdentity(hTokenToCheck.DangerousGetHandle());
                WindowsPrincipal principal = new WindowsPrincipal(id);
                fInAdminGroup = principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            finally
            {
                // Centralized cleanup for all allocated resources. 
                if (hToken != null)
                {
                    hToken.Close();
                    hToken = null;
                }
                if (hTokenToCheck != null)
                {
                    hTokenToCheck.Close();
                    hTokenToCheck = null;
                }
                if (pElevationType != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pElevationType);
                    pElevationType = IntPtr.Zero;
                }
                if (pLinkedToken != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(pLinkedToken);
                    pLinkedToken = IntPtr.Zero;
                }
            }

            return fInAdminGroup;
        }
        #endregion

        #region Is User Account Control enabled
        public static string IsUACEnabled()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\policies\System");
                Object key = rk.GetValue("EnableLUA", null);
                if (key != null)
                {
                    return (key.ToString() == "1") ? Html.Error(Html.B("Enabled")) /*+ ". To run LoadRunner on Windows 7 or Window Server 2008, the User Account Control (UAC) must be disabled."*/ : "Disabled";
                }
                return "UAC is not supported for this OS";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Is the Operating System virtualized
        /// <summary>
        /// Method to check if the Operating System is installed on a Virtual Machine 
        /// Currently only 3 products could be discovered. (VMWare, Virtual Box or Microsoft Virtual PC or Hyper-V)
        /// We obtain the information by using Windows Management Instrumentation calls
        /// to the Win32_BaseBoard class and get the Manufacturer property which is the 
        /// name of the computer manufacturer. For VirtualBox it is "innotek GmbH", 
        /// for VirtualPC it starts with "Microsoft", for VMware it starts with "VMWare"
        /// </summary>
        public static string IsOSVirtualizedInfo()
        {
            try
            { 
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem");
                string manifacturer = null;

                foreach (ManagementObject mo in searcher.Get())
                {
                    manifacturer = mo.GetPropertyValue("Manufacturer").ToString(); 
                }

                if(manifacturer.StartsWith("innotek"))
                {
                    return "Yes (Virtual Box detected)";
                }
                else if (manifacturer.StartsWith("Microsoft"))
                {
                    return "Yes (Micorosft Virtualization detected)";
                }
                else if (manifacturer.StartsWith("VMware"))
                {
                    return "Yes (VMware detected)";
                }
                return "No (Not detected)";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Get environment variable
        public static string GetEnvVariable(string variable, bool system = false)
        {
            try
            {
                variable = system ? Environment.GetEnvironmentVariable(variable, EnvironmentVariableTarget.Machine) : Environment.GetEnvironmentVariable(variable);
                return (variable != null) ? Html.semicolon2br(variable) : Html.Warning("Not set!");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Layered Service Providers detection
        /// <summary>
        /// Method to check the number of installed Layared Service Providers
        /// </summary>
        public static int GetNumberOfInstalledLSPs()
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\WinSock2\Parameters\Protocol_Catalog9\Catalog_Entries");
                return rk.SubKeyCount;
            }
            catch (Exception ex)
            {
                Log.Error(Html.Error(ex.ToString()));
                return 0;
            }
        }


        /// <summary>
        /// Get the list of install level service providers 
        /// and put them into a <div></div> element
        /// </summary>
        /// <param name="id">the id of the wrapping element</param>
        /// <returns></returns>
        public static string GetInstalledLSPs(string id)
        {
            try
            {
                string output = Helper.ExecuteCMDCommand("netsh winsock show catalog");

                StringBuilder info = new StringBuilder(128);

                // split the output by \r\n
                char[] delimiter = new Char[] {'\t', '\r', '\n'};
                string[] parts = output.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                CultureInfo ci = CultureInfo.CurrentUICulture;
                string lang = ci.TwoLetterISOLanguageName;
                Log.Info("Detected system language: " + lang);
                //check for English, German or French syntax
                string description = String.Empty;
                string provider = String.Empty;
                switch (lang)
                {
                    case "fr":
                        description = "Description ";
                        provider = "Chemin d'accŠs fournisseur "; //Chemin d'accŠs fournisseur
                        break;
                    case "de":
                        description = "Beschreibung";
                        provider = "Anbieterpfad";
                        break;
                    case "es":
                        description = "Descripci¢n";
                        provider = "Ruta de proveedor";
                        break;
                    default:
                        description = "Description";
                        provider = "Provider Path";
                        break;
                }

                for (int i = 0; i < parts.Length; i++)
                {

                    if (parts[i].StartsWith(description))
                    {
                        info.Append("\t\t\t" + Html.B(parts[i].Substring(description.Length + 1)));
                    }
                    
                    if (parts[i].StartsWith(provider))
                    {
                        info.Append(" provided by " + parts[i].Substring(provider.Length + 1) + Html.br + "\n");
                    }
                }
                return "\n\t\t\t\t\t<div id=\"" + id + "\" class=\"dontShow\">\n" + info.ToString() + "\n\t\t\t\t\t</div>\n\t\t\t\t";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Is Windows firewall enabled

        /// <summary>
        /// Method to check if Windows firewall is enabled or not. 
        /// It checks the registry entry. 
        /// This registry is missing on Windows XP SP2 and earlier. 
        /// It is also missing on clean XP installations, where you haven't enabled/disabled the firewall
        /// </summary>
        /// <returns></returns>
        public static string IsWindowsFirewallEnabled()
        {
            try
            {
                string keyPath = @"SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile\";
                string firewallFlag = RegistryWrapper.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath, "EnableFirewall");
                // if the key doesn't exist we execute 'netsh firewall show opmode' cmd command
                if (firewallFlag == null || firewallFlag == "")
                {
                    string command = "netsh firewall show opmode";
                    Log.Info("Registry key 'EnableFirewall' doesn't exist in " + keyPath + " '" + command + "' command will be executed");
                    string output = Helper.ExecuteCMDCommand(command);
                    if (output.Contains("Standard profile configuration (current)"))
                    {
                        // split the output by \r\n
                        char[] delimiter = new Char[] { ' ', '\t', '\r', ':' };
                        string[] parts = output.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);

                        for (int i = 0; i < parts.Length; i++)
                        {
                            if (parts[i] == "(current)")
                            {
                                return parts[i + 5] + "d";
                            }
                        }
                    }
                    else
                        return Html.ErrorMsg();
                    return output;
                }
                return (firewallFlag == "1") ? "Yes" : "No";

            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Get list of running processes
        public static Process[] GetRunningProcesses()
        {
            try
            {
                Process[] processes = Process.GetProcesses();
                return processes;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
        }
        #endregion

        #region AppInit Information
        public static string GetAppInitDLLsInfo()
        {
            try
            {
                string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows\";
                StringBuilder entries = new StringBuilder(128);
                
                if (OSInfo.is64BitOperatingSystem)
                {
                    string entries64 = RegistryWrapper.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath, "AppInit_DLLs");
                    string entries32 = RegistryWrapper.GetRegKey32(RegHive.HKEY_LOCAL_MACHINE, keyPath, "AppInit_DLLs");

                    if (entries64 != null && entries64 != "")
                        entries.Append("x64 entries: " + entries64 + Html.br);
                    if (entries32 != null && entries32 != "")
                        entries.Append("x32 entries: " + entries32 + Html.br);
                }
                else
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyPath);
                    if (rk != null)
                        entries.Append(rk.GetValue("AppInit_DLLs").ToString());
                }
                return (entries.Length == 0) ? "No dll entries found" : entries.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }

        /// <summary>
        /// LoadAppInit_DLLs is only available in Windows Vista and later!
        /// </summary>
        /// <returns>Due to the above return null in case of Exception</returns>
        public static string GetLoadAppInitDLLsInfo()
        {
            try
            {
                string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Windows\";
                StringBuilder entries = new StringBuilder(128);
                
                if (OSInfo.is64BitOperatingSystem)
                {
                    string entries64 = RegistryWrapper.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath, "LoadAppInit_DLLs");
                    string entries32 = RegistryWrapper.GetRegKey32(RegHive.HKEY_LOCAL_MACHINE, keyPath, "LoadAppInit_DLLs");

                    if (entries64 != null)
                        entries.Append("x64 entry: " + entries64 + Html.br);
                    if (entries32 != null)
                        entries.Append("x32 entry: " + entries32 + Html.br);
                }
                else
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyPath);
                    string value = String.Empty;
                    if (rk != null)
                        value = rk.GetValue("LoadAppInit_DLLs").ToString();

                    value = (value == "1") ? "0x1 – AppInit_DLLs are enabled." : "0x0 – AppInit_DLLs are disabled.";
                    entries.Append(value);
                }
                return entries.Length > 0 ? entries.ToString() : Html.ErrorMsg();
            
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
        }
#endregion

        #endregion
    }
}
