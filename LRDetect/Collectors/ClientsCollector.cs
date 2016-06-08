using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class ClientsCollector : Collector
  {
    public override string Title { get { return  "Other related information"; } }
    public override int Order { get { return 120; } }

    protected override void Collect()
    {
      //################
      //DOTNET Detection
      //################
      AddDataPair(".NET", ".NET versions installed", DetectOtherSoftware.GetGetDotNetVersionFromRegistry());

      //################
      //Java Detection
      //################
      Logger.Info("Collecting JAVA Information");
      var title = "JAVA";
      AddDataPair(title, "Classpath", DetectJava.GetClassPath());
      AddDataPair(title, "Other java environment variables", DetectJava.GetJavaEnvs());
      AddDataPair(title, "JDKs isntalled", ClientsCollectorHelper.GetJavaProducts("JDK"));
      AddDataPair(title, "JREs isntalled", ClientsCollectorHelper.GetJavaProducts("JRE"));
      AddDataPair(title, "Details", Html.AddLinkToHiddenContent(ClientsCollectorHelper.GetJavaDetails()));

      //DetectJava dj = new DetectJava();
      //AddDataPair("JAVA", "JDK/JRE versions installed", dj.ToString());
      if (ProductDetection.Vugen.IsInstalled)
        AddDataPair("JAVA", "VuGen JRE version", ClientsCollectorHelper.GetVugenJREVersion());


      //################
      //CITRIX Detection
      //################
      title = "Citrix";
      //If any of LR/VuGen/PC Host is not installed
      if (!ProductDetection.Vugen.IsInstalled)
      {
        // Use it only if Vugen is not installed, otherwise the info would be under Vugen Protocols
        Logger.Info("Collecting Citrix Information");
        var ctrxClient = new CitrixHelper.Client();
        AddDataPair(title, "Citrix client version", ctrxClient.GetCitrixClientInfo());
      }
      
      //################
      //CITRIX Server Detection
      //################
      var ctrxServer = new CitrixHelper.Server();
      AddDataPair(title, "Citrix Server", ctrxServer.ToString());
      if (ctrxServer.isInstalled)
      {
        AddDataPair(title, "End disconnected session", ctrxServer.GetIcaMaxDisconnectionTime());
        AddDataPair(title, "Active session limit", ctrxServer.GetMaxConnectionTime());
      }

      //If any of LR/VuGen/PC Host is not installed
      if (!ProductDetection.Vugen.IsInstalled)
      {
        //################
        //RDP Detection
        //################
        Logger.Info("Collecting RDP Information");
        AddDataPair("RDP", "RDP client version", DetectOtherSoftware.GetRDPClientVersion());
      }



      //################
      //Oracle Detection
      //################
      Logger.Info("Collecting Oracle client information");
      AddDataPair("Oracle", "Oracle DB client information", 
      DetectOtherSoftware.GetOracleClientInfo());

      //################
      //SAPGUI Detection
      //################
      Logger.Info("Collecting SAPGUI information");
      AddDataPair("SAPGUI", "Is SAPGUI installed?",
        DetectOtherSoftware.GetSapGuiClientInfo());


      //################
      //JENKINS Detection
      //################
      Logger.Info("Collecting Jenkins information");
      AddDataPair("Jenkins", "Is Jenkins installed?", DetectOtherSoftware.GetJenkinsInfo());
      AddDataPair("Jenkins", "Is HP AAT plugin installed?", DetectOtherSoftware.GetJenkinsPluginInfo());
    }
  }
}
