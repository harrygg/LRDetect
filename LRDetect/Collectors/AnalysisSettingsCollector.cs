using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class AnalysisSettingsCollector : Collector
  {
    public override string Title { get { return "Analysis settings";  } }
    public override int Order { get { return 100; } }
    protected override bool Enabled
    {
      get { return ProductDetection.Analysis.IsInstalled; }
    }

    protected override void Collect()
    {
      var filePath = Path.Combine(ProductDetection.Analysis.ConfigFolder, "LRAnalysis80.ini");
      AddDataPair("Analysis configuration files", "LRAnalysis80.ini file content", Html.AddLinkToHiddenContent(IniParser.ToHtml(filePath)));
    }
  }
}
