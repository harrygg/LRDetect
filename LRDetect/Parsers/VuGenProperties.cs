using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace LRDetect
{
  public class VuGenProperties
  {
    public int startPort = 8080;
    public int endPort = 8090;
    public string url = "http://127.0.0.1:";
    static string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Hewlett-Packard\LoadRunner\Data\Settings\VuGenProperties.xml");
    public List<string> urlPorts = new List<string>();

    public VuGenProperties(string path = null)
    {
      if (filePath != null)
        filePath = path;

      try
      {
        startPort = Convert.ToInt32(GetAttributeValue("BrowserCommunicationServerStartPort", "value", startPort.ToString()));
        endPort = Convert.ToInt32(GetAttributeValue("BrowserCommunicationServerEndPort", "value", endPort.ToString()));
        url = GetAttributeValue("BrowserCommunicationServerURL", "value", url);

        //Generate the url port combinations i.e. 127.0.0.1:8080
        for (var i = startPort; i <= endPort; i++)
          urlPorts.Add(url + i);


      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
    }

    public static string GetAttributeValue(string nodeName, string attributeName = "value", string defaultValue = "")
    {
      try
      {
        if (File.Exists(filePath))
        {
          XmlDocument doc = new XmlDocument();
          doc.Load(filePath);
          XmlElement root = doc.DocumentElement;
          XmlAttribute attribute = null;

          XmlNode node = root.SelectNodes("//VuGenProperties/" + nodeName + "")[0];
          if (node != null)
            attribute = node.Attributes[attributeName];
          if (attribute != null)
            return attribute.Value;
        }


      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
      }
      return defaultValue;
    }

  }
}
