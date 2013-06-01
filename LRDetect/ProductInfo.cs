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

namespace LRDetect
{
    abstract class ProductInfo
    {
        protected abstract string UpgradeCode { get; }
        private string productCode;
        protected abstract string[] ExecutableFiles { get; }
        //protected virtual string agentServiceCaption { get { return null; } }
        //protected virtual string agentServiceName { get { return null; } }
        //protected virtual string agentProcessName { get { return null; } }
        //private int agentProcessId = 0;
        //protected virtual string agentProcessCaption { get { return null; } }
        //public string lrAgentDetails = String.Empty;
        public bool IsProductInstalled = false;
        public string InstallLocation = String.Empty;
        public string InstallDate = String.Empty;
        // path to the product folder in registries
        protected abstract string ProductRegistryPath { get; }
        private string HKLMInstallerKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\";
        private string MSIInstallPropertiesRegPath = String.Empty;
        // PATCHES
        private string MSIPatchesRegPath = String.Empty;
        public string patchesInstalled = String.Empty;
        // setting the name to "no name" to prevent comparing with null values
        public virtual string[] LatestPatchNames { get { return null; } }
        public string latestPatchName = String.Empty;
        public virtual string[] LatestPatchURLs { get { return null; } }
        public string latestPatchURL = "http://support.openview.hp.com/selfsolve/patches";
        
        public bool isLatestPatchInstalled = false;
        public string latestPatchInfo = String.Empty;

        public string ProductName = String.Empty;
        public bool isVuGen = false;
        public bool isAnalysis = false;
        public string ProductVersion = String.Empty;
        public Version version = null;
        public string CustomComponentsInstalled = String.Empty;
        public string mainExecutableFilesInfo = String.Empty;
        protected virtual string[] environmentVarNames { get { return null; } }
        public string environmentVariables = String.Empty;
        protected virtual string[,] importantRegistryKeys { get { return null; } }
        public string ImportantRegKeyValues = String.Empty;
        //SETINGS
        public string correlationRulesSupport = String.Empty;
        public string correlationIgnoredContent = String.Empty;
        public string bbhookVersion = String.Empty;

        public ProductInfo()
        {
            // checks if a product is installed. If it is it should be listed 
            // in HKCR\Installer\~UpgradeCode~
            this.productCode = GetProductCode(this.UpgradeCode);

            if (this.productCode != null)
            {
                this.MSIInstallPropertiesRegPath = HKLMInstallerKey + @"UserData\S-1-5-18\Products\" + this.productCode + @"\InstallProperties";
                this.MSIPatchesRegPath = HKLMInstallerKey + @"UserData\S-1-5-18\Products\" + this.productCode + @"\Patches\";
                this.InstallLocation = this.GetInstallLocation();
                this.IsProductInstalled = this.isFileExist(this.ExecutableFiles[0]);
                this.InstallDate = this.GetInstallDate();
                this.ProductName = this.GetProductName();
                this.isVuGen = Array.Find(this.ExecutableFiles, s => s.Contains("vugen.exe")) != null ? true : false;
                this.isAnalysis = Array.Find(this.ExecutableFiles, s => s.Contains("AnalysisUI.exe")) != null ? true : false;
                this.ProductVersion = this.GetProductVersion();
                this.patchesInstalled = this.GetPatchesInstalled();
                this.latestPatchInfo = this.GetLatestPatchInfo();
                this.CustomComponentsInstalled = this.GetCustomComponentsInstalled();
                this.mainExecutableFilesInfo = this.GetExecutableFilesInfo();
                this.environmentVariables = this.GetEnvironmentVariables();
                this.ImportantRegKeyValues = this.GetImportantRegKeyValues();
                this.correlationRulesSupport = this.GetCorrelationRulesSupport();
                this.correlationIgnoredContent = this.GetCorrelationIgnoredContent();
                this.bbhookVersion = this.GetBBHookVersion();
            }
        }

