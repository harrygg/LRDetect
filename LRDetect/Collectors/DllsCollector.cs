using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class DllsCollector : Collector
  {
    public override string Title { get { return "Dynamic libraries in 'bin' folder"; } }
    int dllsPerThread = 200;
    public override int Order { get { return 75; } }
    string binFolder = "";

    protected override bool Enabled
    {
      get { return FormArguments.dllsCheck || FormArguments.details >= 3; }
    }

    protected override void Collect()
    {
      // Collect DLLs data only if VuGen or LG are installed and the menu DLLs check is selected or CMD argument is passed
      var isInstalled = false;
      

      if (ProductDetection.Vugen.IsInstalled)
      {
        isInstalled = true;
        binFolder = ProductDetection.Vugen.BinFolder;
        Logger.Info("Checking VuGen dlls");
      }
      else if (ProductDetection.Analysis.IsInstalled)
      {
        isInstalled = true;
        binFolder = ProductDetection.Analysis.BinFolder;
        Logger.Info("Checking Analysis dlls");
      }
      else if (ProductDetection.Loadgen.IsInstalled)
      {
        isInstalled = true;
        binFolder = ProductDetection.Loadgen.BinFolder;
        Logger.Info("Checking Load Generator dlls");
      }

      //TODO check if single dll selection works
      if ((isInstalled) && (FormArguments.dllsCheck || FormArguments.checkAllDlls))
      {
        // We will scan all dLLs if the argument does not contain list of dlls to be scanned
        if (FormArguments.checkAllDlls)
        {
          var dllsInBinFolder = Directory.GetFiles(binFolder).Where(name => name.EndsWith(".dll")).ToList();

          if (FormArguments.async == true)
          {
            // Split all dlls in lists of 200 dlls and run the check in parallel
            List<List<string>> tempListVuGenDlls = createTempDllsList(dllsInBinFolder);
            // .Net 3.5 version of Parallel ForEach
            Helper.EachParallel(tempListVuGenDlls, tempListOfDlls => 
            { 
              GetDllsInfo(tempListOfDlls); 
            });
          }
          else // Don't run dlls scan concurrently
            GetDllsInfo(dllsInBinFolder);
        }
        else // if the menu check all dlls is not selected, but dlls are specified as cmd command
        {
          if (FormArguments.dlls.Count() > 0)
            GetDllsInfo(FormArguments.dlls, true);
        }
      }
    }

    void GetDllsInfo(List<string> dlls, bool prependBinDir = false)
    {
      foreach (var dll in dlls)
      {
        var tableRow = new string[3];
        tableRow[0] = Path.GetFileName(dll);
        string filePath = prependBinDir ? Path.Combine(binFolder, dll) : dll;

        if (File.Exists(filePath))
        {
          FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(filePath);
          tableRow[1] = fvi.FileVersion;
          tableRow[2] = File.GetLastWriteTime(filePath).ToLocalTime().ToString();
        }
        else
          tableRow[1] = Html.Error("Dll not Found!");
        AddStringsToDataCells(tableRow);
      }
    }

    // split dlls into a few chunks so we can run them in parallel
    List<List<String>> createTempDllsList(List<string> dllsInBinFolder)
    {
      // split dlls into a few chunks 
      List<List<String>> dllsLists = new List<List<String>>();
      List<string> tempList = new List<string>();
      int i = 1;
      foreach (var dll in dllsInBinFolder)
      {
        if (i % dllsPerThread == 0)
        {
          dllsLists.Add(tempList);
          tempList = new List<string>();
        }
        tempList.Add(dll);
        i++;
      }
      return dllsLists;
    }

    protected override void RenderHtml()
    {
      HtmlTable htmlTable = new HtmlTable(cells);
      htmlTable.id = "dlls";
      htmlTable.tableClass = "alternateColors";
      htmlTable.AddTableHead(new List<String> {"DLL name", "File Version", "Last Modified"});
      buffer.Append(Html.AddLinkToHiddenContent(htmlTable.ToString()));
    }
  }
}
