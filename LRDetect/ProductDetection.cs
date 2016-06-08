using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LRDetect
{
  class ProductDetection
  {
    //variable that will hold any detected full product (LR or PC Host) 
    public static ProductInfo FullLR;
    //variable that will hold any detected product from VuGen family (LR or PC Host or VuGen SA) 
    public static ProductInfo Vugen;
    //variable that will hold any detected product from Analysis family (LR or PC Host or Analysis SA) 
    public static ProductInfo Analysis;
    public static ProductInfo Loadgen;
    public static bool isFullLRInstalled = false;
    public static bool isPCHostInstalled = false;
    public static MonitorOverFirewallInfo monitorOverFirewall;
    public static PerformanceCenterServerInfo pcServer;
    public static ProductInfo miListener;
    public static List<ProductInfo> installedProducts = new List<ProductInfo>();

    static ProductDetection()
    {
      Logger.Debug("Started " + MethodBase.GetCurrentMethod());
      try
      {
        Logger.Info("Started collection of HP  products information");

        //1. Check for LR Full installation
        //2. Check for PC Host installation
        //3. Check for VuGen SA installation, Analysis SA, Load Generator SA etc...
        Logger.Info("Detecting LoadRunner installation");
        FullLR = new LoadRunnerInfo();
        if (FullLR.IsInstalled)
        {
          installedProducts.Add(FullLR);
          isFullLRInstalled = true;
          Vugen = FullLR;
          Analysis = FullLR;
          Loadgen = FullLR;
        }
        else //if no LR is installed check for PC Host
        {
          Logger.Info("Detecting Performance Center Host installation");
          FullLR = new PerformanceCenterHostInfo();
          if (FullLR.IsInstalled)
          {
            installedProducts.Add(FullLR);
            isFullLRInstalled = true;
            isPCHostInstalled = true;
            Vugen = FullLR;
            Analysis = FullLR;
            Loadgen = FullLR;
          }
          else //if no LR and PC Host are not installed check for VuGen SA
          {
            Logger.Info("Detecting VuGen Stand Alone installation");
            Vugen = new VugenSAInfo();
            if (Vugen.IsInstalled)
              installedProducts.Add(Vugen);

            Logger.Info("Detecting Analysis Stand Alone installation");
            Analysis = new AnalysisInfo();
            if (Analysis.IsInstalled)
              installedProducts.Add(Analysis);

            Logger.Info("Detecting Load Generator installation");
            Loadgen = new LoadGeneratorInfo();
            if (Loadgen.IsInstalled)
              installedProducts.Add(Loadgen);

            Logger.Info("Detecting MI Listener installation");
            miListener = new MIListener();
            if (miListener.IsInstalled)
              installedProducts.Add(miListener);
          }
        }

        Logger.Info("Detecting Monitor Over Firewall installation");
        monitorOverFirewall = new MonitorOverFirewallInfo();
        if (monitorOverFirewall.IsInstalled)
          installedProducts.Add(monitorOverFirewall);

        Logger.Info("Detecting Performance Center Host installation");
        pcServer = new PerformanceCenterServerInfo();
        if (pcServer.IsInstalled)
          installedProducts.Add(pcServer);

      }
      catch (Exception ex)
      {
        System.Windows.Forms.MessageBox.Show(ex.ToString());
      }
      finally
      {
        Logger.Debug("Ended " + MethodBase.GetCurrentMethod());      
      }
    }
  }
}
