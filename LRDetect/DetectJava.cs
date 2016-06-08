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
    public StringBuilder javaDetailsFromCMD = new StringBuilder();
    public List<JavaProduct> installedJavaProducts = new List<JavaProduct>();
    #endregion

    internal static string GetClassPath()
    {
      return Html.B("CLASSPATH") + " = " + OSCollectorHelper.GetEnvVariable("CLASSPATH");
    }

    internal static string GetJavaEnvs()
    {
      try
      {
        StringBuilder sb = new StringBuilder();
        // display only those env. variables that are present
        string[] envVarsNames = { "JAVA_HOME", "JAVA_OPTIONS", "_JAVA_OPTIONS", "JAVA_TOOLS_OPTIONS" };
        foreach (string envVarName in envVarsNames)
        {
          string envVar = OSCollectorHelper.GetEnvVariable(envVarName);
          if (!envVar.Contains("Not set!"))
            sb.Append(Html.B(envVarName) + " = " + envVar + Html.br);
        }
        return sb.ToString();
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }

    #region Methods

      #region Constructor
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
      //throw new Exception();
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
                if (File.Exists(Path.Combine(installPath, @"bin\java.exe")))
                {
                  string abreviation = (javaProduct == "Java Development Kit") ? "JDK" : "JRE";
                  string version = GetJavaVersionFromCmd(installPath);

                  javaProductInfo = String.Format("{0} {1}-bit version {2} in {3}"
                    , abreviation, registryView
                    , GetJavaVersionFromCmd(installPath), installPath);

                  installedJavaProducts.Add(new JavaProduct { type = abreviation, bits = registryView, version = version, path = installPath });
                }
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
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
        //search the 64bit registry (WOW6432Node) if we are on 64bit OS
        if (registryView == "64" && OSCollectorHelper.is64BitOperatingSystem)
        {
          keys = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, keyPath + product, RegSAM.WOW64_64Key);
          if (keys != null)
          {
            Logger.Info("Found " + keys.Count + " keys in " + registryView + " registry " + keyPath);
            foreach (string key in keys)
            {
              Logger.Info("Getting 'JavaHome' value from " + keyPath + product + "\\" + key);
              javaPaths.Add(RegistryWrapper.GetRegKey64(RegHive.LocalMachine, keyPath + product + "\\" + key, "JavaHome"));
            }
          }
          else
          {
            Logger.Info("No JAVA found in " + registryView + "bit registry " + keyPath + product);
            return null;
          }
        }
        //search the 32bit registrys
        else if (registryView == "32")
        {
          keys = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, keyPath + product, RegSAM.WOW64_32Key);
          if (keys != null)
          {
            Logger.Info("Found " + keys.Count + " keys in " + registryView + " registry " + keyPath);
            foreach (string key in keys)
            {
                Logger.Info("Getting 'JavaHome' value from " + keyPath + product + "\\" + key);
                javaPaths.Add(RegistryWrapper.GetRegKey32(RegHive.LocalMachine, keyPath + product + "\\" + key, "JavaHome"));
            }
          }
          else
          {
            Logger.Info("No JAVA found in " + registryView + "bit registry " + keyPath + product);
            return null;
          }
        }

      //remove the duplicate entries
      return javaPaths.Distinct().ToList();
      }
      catch (NullReferenceException nre)
      {
          Logger.Info(nre.ToString());
          return null;
      }
      catch (Exception ex)
      {
          Logger.Info(ex.ToString());
          return null;
      }
    }
    #endregion
    /*
    #region Does Java.exe file exist in the installation directory
    /// <summary>
    /// Method to check if Java.exe exists in the JDK/JRE installation folder
    /// </summary>
    /// <param name="pathToJavaExe"></param>
    /// <returns></returns>
    bool IsJavaExeExist(string pathToJavaExe)
    {
        if (pathToJavaExe == null)
            return false;
        return (File.Exists(pathToJavaExe)) ? true : false;
    }
    #endregion*/

    #region Execute 'java -version' command
    /// <summary>
    /// Method to get the java version from command line
    /// </summary>
    /// <param name="pathToJava">path to JRE folder (do not include bin folder)</param>
    /// <returns></returns>
    public string GetJavaVersionFromCmd(string pathToJava)
    {
      var cmdOutput = ExecuteJavaVersionCommand(pathToJava);
      return FormatJavaVersionFromCmdString(cmdOutput);
    }
    #endregion

    public string ExecuteJavaVersionCommand(string pathToJava)
    {
      string command = "\"" + pathToJava + "\\bin\\java.exe\" -version";
      string output = Helper.ExecuteCMDCommand(command);
      Logger.Info("Java version information from " + pathToJava + "\r\n" + output);
      // save the whole output to show it later
      javaDetailsFromCMD.Append(output + Html.br);
      return output;
    }
      

    public string FormatJavaVersionFromCmdString(string output)
    {
      if (output != null && output != "" && !output.Contains("not recognized"))
      {
        char[] delimiter = new Char[] { ' ', '\t', '\r', ':' };
        string[] parts = output.Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
          if (parts[i] == "version")
            return parts[i + 1].Replace("\"", "");
        }
          
        return output;
      }
      return Html.ErrorMsg();
    }
    #endregion
  }

  public class JavaProduct
  {
    public string type;
    public string path;
    public string bits = "32";
    public string version;
    public override string ToString()
    {
      return String.Format("{0}-bit version {1} in {2}", bits, version, path);
    }
  }
}
