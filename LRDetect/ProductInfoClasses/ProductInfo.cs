using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// registries
using Microsoft.Win32;
// reading the 64bit registries
using System.Runtime.InteropServices;
//FileInfo
using System.Diagnostics;
//File
using System.IO;
// isServiceInstalled
using System.ServiceProcess;
// hard drives info
using System.Management;
// hashtable
using System.Collections;
using System.Xml;
using System.Reflection;

namespace LRDetect
{
  public abstract class ProductInfo
  {
    protected abstract string UpgradeCode { get; }
    public string productCode;
    public abstract Dictionary<string, List<string>> Executables { get; }
    public List<String> logs;
    public bool IsInstalled = false;
    public string InstallLocation = String.Empty;
    string binLocation = null;
    public string BinFolder { get { if (binLocation == null) binLocation = Path.Combine(InstallLocation, "bin"); return binLocation; } }
    string configFolder = null;
    public string ConfigFolder { get { if (configFolder == null) configFolder = Path.Combine(InstallLocation, "config"); return configFolder; } }
    string datFolder = null;
    public string DatFolder { get { if (datFolder == null) datFolder = Path.Combine(InstallLocation, "dat"); return datFolder; } }
    string jreFolder = null;
    public string JreFolder { get {
        if (jreFolder == null)
          jreFolder = (version >= new Version (12,5)) ? Path.Combine(InstallLocation, @"lib\openjdk32\jre") : Path.Combine(InstallLocation, "jre");
        return jreFolder; } }

    string webControllerFolder = null;
    public string WebControllerFolder { get { if (webControllerFolder == null) webControllerFolder = Path.Combine(InstallLocation, "controller"); return webControllerFolder; } }

    public string InstallDate = String.Empty;
    // path to the product folder in registries
    protected abstract string ProductRegistryPath { get; }
    private string HKLMInstallerKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\";
    private string MSIInstallPropertiesRegPath = String.Empty;
    // PATCHES
    private string MSIPatchesRegPath = String.Empty;
    public string patchesInstalled = String.Empty;

    public string ProductName = String.Empty;
    public bool isVuGen = false;
    public bool isAnalysis = false;

    public string ProductVersion = String.Empty;
    public string ProductVersionFromInstaller = String.Empty;
    public Version version = new Version(0, 0);
    
    public string CustomComponentsInstalled = String.Empty;
    public string mainExecutableFilesInfo = String.Empty;
    protected virtual string[] environmentVarNames { get { return null; } }
    public string environmentVariables = String.Empty;
    protected virtual string[,] importantRegistryKeys { get { return null; } }
    public string ImportantRegKeyValues = String.Empty;
    //LOGS
    //Key is the Path, Value is the Log name
    public virtual List<LogFile> LogFiles { get { return null; } }
    public static List<LogFile> RegistrationFailureLogs 
    { 
      get 
      {
        var files = Directory.GetFiles(Environment.GetEnvironmentVariable("TEMP", EnvironmentVariableTarget.Machine), "HP*RegistrationFailure.log");
        List<LogFile> logs = new List<LogFile>();
        foreach (var file in files)
          logs.Add(new LogFile { name = Path.GetFileName(file), folder = Path.GetDirectoryName(file) });

        return logs;
      } 
    }
    //SETINGS
    public string correlationRulesSupport = String.Empty;
    public string correlationIgnoredContent = String.Empty;
    public string bbhookVersion = String.Empty;
    //isNew is true if VuGen is >= 11.50
    public bool isNew 
    {
      get { return version.CompareTo(new Version("11.50")) != -1; }
    }

