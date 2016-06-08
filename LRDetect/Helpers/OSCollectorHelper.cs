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
using Microsoft.VisualBasic.Devices;
using System.ServiceProcess;

#endregion

namespace LRDetect
{
  public class OSCollectorHelper
  {
    #region Constructor
      static OSCollectorHelper()
      {
        // LRDetect is a 32-bit programs running. Detect whether the current process is running on a 64-bit system.
        bitsInt = ((DoesWin32MethodExist("kernel32.dll", "IsWow64Process") && IsWow64Process(GetCurrentProcess(), out is64BitOperatingSystem)) && is64BitOperatingSystem) ? 64 : 32;
      }
    #endregion


    #region Members
      static ComputerInfo ci = new ComputerInfo();
      static string bitString { get { return "x" + ((bitsInt == 64) ? "64" : "86"); } } // returns x64 or x86
      static int bitsInt = 64;
      public static bool is64BitOperatingSystem = true;
      public static string language = Thread.CurrentThread.CurrentUICulture.DisplayName;

      static int spInt 
      {
        get 
        {
          int sp = 0;
          try 
          {
            sp = Convert.ToInt32(Environment.OSVersion.ServicePack.Substring(Environment.OSVersion.ServicePack.Length - 1, 1));
          }
          catch (Exception ex) 
          { 
            Logger.Error(ex.ToString()); 
          }
          return sp;
        }
      }

      #region Is Windows Server
      //https://msdn.microsoft.com/en-us/library/windows/desktop/bb773795%28v=vs.85%29.aspx
      //Checks for specified operating systems and operating system features.
      //OS_ANYSERVER 29 - The program is running on any Windows Server product.
      [DllImport("shlwapi.dll", SetLastError = true, EntryPoint = "#437")]
      static extern bool IsOS(int os);
      public static bool IsWindowsServer
      {
        get { return IsOS(29); }
      }

      #endregion
    #endregion

    #region Methods

    #region Get OS name
      /// <summary>
      /// Get Operating system information.
      /// </summary>
      /// <returns></returns>
      public static string GetOSFullNameFormatted()
      {
        try
        {
          return String.Format("{0} {1} {2} ({3})", ci.OSFullName, Environment.OSVersion.ServicePack, bitString, ci.OSVersion);
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.Message;
        }
      }        
    #endregion

      #region Get OS name
      /// <summary>
      /// Get Operating system root folder.
      /// </summary>
      /// <returns></returns>
      public static string GetOSRootDir()
      {
        try
        {
          return Environment.GetEnvironmentVariable("SystemRoot");
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.Message;
        }
      }
      #endregion

    #region IS OS Supported version
    internal static string IsOSSupportedInfo()
    {
      try
      {
        var name = OSCollectorHelper.GetOSFullNameFormatted();
        if (ProductDetection.Vugen.IsInstalled)
        {
          Version osVersion = Environment.OSVersion.Version;
          string osString = String.Format("{0}.{1}.{2}", osVersion.Major.ToString(), osVersion.Minor.ToString(), osVersion.Build.ToString());
          OS hostOS = new OS(osString, spInt, bitsInt);
          Logger.Debug(String.Format("hostOs: OSString:{0}, SP:{1}, Bits:{2}", osString, spInt, bitsInt));
          // Get supported OSes for current product version
          List<OS> supportedOsList = OS.GetSupportedOSForProductVersion(ProductDetection.Vugen.ProductVersion);

          OS supportedOs = OS.FindSupportedOs(hostOS, supportedOsList);

          if (supportedOs != null)
            return supportedOs.recommended ? Html.cYes : Html.Yes;
          else
            return Html.cNo;// "Unknown product version (You might be using an old LRDetect version).";
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return ex.Message;
      }
      return Html.cNo;
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
        IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
        StringBuilder localIP = new StringBuilder();
          
        foreach (IPAddress ip in host.AddressList)
        {
          if (ip.AddressFamily == AddressFamily.InterNetwork || ip.AddressFamily == AddressFamily.InterNetworkV6)
            localIP.Append(ip.ToString() + Html.br);
        }
        return localIP.ToString();
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return ex.Message;
      }
    }
    #endregion

