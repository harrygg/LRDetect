using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Collections;

namespace LRDetect
{
  class DetectRecordingOptions
  {
      bool currentUserSettingsExist = false;

      #region Check if recording options exist in registry
      public DetectRecordingOptions()
      {
        try
        {
          String keyPath = @"Software\Mercury Interactive\LoadRunner";
          RegistryKey regKey = Registry.CurrentUser.OpenSubKey(keyPath);
          //if the key doesn't exist exit
          if (regKey == null)
            this.currentUserSettingsExist = false;
          keyPath = @"Software\Mercury Interactive\Networking";
          regKey = Registry.CurrentUser.OpenSubKey(keyPath);
          if (regKey == null)
            this.currentUserSettingsExist = false;

          this.currentUserSettingsExist = true;
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          this.currentUserSettingsExist = false;
        }
      }
      #endregion

      #region Create checkbox from registry value
      /// <summary>
      /// Creates a html checkbox
      /// </summary>
      /// <param name="text">Text that will appear next to the checkbox</param>
      /// <param name="keyPath">Registry path to value for the checkbox</param>
      /// <param name="keyName">Name of the registry key that holds the value</param>
      /// <returns></returns>
      private static string CreateCheckBoxFromValue(string text, string keyPath, string keyName, string defaultValue = "")
      {
        try
        {
          var key = Registry.CurrentUser.OpenSubKey(keyPath).GetValue(keyName);
          string value = key == null ? defaultValue : key.ToString();
          bool ticked = false;
          if (value == "true" || value == "True" || value == "1")
              ticked = true;
          return Html.CheckBox(ticked) + text;
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return Html.CheckBox(defaultValue == "True") + text;
        }      
      }
      #endregion

      #region Get recording options General Script Node
      public static string GetROScriptGeneral()
      {

        string keyPath = @"Software\Mercury Interactive\LoadRunner\Scripting\";
        StringBuilder output = new StringBuilder();
        output.Append(Html.B("General > Script"));

        try
        {
          if (ProductDetection.Vugen.version > new Version(12,5))
          {
            string lang = RegistryWrapper.GetRegKey64(RegHive.CurrentUser, keyPath, "Language_QTWeb");
            output.Append(Html.br + "Scripting language: " + lang);
          }
          keyPath = @"Software\Mercury Interactive\LoadRunner\Scripting\Options\General Options";
          
          output.Append(Html.br + CreateCheckBoxFromValue("Close all AUT processes when recording stops", keyPath, "CloseAUT", "False"));
          output.Append(Html.br + CreateCheckBoxFromValue("Generate fixed think time after end transaction", keyPath, "FixedThinkTime", "False"));
          output.Append(Html.br + CreateCheckBoxFromValue("Generate recorded events log", keyPath, "GenerateRecordingLog", "False"));
          output.Append(Html.br + CreateCheckBoxFromValue("Generate think time greater than threshold", keyPath, "ThinkTimeThreshold", "True"));
          output.Append(Html.br + CreateCheckBoxFromValue("Maximum number of lines in action file", keyPath, "NumberOfLinesPerFile", "False"));
          output.Append(Html.br + CreateCheckBoxFromValue("Track processes created as COM local servers", keyPath, "TrapLocalServer", "True"));
          //Below key is only available in LR 11.5 and later
          if (ProductDetection.Vugen.isNew)
            output.Append(Html.br + CreateCheckBoxFromValue("Use protected application recording", keyPath, "UseProtectedAppRecording", "False"));
          if (ProductDetection.Vugen.version > new Version(11,5))
            output.Append(Html.br + CreateCheckBoxFromValue("Warn me if application being recorded encounters an error", keyPath, "UseRecordingMalfunctiondiagnostics", "True"));
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return ex.Message;
        }
        return output.ToString() + Html.br;
      }
      #endregion

