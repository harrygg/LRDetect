using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class LRCollector : Collector
  {
    public override string Title { get { return "Installed Products Information"; } }
    public override int Order { get { return 40; } }


    protected override void Collect()
    {
      var link = new HtmlElement("a") { text = "View Readme" };
      link.Attributes.Add("target", "_blank");
      link.Attributes.Add("href", "file:///" + ProductDetection.Vugen.InstallLocation + "dat/Readme.htm");

      string readme = ProductDetection.isFullLRInstalled ? link.ToString() : "";
      string title = "LoadRunner Information";
      string lrInfo = String.Format("{0} {1} {2} {3} {4}", 
        Html.BoolToYesNo(ProductDetection.isFullLRInstalled), ProductDetection.FullLR.ProductName, ProductDetection.FullLR.ProductVersion,
        Helper.ConvertInstallDate(ProductDetection.FullLR.InstallDate), readme);

      AddDataPair(title, "Is full LoadRunner installed?", lrInfo);

      //If Full LR or PC HOST are not installed:
      if (!ProductDetection.isFullLRInstalled)
      { 
        string vugenSAInfo = ProductDetection.Vugen.IsInstalled ? ProductDetection.Vugen.GetProductNameVersionDateFormatted() + " " + link : "No";
        AddDataPair(title, "Is VuGen stand-alone installed?", vugenSAInfo);
        AddDataPair(title, "Is Analysis stand-alone installed?", ProductDetection.Analysis.GetProductNameVersionDateFormatted());
        AddDataPair(title, "Is Load Generator installed?", ProductDetection.Loadgen.GetProductNameVersionDateFormatted());
        AddDataPair(title, "Is Monitor Over Firewall installed?", ProductDetection.monitorOverFirewall.GetProductNameVersionDateFormatted());
        AddDataPair(title, "Is PC Server installed?", ProductDetection.pcServer.GetProductNameVersionDateFormatted());
        AddDataPair(title, "Is MI Listener installed?", ProductDetection.miListener.GetProductNameVersionDateFormatted());
      }

      // Display patches installed for the products. If a product is not install patchesInstalled will return empty string
      if (ProductDetection.installedProducts.Count > 0)
      {
        StringBuilder customComponentsInstalled = new StringBuilder(1024);
        StringBuilder mainExecutableFiles = new StringBuilder(1024);
        //StringBuilder importantRegKeys = new StringBuilder(1024);
        StringBuilder environmentVariables = new StringBuilder(1024);
        StringBuilder patchesInstalledInfo = new StringBuilder(1024);

        foreach (var installedProduct in ProductDetection.installedProducts)
        {
          customComponentsInstalled.Append(installedProduct.CustomComponentsInstalled);
          mainExecutableFiles.Append(installedProduct.mainExecutableFilesInfo);
          //importantRegKeys.Append(installedProduct.ImportantRegKeyValues);
          environmentVariables.Append(installedProduct.environmentVariables);
          patchesInstalledInfo.Append(installedProduct.patchesInstalled);
        }

        // Collect the patches for the installed products
        string patchesInstalled = (patchesInstalledInfo.ToString() != "") ? patchesInstalledInfo.ToString() : "None";
        AddDataPair(title, "Patches installed", patchesInstalled);
        AddDataPair(title, "Custom components installed", customComponentsInstalled.ToString());

        AddDataPair(title, "Main executable files", mainExecutableFiles.ToString());
        //AddStringsToDictionary("LoadRunner Information", "Various registry keys", importantRegKeys.ToString());
        // check if we only have MOF installed
        if (ProductDetection.monitorOverFirewall.IsInstalled && (!ProductDetection.Vugen.IsInstalled || !ProductDetection.Loadgen.IsInstalled))
          AddDataPair(title, "Related environment variables", ProductDetection.monitorOverFirewall.environmentVariables);
        else
          AddDataPair(title, "Related environment variables", environmentVariables.ToString());
      }
    }
  }
}