    #region Netowrk Cards Info
    public static string GetNetworkCardsInfo()
    {
      StringBuilder output = new StringBuilder(128);

      // split the output by \r\n
      string[] rows = OSCollectorHelper.SystemInfo.Split(new Char[] { '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);
      int startingRow = 0;
      int i = 0;

      foreach (string row in rows)
      {
        if (row.Contains("Network Card"))
        {
          startingRow = i + 1;
          output.Append(row.Replace("Network Card(s): ", "") + " ");
          break;
        }
        i++;
      }

      StringBuilder updates = new StringBuilder();
      if (startingRow != 0)
      {
        for (i = startingRow; i < rows.Length; i++)
        {
          if (rows[i].Replace("                           ", "").StartsWith("["))
            updates.Append(Html.B(rows[i]) + Html.br);
          else
            updates.Append("&nbsp;&nbsp;&nbsp;&nbsp;" + rows[i] + Html.br);
        }
        output.Append(Html.AddLinkToHiddenContent(updates.ToString()));
      }
      return output.ToString();
    
    }
    #endregion

    #region Detect Operating System architecture
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
        return false;
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
        var culture = Thread.CurrentThread.CurrentUICulture;
        NumberFormatInfo nfi = culture.NumberFormat;

        return String.Format("{0}, decimal separator is \"{1}\" group separator is \"{2}\", list separator is \"{3}\" {4}", culture.Name, nfi.NumberDecimalSeparator, nfi.NumberGroupSeparator, culture.TextInfo.ListSeparator, Html.AddLinkToHiddenContent(GetOSLocaleDetails()));
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return ex.Message;
      }
    }

    static string GetOSLocaleDetails()
    {
      StringBuilder output = new StringBuilder();
      try
      {
        var culture = Thread.CurrentThread.CurrentUICulture;
        output.Append("Culture Name: " + culture.Name + Html.br);
        output.Append("Currency Symbol: " + culture.NumberFormat.CurrencySymbol + Html.br);
        output.Append("Currency Decimal Digits: " + culture.NumberFormat.CurrencyDecimalDigits + Html.br);
        output.Append("Currency Decimal Separator: " + culture.NumberFormat.CurrencyDecimalSeparator + Html.br);
        output.Append("Currency Group Separator: " + culture.NumberFormat.CurrencyGroupSeparator + Html.br);
        output.Append("Digit Substitution: " + culture.NumberFormat.DigitSubstitution + Html.br);
        output.Append("Negative Sign: " + culture.NumberFormat.NegativeSign + Html.br);
        output.Append("Number Decimal Digits: " + culture.NumberFormat.NumberDecimalDigits + Html.br);
        output.Append("Number Decimal Separator: " + culture.NumberFormat.NumberDecimalSeparator + Html.br);
        output.Append("Number Group Separator: " + culture.NumberFormat.NumberGroupSeparator + Html.br);

        return output.ToString();
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
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
        Logger.Error(ex.ToString());
        return Html.Error(ex.ToString());
      }
    }
    #endregion

