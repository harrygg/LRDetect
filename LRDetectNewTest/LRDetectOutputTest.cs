using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.IO;
using System.Collections.Generic;

namespace LRDetectNewTest
{
  [TestClass]
  public class LRDetectOutputTest
  {
    bool CheckForItems(string report, List<string> items)
    {
      foreach (var i in items)
      {
        if (!report.Contains(i))
          return false;
      }
      return true;
    }

    [TestMethod]
    public void Check_Report_Contains_OS()
    {
      var collector = new OSCollector();

      var titles = new List<string> { "Machine name", "Full name", "Locale", "decimal separator is", "Is OS Virtualized?", "Is 3GB switch enabled?", "Data Execution Prevention", "User Account Control", "Is user Admin?", "Is user connected remotely?", "Is Windows firewall enabled?", "Environment information", "System environment variables", "Kerberos configuration", "Layered Service Providers", "entries found", "AppInit_DLLs registry value", "LoadAppInit_DLLs registry value", "x64 entry", "x32 entry"};

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_Hardware()
    {
      var collector = new HardwareCollector();

      var titles = new List<string> { "Hardware", "Hardware Information", "CPU", "Processor Count", "Total Memory", "Hard Drives", "Monitor information", "detected", "Primary screen resolution", "pixels" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_InstalledSoftware()
    {
      var collector = new SoftwareCollector();
      var titles = new List<string> { "Installed software", "Software Information", "Default browser", "Internet Explorer version", "Internet Explorer Extensions", "Is Google Chrome installed?", "Is Mozilla Firefox installed?", "Is anti-virus software detected?", "Is firewall software detected?", "Windows Add/Remove Programs", "Installed programs", "products found" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_InstalledProductInformation()
    {
      var collector = new LRCollector();

      var titles = new List<string> { "Installed Products Information", "LoadRunner Information", "Is full LoadRunner installed?", "Patches installed", "Custom components installed", "Main executable files", "LARGEADDERSSAWARE", "Related environment variables" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_LRAgent()
    {
      var collector = new MAgentCollector();

      var titles = new List<string> { "LoadRunner Agent", "Name of installed agent", "Status", "Last 10 lines of agent's log:" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_VugenConfiguration()
    {
      var collector = new LRConfigurationCollector();

      var titles = new List<string> { "VuGen configuration", "Correlation", "Rules support", "Ignored content types", "AribaBuyer", "CorrelationSettings.xml content", "Bbhook version", "vugen.ini content", "Log Files", "Registration failures", "lines of HP.LR.VuGen.Log" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_VariousProtocolsSettings()
    {
      var collector = new VugenProtocolsCollector();

      var titles = new List<string> { "Various protocols settings", "WEB (HTTP/HTML)", "Last used recording options", "TruClient IE", "Is enabled?", "RRE version", "General > Browser settings", "General > Interactive options", "TruClient Firefox", "Firefox version", "General > Browser settings", "General > Interactive options", "Lists DACLs", "Java protocols", "Classpath", "VM Params", "Use VM params during replay?", "Use classic Java", "Prepend classpath to -Xbootclasspath", "Citrix", "Is Citrix client installed?", "Is Citrix registry patch installed?", "Recording options", "Citrix_XenApp correlation rules enabled?", "RDP client version", "RDP recording options", "FLEX, AMF", "Recording options", "Other settings", "Flex correlation rules enabled?", "ConvertExternalizableObject.jar", "Environment variable HP_FLEX_JAVA_LOG_FILE", "Siebel Web", "Is Siebel correlation library used?", "Correlation rules", "Is 'Siebel' group of rules enabled?", "Is WebSiebel77Correlation.cor applied?", "Is WebSiebelSpanningRules.cor applied?" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_ControllerSettings()
    {
      var collector = new ControllerSettingsCollector();

      var titles = new List<string> { "Configuration files", "wlrun7.ini file content", "Connections", "Connected load generators" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_AnalysisSettings()
    {
      var collector = new AnalysisSettingsCollector();

      var titles = new List<string> { "Analysis settings", "Analysis configuration files", "LRAnalysis80.ini file content" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }


    [TestMethod]
    public void Check_Report_Contains_OtherRelatedInfo()
    {
      var collector = new ClientsCollector();

      var titles = new List<string> { "Other related information", ".NET versions installed", "Java", "CLASSPATH", "Other java environment variables", "JDKs isntalled", "JREs isntalled", "JAVA_HOME", "VuGen JRE version", "Oracle DB client information", "Is SAPGUI installed?" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_OtherRelatedProducts()
    {
      var collector = new RelatedProductsCollector();

      var titles = new List<string> { "Other HP related products", "HP Virtual Table Server", "Is VTS installed?", "HP Business Process Monitor", "Is BPM installed?", "ALM Platform Loader", "Is ALM Platform Loader installed?", "HP Unified Functional Testing", "Is QTP/UFT installed?", "HP SiteScope", "Is SiteScope installed?" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }

    [TestMethod]
    public void Check_Report_Contains_DLLsInformation()
    {
      FormArguments.dllsCheck = true;
      FormArguments.dlls = new List<string> { "ActionInterfacesWrapperLib.dll","ActiveScreen.dll", "not_existing_dll_12345" };
      var collector = new DllsCollector();

      var titles = new List<string> { "Dynamic libraries in 'bin' folder", "DLL name", "File Version", "Last Modified", "ActionInterfacesWrapperLib", "ActiveScreen.dll", "not_existing_dll_12345", "Dll not Found!" };

      Assert.AreEqual(true, CheckForItems(collector.ToString(), titles));
    }
  }
} 
