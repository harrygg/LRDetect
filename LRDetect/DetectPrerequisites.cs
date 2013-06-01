using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// file 
using System.IO;
// fileinfo
using System.Diagnostics;

namespace LRDetect
{
    class DetectPrerequisites
    {
        #region Contstructor
        public DetectPrerequisites()
        { }
        #endregion

        //LR 11.50
        /// <summary>
        /// Check the Msi.dll version in C:\windows\system32 folder
        /// </summary>
        /// <returns></returns>
        public string GetWindowsInstallerVersion()
        {
            try
	        {
                string path = @"C:\windows\system32\msi.dll";
                FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                return fvi.ProductVersion.ToString();
	        }
	        catch (Exception ex)
	        {
                Log.Error(ex.ToString());
                return ex.Message;
	        }
        }
        public override string ToString()
        {
            return base.ToString();
        }
    }
}
