using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// registries
using Microsoft.Win32;
// file 
using System.IO;
// fileinfo
using System.Diagnostics;
// wmi info
using System.Management;

namespace LRDetect
{
    class DetectOtherSoftware
    {
        #region Citrix Client Detection
        /// <summary>
        /// CITRIX DETECTION
        /// </summary>
        /// <returns></returns>
        public static string GetCitrixClientInfo()
        {
            try
            {
                // check if the new Citrix client exists (>11.2)
                string citrixClientCode = ProductInfo.GetProductCode("9B123F490B54521479D0EDD389BCACC1");
                if (citrixClientCode == null)
                {
                    // check if the old Citrix client exists (<11.2)
                    citrixClientCode = ProductInfo.GetProductCode("6D0FA3AFBC48DDC4897D9845832107CE");
                    if (citrixClientCode == null)
                    {
                        // get install location for 
                        //string file = installLocation + @"wfcrun32.exe";
                        return "Not detected";
                    }
                }

                string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + citrixClientCode + @"\InstallProperties";
                string clientName = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, registryPath, "DisplayName");
                string clientVersion = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, registryPath, "DisplayVersion");

                //string file = installLocation + @"wfcrun32.exe";
                //if (!File.Exists(file)) 
                //{
                //    return Html.Error(clientName + " " + clientVersion + " registry records found but wfcrun32.exe not found in " + installLocation + " directory");
                //}
                return "Yes, " + clientName + " " + clientVersion;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return null;
            }
        }

        public static string GetCitrixRegistryPatchInfo(string productVersion)
        {
            try
            {
                string virtualChannels = "not null";
                string allowSimulationAPI = String.Empty;
            
                string keyPath = @"SOFTWARE\Citrix\ICA Client\Engine\Lockdown Profiles\All Regions\Lockdown\Virtual Channels\Third Party\CustomVC";
                virtualChannels = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, keyPath, "VirtualChannels");
                Log.Info(@"SOFTWARE\Citrix\ICA Client\Engine\Lockdown Profiles\All Regions\Lockdown\Virtual Channels\Third Party\CustomVC\VirtualChannels = " + virtualChannels);

                keyPath = @"SOFTWARE\Citrix\ICA Client\CCM";
                allowSimulationAPI = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, keyPath, "AllowSimulationAPI");
                Log.Info(@"Key: " + keyPath + "\\AllowSimulationAPI = " + allowSimulationAPI);

                // The below key is available only in LR 11 citrix reg patch
                string preApproved = "Default";
                keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Ext\PreApproved\{238F6F83-B8B4-11CF-8771-00A024541EE3}";
                preApproved = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, keyPath, "(Default)");
                string not = (preApproved == null) ? "not " : "";
                Log.Warn("The key does " + not + "exist: " + keyPath);
                
                string warning = String.Empty;
                if (productVersion.StartsWith("11.0") && preApproved == null)
                    warning = "<br />" + Html.Error("The plugin was not found in the list of pre-approved plugins for IE");

                not = (virtualChannels == "" && allowSimulationAPI == "1") ? "" : "not ";
                return "Citrix registry patch is " + not + "installed" + warning;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region RDP Client Detection
        /// <summary>
        /// RDP DETECTION
        /// </summary>
        /// <returns></returns>   
        public static string GetRDPClientVersion()
        {
            try
            {
                string path = Environment.GetEnvironmentVariable("WINDIR").ToString() + @"\system32\";
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path + "mstsc.exe");
                if(fvi != null) 
                    return fvi.ProductVersion.ToString();
                return "No RDP client detected";
            }
            catch(Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Oracle DB Client detection
        /// <summary>
        /// ORACLE DETECTION
        /// </summary>
        /// <returns></returns>   
        public static string GetOracleClientInfo()
        {
            try
            {
                StringBuilder info = new StringBuilder(512);

                //1. we try using tnsping to determine the Oracle client version
                String output = Helper.ExecuteCMDCommand("tnsping");
                if (output != null && output.Contains("TNS Ping"))
                {
                    info.Append("TnsPing returned: ");
                    int version = output.IndexOf("Version", 0, output.Length);
                    if (version >= 0)
                        info.Append(output.Substring(version + "Version".Length + 1, 10));

                    if (output.Contains("32-bit") || output.Contains("32-Bit") || output.Contains("32-BIT"))
                        info.Append(" 32-bit");
                    else
                        info.Append(" 64-bit");
                    info.Append(Html.br);
                }

                //2. Regardless of what tnsping returns we check the registries
                string keyPath = @"SOFTWARE\Oracle\";
                string notDetected = "Not detected";

                List<String> oraHomes = new List<string>();
                if (OSInfo.is64BitOperatingSystem)
                {
                    //1. Check if Oracle entry is found in x64 registries
                    string key64 = RegistryWrapper.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath, "(Default)");
                    //2. Check if Oracle entry is found in x32 registries (WOW6432Node)                    
                    string key32 = RegistryWrapper.GetRegKey32(RegHive.HKEY_LOCAL_MACHINE, keyPath, "(Default)");
                    if (key64 != null && key64 == "value not set")
                    {
                        List<string> oracleSubKeyNames64 = RegistryWrapper.GetSubKeyNames(RegHive.HKEY_LOCAL_MACHINE, keyPath, RegSAM.WOW64_64Key);
                        foreach (string keyName in oracleSubKeyNames64)
                        {
                            if (keyName.StartsWith("KEY") || keyName.StartsWith("HOME"))
                            {
                                string value = RegistryWrapper.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath + keyName, "ORACLE_HOME");
                                if (value != null)
                                    oraHomes.Add("x64 ORACLE_HOME = " + value);
                            }
                        }
                    }

                    if (key32 != null && key32 == "value not set")
                    {
                        List<string> oracleSubKeyNames32 = RegistryWrapper.GetSubKeyNames(RegHive.HKEY_LOCAL_MACHINE, keyPath, RegSAM.WOW64_32Key);
                        foreach (string keyName in oracleSubKeyNames32)
                        {
                            if (keyName.StartsWith("KEY") || keyName.StartsWith("HOME"))
                            {
                                string value = RegistryWrapper.GetRegKey32(RegHive.HKEY_LOCAL_MACHINE, keyPath + keyName, "ORACLE_HOME");
                                if (value != null)
                                    oraHomes.Add("x32 ORACLE_HOME = " + value);
                            }
                        }
                    }
                }
                else // if the system is 32 bit
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyPath);
                    //if SOFTWARE\Oracle\ key doesn't exist
                    if (rk == null)
                        return notDetected;

                    RegistryKey subKey = Registry.LocalMachine;
                    string[] oracleKeyNames = rk.GetSubKeyNames();

                    foreach (string keyName in oracleKeyNames)
                    {
                        if (keyName.StartsWith("KEY") || keyName.StartsWith("HOME"))
                        {
                            string value = subKey.OpenSubKey(keyPath + keyName).GetValue("ORACLE_HOME").ToString();
                            if (value != null)
                                oraHomes.Add("ORACLE_HOME = " + value);
                        }
                    }
                }

                if (oraHomes.Count > 1)
                    info.Append(oraHomes.Count + " definitions for ORACLE_HOME found " + Html.br + String.Join(Html.br, oraHomes.ToArray()));
                else if (oraHomes.Count == 1)
                    info.Append(oraHomes[0]);

                return (info.ToString() == "") ? notDetected : info.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg();
            }

        }
        #endregion

        #region SAP GUI Detection - TODO - not implemented yet
        /// <summary>
        /// SAPGUI DETECTION
        /// </summary>
        /// <returns></returns>   
        //public static string GetSapGuiClientVersion()
        //{
        //    try
        //    {
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //    }
        //    return "Not implemented yet";
        //}

        //public static string SapGuiScriptingEnabledInfo()
        //{
        //    try
        //    {
        //    }
        //    catch (Exception ex)
        //    {
        //        return ex.ToString();
        //    }
        //    return "Not implemented yet";
        //}
        #endregion

        #region DotNet detection
        public static string GetDotNetVersion()
        {
            try
            {
                RegistryKey dotNetVersions = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
                string versions = String.Join(", ", dotNetVersions.GetSubKeyNames());
                return versions;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }

        /// <summary>
        /// Check if certain .net version is installed
        /// </summary>
        /// <param name="version">v3.5 | v4.0</param>
        /// <returns></returns>
        public bool IsDotNetVersionInstalled(string version)
        {
            try
            {
                string path = (version == "v4") ? @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" : @"SOFTWARE\Microsoft\NET Framework Setup\NDP\" + version;
                string key = Registry.LocalMachine.OpenSubKey(path).GetValue("Install").ToString();
                return key == "1" ? true : false;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return false;
            }
            
        }



        #endregion

        #region SiteScope detection
        /// <summary>
        /// SiteScope DETECTION
        /// </summary>
        /// <returns></returns>  
        public static string GetSiteScopeInfo()
        {
            try
            {
                // check for installations of
                String[] siteScopeUpgradeCodes = new String[] 
                {
                    "7C9790C8356D6A142B1E73F0AE4F22AB", //HP SiteScope for LoadTesting
                    "55D444412FEC9B349A920326911A26F1", //HP SiteScope Integrations
                    "F60D57E75AF65A84E888D007E2799EE9", //HP SiteScope Monitoring
                    "350F46B85A30F7240911CD2A1C293400", //HP SiteScope Server
                    "54509F95E83F79C43A15FDAFB2FF3CA3", //HP SiteScope User Interface
                    "CFCBC45A6F7CE934286FF54746AF7708"  //HP SiteScope Tools
                };

                // Get product codes for installed SiteScope features
                StringBuilder productInfo = new StringBuilder(1024);
                string productCode = String.Empty;
                foreach (string upgradeCode in siteScopeUpgradeCodes)
                {
                    productCode = ProductInfo.GetProductCode(upgradeCode);
                    if (productCode != null)
                    {
                        string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCode + @"\InstallProperties";
                        //Helper.Log("Searching for SiteScope in registry path " + registryPath);
                        string productName = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, registryPath, "DisplayName");
                        Log.Info("SiteScope product name " + productName);
                        // Product name found now find the product version
                        string productVersion = String.Empty;
                        if (productName != null)
                            productVersion = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, registryPath, "DisplayVersion");

                        string intallDate = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, registryPath, "InstallDate");
                        productInfo.Append(productName + ", version " + productVersion + " " + Helper.ConvertInstallDate(intallDate) + Html.br);
                    }
                    else
                    {
                        //Helper.Log("SiteScope product with UpgradeGUID " + upgradeCode + " was not found", 1);
                    }
                }
                return (productInfo.Length != 0) ? productInfo.ToString() : "No";
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return Html.ErrorMsg(); 
            }
        }
        #endregion

    }
}