    #region Get memory information
    private static int GetTotalMemory(string type = "Physical")
    {
      try
      {
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
        if (!is64BitOperatingSystem)
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
      if (is64BitOperatingSystem)
      {
        return "Not applicable to 64-bit Operating Systems";
      }
      try
      {
        int virtualMemory = OSCollectorHelper.GetTotalMemory("Virtual");
        return (virtualMemory > 2048) ? "Yes" : "No";
      }
      catch (Exception ex)
      {
        Logger.Info(ex.ToString());
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
            Logger.Error(ex.ToString());
            return Html.ErrorMsg();
        }
    }
            #endregion

    #region Get Internet Explorer version

    public static string GetIEVersion()
    {
      return IEVersion != null ? IEVersion.ToString() : Html.ErrorMsg(); 
    }

    private static Version ieVersion = null;
    /// <summary>
    /// Get version for Ineternet Explorer
    /// </summary>
    /// <returns></returns>
    public static Version IEVersion
    {
      //Logger.Debug("Starting OSInfo.GetIEVersion()");
      get
      {
        if (ieVersion == null)
        {
          try
          {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Internet Explorer");
            var verKey = rk.GetValue("svcVersion");
            var v = verKey == null ? rk.GetValue("Version").ToString() : verKey.ToString();
            ieVersion = new Version(v);
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
          }
        }
        return ieVersion;

        //finally
        //{
        //  Logger.Debug("Finished OSInfo.GetIEVersion()");
        //}
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
            Logger.Error(ex.ToString());
            return Html.ErrorMsg();
        }
    }
            #endregion

    #region Is Data Execution Prevention enabled

    public static bool IsDEPEnabled()
    {
      return DepLevel == 0 ? false : true;
    }

    static int _depLevel = -1;
    public static int DepLevel { 
      get {
        // if depLevel is already set return the value
        if (_depLevel > -1)
          return _depLevel;
        // otherwise get it from the WMI
        else 
        {
          try
          {
            _depLevel = Convert.ToInt16(Helper.QueryWMI("DataExecutionPrevention_SupportPolicy", "root\\CIMV2", "Win32_OperatingSystem"));
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
          }
          return _depLevel;
        }
      } 
    }

    public static string DepInfo()
    {
      switch (DepLevel)
      {
        case 0:
          return "DEP is disabled for all processes.\n(AlwaysOff)";
        case 1:
          return "DEP is " + Html.B(Html.Error("enabled")) + " for all processes.\n(AlwaysOn)";
        case 2:
          return "DEP is " + Html.B(Html.Error("enabled")) + " for only Windows system components and services. Default setting.\n(OptIn)";
        case 3:
          return "DEP is " + Html.B(Html.Error("enabled")) + " for all processes. Administrators can manually create a list of specific applications which do not have DEP applied.\n(OptOut)";
        default:
          return Html.Warning("DEP status unknown\n");
      }
    }
    #endregion

    #region Get default browser
    public static string GetDefaultBrowser()
    {
      Logger.Debug("Starting OSInfo.GetDefaultBrowser()");
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
        Logger.Error(ex.ToString());
      }
      Logger.Debug("Finished OSInfo.GetDefaultBrowser()");
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

    static int _uacLevel = -1;
    public static string uacKeyName = "EnableLUA";
    public static int UACLevel
    {
      get
      {
        // Do not check if the value is already by previous queries
        if (_uacLevel > -1)
          return _uacLevel;
        else
        {
          try
          {
            var rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\policies\System").GetValue(uacKeyName, null);
            if (rk != null)
              _uacLevel = Convert.ToInt16(rk.ToString());
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
          }
          return _uacLevel;
        }
      }
    }

    public static string UACInfo()
    {
      switch (UACLevel)
      { 
        case 0:
          return "Disabled";
        case 1:
          return Html.Error(Html.B("Enabled"));
        default:
          return "UAC is not supported for this OS";
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
        //ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystem");
        //string manifacturer = null;

        string manifacturer = Helper.QueryWMI("Manufacturer", "root\\CIMV2", "Win32_ComputerSystem");

        //foreach (ManagementObject mo in searcher.Get())
        //{
        //  manifacturer = mo.GetPropertyValue("Manufacturer").ToString(); 
        //}

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
        Logger.Error(ex.ToString());
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
            return (variable != null) ? Html.Semicolon2br(variable) : Html.Warning("Not set!");
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return Html.ErrorMsg();
        }
    }

    public static String GetEnvVariables()
    {
      try
      {
        var output = new StringBuilder();
        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
          output.Append(Html.B(env.Key.ToString()) + "=" + env.Value.ToString() + Html.br);
        return output.ToString();
      }
      catch (Exception ex)
      {
        return ex.ToString();
      }
    }
    public static string GetUsrEnvVariables()
    {
      try
      {
        var output = new StringBuilder();
        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
          output.Append(Html.B(env.Key.ToString()) + "=" + env.Value.ToString() + Html.br);
        return output.ToString();
      }
      catch (Exception ex)
      {
        return ex.ToString();
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
            Logger.Error(Html.Error(ex.ToString()));
            return 0;
        }
    }


    /// <summary>
    /// Get the list of install level service providers 
    /// and put them into a <div></div> element
    /// </summary>
    /// <param name="id">the id of the wrapping element</param>
    /// <returns></returns>
    public static string GetInstalledLSPs()
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
            Logger.Info("Detected system language: " + lang);
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
            return info.ToString();
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
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
        string firewallFlag = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, keyPath, "EnableFirewall");
        // if the key doesn't exist we execute 'netsh firewall show opmode' cmd command
        if (firewallFlag == null || firewallFlag == "")
        {
          string command = "netsh firewall show opmode";
          Logger.Info("Registry key 'EnableFirewall' doesn't exist in " + keyPath + " '" + command + "' command will be executed");
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
          Logger.Error(ex.ToString());
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
            Logger.Error(ex.ToString());
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
                
            if (is64BitOperatingSystem)
            {
                string entries64 = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, keyPath, "AppInit_DLLs");
                string entries32 = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, keyPath, "AppInit_DLLs");

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
            Logger.Error(ex.ToString());
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
                