        /// <summary>
        /// Method to get the software product code by its upgrade code
        /// </summary>
        /// <returns></returns>
        public static string GetProductCode(string upgradeCode)
        {
            try
            {
                string value = RegistryWrapper.GetFirstValueName(RegHive.HKEY_CLASSES_ROOT, @"Installer\UpgradeCodes\" + upgradeCode);
                //Helper.Log("Product Code: " + value);
                return value;
            }
            catch (NullReferenceException)
            {
                Log.Warn("Product with UpgradeGUID " + upgradeCode + " was not detected");
                return null;
            }
            catch (Exception ex)
            {
                Log.Warn("Product code not found for the following product upgrade code: " + upgradeCode + "\n" + ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Method to get the product directory where the product is installed
        /// </summary>
        /// <returns></returns>
        private string GetInstallLocation()
        {
            try
            {
                //string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + this.ProductCode + @"\InstallProperties";
                string installLocation = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, this.MSIInstallPropertiesRegPath, "InstallLocation");
                return installLocation;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Method to get the product directory where the product is installed
        /// </summary>
        /// <returns></returns>
        //public static string GetInstallLocation(string productCode)
        //{
        //    try
        //    {
        //        string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCode + @"\InstallProperties";
        //        string installLocation = RegistryWrapper.FindRegValue(RegHive.HKEY_LOCAL_MACHINE, registryPath, "InstallLocation");
        //        //RegistryKey rk = Registry.LocalMachine.OpenSubKey(registryPath);
        //        return installLocation;
        //    }
        //    catch (Exception ex)
        //    {
        //        Helper.Log(ex.ToString(), 1);
        //        return null;
        //    }
        //}

        /// <summary>
        /// Method to get the installation date for the current product
        /// </summary>
        /// <returns>String date<returns>
        private string GetInstallDate()
        {
            try
            {
                //string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + this.ProductCode + @"\InstallProperties";
                string installDate = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, this.MSIInstallPropertiesRegPath, "InstallDate");
                return Helper.ConvertInstallDate(installDate);
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// Method to get the product name
        /// </summary>
        /// <returns>Product name<returns>
        private string GetProductName()
        {
            try
            {
                string displayName = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, this.MSIInstallPropertiesRegPath, "DisplayName");
                return Html.B(displayName);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
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
                Log.Info("Starting product version detection");
                Log.Info("Registry path to search: " + path);
                string major = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, path, "Major");
                Log.Info(path + @"\Major: " + major);
                string minor = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, path, "Minor");
                Log.Info(path + @"\Major: " + minor);

                if (major == null)
                {
                    Log.Warn("Search for DisplayVersion key in registry " + this.MSIInstallPropertiesRegPath);
                    major = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, this.MSIInstallPropertiesRegPath, "DisplayVersion");
                    Log.Warn(this.MSIInstallPropertiesRegPath + @"\DisplayVersion: " + major);

                    if (major != null)
                    {
                        // major would be something like 11.04.000
                        if (major.StartsWith("1"))
                            major = major.Substring(0, 2);
                        else
                            major = major.Substring(0, 1);
                    }
                    else
                    {
                        Log.Error("Major version not detected!");
                    }

                }

                // TODO - this may be removed later
                // define the latest patch according to the Product Version
                // applicable only to LoadRunner and its family
                if (this.LatestPatchNames != null)
                {
                    switch (Convert.ToInt32(major))
                    {
                        case 11:
                            if (minor.StartsWith("0"))
                            {
                                this.latestPatchName = this.LatestPatchNames[1];
                                this.latestPatchURL = this.LatestPatchURLs[1];
                            }
                            else
                            {
                                this.latestPatchName = this.LatestPatchNames[2];
                                this.latestPatchURL = this.LatestPatchURLs[2];
                            }
                            break;
                        case 9:
                            this.latestPatchName = this.LatestPatchNames[0];
                            this.latestPatchURL = this.LatestPatchURLs[0];
                            break;
                        default:
                            this.latestPatchName = "Error during patch definition";
                            break;
                    }
                }

                this.version = new Version(Convert.ToInt32(major), Convert.ToInt32(minor));
                return major + "." + minor;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return null;
            }
        }

        public string GetProductNameVersionDate()
        {
            return this.ProductName + " " + this.ProductVersion + " " + this.InstallDate;
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
                string keyPath = @"Installer\Products\" + this.productCode + @"\Patches";
                patchesCodes = RegistryWrapper.GetValueNames(RegHive.HKEY_CLASSES_ROOT, keyPath);
                return patchesCodes;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
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
                List<string> patchesCodes = this.GetPatchesCodes();
                StringBuilder patchesInstalled = new StringBuilder();

                if (patchesCodes != null)
                {
                    //Log.Info("Patches codes" + patchesCodes.ToString());

                    foreach (string patchCode in patchesCodes)
                    {
                        //TODO this check might be unnecessarry 
                        if (patchCode != "Patches")
                        {
                            //string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + this.ProductCode + @"\Patches\" + patchCode;
                            string keyPath = this.MSIPatchesRegPath + patchCode;
                            string dateInstalled = Helper.ConvertInstallDate(RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, keyPath, "Installed"));
                            string displayName = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, keyPath, "DisplayName");

                            // check if latest patch is installed
                            if (displayName == this.latestPatchName)
                            {
                                this.isLatestPatchInstalled = true;
                            }
                            
                            patchesInstalled.Append(Html.B(displayName) + " " + dateInstalled + Html.br);
                        }
                    }
                    //stopWatch.Stop();
                    //TimeSpan ts = stopWatch.Elapsed;
                    //return patchesInstalled + " Duration: " + ts.ToString();
                    return patchesInstalled.ToString();// +" Method execution time: " + ts.ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return null;

            }
        }

