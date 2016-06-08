using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Packaging;
using System.Net.Mime;
using System.Windows.Forms;

namespace LRDetect
{
  class LRDetectZipper
  {
    private string fileName = "LRDetect.zip";
    public string ZipFileName { get { return Path.Combine(Path.GetTempPath(), fileName); } set { fileName = value; } }

    public List<LogFile> filesToBeZipped;

    public LRDetectZipper()
    {
      try
      {
        // Delete the zip file if it exists
        if (File.Exists(ZipFileName))
          File.Delete(ZipFileName);
      }
      catch (IOException ioe)
      {
        var date = DateTime.Now.ToString("yyMMddHmmss");
        ZipFileName = "LRDetect" + date + ".zip";
        MessageBox.Show(ioe.Message + " New name will be used: " + ZipFileName);
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
      }
      finally
      {
        filesToBeZipped = GetLogFilesForInstalledProducts();
        var regFailureFiles = ProductInfo.RegistrationFailureLogs;
        if (regFailureFiles.Count > 0)
          AddFilesToZip(regFailureFiles);
      }
    }

    public void AddFilesToZip(List<LogFile> fullPaths)
    {
      foreach (var file in fullPaths)
      {
          filesToBeZipped.Add(file);
      }
    }

    internal bool ZipFiles()
    {
      return CreateZip();
    }

    private bool CreateZip()
    {
      try
      {
        //Open the zip file if it exists, else create a new one 
        using (Package zip = ZipPackage.Open(ZipFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
          foreach (var file in filesToBeZipped)
          {
            if (File.Exists(file.fullPath))
              AddToArchive(zip, file.fullPath);
          }
        }
        return true;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        MessageBox.Show(ex.ToString());
        return false;
      }
    }

    private void AddToArchive(Package zip, string fileToAdd)
    {
      try
      {
        //Replace spaces with an underscore (_) 
        string uriFileName = fileToAdd.Replace(" ", "_");

        //A Uri always starts with a forward slash "/" 
        string zipUri = string.Concat("/", Path.GetFileName(uriFileName));

        Uri partUri = new Uri(zipUri, UriKind.Relative);
        string contentType = MediaTypeNames.Application.Zip;

        //The PackagePart contains the information: 
        // Where to extract the file when it's extracted (partUri) 
        // The type of content stream (MIME type):  (contentType) 
        // The type of compression:  (CompressionOption.Normal)   
        PackagePart pkgPart = zip.CreatePart(partUri, contentType, CompressionOption.Normal);

        //Read all of the bytes from the file to add to the zip file 
        byte[] bites = ReadAllBytes(fileToAdd);

        //Compress and write the bytes to the zip file 
        pkgPart.GetStream().Write(bites, 0, bites.Length);
      }
      catch (IOException ioe)
      {
        MessageBox.Show(ioe.Message + " File not added to zip.");
        return;
      }
      catch (Exception)
      {
        return;
      }

    }
    public static byte[] ReadAllBytes(String path)
    {
      byte[] bytes;
      using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      {
        int index = 0;
        long fileLength = fs.Length;
        if (fileLength > Int32.MaxValue)
        throw new IOException("File too long");
        int count = (int)fileLength;
        bytes = new byte[count];
        while (count > 0)
        {
          int n = fs.Read(bytes, index, count);
          if (n == 0)
          throw new InvalidOperationException("End of file reached before expected");
          index += n;
          count -= n;
        }
      }
      return bytes;
    }
    private static List<LogFile> GetLogFilesForInstalledProducts()
    {
      try
      {
        List<LogFile> logFiles = new List<LogFile>();
        logFiles = logFiles.Union(ProductDetection.installedProducts[0].LogFiles).ToList();
        ProductDetection.installedProducts.ForEach(x => logFiles.Union(x.LogFiles));
        return logFiles;
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.ToString());
        return null;
      }
    }
  }
}