    public ProductInfo()
    {
      var className = GetType().Name;
      Logger.Debug("Started " + className + "." + MethodBase.GetCurrentMethod().Name);

      // checks if a product is installed. If it is it should be listed 
      // in HKCR\Installer\*UpgradeCode*
      productCode = GetProductCode(UpgradeCode);

      if (productCode != null)
      {
        IsInstalled = true;
        MSIInstallPropertiesRegPath = HKLMInstallerKey + @"UserData\S-1-5-18\Products\" + productCode + @"\InstallProperties";
        MSIPatchesRegPath = HKLMInstallerKey + @"UserData\S-1-5-18\Products\" + productCode + @"\Patches\";
        InstallLocation = GetInstallLocation();
        InstallDate = GetInstallDate();
        ProductName = GetProductName();
        ProductVersion = GetProductVersion();
        ProductVersionFromInstaller = GetProductVersionFromInstaller();
        isVuGen = Executables[Executables.Keys.First()].Any(s => s.Contains("vugen.exe")) ? true : false;
        isAnalysis = Executables[Executables.Keys.First()].Any(s => s.Contains("AnalysisUI.exe")) ? true : false;
        patchesInstalled = GetPatchesInstalled();
        CustomComponentsInstalled = GetCustomComponentsInstalled();
        mainExecutableFilesInfo = GetExecutableFilesInfo();
        environmentVariables = GetEnvironmentVariables();
        //ImportantRegKeyValues = GetImportantRegKeyValues();
        correlationRulesSupport = GetCorrelationRulesSupport();
        correlationIgnoredContent = GetCorrelationIgnoredContent();
        bbhookVersion = GetBBHookVersion();
      }
      Logger.Debug("Finished " + className + "." + MethodBase.GetCurrentMethod().Name);
    }