        private string GetLatestPatchInfo()
        {
            try
            {
                if (this.isLatestPatchInstalled)
                {
                    return "Yes";
                }
                else
                {
                    if (this.latestPatchName != "")
                    {
                        return Html.Error("No, the latest patch is " + Html.Link(this.latestPatchURL, this.latestPatchName));
                    }
                    else
                    {
                        return "There are no patches currently available for this version";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return Html.Error("Error during collection of information");
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
                Log.Warn(ex.ToString());
                return null;
            }
        }

        private string GetEnvironmentVariables()
        {
            try
            {
                string EnvironmentVariables = String.Empty;
                foreach (string envVar in this.environmentVarNames)
                {
                    EnvironmentVariables += Html.B(envVar + " = ") + OSInfo.GetEnvVariable(envVar) + Html.br;

                }
                return EnvironmentVariables;
            }
            catch (Exception ex)
            {
                Log.Warn(ex.ToString());
                return "None";
            }
        }

        /// <summary>
        /// Wrapper of the File.Exists method. Executed only if the product is installed.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        private bool isFileExist(string file)
        {
            if (this.InstallLocation != null)
            {
                string path = this.InstallLocation + file;
                if (File.Exists(path))
                    return true;
            }
            return false;
        }

        private string GetExecutableFilesInfo()
        {
            try
            {
                //string filesInfo = String.Empty;
                StringBuilder output = new StringBuilder(1024);
                string filePath = String.Empty;

                foreach (string file in this.ExecutableFiles)
                {
                    filePath = this.InstallLocation + file;

                    if (this.isFileExist(file))
                    {
                        var fileInfo = this.GetFileInfo(file);
                        if (fileInfo["Name"].ToString().Contains("firefox"))
                        {
                            string version = fileInfo["Version"].ToString();
                            output.Append(Html.B(filePath) + Html.br + "Version: " + version + " last modified on: " + fileInfo["ModifiedOn"] + Html.br + Html.hr);
                        }
                        else
                            output.Append(Html.B(filePath) + Html.br + "Version: " + fileInfo["Version"] + " last modified on: " + fileInfo["ModifiedOn"] + " " + IsFileLargeAddressAware(filePath) + Html.br + Html.hr);
                    }
                    else
                    {
                        //if file is not found check if we are searching for firefox.exe which is not available in earlier versions
                        //CopareTo will return -1 if vugen is ealier than 11
                        if (file.Contains("firefox") && this.version.CompareTo(new Version("11.0")) == -1)
                        { }
                        else
                            output.Append(Html.Error("File not found: " + Html.br + filePath) + Html.br + Html.hr);
                    }
                }
                return output.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }

        #region Is file large address aware
        protected string IsFileLargeAddressAware(string file)
        {
            try
            {
                Stream stream = File.OpenRead(file);
                return IsFileLargeAddressAware(stream) ? " the file is " + Html.Warning("LARGEADDERSSAWARE") : "the file is not LARGEADDERSSAWARE";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
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
                Log.Warn(ex.ToString());
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
        private string GetImportantRegKeyValues()
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
                Log.Warn(ex.ToString());
                return ex.Message;
            } 
        }

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
                Log.Error(ex.ToString());
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
                if (this.isVuGen)
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(this.InstallLocation + @"config\IgnoredContent.xml");

                    XmlNodeList nodes = xmlDoc.GetElementsByTagName("string");
                    List<String> content = new List<string>();
                    for (int i = 0; i < nodes.Count; i++)
                        content.Add('"' + nodes[i].InnerText + '"');

                    string output = String.Join(", ", content.ToArray());

                    return output;
                }
                return null;
            }
            catch (FileNotFoundException )
            {
                Log.Error(this.InstallLocation + @"content\IgnoredContent.xml not found");
                return Html.Error(this.InstallLocation + @"\content\IgnoredContent.xml not found");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
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

                if (this.isVuGen)
                {
                    var fileInfo = this.GetFileInfo( @"bin\bbhook.dll");
                    output.Append("bbhook.dll - version: " + fileInfo["Version"] + ", " + fileInfo["Size"] + " bytes" + Html.br);
                    if (OSInfo.is64BitOperatingSystem && this.version.CompareTo(new Version("11.50")) != -1)
                    {
                        fileInfo = this.GetFileInfo(@"bin\bbhook_x64.dll");
                        output.Append("bbhook_x64.dll - version: " + fileInfo["Version"] + ", " + fileInfo["Size"] + " bytes");
                    }
                    return output.ToString(); 
                }
                return null;
            }
            catch (FileNotFoundException fnfe)
            {
                Log.Error(fnfe.ToString());
                return Html.Error("bbhook library not found in " + this.InstallLocation + @"\bin");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.Error(ex.Message);
            }
        }
        #endregion

