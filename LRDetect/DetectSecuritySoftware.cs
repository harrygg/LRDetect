using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
    class DetectSecuritySoftware
    {
        #region Upgrade codes
        //McAfee VirusScan Enterprise 
        private string[] upgradeCodes = 
        { 
            "38F747DBDC97B4E459142E21199F9D10", //AVG 2013 
            "06DD9E4F7F3FF9C41BC2BD64A2CE18FE", //AVG 2013 
            "9FF15957780018945A6265BC95AD719D", //McAfee VirusScan Enterprise
            "6503B6B43E448814680B00A323994B73", //McAfee Host Intrusion Prevention
            "26D13F39948E1D546B0106B5539504D9", //Microsoft Security Essentials
            "82D9ADE749FF8CF439F889EBE1D3F767", //Sophos Client Firewall
            "E932B7952303A1943A2218777329E5A8", //Sophos AV
            "79AA332A50D011E4585D700F695D0537", //ESET NOD32
            "F2FD14AAC93F01846915CEEB3011F45B" //Trend Micro
        };

        private List<String> productsCodes = new List<String>();
        private List<String> productsDetails = new List<String>();
        public int numberOfDetectedProducts = 0;
        #endregion

        //CONSTRUCTOR
        public DetectSecuritySoftware()
        {
            try
            {
                //get the product codes
                this.productsCodes = GetProductCodes();
                this.productsDetails = GetProductDetails();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }
        }

        private List<String> GetProductCodes()
        {
            List<String> productCodes = new List<string>();
            foreach (string guid in upgradeCodes)
            {
                string productCode = ProductInfo.GetProductCode(guid);
                if (productCode != null)
                    productCodes.Add(productCode);
            }
            return productCodes;
        }

        public List<String> GetProductDetails()
        {
            //Get the product names
            List<String> productDetails = new List<string>();
            foreach (string productCode in this.productsCodes)
            {
                string keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Installer\UserData\S-1-5-18\Products\" + productCode + @"\InstallProperties";
                string productName = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, keyPath, "DisplayName");
                //Get the product version
                if (productName != null)
                {
                    string productVersion = RegistryWrapper.GetValue(RegHive.HKEY_LOCAL_MACHINE, keyPath, "DisplayVersion");
                    this.numberOfDetectedProducts++;
                    productDetails.Add(productName + " " + productVersion);
                }
            }
            return productDetails;
        }


        #region Anti-virus & Firewall Products Detection
        //Works differently on different OS types. 
        //If we have a desktop OS we can simply query the AntiVirusProduct/FirewallProduct WMI object
        //Those objects however don't exist on Server OS. In such cases we query the Win32_Product class
        //and search for any names that match the most common security software
        public static string FirewallOnDesktopOSInfo()
        {
            return GetSecurityProductInfo("FirewallProduct");
        }

        public static string AntiVirusOnDesktopOSInfo()
        {
            return GetSecurityProductInfo("AntiVirusProduct");
        }
        // works only on Server OS
        //TODO class not found on server 2k3 because Win32_Product is not enabled by default
        //You can install it with Add/Remove Windows Component. Look for the
        //"WMI Windows Installer Provider" component under "Management and Monitoring Tools".
        public static string GetAntiVirusOnServerOSInfo()
        {
            //Query for common AV providers + 
            //{
            string where = " WHERE (Name LIKE \"%mcafee%\" OR Name LIKE \"%norton%\" OR Name LIKE \"%Nod32%\" OR Name LIKE \"%F-Secure%\""
                + " OR Name LIKE \"%Kaspersky%\" OR Name LIKE \"%Security Essentials%\" OR Name LIKE \"%Zonealarm%\" OR Name LIKE \"%BitDefender%\""
                + " OR Name LIKE \"%AVG%\" OR Name LIKE \"%Panda%\" OR Name LIKE \"%Comodo%\" OR Name LIKE '%Sophos%' OR Name LIKE '%virus%' OR Name LIKE '%Trend%')"
                + " AND (Name LIKE \"%virus%\" OR Name LIKE \"%secur%\" OR Name LIKE '%scan%')";

            //List<String> names = new List<string>();

            return Helper.QueryWMI("Name", @"root\CIMV2", "Win32_Product", where);
        }
        #endregion


        #region WMI queries to get information for installed Security products
        /// <summary>
        /// used in OSInfo.FirewallOnDesktopOSInfo and OSInfo.AntiVirusOnDesktopOSInfo
        /// </summary>
        /// <param name="wmiClass">AntiVirusProduct or FirwallProduct</param>
        /// <returns></returns>
        public static string GetSecurityProductInfo(string wmiClass)
        {
            try
            {
                string output = String.Empty;
                if (OSInfo.isOSDesktopEdition)
                {
                    string scope = @"root\SecurityCenter2";
                    if (OSInfo.OSVersion < new Version(6, 0))
                        scope = @"root\SecurityCenter";

                    string name = "displayName";

                    output = Helper.QueryWMI(name, scope, wmiClass);
                }
                return output != null ? output : Html.ErrorMsg();
            }
            catch (Exception ex)
            {
                Log.Error("Error in Antivirus/Firewall detection. " + ex.ToString());
                return Html.ErrorMsg();
            }
        }
        #endregion

        #region Construct the output ToString()
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            try
            {
                StringBuilder output = new StringBuilder();

                foreach (string product in this.productsDetails)
                {
                    output.Append(product + Html.br);
                }
                return output.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                return ex.Message;
            }
        }

        #endregion
    }
}
