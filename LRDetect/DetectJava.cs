using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace LRDetect
{
    class DetectJava
    {
        #region Members

        string keyPath = @"SOFTWARE\JavaSoft\";
        List<string> installedJavaProducts = new List<string>();
        StringBuilder javaDetailsFromCMD = new StringBuilder();
        #endregion

        #region Methods

        #region Contstructor
        /// <summary>
        /// Default contructor. Detects 64bit JDK, JRE then 32bit JDK, JRE
        /// and assigns it to the appropriate members
        /// </summary>
        public DetectJava()
        {
            List<String> installPaths = new List<string>();
            List<String> pathsToJavaExe = new List<string>();

            string[] registryViews = { "64", "32" };            
            string[] javaProducts = { "Java Development Kit", "Java Runtime Environment" };
            string javaProductInfo = String.Empty;

            try
            {
                // Detection order: 64bit JDK, 32bit JDK, 64bit JRE, 32bit JRE
                foreach (string javaProduct in javaProducts)
                {
                    foreach (string registryView in registryViews)
                    {
                        installPaths = GetPathsToJava(javaProduct, registryView);
                        if (installPaths != null)
                        {
                            foreach (string installPath in installPaths)
                            {
                                if (IsJavaExeExist(Path.Combine(installPath, @"bin\java.exe")))
                                {
                                    string abreviation = (javaProduct == "Java Development Kit") ? "JDK" : "JRE";
                                    javaProductInfo = abreviation + " " + registryView + "-bit"
                                        + " version " + GetJavaVersionFromCmd(installPath) + " in " + installPath;
                                }
                                installedJavaProducts.Add(javaProductInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }
        #endregion

        #region Get the java bin directory from registries
        /// <summary>
        /// Method to get the installation folder to JDK or JRE
        /// 1. Check 64bit registry
        /// </summary>
        /// <param name="product"></param>
        /// <param name="registryView">32 or 64</param>
        /// <returns></returns>
        private List<String> GetPathsToJava(string product, string registryView = null)
        {
            List<String> javaPaths = new List<string>();
            List<string> keys = new List<string>();
            //String key = String.Empty;
            try
            {
                /*
                var regSam = RegSAM.WOW64_32Key;
                if (registryView == "64" && OSInfo.is64BitOperatingSystem)
                    regSam = RegSAM.WOW64_64Key;
                else if (registryView == "32")
                    regSam = RegSAM.WOW64_32Key;

                keys = RegistryWrapper.GetSubKeyNames(RegHive.HKEY_LOCAL_MACHINE, keyPath + product, regSam);
                foreach (string key in keys)
                {
                    Log.Info("Getting 'JavaHome' value from " + keyPath + product + "\\" + key);
                    javaPaths.Add(RegistryWrapper.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath + product + "\\" + key, "JavaHome"));
                }
                */

                //search the 64bit registry (WOW6432Node) if we are on 64bit OS
                if (registryView == "64" && OSInfo.is64BitOperatingSystem)
                {
                    keys = RegistryWrapper.GetSubKeyNames(RegHive.HKEY_LOCAL_MACHINE, keyPath + product, RegSAM.WOW64_64Key);
                    if (keys != null)
                    {
                        Log.Info("Found " + keys.Count + " keys in " + registryView + " registry " + keyPath);
                        foreach (string key in keys)
                        {
                            Log.Info("Getting 'JavaHome' value from " + keyPath + product + "\\" + key);
                            javaPaths.Add(RegistryWrapper.GetRegKey64(RegHive.HKEY_LOCAL_MACHINE, keyPath + product + "\\" + key, "JavaHome"));
                        }
                    }
                    else
                    {
                        Log.Info("No JAVA found in " + registryView + "bit registry " + keyPath + product);
                        return null;
                    }
                }
                //search the 32bit registrys
                else if (registryView == "32")
                {
                    keys = RegistryWrapper.GetSubKeyNames(RegHive.HKEY_LOCAL_MACHINE, keyPath + product, RegSAM.WOW64_32Key);
                    if (keys != null)
                    {
                        Log.Info("Found " + keys.Count + " keys in " + registryView + " registry " + keyPath);
                        foreach (string key in keys)
                        {
                            Log.Info("Getting 'JavaHome' value from " + keyPath + product + "\\" + key);
                            javaPaths.Add(RegistryWrapper.GetRegKey32(RegHive.HKEY_LOCAL_MACHINE, keyPath + product + "\\" + key, "JavaHome"));
                        }
                    }
                    else
                    {
                        Log.Info("No JAVA found in " + registryView + "bit registry " + keyPath + product);
                        return null;
                    }
                }

                //remove the duplicate entries
                return javaPaths.Distinct().ToList();
            }
            catch (NullReferenceException nre)
            {
                Log.Info(nre.ToString());
                return null;
            }
            catch (Exception ex)
            {
                Log.Info(ex.ToString());
                return null;
            }
            
        }
        #endregion

        #region Does Java.exe file exist in the installation directory
        /// <summary>
        /// Method to check if Java.exe exists in the JDK/JRE installation folder
        /// </summary>
        /// <param name="pathToJavaExe"></param>
        /// <returns></returns>
        private bool IsJavaExeExist(string pathToJavaExe)
        {
            if (pathToJavaExe == null)
                return false;
            return (File.Exists(pathToJavaExe)) ? true : false;
        }
        #endregion

        #region Execute 'java -version' command
        /// <summary>
        /// Method to get the java version from command line
        /// </summary>
        /// <param name="pathToJava"></param>
        /// <returns></returns>
        public string GetJavaVersionFromCmd(string pathToJava)
        {
            string output = String.Empty;
            string command = "\"" + pathToJava + "\\bin\\java.exe\" -version";

            output = Helper.ExecuteCMDCommand(command);
            Log.Info("Java version information from " + pathToJava + "\r\n" + output);
            // save the whole output to show it later
            javaDetailsFromCMD.Append(output + Html.br);

            if (output != "" && !output.Contains("not recognized"))
            {
                char[] delimiter = new Char[] { ' ', '\t', '\r', ':' };
                string[] parts = output.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i] == "version")
                        return parts[i + 1];
                }
                return output;
            }
            return Html.ErrorMsg();
        }
        #endregion

        #region ToString
        /// <summary>
        /// Method to output the collected information
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                StringBuilder sb = new StringBuilder(512);
                // always display the CLASSPATH environment variable
                sb.Append(Html.B("CLASSPATH") + " = " + OSInfo.GetEnvVariable("CLASSPATH") + Html.br);
                // display only those env. variables that are present
                string[] envVarsNames = { "JAVA_HOME", "JAVA_OPTIONS", "_JAVA_OPTIONS", "JAVA_TOOLS_OPTIONS" };
                foreach (string envVarName in envVarsNames)
                {
                    string envVar = OSInfo.GetEnvVariable(envVarName);
                    if (!envVar.Contains("Not set!"))
                        sb.Append(Html.B(envVarName) + " = " + envVar + Html.br);
                }

                if (installedJavaProducts.Count == 0)
                {
                    sb.Append(Html.Warning("Java installation was not detected!") + Html.br);
                }
                else
                {
                    //List all javas installed
                    foreach (string intalledJavaProduct in installedJavaProducts)
                        sb.Append(intalledJavaProduct + Html.br);
                    //add the detailed info
                    sb.Append(Html.br + "Details: " + Html.LinkShowContent("java"));
                    sb.Append("\n\t<div id=\"java\" class=\"dontShow\">"
                        + javaDetailsFromCMD.ToString() + "\n\t</div>");

                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }
        #endregion

        #endregion
    }
}
