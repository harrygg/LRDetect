using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ServiceProcess;
using System.Xml.Serialization;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;

namespace LRDetect
{
  class DetectSecuritySoftware
  {

    #region Contructor
    static DetectSecuritySoftware()
    {
      try
      {
        Assembly assembly = Assembly.GetExecutingAssembly();
        XmlDocument doc = new XmlDocument();
        doc.Load(assembly.GetManifestResourceStream("LRDetect.SecuritySoftware.xml"));
        XmlNode root = doc.DocumentElement;
        XmlNodeList avNodes = root.SelectNodes("//AntiVirus/Products/Product");
        XmlNodeList fwNodes = root.SelectNodes("//Firewall/Products/Product");

        PopulateProductsFromXml(avNodes, avProducts);
        PopulateProductsFromXml(fwNodes, fwProducts);
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
    }
    #endregion 

    
    static List<SecurityProduct> avProducts = new List<SecurityProduct>();
    static List<SecurityProduct> fwProducts = new List<SecurityProduct>();

    static void PopulateProductsFromXml(XmlNodeList nodes, List<SecurityProduct> products)
    {
      foreach (XmlNode node in nodes)
      {
        XmlNodeList servicesNodes = node.FirstChild.ChildNodes;// SelectNodes("/Services/Service");

        List<string> listOfServices = servicesNodes.Cast<XmlNode>().Select(n => n.InnerText).ToList();
        var product = new SecurityProduct() { name = node.Attributes["Name"].Value, services = listOfServices };
        products.Add(product);
      }
    }


    public static string GetSecurityProductsInstalled(List<SecurityProduct> commonProducts)
    {
      StringBuilder output = new StringBuilder();
      try
      {
        foreach (var product in commonProducts)
        {
          Logger.Debug("Searching for installed product: " + product.name);
          var program = InstalledProgramsHelper.GetInstalledProgramByName(new Regex ("^" + product.name + ".*", RegexOptions.IgnoreCase));
          if (program != null)
          {
            output.Append(program.ToString());
            output.Append(InstalledProgramsHelper.GetInfoForServices(product.services));
          }
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
      return output.Length > 0 ? output.ToString() : "Not detected";
    }
    #region Anti virus products
    public static string GetAntiVirusProgramsInstalled()
    {
      return GetSecurityProductsInstalled(avProducts);
    }
    #endregion

    #region Firewall products
    public static string GetFirewallProgramsInstalled()
    {
      return GetSecurityProductsInstalled(fwProducts);
    }
    #endregion

  }

  public class SecurityProduct
  {
    public string name;
    public List<string> services;
  }
}