      #region Get recording options General Recording Node
      public static string GetROScriptLevel()
      {
        StringBuilder output = new StringBuilder();
        string keyPath = @"Software\Mercury Interactive\Networking\Multi Settings\QTWeb\Recording";

        output.Append(Html.br + Html.B("General > Recording") + Html.br);
        output.Append(Html.IndentWithSpaces() + "HTTP/HTML Level" + Html.br);

        try
        {
          String AnalogMode = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "Analog Mode", "0");
          String RecordDILFlag = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "RecordDILFlag", "0");
          String RecordOutOfContext = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "Record OutOfContext", "2");
          String ConcurrentGroupFlag = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "ConcurrentGroupFlag", "1");

          //check if we use HTML mode 
          output.Append( Html.IndentWithSpaces() + Html.Radio((AnalogMode == "0"), 0) + "HTML-based script" + Html.br);

          if (AnalogMode == "0")
          {
            output.Append(Html.IndentWithSpaces() + "Script type: " + Html.br);
            output.Append(Html.Radio(RecordDILFlag == "0", 8) + "A script describing user actions (e.g. web_link, web_submit_form)" + Html.br);
            output.Append(Html.Radio(RecordDILFlag == "1", 8) + "A script containing explicit URLs only (e.g. web_url, web_submit_data)" + Html.br);
            output.Append(Html.IndentWithSpaces() + "Non HTML-generated elements (e.g. JavaScript, VBScript, ActiveX, Applets)" + Html.br);
            output.Append(Html.Radio(RecordOutOfContext == "2", 8) + "Record within the current script step" + Html.br);
            output.Append(Html.Radio(RecordOutOfContext == "1", 8) + "Do not record" + Html.br);
            output.Append(Html.Radio(RecordOutOfContext == "0", 8) + "Record within the current script step" + Html.br);
          }

          output.Append(Html.Radio(Convert.ToInt16(AnalogMode) > 0) + "URL-based script" + Html.br);
          if (Convert.ToInt16(AnalogMode) > 0)
          {
            output.Append(Html.CheckBox(ConcurrentGroupFlag == "1") + "Record within the current script step" + Html.br);
            //If AnalogMode is 3 then the option is checked, if it is 2 it is unchecked
            output.Append(Html.CheckBox(AnalogMode == "3") + "Use web_custom_request only" + Html.br);
          }
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
        return output.ToString();
      }
      #endregion

      #region Get recording options Code Generation Node
      public static string GetROCodeGen()
      {
        StringBuilder output = new StringBuilder();
        output.Append(Html.br + Html.B("General > Code Generation") + Html.br);
        output.Append(Html.IndentWithSpaces() + "Code generation process includes:" + Html.br);

        string textValue = CorrelationConfigurationHelper.Settings["ScanAfterCodeGeneration"].ToString();
        output.Append(Html.CheckBox(textValue == "True", true, 8) + "Correlations scan");

        return output.ToString();
      }
      #endregion

      #region Get recording options Port Mapping Node
      public static string GetROPortMapping()
      {
          StringBuilder output = new StringBuilder();
          output.Append(Html.br + Html.br + Html.B("Network > Port Mapping") + Html.br);
          try
          {
            string protocolsKey = @"Software\Mercury Interactive\LoadRunner\Protocols\";
            string sockets = RegistryWrapper.GetValue(RegHive.CurrentUser, protocolsKey + @"WPlus\Analyzer", "EnableSocketLevelTrappingForWeb", "1");
            string wininet = RegistryWrapper.GetValue(RegHive.CurrentUser, protocolsKey + @"HTTP\Analyzer", "EnableWinINetTrapForWeb", "0");
              
            string captureLevel = "Socket level data";
            if (sockets == "1" && wininet == "1")
              captureLevel = "Socket level and WinINet level data";
            else if (wininet == "1")
              captureLevel = "WinINet level data";
 
            output.Append(Html.IndentWithSpaces() + "Capture level: " + captureLevel + Html.br);
          }
          catch (Exception ex)
          {
              Logger.Error(ex.ToString());
          }
          return output.ToString();
      }
      #endregion

      #region Get recording options Port Mapping Servers
      public static string GetROPortMappingServers()
      {
          StringBuilder output = new StringBuilder();
          output.Append((Html.IndentWithSpaces() + "Server entries: "));
          string keyPath = @"Software\Mercury Interactive\LoadRunner\Protocols\WPlus\Servers";

          try
          {
            string[] serverEntries = Registry.CurrentUser.OpenSubKey(keyPath).GetSubKeyNames();

            foreach (string entry in serverEntries)
            {
              RegistryKey rk = Registry.CurrentUser.OpenSubKey(keyPath + "\\" + entry);
              string Acvite = rk.GetValue("Active").ToString();
              string UseProxy = rk.GetValue("UseProxy").ToString();
              string SSL_Active = rk.GetValue("SSL_Active").ToString();
              string SSL_AutoSslDetect = rk.GetValue("SSL_AutoSslDetect").ToString();

              bool selected = Acvite == "true";
              string recordType = UseProxy == "true" ? "Proxy" : "Direct";
              string connectionType = "";
              string Clnt_SSL_Version = "";
              string Clnt_SSL_Ciphers = "";
              string Svr_SSL_Certificate = "";
              string Clnt_SSL_Certificate = "";
              
              if (recordType == "Proxy")
              {
                connectionType = ", Connection type: Plain";
                if (SSL_AutoSslDetect == "true")
                    connectionType = ", Connection type: Auto";
                if (SSL_Active == "true")
                    connectionType = ", Connection type: SSL";

                Clnt_SSL_Version = rk.GetValue("Clnt_SSL_Version").ToString();
                Clnt_SSL_Version = ", SSL version " + sslVersions[Clnt_SSL_Version].ToString();

                Clnt_SSL_Ciphers = rk.GetValue("Clnt_SSL_Ciphers").ToString();
                Clnt_SSL_Ciphers = ", SSL cipher " + Clnt_SSL_Ciphers;

                Svr_SSL_Certificate = rk.GetValue("Svr_SSL_Certificate").ToString();
                if (Svr_SSL_Certificate != "")
                  Svr_SSL_Certificate = ", client certificate: " + Svr_SSL_Certificate;

                Clnt_SSL_Certificate = rk.GetValue("Clnt_SSL_Certificate").ToString();
                if (Clnt_SSL_Certificate != "")
                  Clnt_SSL_Certificate = ", proxy-server certificate: " + Clnt_SSL_Certificate;
              }

              output.Append(Html.br + Html.CheckBox(selected, true, 8) + entry + ", Record type: " + recordType + connectionType + Clnt_SSL_Version + Svr_SSL_Certificate + Clnt_SSL_Certificate + Html.br);    
            }
          }
          catch (NullReferenceException ex)
          {
            Logger.Error(ex.ToString());
            output.Append("None" + Html.br);
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return ex.Message;
          }
          return output.ToString();        
      }
      #endregion

      static Hashtable sslVersions = new Hashtable() { { "1", "TLS 1.0" }, { "2", "SSL 2.0" }, { "3", "SSL 3.0" }, { "11", "TLS 1.1" }, { "12", "TLS 1.2" }, { "23", "SSL 2/3" }, { "4", "TLS NPN" }, { "5", "TLS ALPN" } };

      #region Get recordion options Port Mapping Options Node
      public static string GetROPortMappingOptions()
      {
          StringBuilder output = new StringBuilder();
          string keyPath = @"Software\Mercury Interactive\LoadRunner\Protocols\WPlus\SSL";
          output.Append(Html.br + Html.B("Network > Port Mapping > Options") + Html.br);
          output.Append(Html.IndentWithSpaces() + "Record-time auto SSL detection and configuration:" + Html.br);

          try
          {
            string EnableAutoSslDetect = CreateCheckBoxFromValue("Enable auto SSL detection", keyPath, "EnableAutoSslDetect");

            string DefaultSSLVersion = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "DefaultSSLVersion", "23");
            string DefaultSSLCiphers = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "DefaultSSLCiphers", "(Default OpenSsl Ciphers)");

            output.Append(Html.IndentWithSpaces(8) + "SSL Version: " + sslVersions[DefaultSSLVersion] + Html.br);
            output.Append(Html.IndentWithSpaces(8) + "SSL Ciphers: " + DefaultSSLCiphers + Html.br);

            keyPath = @"Software\Mercury Interactive\LoadRunner\Protocols\SOCKET";
            output.Append(Html.IndentWithSpaces() + "Record-time auto socket detection:" + Html.br);
            output.Append(CreateCheckBoxFromValue("Enable auto-detection of SOCKET based communication", keyPath, "EnableSocketAutoDetect", "1") + Html.br);

            string SendRecvTransitionThreshold = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "SendRecvTransitionThreshold", "4");
            string SendRecvBufferSizeThreshold = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "SendRecvBufferSizeThreshold", "2048");

            output.Append(Html.IndentWithSpaces(8) + "Send-Receive transition threshold: " + SendRecvTransitionThreshold + Html.br);
            output.Append(Html.IndentWithSpaces(8) + "Send-Receive buffer size threshold: " + SendRecvBufferSizeThreshold + Html.br);

            keyPath = @"Software\Mercury Interactive\LoadRunner\Protocols\WPlus\Analyzer";
            string WSPDebuggingLevelKey = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "WSPDebuggingLevel", "0");
            var debuggingLevel = new Hashtable() { { "0", "None" }, { "5", "Standard (Default)" }, { "6", "Debug" }, { "9", "Advanced Debug" } };

            output.Append(Html.br + Html.IndentWithSpaces() + "Log level: " + debuggingLevel[WSPDebuggingLevelKey] + Html.br);
            return output.ToString();
          }
          catch (Exception ex)
          {
              Logger.Error(ex.ToString());
              return ex.Message;
          }
      }
      #endregion

      #region Get recording options node HTTP properties
      public static string GetROHttpProperties()
      {
          StringBuilder output = new StringBuilder();
          output.Append(Html.br + Html.B("HTTP Properties > Advanced") + Html.br);

          try
          {
            string keyPath = @"Software\Mercury Interactive\Networking\Multi Settings\QTWeb\Recording";

            output.Append(CreateCheckBoxFromValue("Reset context for each action", keyPath, "ResetContextBetweenActions") + Html.br);
            output.Append(CreateCheckBoxFromValue("Save snapshots locally", keyPath, "SaveSnapshotResources") + Html.br);
            output.Append(CreateCheckBoxFromValue("Generate web_reg_find functions for page titles", keyPath, "RecordWebRegFind") + Html.br);
            output.Append(Html.IndentWithSpaces() + CreateCheckBoxFromValue("Generate web_reg_find functions for sub-frames", keyPath, "RecordWebRegFindNonPrimary") + Html.br);
            output.Append(CreateCheckBoxFromValue("Add comment to script for HTTP errors while recording", keyPath, "AddCommentsIfResponceBiggerThen400") + Html.br);
            output.Append((Html.IndentWithSpaces() + "Support charset") + Html.br);
            output.Append(Html.IndentWithSpaces() + CreateCheckBoxFromValue("Support charset UTF-8", keyPath, "Utf8Support") + Html.br);
            output.Append(Html.IndentWithSpaces() + CreateCheckBoxFromValue("Support charset EUC-JP", keyPath, "EUC Encode") + Html.br);
            //on LR < 11.5 this doesn't exist
            string ParameterizeServerNames = RegistryWrapper.GetRegKey32(RegHive.CurrentUser, @"Software\Mercury Interactive\Networking\Multi Settings\GlobalParameterizeServer\Recording", "ParameterizeServerNames");
            if (ParameterizeServerNames != null)
              output.Append(CreateCheckBoxFromValue("Parameterize server names", @"Software\Mercury Interactive\Networking\Multi Settings\GlobalParameterizeServer\Recording", "ParameterizeServerNames") + Html.br);

            String file = Path.Combine(ProductDetection.Vugen.ConfigFolder, "vugen.ini");
            String settingEngine = Html.ErrorMsg();

            if (File.Exists(file))
            {
              IniParser vugenIni = new IniParser(file);
              //Pre 12 versions:
              if (ProductDetection.Vugen.version < new Version(12, 00))
              {
                string oldRecEngine = vugenIni.GetSetting("WebRecorder", "EnableOldSingleRecorded");
                settingEngine = Html.CheckBox(oldRecEngine == "1") + "Record using earlier recording engine";

                output.Append(Html.br + (Html.IndentWithSpaces() + "Recording engine: " + Html.br + Html.IndentWithSpaces() + settingEngine + Html.br));
              }
              else
              {
                string UseProxyRecorder = vugenIni.GetSetting("WebRecorder", "UseProxyRecorder");
                string ProxyInStreamMode = vugenIni.GetSetting("WebRecorder", "ProxyInStreamMode");

                output.Append(Html.br + Html.IndentWithSpaces(4) + "Proxy Recording settings:" + Html.br);

                output.Append(Html.CheckBox(UseProxyRecorder == "1", false, 8) + "Use the LoadRunner Proxy to record a local application" + Html.br);
                output.Append(Html.CheckBox(ProxyInStreamMode == "1", false, 8) + "Use streaming mode when recording with the LoadRunner Proxy" + Html.br);

              }
            }
            output.Append(Html.br + Html.IndentWithSpaces(4) + "Recording schemes: " + Html.br);
            string CustomHeadersKey = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "CustomHeaderFlag", "2");
            string headersMessage = "Record headers <b>not</b> in the list";

            if (CustomHeadersKey == "1")
              headersMessage = "Record headers in the list";
            else if (CustomHeadersKey == "0")
              headersMessage = "Do not record headers";
            output.Append(Html.IndentWithSpaces(8) + "Headers: " + headersMessage + Html.br);

            if (CustomHeadersKey != "0")
            {
              string headersKey = CustomHeadersKey == "2" ? "CustomHeadersExclude" : "CustomHeaders";
              string headers = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, headersKey, "");
              var parts = Regex.Split(headers, @"(?<=[01])").ToList();
              parts.RemoveAll(item => item.EndsWith("0") || item.Equals(""));
              output.Append(Html.IndentWithSpaces(12) + String.Join(Html.IndentWithSpaces(8), parts.ToArray()).Replace("\n1", Html.br).Replace("\n", Html.IndentWithSpaces(2)));
            }

            string ContentTypeFilterKey = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "ContentTypeFilterFlag", "0");
            var contentMessage = new Hashtable() { { "0", "Do not filter content types" }, { "1", "Exclude content types in list:" }, { "2", "Exclude content types <b>not</b> in list:" } };
            output.Append(Html.br + Html.IndentWithSpaces(8) + "Content types: " + contentMessage[ContentTypeFilterKey] + Html.br);

            if (ContentTypeFilterKey != "0")
            {
              string filtersKey = ContentTypeFilterKey == "2" ? "ContentTypeFilterExclude" : "ContentTypeFilter";
              string content = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, filtersKey, "");

             var parts = Regex.Split(content, @"(?<=[01])").ToList();
             parts.RemoveAll(item => item.EndsWith("0") || item.Equals(""));
             output.Append(Html.IndentWithSpaces(12) + String.Join(Html.IndentWithSpaces(8), parts.ToArray()).Replace("\n1", Html.br).Replace("\n", Html.IndentWithSpaces(2)));
            }
          }
          catch (Exception ex)
          {
            Logger.Error(ex.ToString());
            return output + ex.Message;
          }
          return output.ToString();
      }
      #endregion

      #region Get recording options for Correlations
      /// <summary>
      /// Must be executed only on vugen > 11.5
      /// </summary>
      /// <returns></returns>
      public static string GetROCorrelationConfig()
      {
          StringBuilder output = new StringBuilder();
          output.Append(Html.br + Html.B("Correlations > Configuration") + Html.br);
          output.Append((Html.IndentWithSpaces() + "Scan for correlations applying: ") + Html.br);

          try
          {
              XmlDocument doc = new XmlDocument();
              string filePath = Path.Combine(ProductDetection.Vugen.ConfigFolder, "CorrelationConfiguration.xml");
              if (File.Exists(filePath))
                doc.Load(filePath);
                
              //string textValue = GetNodeText(doc, "/CorrelationOptions/Correlation.RuleScanEnabled", "True");
              string textValue = CorrelationConfigurationHelper.Settings["RuleScanEnabled"].ToString();
              output.Append(Html.CheckBox(textValue == "True", false) + "Rules scan" + Html.br);

              textValue = CorrelationConfigurationHelper.Settings["AutomaticApplyRules"].ToString();
              output.Append(Html.CheckBox(textValue == "True", false, 8) + "Automatically correlate values found" + Html.br);

              textValue = CorrelationConfigurationHelper.Settings["RecordScanEnabled"].ToString();
              output.Append(Html.CheckBox(textValue == "True", false) + "Record scan" + Html.br);
              
              textValue = CorrelationConfigurationHelper.Settings["ReplayScanEnabled"].ToString();
              output.Append(Html.CheckBox(textValue == "True", false) + "Replay scan" + Html.br);

              output.Append((Html.IndentWithSpaces(8) + "Record and Replay scan configuration") + Html.br);

              var UseRegExpCorrelationText = new Hashtable() { {"True", "web_reg_save_param_regexp"}, {"False", "web_reg_save_param_ex"} };
              textValue = CorrelationConfigurationHelper.Settings["UseRegExpCorrelation"].ToString();
              output.Append(Html.IndentWithSpaces(12) + "API used for correlations: " + UseRegExpCorrelationText[textValue] + Html.br);

              textValue = CorrelationConfigurationHelper.Settings["MinCorrelationLength"].ToString();
              output.Append(Html.IndentWithSpaces(12) + "Ignore values shorter than: " + textValue + Html.br);

              textValue = CorrelationConfigurationHelper.Settings["MaxCorrelationLength"].ToString();
              output.Append(Html.IndentWithSpaces(12) + "Ignore values longer than: " + textValue + Html.br);

              textValue = CorrelationConfigurationHelper.Settings["WarnOnLargeDifferences"].ToString();
              output.Append(Html.CheckBox(textValue == "True", false, 12) + "Warn me if the dynamic string size is greater than 10 KB" + Html.br);

              textValue = CorrelationConfigurationHelper.Settings["IsIgnoreCases"].ToString();
              output.Append(Html.CheckBox(textValue == "True", false, 12) + "Ignore case when searching for correlatable values" + Html.br);
              
              output.Append((Html.IndentWithSpaces(8) + "Record scan configuration:") + Html.br);
              
              textValue = CorrelationConfigurationHelper.Settings["RuleHeuristicLevel"].ToString();
              var RuleHeuristicLevelText = new Hashtable() { { "Medium", "Medium" }, { "Strict", "High" }, { "Loose", "Low" } };
              output.Append((Html.IndentWithSpaces(12) + "Scan for differences between snapshots using: ") + RuleHeuristicLevelText[textValue] + Html.br);


              output.Append((Html.IndentWithSpaces(8) + "Replay scan configuration:") + Html.br);
              output.Append((Html.IndentWithSpaces(12) + "Scan for differences between snapshots using:") + Html.br);

              textValue = CorrelationConfigurationHelper.Settings["TypeOfScanDiff"].ToString();
              output.Append((Html.CheckBox(textValue == "1", false, 14) + "HTML comparison") + Html.br);
              output.Append((Html.CheckBox(textValue == "0", false, 14) + "Text comparison") + Html.br);
          }
          catch (Exception ex)
          {
              Logger.Error(ex.ToString());
              return output.ToString() + Html.br + ex.Message;
          }
          return output.ToString();
      }

      static string GetNodeText(XmlDocument doc, string path, string expected)
      {
        try
        {
          var node = doc.SelectSingleNode(path);
          return node.InnerText;
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
          return expected;
        }
      }

      #endregion

      #region Get recording options for Data Format Extension
      public static string GetRODataFormat()
      {
        StringBuilder output = new StringBuilder();
        output.Append(Html.br + Html.B("Data Format Extention") + Html.br);
                
        try
        {
          string keyPath = @"Software\Mercury Interactive\Networking\Multi Settings\GlobalDfe\Recording\DFE";
          output.Append(CreateCheckBoxFromValue("Enable data format extention", keyPath, "Enabled", "False") + Html.br);
          output.Append(CreateCheckBoxFromValue("Verify formatted data", keyPath, "VerifyFormatedData", "False") + Html.br);
          string whatToFormat = RegistryWrapper.GetValue(RegHive.CurrentUser, keyPath, "WhatToFormat", "1");
          whatToFormat = whatToFormat == "1" ? "Code and snapshots" : "Snapshots";
          output.Append(Html.IndentWithSpaces() + "Format: " + whatToFormat + Html.br);
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
        return output.ToString();
      }
      #endregion

      #region Method to output the collected information in the necessarry format
      public override string ToString()
      {
          StringBuilder ro = new StringBuilder();

          if (this.currentUserSettingsExist)
          {
              ro.Append(DetectRecordingOptions.GetROScriptGeneral());
              ro.Append(DetectRecordingOptions.GetROScriptLevel());
              //if vugen is not earlier than 11.50 (if Vugen is earlier then CompareTo returns -1)
              if (ProductDetection.Vugen.isNew)
                  ro.Append(DetectRecordingOptions.GetROCodeGen());
              ro.Append(DetectRecordingOptions.GetROPortMapping());
              ro.Append(DetectRecordingOptions.GetROPortMappingServers());
              ro.Append(DetectRecordingOptions.GetROPortMappingOptions());
              ro.Append(DetectRecordingOptions.GetROHttpProperties());

              //if vugen is not earlier than 11.50
              if (ProductDetection.Vugen.isNew)
                  ro.Append(DetectRecordingOptions.GetROCorrelationConfig());
              ro.Append(DetectRecordingOptions.GetRODataFormat());
          }
          else
              ro.Append("No settings found for current user " + Html.B(Environment.UserName));

          return ro.ToString();
      }
      #endregion 

  }
}


