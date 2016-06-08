using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
  class OracleClientsHelper
  {
    public string keyPath = @"SOFTWARE\Oracle\";
    public List<OracleClient> clients = new List<OracleClient>();
    StringBuilder output = new StringBuilder();

    /// <summary>
    /// /// ORACLE DETECTION
    /// 1. Check if ORACLE_HOME exists in the 64 or 32 bit registry
    ///   a. How many clients we have installed?
    /// 2. Check if ORACLE_HOME env. variable exists
    /// 3. Instant Client (i.e. no sqlplus) from 10.2.0.3 up can run %ORACLE_HOME%/genezi -v to get client version. 
    /// Available in regular client install but useful in Instant Client.
    /// If no registry entries exist Instant Client must be added to %PATH% otherwise it won't work, so we execute 'genezi -v' directly
    /// </summary>
    public OracleClientsHelper()
    {
      try
      {
        //Check if x32 bit Oracle client exist in registries (\WOW6432Node\SOFTWARE\Oracle on x64 machines)
        RegistryKey rk = Registry.LocalMachine.OpenSubKey(keyPath);
        if (rk != null)
          GetOracleClients(32, rk.GetSubKeyNames().ToList<String>());

        //If the system is x64, check also the x64 registries registries \SOFTWARE\Oracle
        if (OSCollectorHelper.is64BitOperatingSystem)
        {
          List<string> oracleKeyNames = RegistryWrapper.GetSubKeyNames(RegHive.LocalMachine, keyPath, RegSAM.WOW64_64Key);
          GetOracleClients(64, oracleKeyNames);
        }

        //If nothing was found in registry, check for Instant client
        if (clients.Count == 0)
        {
          //Check for instant client
          //Check if ORACLE_HOME env. variable exists
          string home = Environment.GetEnvironmentVariable("ORACLE_HOME");

          //If ORACLE_HOME exists run 'genezi -v' in that dir, otherwise simply run it hoping genezi.exe is in the %PATH%
          string command = (home != null && Directory.Exists(home)) ? home + @"\genezi -v" : "genezi -v";

          string geneziInfo = Helper.ExecuteCMDCommand(command);

          if (geneziInfo.StartsWith("Client"))
            output.Append(geneziInfo.Split('\r')[0] + " at " + home);

        }
        else
        {
          output.Append(clients.Count + " client(s) found: " + Html.br);
          foreach (var client in clients)
            output.Append(client.ToString() + Html.br);
        }
      }     
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
    }


    public void GetOracleClients(int bits, List<string> oracleSubKeyNames)
    {
      if (oracleSubKeyNames != null)
      {
        foreach (string keyName in oracleSubKeyNames)
          if (keyName.StartsWith("KEY") || keyName.StartsWith("HOME"))
            clients.Add(new OracleClient(bits, keyName));
      }
    }

    public override string ToString()
    {
      return output.ToString() == "" ? "Not detected" : output.ToString();
    }

    public class OracleClient
    {
      public string version;
      public string home;
      public string name;
      public int bits;
      public string tnsContent = "";
      const string TNSNAMESORA = "tnsnames.ora";

      public OracleClient(int bits, string keyName)
      {
        RegSAM in64or32 = bits == 64 ? RegSAM.WOW64_64Key : RegSAM.WOW64_32Key;
        keyName = @"SOFTWARE\Oracle\" + keyName;

        this.name = RegistryWrapper.GetRegKey(RegHive.LocalMachine, keyName, in64or32, "ORACLE_HOME_NAME");
        this.home = RegistryWrapper.GetRegKey(RegHive.LocalMachine, keyName, in64or32, "ORACLE_HOME");
        this.bits = bits;
        if (name != null)
          this.tnsContent = GetTnsnamesContent(home);
      }

      string GetTnsPing()
      {
        return "";
      }

      /// <summary>
      /// 1. tnsnames.ora file usually resides in %ORACLE_HOME%\network\admin folder
      /// 2. If it is not there we can try looking up the %TNS_ADMIN% environment variable if it exists
      /// 3. Otherwise it could simply be in the %PATH%
      /// TODO check if Oracle client actually works if tnsnames.ora is in the %PATH% only
      /// </summary>
      /// <param name="home"></param>
      /// <returns></returns>
      string GetTnsnamesContent(string home)
      {
        string content = "";
        try
        {
          //Usually tnsnames.ora is in ORACLE_HOME\network\admin folder
          string tnsFilePath = Path.Combine(home, @"network\admin\" + TNSNAMESORA);

          if (File.Exists(tnsFilePath))
            content = String.Join(Html.br, File.ReadAllLines(tnsFilePath));
          else 
          { 
            //Search for TNS_ADMIN variable
            var tns_admin = Environment.GetEnvironmentVariable("TNS_ADMIN");
            //If TNS_ADMIN exists prepend the directory to the file name
            tnsFilePath = tns_admin != null ? Path.Combine(tns_admin, TNSNAMESORA) : TNSNAMESORA;
            content = File.Exists(tnsFilePath) ? String.Join(Html.br, File.ReadAllLines(tnsFilePath)) : "";
          }
        }
        catch (Exception ex)
        {
          Logger.Error(ex.ToString());
        }
        return content;
      }

      public override string ToString()
      {
        string tns = tnsContent != "" ? TNSNAMESORA + " " + Html.AddLinkToHiddenContent(tnsContent) : "No "+ TNSNAMESORA + " found!";
        return String.Format("{0} x{1} client found in {2}{3} {4}", Html.B(name), bits, home, Html.br, tns);
      }
    }
  }
}