            if (is64BitOperatingSystem)
            {
                string entries64 = RegistryWrapper.GetRegKey64(RegHive.LocalMachine, keyPath, "LoadAppInit_DLLs");
                string entries32 = RegistryWrapper.GetRegKey32(RegHive.LocalMachine, keyPath, "LoadAppInit_DLLs");

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
            Logger.Error(ex.ToString());
            return null;
        }
    }
#endregion

    #region Get Kerberos Configuration

      public static string GetKerberosConfiguration()
      {
        StringBuilder krb5Content = new StringBuilder();
        // Get the KRB5_CONFIG environment variable
        string krb5File = OSCollectorHelper.GetEnvVariable("KRB5_CONFIG");
        // If KRB5_CONFIG is not set
        if (krb5File.Contains("Not set"))
        {
          // Sometimes KRB5_CONFIG is not set because the krb5.ini is placed in C:\Windows
          Logger.Info("No KRB5_CONFIG eng. variable found. Searching for the file in C:\\Windows");
          if (File.Exists(@"C:\Windows\krb5.ini"))
            krb5File = @"C:\Windows\krb5.ini";
          else
            return "Not detected";
        }
        else
        {
          krb5Content.Append(Html.B("KRB5_CONFIG = ") + krb5File + Html.br);
        }
        // Check if KRB5_TRACE exists and if it does add it's value
        var krb5trace = OSCollectorHelper.GetEnvVariable("KRB5_TRACE");
        if (!krb5trace.Contains("Not set"))
          krb5Content.Append(Html.B("KRB5_TRACE = ") + krb5trace + Html.br + Html.br);

        // Get the content of krb5.ini
        Logger.Info("Getting the content from " + krb5File);
        krb5Content.Append("krb5.ini " + Html.AddLinkToHiddenContent(IniParser.ToHtml(krb5File)));

        return krb5Content.ToString();
      }
    #endregion 

    #region Execute systeminfo
      private static string systemInfo = null;
      public static string SystemInfo 
      { 
        get 
        {
          if (systemInfo == null)
            systemInfo = Helper.ExecuteCMDCommand("systeminfo");
          return systemInfo;
        } 
      }
    #endregion

    #endregion



      internal static string IsSecondaryLogonEnabledInfo()
      {
        string name = "seclogon";
        string details = ", seclogon service ";
        try
        {
          ServiceController sc = new ServiceController(name);

          switch (sc.Status)
          {
            case ServiceControllerStatus.Running:
              return Html.Yes;
            case ServiceControllerStatus.Stopped:
              return Html.Yes + details + " is stopped";
            case ServiceControllerStatus.Paused:
              return Html.cNo + details + " is paused";
            case ServiceControllerStatus.StopPending:
              return Html.cNo + details + " is stopping";
            case ServiceControllerStatus.StartPending:
              return Html.Yes + details + " is starting";
            default:
              return Html.Yes + details + " status is changing";
          }
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.cNo + ", " + ex.Message;
        }
      }
  }
}