    /// <summary>
    /// Method to get the software product code by its upgrade code
    /// </summary>
    /// <returns></returns>
    public static string GetProductCode(string upgradeCode)
    {
      try
      {
        string value = RegistryWrapper.GetFirstValueName(RegHive.ClassesRoot, @"Installer\UpgradeCodes\" + upgradeCode);
        if (Logger.level > 3)
        {
          if (value != null)
            Logger.Debug("Product code " + value + " matched upgrade code " + upgradeCode);
          else
            Logger.Debug("No product code " + value + " matched upgrade code " + upgradeCode);
        }
        return value;
      }
      catch (Exception ex)
      {
        Logger.Warn("Product code not found for the following product upgrade code: " + upgradeCode + "\n" + ex.ToString());
        return null;
      }
    }

    /// <summary>
    /// Method to get the product directory where the product is installed
    /// </summary>
    /// <returns></returns>
    private string GetInstallLocation()
    {
      string installLocation = RegistryWrapper.GetValue(RegHive.LocalMachine, MSIInstallPropertiesRegPath, "InstallLocation");
        return installLocation;
    }

        /// <summary>
        /// Method to get the installation date for the current product
        /// </summary>
        /// <returns>String date<returns>
        private string GetInstallDate()
        {
          //string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + this.ProductCode + @"\InstallProperties";
          string installDate = RegistryWrapper.GetValue(RegHive.LocalMachine, MSIInstallPropertiesRegPath, "InstallDate");
          return installDate;
        }

        /// <summary>
        /// Method to get the product name
        /// </summary>
        /// <returns>Product name<returns>
        private string GetProductName()
        {
          string displayName = RegistryWrapper.GetValue(RegHive.LocalMachine, MSIInstallPropertiesRegPath, "DisplayName");
          return (displayName != null) ? Html.B(displayName) : null;
        }

        /// <summary>
        /// Method to return the product version from Windows Add Remove Programs
        /// </summary>
        /// <returns></returns>
        public string GetProductVersionFromInstaller()
        {
            Logger.Info("Search for DisplayVersion key in registry " + MSIInstallPropertiesRegPath);
            string displayVersion = RegistryWrapper.GetValue(RegHive.LocalMachine, MSIInstallPropertiesRegPath, "DisplayVersion");
            Logger.Info(MSIInstallPropertiesRegPath + @"\DisplayVersion: " + displayVersion);
            return (displayVersion != null && displayVersion != "") ? displayVersion : Html.ErrorMsg();
        }

        /// <summary>
        /// Method to get the product version
        /// </summary>
        /// <returns>Product version<returns>
        private string GetProductVersion()
        {
          try
          {
            //example HKLM\SOFTWARE\Mercury Interactive\LoadRunner\CurrentVersion
            string path = this.ProductRegistryPath + "CurrentVersion";
            Logger.Info("Starting product version detection");
            Logger.Info("Registry path to search: " + path);
            string major = RegistryWrapper.GetValue(RegHive.LocalMachine, path, "Major");
            Logger.Info(path + @"\Major: " + major);
            string minor = RegistryWrapper.GetValue(RegHive.LocalMachine, path, "Minor", "00");
            Logger.Info(path + @"\Minor: " + minor);

            //if the version is not found in \Mercury Interactive\LoadRunner\CurrentVersion
            //get it from the Add Remove Programs
            if (major == null || major == "")
            {
              string versionFromInstaller = GetProductVersionFromInstaller();
              if (versionFromInstaller != null && versionFromInstaller != "")
              {
                // major would be something like 11.04.000 || 11.50.123
                if (versionFromInstaller.Contains('.'))
                {
                  string[] parts = versionFromInstaller.Split('.');
                  major = parts[0];
                  minor = parts[1];
                }
                else
                  major = versionFromInstaller;
              }
              else
              {
                Logger.Error("Major version not detected!");
                version = null;
                return Html.Error("Version NOT detected");
              }
            }
            // Set the version
            version = new Version(Convert.ToInt32(major), Convert.ToInt32(minor));
            // Return the version key
            return major + "." + minor;
          }
          catch (Exception ex)
          {
            Logger.Warn(ex.ToString());
            return null;
          }
        }

        public string GetProductNameVersionDateFormatted()
        {
          try
          {
            return Html.BoolToYesNo(IsInstalled) + " " + ProductName + " " + ProductVersion + Helper.ConvertInstallDate(InstallDate);
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return Html.ErrorMsg();
          }
        }

        /// <summary>
        /// Method to get the GUID codes of all patches installed
        /// </summary>
        /// <returns>Array of GUIDs<returns>
        /// <seealso cref="getPatchesInstalled()">
        /// See also the getPatchesInstalled method </seealso>
        private List<string> GetPatchesCodes()
        {
          try
          {
            List<string> patchesCodes = new List<string>();
            string keyPath = @"Installer\Products\" + productCode + @"\Patches";
            patchesCodes = RegistryWrapper.GetValueNames(RegHive.ClassesRoot, keyPath);
            return patchesCodes;
          }
          catch (Exception ex)
          {
            Logger.Warn(ex.ToString());
            return null;
          }
        }

        /// <summary>
        /// Method to get a HTML formated list of all patches installed for current product
        /// </summary>
        /// <returns> HTML formated string with all patches installed <returns>
        /// <seealso cref="getPatchesCodes()">
        /// See also the getPatchesCodes method </seealso>
        private string GetPatchesInstalled()
        {
          try
          {
            //Stopwatch stopWatch = Stopwatch.StartNew();
            List<string> patchesCodes = GetPatchesCodes();
            StringBuilder patchesInstalled = new StringBuilder();

            if (patchesCodes != null)
            {
              foreach (string patchCode in patchesCodes)
              {
                //TODO this check might be unnecessarry 
                if (patchCode != "Patches")
                {
                  string keyPath = MSIPatchesRegPath + patchCode;
                  string dateInstalled = Helper.ConvertInstallDate(RegistryWrapper.GetValue(RegHive.LocalMachine, keyPath, "Installed"));
                  string displayName = RegistryWrapper.GetValue(RegHive.LocalMachine, keyPath, "DisplayName");
                            
                  patchesInstalled.Append(Html.B(displayName) + " " + dateInstalled + Html.br);
                }
              }
              return patchesInstalled.ToString();
            }
            return null;
          }
          catch (Exception ex)
          {
              Logger.Warn(ex.ToString());
              return null;
          }
        }

        private string GetCustomComponentsInstalled()
        {
            StringBuilder intalledComponents = new StringBuilder(1024);
            try
            {
                string keyPath = this.ProductRegistryPath + @"CustComponent\";
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyPath);

                foreach (string subKeyName in rk.GetSubKeyNames())
                {
                    RegistryKey subKey = rk.OpenSubKey(subKeyName + @"\CurrentVersion");
                    if (subKey != null)
                    {
                        intalledComponents.Append(Html.B(subKeyName) + " " + subKey.GetValue("Major").ToString() + "." + subKey.GetValue("Minor").ToString() + Html.br);
                    }
                }
                return intalledComponents.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
                return null;
            }
        }