        #region Protocols information
        public string GetProtocolFlexInfo()
        {
            try
            {
                StringBuilder output = new StringBuilder();
                output.Append(GetFlexRoInfo());

                var fileInfo = GetFileInfo(@"jars\ConvertExternalizableObject.jar");
                if (fileInfo != null)
                    output.Append("ConvertExternalizableObject.jar exists, " + fileInfo["Size"].ToString() + " bytes, last modified on: " + fileInfo["ModifiedOn"].ToString() + Html.br);
                else
                    output.Append(Html.Error("ConvertExternalizableObject.jar does not exist in " + this.InstallLocation + @"\jars folder") + Html.br);


                string env = OSInfo.GetEnvVariable("HP_FLEX_JAVA_LOG_FILE");

                output.Append("Environment variable HP_FLEX_JAVA_LOG_FILE = " + env);

                return output.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.Error(ex.Message);
            }
        }

        private string GetFlexRoInfo()
        {
            try
            {

                StringBuilder output = new StringBuilder();

                IniParser ini = new IniParser(this.InstallLocation + @"dat\flex_ro.ini");
                string useExternalVm = ini.GetSetting("FLEX_RO", "UseExternalVm");
                output.Append(useExternalVm + Html.br);
                if (useExternalVm.EndsWith("1"))
                    output.Append(ini.GetSetting("FLEX_RO", "ExternalVmPath") + Html.br);
                output.Append(ini.GetSetting("FLEX_RO", "FlexJvmParams") + Html.br);

                return output.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.Error(ex.Message) + Html.br ;
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
        private Hashtable GetFileInfo(string fileName)
        {
            var fileInfo = new Hashtable();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(this.InstallLocation + fileName);
            FileInfo fi = new FileInfo(this.InstallLocation + fileName);

            fileInfo.Add("Name", fi.Name);
            fileInfo.Add("Version", fvi.FileVersion == null ? "Unknown" : fvi.FileVersion);
            fileInfo.Add("ModifiedOn", fi.LastWriteTime.ToLocalTime());
            fileInfo.Add("Size", fi.Length);

            return fileInfo;
        }
        #endregion

        #region Analysis Configuration
        public string parseAnalysisIniFile()
        {
            try 
            {
                string filePath = this.InstallLocation + @"config\LRAnalysis80.ini";
                Log.Info("Parsing " + filePath);
                return IniParser.ToString(filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.Error(ex.Message);
            }
        }
        #endregion

    }
}
