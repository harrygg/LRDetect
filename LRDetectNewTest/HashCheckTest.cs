using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LRDetect;
using System.IO;
using System.Text;

namespace LRDetectNewTest
{
  [TestClass]
  public class HashCheckTest
  {
    [TestMethod]
    public void Check_HashFile12_Returns_Filename()
    {
      Assert.AreEqual(@"C:\Program Files (x86)\HP\LoadRunner\plugins\Hashes\LR_12_0.csv", HashCheck.HashFile);
    }

    [TestMethod]
    public void Check_HashFile115_Returns_Null()
    {
      ProductDetection.Vugen.version = new Version("11.52");
      Assert.AreEqual(null, HashCheck.HashFile);
    }

    [TestMethod]
    public void Check_GetHashesFromFile_Returns_Dictionary()
    {
      var hashes = HashCheck.GetHashesFromFile(HashCheck.HashFile);
      Assert.AreEqual(true, hashes.Keys.Count > 0);
    }

    [TestMethod]
    public void Check_GetHashesFromFile_CorruptedFile_Returns_null()
    {
      var hashes = HashCheck.GetHashesFromFile(HashCheck.HashFile);
      Assert.AreEqual(true, hashes.Keys.Count > 0);
    }

    [TestMethod]
    public void Check_GetHashesFromFile_Returns_OneEntry()
    {
      var entries = HashCheck.GetHashesFromFile(testHashFileWrong, "");
      var output = HashCheck.ToString();

      Assert.AreEqual(true, output.Contains(testFile));
    }

    public HashCheckTest()
    {
      createCsvFile();
      createCsvFileWithBadEntry();
    }

    string testFile = "hashes.txt";
    string testHashFile = "hashes.csv";
    string testHashFileWrong = "hashesWrong.csv";

    void createCsvFile()
    {
      // Create test file. 
      if (!File.Exists(testFile))
      {
        // Create a file to write to. 
        string[] createText = { "This is a test file", "Line 2 of file that will be hashed", "Line 3. End" };
        File.WriteAllLines(testFile, createText);
      }
      // Calculate the hash for the test file
      // Create the hashes.csv file 
      if (!File.Exists(testHashFile))
      {
        string hash = HashCheck.CalculateHashOfFile(testFile);

        string[] createText = { testFile + ":" + hash };
        File.WriteAllLines(testHashFile, createText);
      }
    }

    void createCsvFileWithBadEntry()
    {
      // Create the testHashFileWrong file 
      if (!File.Exists(testHashFileWrong))
      {
        string[] createText = { testFile + ":d5142b9679151c7a5e13a79d8c99efd4ddebdcd_" };
        File.WriteAllLines(testHashFileWrong, createText);
      }
    }
  }
}