        private string GetEnvironmentVariables()
        {
          try
          {
            string EnvironmentVariables = String.Empty;
            foreach (string envVar in environmentVarNames)
            {
                EnvironmentVariables += Html.B(envVar + " = ") + OSCollectorHelper.GetEnvVariable(envVar) + Html.br;
            }
            return EnvironmentVariables;
          }
          catch (Exception ex)
          {
            Logger.Warn(ex.ToString());
            return "None";
          }
        }

        /// <summary>
        /// Wrapper of the File.Exists method. Executed only if the product is installed.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool isFileExistInInstallDir(string file)
        {
            if (InstallLocation != null)
            {
                string path = InstallLocation + file;
                if (File.Exists(path))
                    return true;
            }
            return false;
        }

        public string GetExecutableFilesInfo(string ver = "")
        {
          try
          {
            StringBuilder output = new StringBuilder();
            List<string> currentExes = null;

            ver = "12.0x";
            if (ProductVersion != null || ProductVersion != "")
              ver = ProductVersion;
            else
              ver = "12.0x";

            // if we can't find the version match, we will use the latest one
            if (!Executables.TryGetValue(ver, out currentExes))
            {
              //if no exact version is found, replace the last digit with an x
              ver = ver.Remove(ver.Length - 1, 1) + "x";
              // try to find something like 11.5x. If not found return the latest version
              if (!Executables.TryGetValue(ver, out currentExes))
                currentExes = Executables[Executables.Keys.First()];
            }

            string filePath = String.Empty;
            foreach (string currentExe in currentExes)
            {
              var dir = BinFolder;
              if (currentExe.Contains(@"\"))
                dir = InstallLocation;
              filePath = Path.Combine(dir, currentExe);
              if (File.Exists(filePath))
              {
                var fileInfo = Helper.GetFileInfo(filePath);
                string versionInfo = Html.Small(String.Format("Version: {0} last modified on: {1} {2} {3} {4}", fileInfo["Version"].ToString(), fileInfo["ModifiedOn"], IsFileLargeAddressAware(filePath), Html.br, Html.hr));
                output.Append(filePath + Html.br + versionInfo);
              }
              else
              {
                //if file is not found check if we are searching for firefox.exe which is not available in earlier versions
                //CopareTo will return -1 if vugen is ealier than 11 so anything greated than -1 means it's the a version
                if (filePath.Contains("firefox") /*&& version.CompareTo(new Version("11.0")) > -1*/)
                  output.Append(Html.Error(String.Format("File not found: {0} {1}", Html.br, filePath)) + Html.br + Html.hr);
              }
            }

            return output.ToString();
          }
          catch (Exception ex)
          {
            return ex.ToString();
          }
        }

        #region  Is File LARGEADDRESSAWARE
        protected string IsFileLargeAddressAware(string file)
        {
            try
            {
                Stream stream = File.OpenRead(file);
                return IsFileLargeAddressAware(stream) ? " the file is LARGEADDERSSAWARE" : "the file is " + Html.B("not") + " LARGEADDERSSAWARE";
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return "Could not define";
            }
        }

        /// <summary>
        /// Checks if the stream is a MZ header and if it is large address aware
        /// </summary>
        /// <param name="stream">Stream to check, make sure its at the start of the MZ header</param>
        /// <exception cref=""></exception>
        /// <returns></returns>
        private bool IsFileLargeAddressAware(Stream stream)
        {
            try
            {
                const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x20;

                var br = new BinaryReader(stream);

                if (br.ReadInt16() != 0x5A4D)       //No MZ Header
                    return false;

                br.BaseStream.Position = 0x3C;
                var peloc = br.ReadInt32();         //Get the PE header location.

                br.BaseStream.Position = peloc;
                if (br.ReadInt32() != 0x4550)       //No PE header
                    return false;

                br.BaseStream.Position += 0x12;
                return (br.ReadInt16() & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE;

            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
                return false;
            }
        }
        #endregion

        #region Get some registry settings
        /// <summary>
        /// Gets the value of some registry keys
        /// </summary>
        /// <exception cref=""></exception>
        /// <returns></returns>
        /*private string GetImportantRegKeyValues()
        {
            try
            {
                StringBuilder importantKeys = new StringBuilder(1024);
                //check if we have important registry keys for this product
                if (this.importantRegistryKeys != null)
                {
                    for (int i = this.importantRegistryKeys.GetLowerBound(0); i <= this.importantRegistryKeys.GetUpperBound(0); i++)
                    {
                        string inHive = this.importantRegistryKeys[i, 0];
                        string keyPath = this.importantRegistryKeys[i, 1];
                        string keyName = this.importantRegistryKeys[i, 2];
                        string keyValue = null;

                        try
                        {
                            RegistryKey rk = (inHive == "RegHive.HKEY_CURRENT_USER") ? Registry.CurrentUser.OpenSubKey(keyPath) : Registry.LocalMachine.OpenSubKey(keyPath);
                            keyValue = rk.GetValue(keyName).ToString();
                            importantKeys.Append(keyName + " = " + keyValue + Html.br);
                            //keyValue = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath, keyName);
                        }
                        catch (NullReferenceException)
                        {
                            importantKeys.Append(keyName + " = " + Html.Error("Not found!") + Html.br);
                        }
                    }
                }
                return importantKeys.ToString();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex.ToString());
                return ex.Message;
            } 
        }*/

        #endregion

        #region Correlation settings
        /// Settings
        /// Recording engine check
        private string GetCorrelationRulesSupport()
        {
            try
            {
                StringBuilder output = new StringBuilder();
                IniParser ini = new IniParser(this.InstallLocation + @"\dat\protocols\QTWeb.lrp");
                output.Append(ini.GetSetting("Vugen", "CorrelationRulesSupport"));
                return output.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return Html.Error(ex.Message);
            }
        }

        /// <summary>
        /// Method to parse the content of the config\IgnoredContent.xml
        /// Anything that is listed here will not be parsed for correlations
        /// </summary>
        /// <returns>A list of content types that are not scanned for correlations</returns>
        private string GetCorrelationIgnoredContent()
        {
            try
            {
                if (isVuGen)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(InstallLocation + @"config\IgnoredContent.xml");

                    XmlNodeList nodes = xmlDoc.GetElementsByTagName("string");
                    List<String> content = new List<string>();
                    for (int i = 0; i < nodes.Count; i++)
                        content.Add('"' + nodes[i].InnerText + '"');

                    string output = String.Join(", ", content.ToArray());
                    return output;
                }
                return "";
            }
            catch (FileNotFoundException )
            {
                Logger.Error(InstallLocation + @"content\IgnoredContent.xml not found");
                return Html.Error(InstallLocation + @"\content\IgnoredContent.xml not found");
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return Html.Error(ex.Message);
            }
        }
        #endregion

        #region Get BBHOOK.DLL version, size and modification date
        private string GetBBHookVersion()
        {
            try
            {
                StringBuilder output = new StringBuilder();

                if (isVuGen)
                {
                  var fileInfo = Helper.GetFileInfo(Path.Combine(BinFolder, "bbhook.dll"));
                    output.Append("bbhook.dll - version: " + fileInfo["Version"] + ", " + fileInfo["Size"] + " bytes" + Html.br);
                    if (OSCollectorHelper.is64BitOperatingSystem && this.isNew)
                    {
                      fileInfo = Helper.GetFileInfo(Path.Combine(BinFolder, "bbhook_x64.dll"));
                        output.Append("bbhook_x64.dll - version: " + fileInfo["Version"] + ", " + fileInfo["Size"] + " bytes");
                    }
                    return output.ToString(); 
                }
                return null;
            }
            catch (FileNotFoundException fnfe)
            {
              Logger.Error(fnfe.ToString());
              return Html.Error("bbhook library not found in " + BinFolder);
            }
            catch (Exception ex)
            {
              Logger.Error(ex.ToString());
              return Html.Error(ex.Message);
            }
        }
        #endregion
    }

    public class LogFile 
    {
      public string folder = "";
      public string name = "";
      public string fullPath { get { return Path.Combine(folder, name); } }
      public override string ToString()
      {
        return Path.Combine(folder, name);
      }
    }
}
