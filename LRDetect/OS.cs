using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  public class OS
  {
    public string version;
    public int servicePack = 0;
    public int bits = 3264;
    public bool recommended;

    public OS(string version, int servicePack = 0, int bits = 3264, bool recommended = false)
    {
      this.version = version;
      this.servicePack = servicePack;
      this.bits = bits;
      this.recommended = recommended;
    }

    public bool Equals(OS obj)
    {
      OS osObj = obj as OS;
      if (osObj != null)
      {
        if (this.version == osObj.version && this.bits == osObj.bits && this.servicePack == osObj.servicePack)
          return true;
      }
      return false;
    }

    public static List<OS> GetSupportedOSForProductVersion(string productVersionString)
    {
        List<OS> supportedOs = new List<OS>();
        OS.SupportedOSystems.TryGetValue(productVersionString, out supportedOs);
        return supportedOs;
    }

    internal static OS FindSupportedOs(OS hostOS, List<OS> supportedOs)
    {
      OS os = supportedOs.FirstOrDefault(o => o.Equals(hostOS));
      return os;
    }
    /// <summary>
    /// Windows XP, Service Pack 3	5.1.2600 
    /// Windows Server 2003, Service Pack 1	5.2.3790
    /// Windows Vista, Service Pack 2	6.0.6002
    /// Windows Server 2008	6.0.6001
    /// Windows 7	6.1.7601
    /// Windows Server 2008 R2, SP1	6.1.7601
    /// Windows Server 2012	6.2.9200
    /// Windows 8	6.2.9200
    /// Windows Server 2012 R2	6.3.9200 
    /// Windows 8.1	6.3.9200
    /// Windows 8.1, Update 1	6.3.9600
    /// </summary>
    public static Dictionary<string, List<OS>> SupportedOSystems
    {
      get 
      {
        
        var osList = new List<OS>();
        var dict = new Dictionary<string, List<OS>>();

        // Supported OS for 12.01 http://support.openview.hp.com/selfsolve/document/KM01036284
        osList.Add(new OS("6.1.7601", 1, 64, true)); //Windows 7 64bit, Windows Server 2008 R2 64bit
        osList.Add(new OS("6.1.7601", 1, 32)); //Windows 7 32bit
        osList.Add(new OS("6.3.9200", 0, 64)); //Windows 8.1, Windows Server 2012 R2
        dict.Add("12.01", osList);
        osList = new List<OS>();

        // Supported OS for 12.00 http://support.openview.hp.com/selfsolve/document/KM00828655/53d8cf140014c119818bf3c0/LR12.00_Product_Availability_Matrix.pdf
        osList.Add(new OS("6.1.7601", 1, 64, true)); //Windows 7 64bit, Windows Server 2008 R2 64bit
        osList.Add(new OS("6.1.7601", 1, 32)); //Windows 7 32bit
        osList.Add(new OS("6.2.9200", 0, 64)); //Windows 8 64bit, Windows Server 2012 64bit
        dict.Add("12.00", osList);
        osList = new List<OS>();

        // Supported OS for 11.52 http://support.openview.hp.com/selfsolve/document/KM1417723/52260178001ae00780426df0/LR_11.52_patch1_PAM.pdf
        osList.Add(new OS("6.0.6001", 1)); //Windows Server 2008 64bit
        osList.Add(new OS("6.1.7601", 1, 64, true)); //Windows 7 64bit, Windows Server 2008 R2
        osList.Add(new OS("6.1.7601", 1)); //Windows 7 32bit
        osList.Add(new OS("6.2.9200", 0, 64, true)); //Windows 8 64bit
        osList.Add(new OS("5.2.3790", 0, 32)); //Windows Server 2003 R2 32bit Standard/Enterprise Editions
        osList.Add(new OS("5.1.2600", 0, 32)); //Windows XP Professional 32bit
        dict.Add("11.52", osList);
        osList = new List<OS>();

        // Supported OS for 11.51 http://support.openview.hp.com/selfsolve/document/KM1417723/5225ffa0003620ae811bd6b8/LR_11.51_ProductAvalMatrix.pdf
        osList.Add(new OS("6.0.6001", 1)); //Windows Server 2008 32bit|64bit
        osList.Add(new OS("6.1.7601", 1, 64, true)); //Windows 7 64bit, Windows Server 2008 R2 64bit
        osList.Add(new OS("6.1.7601", 1)); //Windows 7 32bit
        osList.Add(new OS("5.2.3790", 0, 32)); //Windows Server 2003 R2 32bit Standard/Enterprise Editions
        osList.Add(new OS("5.1.2600", 0, 32)); //Windows XP Professional 32bit
        dict.Add("11.51", osList);
        osList = new List<OS>();

        // Supported OS for 11.50 http://support.openview.hp.com/selfsolve/document/KM1417723/5225ff9d002bb0ad811bd6b8/LR11.50_ProductAvaMatrix.pdf
        osList.Add(new OS("6.0.6001", 1)); //Windows Server 2008 32bit|64bit
        osList.Add(new OS("6.1.7601", 1, 64, true)); //Windows 7 64bit, Windows Server 2008 R2 64bit
        osList.Add(new OS("6.1.7601", 1, 32)); //Windows 7 32bit
        osList.Add(new OS("5.2.3790", 0, 32)); //Windows Server 2003 R2 32bit Standard/Enterprise Editions
        osList.Add(new OS("5.1.2600", 0, 32)); //Windows XP Professional 32bit
        dict.Add("11.50", osList);
        osList = new List<OS>();

        // Supported OS for 11.04 http://support.openview.hp.com/selfsolve/document/KM1378808/4fac42ce56665247e7ff7e28/LR11.00_patch4_ProductAvaMatrix.pdf
        osList.Add(new OS("6.0.6001", 2, 64, true)); //Windows Server 2008 64bit
        osList.Add(new OS("6.0.6001", 2, 32)); //Windows Server 2008 32bit
        osList.Add(new OS("6.0.6002", 2, 32)); //Windows Vista SP2 32bit
        osList.Add(new OS("6.1.7601", 0, 32, true)); //Windows 7 32bit
        osList.Add(new OS("6.1.7601", 0)); //Windows 7 64bit, Windows Server 2008 R2
        osList.Add(new OS("5.2.3790", 0, 32)); //Windows Server 2003 32bit, Windows Server 2003 R2 32bit Standard/Enterprise Editions
        osList.Add(new OS("5.1.2600", 0, 32)); //Windows XP Professional 32bit
        dict.Add("11.04", osList);
        osList = new List<OS>();

        return dict; 
      }
    }

  }
}
