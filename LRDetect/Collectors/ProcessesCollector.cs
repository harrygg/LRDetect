using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// process
using System.Diagnostics;
namespace LRDetect
{
  class ProcessesCollector : Collector
  {
    public override string Title { get { return String.Format("List of {0} running processes", OSCollectorHelper.GetRunningProcesses().Count()); } }
    private String title = String.Format("List of {0} running processes", OSCollectorHelper.GetRunningProcesses().Count());
    public override int Order { get { return 140; } }
    
    protected override bool Enabled 
    { 
      get { return FormArguments.processes || FormArguments.details >= 3; } 
    }

    protected override void Collect()
    {
      Process[] processes = OSCollectorHelper.GetRunningProcesses();
      foreach (var p in processes)
      {
        AddStringsToDataCells(new String[3] 
        { 
          p.ProcessName
          , (p.WorkingSet64 / 1024).ToString() + " Kb"
          , p.Id.ToString() 
        });
      }
    }

    protected override void RenderHtml()
    {
      HtmlTable t = new HtmlTable(cells);
      t.id = "processes";
      t.tableClass = "alternateColors";
      t.AddTableHead(new List<String> {"Process Name", "Memory Usage", "Process ID"}, true);
      buffer.Append(Html.AddLinkToHiddenContent(t.ToString()));
    }
  }
}
