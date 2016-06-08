using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LRDetect
{
  class VugenProtocolsCollectorHelper
  {

    public static Hashtable GetPreferencesFromJson(string fileName, string settingsDir, string settingsName = "TruClient")
    {
      try
      {
        //TODO Check for version 12
        string webIEFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), settingsDir);
        var prefsFile = Path.Combine(webIEFolder, fileName);
        string fileContents = "";

        if (File.Exists(prefsFile))
          fileContents = File.ReadAllText(prefsFile);

        Hashtable ht = (Hashtable)JSON.JsonDecode(fileContents);
        ht = (Hashtable)ht[settingsName];

        return (Hashtable)JSON.FlattenValues(ht);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }

    #region TruClient IE General Settings
    public class TruClientIE
    {
      //public const string BROWSER_MASTER_PREF = "lrwebIE_browser_master_prefs.json";
      //public const string MASTER_PREF = "lrwebIE_master_prefs.json";

      public static string WebIEConfigDir { 
        get { return ProductDetection.Vugen.version >= new Version(12, 5) ? @"Hewlett-Packard\LoadRunner\TruClientWeb" : @"Hewlett-Packard\LoadRunner\WebIE"; } }

      public static string MasterPrefFile
      {
        get { return ProductDetection.Vugen.version >= new Version(12, 5) ? @"lrtruclientweb_master_prefs.json" : "lrwebIE_master_prefs.json"; }
      }
      public static string BrowserMasterPrefFile
      {
        get { return ProductDetection.Vugen.version >= new Version(12, 5) ? @"lrtruclientweb_browser_master_prefs.json" : "lrwebIE_browser_master_prefs.json"; }
      }
      public static string IsEnabled()
      {
        return OSCollectorHelper.IEVersion >= new Version(9, 0) ? "Yes, IE version is greater than 9" : "No, IE 9 or later is not found";
      }



      internal static string GetGeneralBrowserSettings()
      {
        try
        {
          var s = new StringBuilder();
          Hashtable ht = GetPreferencesFromJson(BrowserMasterPrefFile, WebIEConfigDir);
          foreach (DictionaryEntry h in ht)
            s.Append(String.Format("{0} : {1} {2}", h.Key, h.Value, Html.br));

          return s.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return null;
        }
      }

      internal static string GetInteractiveOptions()
      {
        try
        {
          StringBuilder s = new StringBuilder();
          //TODO Check for version 12
          Hashtable ht = GetPreferencesFromJson(MasterPrefFile, WebIEConfigDir);
          foreach (DictionaryEntry h in ht)
            s.Append(String.Format("{0} : {1} {2}", h.Key, h.Value, Html.br));

          return s.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return null;
        }
      }

      internal static string GetGeneralSettings()
      {
        throw new NotImplementedException();
      }

      internal static string GetTCIEVersion()
      {
        var version = Html.ErrorMsg();
        try
        {
          var fileName = Path.Combine(ProductDetection.Vugen.DatFolder, @"WebIE\RRE\content\version.txt");
          version = File.ReadAllText(fileName);
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
        return version;
      }
    }
    #endregion

    #region TruClient Firefox
    public class TruClientFF
    {
      public const string BROWSER_MASTER_PREF = "lrweb2_browser_master_prefs.json";
      public const string MASTER_PREF = "lrweb2_master_prefs.json";
      public const string WEB2_DIR = @"Hewlett-Packard\LoadRunner\Web2";

      internal static string GetFirefoxVersion()
      {
        try
        {
          var filePath = Path.Combine(ProductDetection.Vugen.BinFolder, @"firefox\firefox.exe");
          if (File.Exists(filePath))
          {
            var fileInfo = Helper.GetFileInfo(filePath);
            var versionInfo = fileInfo["Version"].ToString();
            return versionInfo;
          }
          else
          {
            return Html.Error(ProductDetection.Vugen.BinFolder + @"\firefox\firefox.exe not found!");
          }
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      internal static string GetBrowserSettings()
      {
        try
        {
          StringBuilder s = new StringBuilder();
          //TODO Check for version 12
          Hashtable ht = GetPreferencesFromJson(BROWSER_MASTER_PREF, WEB2_DIR);
          foreach (DictionaryEntry h in ht)
            s.Append(String.Format("{0} : {1} {2}", h.Key, h.Value, Html.br));

          return s.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return null;
        }
      }

      internal static string GetInteractiveOptions()
      {
        try
        {
          StringBuilder s = new StringBuilder();
          //TODO Check for version 12
          Hashtable ht = GetPreferencesFromJson(MASTER_PREF, WEB2_DIR);
          foreach (DictionaryEntry h in ht)
            s.Append(String.Format("{0} : {1} {2}", h.Key, h.Value, Html.br));

          return s.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return null;
        }
      }

      /// <summary>
      /// Get the ACL info
      /// 1. Get the port range from %APPDATA%\Hewlett-Packard\LoadRunner\Data\Settings\VuGenProperties.xml
      /// 2. Run the 'netsh http show urlacl' command
      /// 3. Filter the output for the ports from 1.
      /// </summary>
      /// <returns></returns>
      public static string GetUrlAclInfo()
      {
        try
        {
          StringBuilder output = new StringBuilder();

          string commandOutput = Helper.ExecuteCMDCommand("netsh http show urlacl");
          output.Append(FilterPorts(commandOutput));

          return output.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      internal static string FilterPorts(string commandOutput)
      {
        StringBuilder output = new StringBuilder();
        string[] lines = commandOutput.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
        for(int i = 0; i < lines.Length; i++)
        { 
          if (ContainsUrl(lines[i]))
            output.Append(lines[i] + Html.br + lines[i + 1] + Html.br + lines[i + 2] + Html.br + lines[i + 3] + Html.br + lines[i + 4] + Html.br + Html.br);
        }

        commandOutput = output.ToString();

        return output.ToString();
      }

      private static bool ContainsUrl(string line)
      {
        var vp = new VuGenProperties();
        foreach (var urlPort in vp.urlPorts)
        {
          if (line.Contains(urlPort))
            return true;
        }
        return false;
      }


      internal static string GetGeneralSettings()
      {
        throw new NotImplementedException();
      }
    }

    #endregion

    #region Java Recording Options
    public class Java
    {
      public static IniParser ini;
      static string tabName = "JavaVM:Options";

      static Java()
      {
        try
        {
          ini = new IniParser(Path.Combine(ProductDetection.Vugen.ConfigFolder, "vugen.ini"));
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      internal static string GetJavaIniBoolOption(string option)
      {
        try
        {
          var setting = ini.GetBoolSetting(tabName, option, false);
          return Html.BoolToYesNo(setting);
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg() + " " + option + " option";
        }
      }

      internal static string GetJavaIniOption(string option)
      {
        try
        {
          var setting = ini.GetSetting(tabName, option);
          return setting.Length < 100 ? setting : Html.AddLinkToHiddenContent(setting.Replace(";", Html.br));
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg() + " " + option + " option";
        }
      }
    }
    #endregion


    public class Citrix
    {
      public static IniParser ini;

      static Citrix()
      {
        try
        {
          ini = new IniParser(Path.Combine(ProductDetection.Vugen.DatFolder, "citrix_ro.ini"));
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      internal static string GetCitrixRecOptions()
      {
        StringBuilder output = new StringBuilder();

        try
        {
          output.Append(Html.B("Configuration") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Encryption Level: " + ini.GetSetting("Citrix", "Enctyption") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Window size: " + ini.GetSetting("Citrix", "Window") + Html.br);

          output.Append(Html.B("Recorder") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Window name: " + VugenProtocolsCollectorHelper.Citrix.GetCitrixWindowNameOption("CommonNames", "NAMES") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("BITMAPS", "SaveBitmaps", true)) + "Save snapshots");

          output.Append(Html.br + Html.B("Code Generation") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("AGENT", "UseAgent", true)) + "Use Citrix agent input in Code Generation" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("AGENT", "SyncOnText", false)) + "Automatically generate text synchronization calls " + Html.br);
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.Error(ex.ToString());
        }
        return output.ToString();
      }

      private static string GetCitrixWindowNameOption(string settingName, string tabName)
      {
        try
        {
          var index = Convert.ToInt16(ini.GetSetting(tabName, settingName, "0"));
          var settingText = new string[3] { "Use new window name as is", "Use common preffix for the new window names", "Use common suffix for the new window names" };
          return settingText[index];
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg() + " " + settingName + " option";
        }
      }
    }

    public class Rdp
    {
      public static IniParser ini;

      static Rdp()
      {
        try
        {
          ini = new IniParser(Path.Combine(ProductDetection.Vugen.DatFolder, "rdp_ro.ini"));
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      internal static string GetRdpRecOptions()
      {
        StringBuilder output = new StringBuilder();

        try
        {
          output.Append(Html.B("Client Startup") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Run RDP client application: " + VugenProtocolsCollectorHelper.Rdp.GetRdpStartup() + Html.br);

          output.Append(Html.B("Code Generation - Basic") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Script generation level: " + ini.GetSetting("CodeGeneration", "RDPScriptLevel") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("CodeGeneration", "MouseMovement", false)) + "Generate mouse movement calls" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("CodeGeneration", "RawMouse", false)) + "Generate raw mouse calls" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("CodeGeneration", "RawKeyboard", false)) + "Generate raw keyboard calls" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("CodeGeneration", "GenerateConnection", false)) + "Always generate connection name" + Html.br);
          output.Append(Html.IndentWithSpaces() + "Automatic generation of synchronization points: " + GetGenerateAutoSyncPointsInfo() + Html.br);
          output.Append(Html.IndentWithSpaces(8) + "Sync radius(pixels): " + ini.GetSetting("CodeGeneration", "AutoSyncDelta", "20") + Html.br);

          //output.Append(Html.CheckBox(ini.GetBoolOption("SaveBitmaps", "BITMAPS", true)) + "Save snapshots");
          output.Append(Html.br + Html.B("Code Generation - Adv") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Double-click timeout (msec): " + ini.GetSetting("CodeGeneration", "DblClickThreshold", "500") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Prefix for snapthos names: " + ini.GetSetting("CodeGeneration", "SnapshotsPrefix", "snapshot_") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Prefix for clipboard parameters: " + ini.GetSetting("CodeGeneration", "ClipboardParamsPrefix", "ClipboardDataParam_") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("CodeGeneration", "ClipboardParamsCorrelation", true)) + "Correlate clipboard parameters" + Html.br);

          output.Append(Html.br + Html.B("Code Generation - Agent") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("CodeGeneration", "UseRdpAgent", false)) + "Use RDP agent" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("CodeGeneration", "EnableRdpAgentLog", false)) + "Enable RDP agent log" + Html.br);
          output.Append(Html.IndentWithSpaces(5) + "RDP agent log detail level: " + ini.GetSetting("CodeGeneration", "RdpAgentLogSeverityLevel", "Standard") + Html.br);
          output.Append(Html.IndentWithSpaces(5) + "RDP agent log destination: " + ini.GetSetting("CodeGeneration", "RdpAgentLogDestination", "File") + Html.br);
          output.Append(Html.IndentWithSpaces(5) + "RDP agent log folder: " + ini.GetSetting("CodeGeneration", "RdpAgentLogFileFolder", "") + Html.br);

        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.ToString();
        }
        return output.ToString();
      }

      private static string GetRdpStartup()
      {
        try
        {
          var index = Convert.ToInt16(ini.GetSetting("RDP", "Connection", "0"));
          var settingText = new string[3] { "Run RDP client application", "Use custom connection file" + Html.br + ini.GetSetting("RDP", "FileName"), "Use default connection file" };
          return settingText[index];
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      private static string GetGenerateAutoSyncPointsInfo()
      {
        try
        {
          int index = Convert.ToInt16(ini.GetSetting("CodeGeneration", "GenerateAutoSyncPoints", "1"));
          string[] settingText = new string[3] {"None", "Rectangular", "Enhanced"};
          return settingText[index];
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      private static string GetRdpCustomOption(string p1, string p2)
      {
        throw new NotImplementedException();
      }
    }


    public class DotNet
    { 
      public static IniParser ini;

      static DotNet()
      {
        try
        {
          ini = new IniParser(Path.Combine(ProductDetection.Vugen.DatFolder, "dotnet_ro.ini"));
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      public static string GetDotNetRecOptions()
      {
        StringBuilder output = new StringBuilder();

        try
        {
          output.Append(Html.B("Microsoft .NET > Recording") + Html.br);
          output.Append(Html.IndentWithSpaces() + Html.B("Application launch") + Html.br);
          string iniSection = "General";
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "InvasiveAnyCpuRecording", false), false, 6) + "Modify .NET 'Any CPU' type executable files before recording." + Html.br);

          output.Append(Html.IndentWithSpaces() + Html.B("Support for previous .NET versions") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "EnableV2Emulation", false), false, 6) + "Emulate previous .NET version in transport level" + Html.br);

          output.Append(Html.IndentWithSpaces() + Html.B("Logging") + Html.br);
          output.Append(Html.IndentWithSpaces(6) + "Log severity: " + ini.GetSetting(iniSection, "LogSeverity") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "InstrumentationLog", false), false, 6) + "Instrumentation log" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "RecordLog", false), false, 6) + "Recording log" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "CodeGenLog", false), false, 6) + "Code generation log" + Html.br);

          output.Append(Html.IndentWithSpaces() + Html.B("Serialization") + Html.br);
          output.Append(Html.IndentWithSpaces(6) + "Serialization format: " + ini.GetSetting(iniSection, "SerializationFormat", "Binary") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "SerializeLongArray", false), false, 6) + "Serialize long arrays" + Html.br);
          output.Append(Html.IndentWithSpaces(8) + "Treshold value for long array size: " + ini.GetSetting(iniSection, "MaxArrayLength", "32") + Html.br);

          output.Append(Html.IndentWithSpaces() + Html.B("Remote Objects") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("Remoting", "RecordInprocProxy", false), false, 6) + "Record in-process objects" + Html.br);


          output.Append(Html.IndentWithSpaces(6) + Html.B("Asynchronous calls") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "RecordOriginalCallbackByDefault", false), false, 6) + "Call original callstack by default" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "GenerateAsyncCallback", true), false, 6) + "Generate asynchronous callbacks" + Html.br);

          output.Append(Html.IndentWithSpaces(6) + Html.B("WCF duplex binding") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "GenerateDummyDuplexCallback", true), false, 6) + "Generate dummy callback handler" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "GenerateUniqueClientBaseAddress", true), false, 6) + "Generate unique client based address" + Html.br);

          output.Append(Html.IndentWithSpaces() + Html.B("Debug Options") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("Debug", "StackTrace", true), false, 6) + "StackTrace" + Html.br);
          output.Append(Html.IndentWithSpaces(6) + "Stack trace limit: " + ini.GetSetting("Debug", "StackTraceLimit", "20") + Html.br);

          output.Append(Html.IndentWithSpaces() + Html.B("Filters") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "IgnoreAllAssemblies", false), false, 6) + "Ignore all assemblies by default" + Html.br);

          output.Append(Html.IndentWithSpaces() + Html.B("Code generation") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "ShowWarnings", true), false, 6) + "Show warnings" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "ShowStackTrace", false), false, 6) + "Show stack trace" + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting(iniSection, "GenerateNewSubscribers", false), false, 6) + "Show all events subscriptions" + Html.br);

        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.ToString();
        }
        return output.ToString();
      }

      internal static string GetDotNetFilters()
      {
        StringBuilder output = new StringBuilder();

        try
        {
          string filtersDir = Path.Combine(ProductDetection.Vugen.DatFolder, "DotnetFilters");
          if (Directory.Exists(filtersDir))
          {
            string[] names = Directory.GetFiles(filtersDir, "*.xml");
            foreach (string name in names)
              output.Append(Path.GetFileName(name).Replace(".xml", "") + Html.br);
          }
          else
            output.Append("No filters found");
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.ToString();
        }
        return output.ToString();
      }
    }

    #region SiebelWeb
    public class SiebelWeb
    {
      static string dllName = "ssdtcorr.dll";

      public static string GetSiebelDllVersionInfo()
      {
        try
        {
          if (ProductDetection.Vugen.IsInstalled)
          {
            var filePath = Path.Combine(ProductDetection.Vugen.BinFolder, dllName);
            if (File.Exists(filePath))
            {
              FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath);
              return Html.Yes + ", " + fvi.FileVersion;
            }
            else
              return dllName + " not found!";
          }
          else
            return "Vurtual user generator is not installed";
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }

      public static string GetIsSiebelCorrelationEnabledInfo()
      {
        StringBuilder output = new StringBuilder();

        //TODO load correlations xml only once
        //use the VugenProtocols.CorrelationRulesEnabledInfo
        try
        {
          if (ProductDetection.Vugen.IsInstalled)
          {
            var rule = CorrelationRules.GetRuleByName("AutoDetect_Siebel_Parse_Page", "Siebel");
            bool ruleExists = (rule != null && rule.callbackName == "flCorrelationCallbackParseWebPage");

            output.Append("Is 'Siebel' group of rules enabled? " + CorrelationRules.IsGroupEnabledText("Siebel") + Html.br);
            output.Append(@"Is WebSiebel77Correlation.cor applied? " + Html.BoolToYesNo(ruleExists) + Html.br);
            output.Append("Is WebSiebelSpanningRules.cor applied? " + CorrelationRules.IsGroupEnabledText("Siebel_Spanning") + Html.br);
            return output.ToString();
          }
          else
            return "Vurtual user generator is not installed";
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.ErrorMsg();
        }
      }
    }
    #endregion

    #region Flex
    public class FlexAmf
    {
      public static IniParser ini;

      static FlexAmf()
      {
        try
        {
          ini = new IniParser(Path.Combine(ProductDetection.Vugen.DatFolder, "flex_ro.ini"));
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
      }

      #region Protocols information
      public static string GetFlexInfo()
      {
        try
        {
          StringBuilder output = new StringBuilder();

          var fileInfo = Helper.GetFileInfo(Path.Combine(ProductDetection.Vugen.InstallLocation, @"jars\ConvertExternalizableObject.jar"));
          if (fileInfo != null)
            output.Append("ConvertExternalizableObject.jar exists, " + fileInfo["Size"].ToString() + " bytes, last modified on: " + fileInfo["ModifiedOn"].ToString() + Html.br);
          else
            output.Append(Html.Error("ConvertExternalizableObject.jar does not exist in " + ProductDetection.Vugen.InstallLocation + @"\jars folder") + Html.br);


          string env = OSCollectorHelper.GetEnvVariable("HP_FLEX_JAVA_LOG_FILE");

          output.Append("Environment variable HP_FLEX_JAVA_LOG_FILE = " + env);

          return output.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.Error(ex.Message);
        }
      }

      public static string GetFlexRoInfo()
      {
        try
        {

          StringBuilder output = new StringBuilder();

          output.Append(Html.B("Flex > RTMP") + Html.br);
          output.Append(Html.CheckBox(ini.GetBoolSetting("RTMP_RO", "GenerateReceiveStream", true)) + "Generate single step for RTMP/T stream handling" + Html.br);

          output.Append(Html.B("Flex > Configuration") + Html.br);
          bool useExternalVm = ini.GetBoolSetting("FLEX_RO", "UseExternalVm", false);
          output.Append(Html.CheckBox(useExternalVm) + "Use external JVM" + Html.br);
          int indentLevel = 8;
          if (useExternalVm)
            output.Append(Html.IndentWithSpaces(indentLevel) + "External JVM path: " + ini.GetSetting("FLEX_RO", "ExternalVmPath", "") + Html.br);

          output.Append(Html.CheckBox(ini.GetBoolSetting("FLEX_RO", "ExternalDsParser", false)) + "Use GraniteDS configuration" + Html.br);
          output.Append(Html.IndentWithSpaces(indentLevel) + "Maximum formatted Request/Response size to print (in characters): " + ini.GetSetting("FLEX_RO", "MaxReqResSizeToWriteForLog") + Html.br);

          output.Append(Html.B("Flex > Externalizable objects") + Html.br);

          bool EncodeExternalizableObject = ini.GetBoolSetting("FLEX_RO", "EncodeExternalizableObject", false);
          output.Append(Html.CheckBox(!EncodeExternalizableObject) + "Do not serialize externalizable objects" + Html.br);
          if (EncodeExternalizableObject == true)
          {
            output.Append(Html.CheckBox(true) + "Serialize objects using:" + Html.br);
            bool UseServerParserToParseAmf3 = ini.GetBoolSetting("FLEX_RO", "UseServerParserToParseAmf3", false);

            bool isCheckBoxDisabled = false;
            output.Append(Html.CheckBox(!UseServerParserToParseAmf3, isCheckBoxDisabled, indentLevel) + "LoadRunner AMF serializer" + Html.br);

            isCheckBoxDisabled = UseServerParserToParseAmf3 ? false : true;
            output.Append(Html.CheckBox(UseServerParserToParseAmf3, isCheckBoxDisabled, indentLevel) + "Custom Java classes" + Html.br);

            indentLevel = 12;
            output.Append(Html.CheckBox(ini.GetBoolSetting("FLEX_RO", "UseFlexGlobalJars", false), isCheckBoxDisabled, indentLevel) + "Use Flex LCDS/BlazeDS jars" + Html.br);
            bool UseAdditionalJars = ini.GetBoolSetting("FLEX_RO", "UseAdditionalJars", false);

            string additionalJarsList = "";
            if (UseAdditionalJars)
              additionalJarsList = Html.AddLinkToHiddenContent(GetFlexAdditionalJarFilesInfo(isCheckBoxDisabled));

            output.Append(Html.CheckBox(UseAdditionalJars, isCheckBoxDisabled, indentLevel) + "Use additional jars " + additionalJarsList + Html.br);
          }

          output.Append(Html.br + ini.GetSetting("FLEX_RO", "FlexJvmParams") + Html.br);

          return output.ToString();
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.Error(ex.Message) + Html.br;
        }
      }

      private static string GetFlexAdditionalJarFilesInfo(bool isCheckBoxDisabled)
      {
        StringBuilder output = new StringBuilder();
        try
        {
          string jars = ini.GetSetting("JAR_FILES", "Items", "");
          if (jars != "" && jars != null)
          {
            char[] charSeparators = new char[] { ';' };
            string[] jarItems = jars.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            int indentLevel = 16;

            foreach (var jarItem in jarItems)
              output.Append(Html.CheckBox(ini.GetBoolSetting(jarItem, "Checked", false), isCheckBoxDisabled, indentLevel) + ini.GetSetting(jarItem, "Item", "") + Html.br);
          }
          else
            return Html.IndentWithSpaces(16) + "No jar files added"; //Indent the text 16 spaces
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.Error(ex.Message) + Html.br;
        }
        return output.ToString();
      }
      #endregion


    }
    #endregion

    /*
    /// <summary>
    /// TODO move it to the INI class
    /// </summary>
    /// <param name="ini"></param>
    /// <param name="optionName"></param>
    /// <param name="tabName"></param>
    /// <returns></returns>
    internal static string GetIniTextOption(IniParser ini, string optionName, string tabName)
    {
      try
      {
        var setting = ini.GetSetting(tabName, optionName);
        return setting.Length < 100 ? setting : Html.AddLinkToHiddenContent(setting);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg() + " " + optionName + " option";
      }
    }


    internal static string CorrelationRulesEnabledInfo(string groupName)
    {
      try
      {
        if (ProductDetection.Vugen.IsInstalled)
        {
          string correlationFileName = "CorrelationSettings.xml";
          var filePath = Path.Combine(ProductDetection.Vugen.ConfigFolder, correlationFileName);

          if (File.Exists(filePath))
          {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            XmlElement root = doc.DocumentElement;

            XmlNode node = root.SelectNodes("//CorrelationSettings/Group[@Name='" + groupName + "']")[0];
            string value = node.Attributes["Enable"].Value;
            return Html.StringToYesNo(value);
          }
          else
            return Html.Error(filePath + " not found!");
        }
        else
          return "Vurtual user generator is not installed";
      }
      catch (NullReferenceException nre)
      {
        Logger.Error(nre.ToString());
        return Html.Error("Information about " + groupName + " ruleset not found!");
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return Html.ErrorMsg();
      }
    }*/
  }

  
}


