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
namespace LRDetect
{
    class LoadRunner
    {
        public static string LRUpdateCode = "B497C0589D908C14C9713A936FCCB3C6";
        public static string VugenSAUpdateCode = "8B408CD7247E07943A21B9C23B75ACEB";
        public static string AnalysisSAUpdateCode = "CE53A34494FAD3C4BB524D4EA62DB6FF";
        public static string LRPCHostUpdateCode = "4C4B5A63B71605F4E93E5AF6EBDCB833";



        public static string isFullLRInstalled()
        {
            try
            {
                RegistryKey rk = Registry.ClassesRoot.OpenSubKey(@"\Installer");
                if (rk != null)
                    Helper.Log("Number of keys in isntaller: " + rk.SubKeyCount.ToString());
                else
                    Helper.Log("Registry.ClassesRoot.OpenSubKey(\"Installer\") returned null!!!");

                //TODO check if the codes exists after uninstall
                string productCodeLR = Helper.getProductCode(LRUpdateCode);
                Helper.Log("LRUpdateCode: " + LRUpdateCode);
                Helper.Log("productCodeLR: " + productCodeLR);
                string productCodeLRPCHost = Helper.getProductCode(LRPCHostUpdateCode);
                Helper.Log("LRPCHostUpdateCode: " + LRPCHostUpdateCode);
                Helper.Log("productCodeLRPCHost: " + productCodeLRPCHost);
                string installDate = null;
                if (productCodeLR != null)
                {
                    Helper.Log("productCodeLR: " + productCodeLR);
                    string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCodeLR + @"\InstallProperties";
                    //Helper.log("Registry path: " + registryPath);
                    //installDate = Registry.LocalMachine.OpenSubKey(registryPath).GetValue("InstallDate").ToString();
                    //RegistryKey rk2 = Registry.LocalMachine.OpenSubKey(registryPath);
                    installDate = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, registryPath, "InstallDate");
                    //Helper.log("Registry key InstallDate: " + installDate);

                    //installDate = rk2.GetValue("InstallDate").ToString();
                    //installDate = rk2.GetValue("InstallerLocation").ToString();
                    //Helper.log("Install date: " + installDate);
                }
                if (productCodeLRPCHost != null)
                {
                    
                    string registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCodeLRPCHost + @"\InstallProperties";
                    installDate = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, registryPath, "InstallDate");
                }
                return (productCodeLR != null || productCodeLRPCHost != null) ? "Yes " + Html.B(LoadRunner.getLRVersion()) + Helper.convertInstallDate(installDate) : Html.Warning("No");
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public static string isAnalysisSAInstalled()
        {
            try
            {
                if (isFullLRInstalled().Contains("No"))
                {
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mercury Interactive\LoadRunner\CustComponent\Analysis\CurrentVersion");
                    if (rk != null)
                    {
                        string installPath = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mercury Interactive\Analysis\CurrentVersion\").GetValue("Analysis").ToString();
                        if (installPath != null && System.IO.File.Exists(installPath + @"\AnalysisUI.exe"))
                            return "Yes" + rk.GetValue("Major").ToString() + "." + rk.GetValue("Minor").ToString(); ;                   
                    }
                    return Html.Warning("No");
                }
                return Html.Warning("Checking skipped, full LoadRunner intallation detected");
            }
            catch (Exception ex)
            {
                return Html.Error(ex.ToString());
            }
        }

        public static string isVugenSAInstalled()
        {
            try
            {
                string installDate = null;
                string installLocation = null;
                string registryPath = null;

                if (isFullLRInstalled().Contains("No"))
                {
                    string productCodeVugenSA = Helper.getProductCode(VugenSAUpdateCode);
                    //Helper.log("ProductCodeVugenSA: " + ProductCodeVugenSA);
                    if (productCodeVugenSA != null)
                    {
                        //Helper.log("ProductCodeVugenSA found in Installer");
                        installLocation = Helper.getProductInstallLocation(productCodeVugenSA);
                        registryPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCodeVugenSA + @"\InstallProperties";
                        //InstallLocation = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, registryPath, "InstallLocation");
                        installDate = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, registryPath, "InstallDate");

                        string Major = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Mercury Interactive\LoadRunner\CustComponent\Vuser Generator\CurrentVersion", "Major");
                        string Minor = RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Mercury Interactive\LoadRunner\CustComponent\Vuser Generator\CurrentVersion", "Minor"); 
                        if (installLocation != null && System.IO.File.Exists(installLocation + @"\bin\vugen.exe"))
                            return "Yes - Virtual user generator " + Major + "." + Minor + installDate;
                    }
                    return Html.Warning("No");
                }
                return Html.Warning("Checking skipped, full LoadRunner intallation detected");
            }
            catch (Exception ex)
            {
                return Html.Error(ex.ToString());
            }
        }


