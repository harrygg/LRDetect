using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LRDetect
{
  class HashCheck
  {

    static string hashFile = null;
    public static string HashFile 
    { 
      get 
      { 
        if (hashFile == null) 
        {
          if (ProductDetection.Vugen.IsInstalled && (ProductDetection.Vugen.version >= new Version("12.00")))
          {
            string fileName = "LR_" + ProductDetection.Vugen.version.Major + "_" + ProductDetection.Vugen.version.Minor + ".csv";
            string filePath = Path.Combine(ProductDetection.Vugen.InstallLocation, @"plugins\Hashes\" + fileName);
            hashFile = File.Exists(filePath) ? filePath : null;
          }
        }
        return hashFile;
      } 
    }

    private static List<string> badFiles = new List<string>();

    public static Dictionary<string, string> GetHashesFromFile(string csvFileWithHashes = null, string rootDir = null)
    {
      var fileHashTable = new Dictionary<string,string>();

      // csvFileWithHashes will not be null if called from Unit testing
      if (csvFileWithHashes == null)
        csvFileWithHashes = hashFile;
      if (rootDir == null)
        rootDir = ProductDetection.Vugen.InstallLocation;

      try
      {
        string[] hashes = File.ReadAllLines(csvFileWithHashes);
        foreach (var hash in hashes)
        {
          var fileHash = hash.Split(':');
          fileHashTable.Add(fileHash[0], fileHash[1]);

          string fullFilePath = Path.Combine(rootDir, fileHash[0]);
          var tempHash = CalculateHashOfFile(fullFilePath);
          if (tempHash != fileHash[1])
            badFiles.Add(fullFilePath);
        }
        return fileHashTable;
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }

    private static SHA1 sha1 = new SHA1CryptoServiceProvider();

    public static string CalculateHashOfFile(string filePath)
    {
      try
      {
        byte[] byteResult;
        StringBuilder result = new StringBuilder();
        int i;
        using (FileStream stream = File.OpenRead(filePath))
        {
          stream.Position = 0;
          byteResult = sha1.ComputeHash(stream);
          for (i = 0; i < byteResult.Length; i++)
            result.AppendFormat("{0:x2}", byteResult[i]);
          return result.ToString();
        }
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return null;
      }
    }
    
    public static string ToString(List<string> entries = null)
    {
      // entries will not be null if called from Unit Testing
      if (entries == null)
        entries = badFiles;

      StringBuilder output = new StringBuilder();
      try
      {
        foreach (var entry in entries)
        {
          output.Append(entry + Html.br);
        }
        return output.ToString();
      }
      catch (Exception ex)
      {
        Logger.Error(ex.ToString());
        return "No hash mismatches found!";
      }
    }
  }
}
