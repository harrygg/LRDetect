using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LRDetect
{
    public class IniParser
    {
        private Hashtable keyPairs = new Hashtable();
        private String iniFilePath;

        private struct SectionPair
        {
            public String Section;
            public String Key;
        }

        /// <summary>
        /// Opens the INI file at the given path and enumerates the values in the IniParser.
        /// </summary>
        /// <param name="iniPath">Full path to INI file.</param>
        public IniParser(String iniPath)
        {
            TextReader iniFile = null;
            String strLine = null;
            String currentRoot = null;
            String[] keyPair = null;

            this.iniFilePath = iniPath;

            if (File.Exists(iniPath))
            {
                try
                {
                    iniFile = new StreamReader(iniPath);
                    strLine = iniFile.ReadLine();

                    while (strLine != null)
                    {
                        strLine = strLine.Trim();

                        if (strLine != "")
                        {
                            if (strLine.StartsWith("[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            //if we hit a disabled section
                            else if (strLine.StartsWith(";[") && strLine.EndsWith("]"))
                            {
                                currentRoot = strLine.Substring(1, strLine.Length - 2);
                            }
                            else
                            {
                                //escape lines without '='
                                if (strLine.Contains('='))
                                {
                                    keyPair = strLine.Split(new char[] { '=' }, 2);

                                    SectionPair sectionPair;
                                    String value = null;

                                    if (currentRoot == null)
                                        currentRoot = "ROOT";

                                    sectionPair.Section = currentRoot;
                                    sectionPair.Key = keyPair[0];

                                    if (keyPair.Length > 1)
                                        value = keyPair[1];

                                    keyPairs.Add(sectionPair, value);
                                }
                            }
                        }

                        strLine = iniFile.ReadLine();
                    }

                }
                catch (ArgumentException ae)
                {
                    Log.Error("Duplicated entry: " + strLine + " in " + iniPath + "\r\n" + ae.ToString());
                }
                catch (Exception ex)
                {
                    Log.Error(ex.ToString());
                }
                finally
                {
                    if (iniFile != null)
                        iniFile.Close();
                }
            }
            else
                throw new FileNotFoundException("Unable to locate " + iniPath);
        }

        /// <summary>
        /// Returns the value for the given section, key pair.
        /// </summary>
        /// <param name="sectionName">Section name.</param>
        /// <param name="settingName">Key name.</param>
        public String GetSetting(String sectionName, String settingName)
        {
            SectionPair sectionPair;
            sectionPair.Section = sectionName;
            sectionPair.Key = settingName;

            //check if the option exists under the correct section
            string setting = (String)this.keyPairs[sectionPair];

            //if no key is found check if it is under the correct section but it is disabled
            if (setting == null)
            {
                sectionPair.Key = ';' + sectionPair.Key;
                setting = (String)this.keyPairs[sectionPair];

                // if the option is really missing from this section check other sections
                if (setting == null)
                {
                    foreach (DictionaryEntry keyPair in this.keyPairs)
                    {
                        //reset the section key to remove the ';' in front
                        sectionPair.Key = settingName;

                        sectionPair = (SectionPair) keyPair.Key;
                        if (sectionPair.Key == settingName)
                            return Html.Error(settingName + "=" + keyPair.Value + " found under the wrong section [" + sectionPair.Section.ToString() + ']');
                        if (sectionPair.Key == ';' + settingName)
                            return Html.Error(settingName + "=" + keyPair.Value + " found under the wrong section [" + sectionPair.Section.ToString() + "]. The option is disabled!");
                    }
                }

                // if the option is missing from the file at all
                if (setting == null)
                    return settingName + " option not set. Using the default value.";
                return Html.Error(settingName + "=" + setting + " the option exists but it is disabled!");
            }

            return settingName + "=" + setting;
        }

        /// <summary>
        /// Enumerates all lines for given section.
        /// </summary>
        /// <param name="sectionName">Section to enum.</param>
        public String[] EnumSection(String sectionName)
        {
            ArrayList tmpArray = new ArrayList();

            foreach (SectionPair pair in keyPairs.Keys)
            {
                if (pair.Section == sectionName)
                    tmpArray.Add(pair.Key);
            }

            return (String[])tmpArray.ToArray(typeof(String));
        }


        #region ToString
        /// <summary>
        /// Opens the INI file from the given path.
        /// </summary>
        /// <param name="iniFile">Full path to INI file.</param>
        /// <param name="raw">Raw content or HTML prettified</param>
        /// <returns>String containing the content of the ini file</returns>
        public static string ToString(String iniFile, bool raw = false)
        {
            try
            {
                string content = File.ReadAllText(iniFile);
                //if we want a raw output
                if (raw)
                    return content;

                // html encode < and > so they don't break the html                
                content =
                    content.Replace("<", "&lt;").Replace(">", "&gt;")
                    .Replace("\r\n", Html.br)
                    .Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;")
                    .Replace("[", "<b>[").Replace("]", "]</b>");
                return content;
            }
            catch (FileNotFoundException)
            {
                Log.Error("File not found: " + iniFile);
                return ("File not found: " + iniFile);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.ToString();
            }
        }
        #endregion 
    }
}