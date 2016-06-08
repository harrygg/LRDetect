using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class LRConfigurationCollector : Collector
  {
    public override string Title { get {  return "VuGen configuration"; } }
    public override int Order { get { return 70; } }

    protected override bool Enabled
    {
      get { return ProductDetection.Vugen.IsInstalled; }
    }

    protected override void Collect()
    {
      string title = "Correlation";
      AddDataPair(title, "Rules support", ProductDetection.Vugen.correlationRulesSupport);
      //check for ignored content only on versions > 11.50
      if (ProductDetection.Vugen.isNew)
        AddDataPair(title, "Ignored content types", ProductDetection.Vugen.correlationIgnoredContent);
      AddDataPair(title, "Rules", CorrelationRules.GetListOfRulesText());
      AddDataPair(title, CorrelationRules.settingsFileName + " content", Html.AddLinkToHiddenContent(CorrelationRules.GetRawContent()));


      //BBHOOK vesrion
      title = "Files";
      AddDataPair(title, "Bbhook version", ProductDetection.Vugen.bbhookVersion);
      //AddStringsToDictionary("Miscellaneous registry settings", "Registry keys", importantRegKeys.ToString());

      var iniContent = IniParser.ToHtml(Path.Combine(ProductDetection.Vugen.ConfigFolder, "vugen.ini"));
      AddDataPair(title, "vugen.ini content", Html.AddLinkToHiddenContent(iniContent));

      title = "Log Files";
      AddDataPair(title, "Registration failures", Html.AddLinkToHiddenContent(Helper.GetRegistraionFailuresContent()));

      if (ProductDetection.Vugen.isNew)
      {
        //TODO see how fast we parse 10MB log file
        int lines = 50;
        string vugenLogContent = Helper.GetLastLinesFromFile(2048 * 1024, System.IO.Path.GetTempPath() + "HP.LR.VuGen.log", lines);
        AddDataPair(title, "Last " + lines + " lines of HP.LR.VuGen.Log", Html.AddLinkToHiddenContent(vugenLogContent));
      }
      
    }
  }
}