        public static string getLRVersion()
        {
            try
            {
                string displayName = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mercury Interactive\LoadRunner\CurrentVersion\").GetValue("DisplayName").ToString();
                return (displayName != null) ? displayName : @"Currupted or missing entries in registry HKEY_LOCAL_MACHINE\SOFTWARE\Mercury Interactive\LoadRunner\CurrentVersion] Registry";
            }
            catch(Exception ex)
            {
                return @"Currupted or missing entries in registry HKEY_LOCAL_MACHINE\SOFTWARE\Mercury Interactive\]" + Html.br + ex.ToString();
            }
        }

        public static string[] getPatchesCodes(string productCode)
        {
            //Helper.log("getPatchesCodes() started");
            try
            {
                string[] patchesCodes = null;
                RegistryKey rk = Registry.ClassesRoot.OpenSubKey(@"Installer\Products\" + productCode + @"\Patches");
                if(rk != null)
                    patchesCodes = rk.GetValueNames();
                return patchesCodes;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string getPatchesInstalled(string productCode)
        {
            //Helper.log("for product code " + productCode);
            try
            {
                string[] patchesCodes = LoadRunner.getPatchesCodes(productCode);
                string patchesInstalled = null;

                if (patchesCodes != null)
                {
                    //Helper.log("Patches codes" + patchesCodes.ToString());
                    
                    foreach (string patchCode in patchesCodes)
                    {
                        if (patchCode != "Patches")
                        {
                            string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCode + @"\Patches\" + patchCode;
                            Helper.Log("KeyPath to patch: " + keyPath);

                            if (OSInfo.getOperatingSystemArchitecture().StartsWith("x86"))
                            {
                                RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyPath);
                                patchesInstalled += rk.GetValue("DisplayName").ToString() + " "
                                    + Helper.convertInstallDate(rk.GetValue("Installed").ToString());
                            }
                            else
                            {
                                patchesInstalled += RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath, "DisplayName") + " "
                                    + Helper.convertInstallDate(RegistryWOW6432.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath, "Installed"));
                            }

                            patchesInstalled += Html.br;
                        }

                    }
                    return patchesInstalled;
                }
                return "No patches were detected";
                
            }
            catch (Exception ex)
            {
                return Html.Error(ex.ToString());
            }
        }


        public static string getCustomComponentsInstalled()
        {
            string intalledComponents = null;
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mercury Interactive\LoadRunner\CustComponent\");
                if (rk != null)
                {
                    foreach (string subKeyName in rk.GetSubKeyNames())
                    {
                        RegistryKey subKey = rk.OpenSubKey(subKeyName + @"\CurrentVersion");
                        if (subKey != null)
                        {
                            intalledComponents += Html.B(subKeyName) + " " + subKey.GetValue("Major").ToString() + "." + subKey.GetValue("Minor").ToString() + Html.br;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Html.Error(ex.ToString());
            }
            return intalledComponents;
        }

        public static string getImportantRegKeyValues()
        {
            try
            {
                string importantKeys = null;

                RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mercury Interactive\LoadRunner\VuGen\Thumbnails");
                if (rk != null)
                    importantKeys = "GenerateThumbnails" + " = " + rk.GetValue("GenerateThumbs").ToString() + Html.br;
                rk = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Mercury Interactive\LoadRunner\Protocols\HTTP\Analyzer");
                if (rk != null)
                    importantKeys += "DisableBrowserCaching" + " = " + rk.GetValue("AddNoCacheHeaderToHtml").ToString() + Html.br;
                    importantKeys += "DisableStaticCaching" + " = " + rk.GetValue("AddNoCacheHeaderToHtml").ToString() + Html.br;
                    importantKeys += "AddNoCacheHeaderToHtml" + " = " + rk.GetValue("AddNoCacheHeaderToHtml").ToString() + Html.br;

                return importantKeys;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public static string getFileVersion(string fileName)
        {
            try
            {
                string path = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mercury Interactive\LoadRunner\CurrentVersion\").GetValue("Vuser Generator").ToString() + @"\bin\" + fileName;
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                System.IO.FileInfo fi = new System.IO.FileInfo(path);

                if (fileName.StartsWith("firefox")) 
                    return fvi.ProductVersion.ToString();
                return fvi.FileVersion.ToString() + " last modified on: " + fi.LastWriteTime.ToString() + " (d.m.y H:m:s)";
            }
            catch (System.IO.FileNotFoundException)
            {
                return Html.Error("File not found");
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }


        public static string isFileLargeAddressAware(string fileName)
        {
            try
            {
                string path = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Mercury Interactive\LoadRunner\CurrentVersion\").GetValue("Vuser Generator").ToString() + @"\bin\" + fileName;
                Stream stream = File.OpenRead(path);
                return isFileLargeAddressAware(stream) ? "the file is LARGEADDERSSAWARE" : "the file is not LARGEADDERSSAWARE";
            }
            catch (Exception ex)
            {
                return Html.Error(ex.ToString());
            }
        }


        /// <summary>
        /// Checks if the stream is a MZ header and if it is large address aware
        /// </summary>
        /// <param name="stream">Stream to check, make sure its at the start of the MZ header</param>
        /// <exception cref=""></exception>
        /// <returns></returns>
        static bool isFileLargeAddressAware(Stream stream)
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
    }
}
