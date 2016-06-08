using System;
using System.Runtime.InteropServices;
using System.Text;
//debug.write
using System.Diagnostics;
//list<t>
using System.Collections.Generic;
// registries
using Microsoft.Win32;
//##############################################
//# Emulate the .NET4 RegistryView.Registry64  #
//##############################################

namespace LRDetect
{
    /// <summary>
    /// Registry wrapper to access the 64/32 bit registries.
    /// TODO remove it in .NET 4
    /// </summary>
    public static class RegistryWrapper
    {
        #region Member Variables
        [DllImport("Advapi32.dll")]
        static extern uint RegOpenKeyEx(
            UIntPtr hKey,
            string lpSubKey,
            uint ulOptions,
            int samDesired,
            out int phkResult);

        [DllImport("Advapi32.dll")]
        static extern uint RegCloseKey(int hKey);

        [DllImport("advapi32.dll", EntryPoint = "RegQueryValueEx")]
        public static extern int RegQueryValueEx(
            int hKey, 
            string lpValueName,
            int lpReserved,
            ref RegistryValueKind lpType,
            IntPtr lpData,
            ref int lpcbData);

        [DllImport("advapi32.dll", EntryPoint = "RegEnumValue")]
        private static extern int RegEnumValue(
            int hkey,
            uint index,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            IntPtr reserved,
            IntPtr lpType,
            IntPtr lpData,
            IntPtr lpcbData);

        [DllImport("advapi32.dll", EntryPoint = "RegEnumKeyEx")]
        private static extern int RegEnumKeyEx(
            int hkey,
            uint index,
            StringBuilder lpValueName,
            ref uint lpcchValueName,
            IntPtr reserved,
            IntPtr lpType,
            IntPtr lpData,
            IntPtr lpcbData);

        [DllImport("advapi32.dll", EntryPoint = "RegGetValue")]
        private static extern int RegGetValue(
            int hkey,
            String lpSubKey,
            String lpValue,
            int dwFlags,
            ref uint pdwType,
            StringBuilder pvData,
            ref uint pcbData);
         

        #endregion

        #region Methods

        /// <summary>
        /// Method to find a registry value. 
        /// 1. It uses .NET 3.5 GetValue() method
        /// 2. If nothing is found it uses the custom GetRegKey64 method to search the registry 
        /// and escape the Registry Redirection on 64bit machines,
        /// 3) If still nothing is found we use the custom GetRegKey32 to search the WOW6432Node registries
        /// </summary>
        /// <param name="inHive">i.e. RegHive.HKEY_LOCAL_MACHINE</param>
        /// <param name="registryPath">Path to registry key</param>
        /// <param name="keyName">Name of the wero</param>
        /// <returns></returns>
        public static string GetValue(UIntPtr inHive, string registryPath, string keyName, string expected = null)
        {
          string value = RegistryWrapper.GetRegKey64(inHive, registryPath, keyName);
          //first we'll check the 64bit registry - 
          //if nothing is found we'll check the 32bit WOW6432Node registry
          if (value == null)
            value = RegistryWrapper.GetRegKey32(inHive, registryPath, keyName);

          // if nothing is found or the value is empty and no default value is specified (expected == null) then return null otherwise return the found or default value
          return (value == null || value == "") ? expected : value;
        }
        /// <summary>
        /// Method to get registry key value from 64bit registries
        /// </summary>
        /// <param name="inHive">i.e. RegHive.HKEY_LOCAL_MACHINE</param>
        /// <param name="inKeyName">Name of the key</param>
        /// <param name="inPropertyName">Name of the Value</param>
        /// <returns></returns>
        public static string GetRegKey64(UIntPtr inHive, String inKeyName, String inPropertyName)
        {
            return GetRegKey(inHive, inKeyName, RegSAM.WOW64_64Key, inPropertyName);
        }
        /// <summary>
        /// Method to get registry value from WOW6432Node
        /// </summary>
        /// <param name="inHive">i.e. RegHive.HKEY_LOCAL_MACHINE</param>
        /// <param name="inKeyName">Name of the key</param>
        /// <param name="inPropertyName">Name of the Value</param>
        /// <returns></returns>
        public static string GetRegKey32(UIntPtr inHive, String inKeyName, String inPropertyName)
        {
            return GetRegKey(inHive, inKeyName, RegSAM.WOW64_32Key, inPropertyName);
        }
        /// <summary>
        /// Method to enumerate the values for the specified open registry key.
        /// </summary>
        /// <param name="inHive">i.e. RegHive.HKEY_LOCAL_MACHINE</param>
        /// <param name="inKeyName">Name of the key</param>
        /// <returns></returns>
        public static List<string> GetValueNames(UIntPtr inHive, String inKeyName)
        {
          List<string> valueNames = GetValueNames(inHive, inKeyName, RegSAM.WOW64_64Key);
          // If the key is not found in 64bit registry we will search the 32bit ones Wow6432Node
          return (valueNames == null) ? GetValueNames(inHive, inKeyName, RegSAM.WOW64_32Key) : valueNames;
        }
        /// <summary>
        /// Method to enumerate the first value for the specified open registry key.
        /// </summary>
        /// <param name="inHive">i.e. RegHive.HKEY_LOCAL_MACHINE</param>
        /// <param name="inKeyName">Name of the key</param>
        /// <returns>Null if not found</returns>
        public static string GetFirstValueName(UIntPtr inHive, String inKeyName)
        {
            List<string> valueName = GetValueNames(inHive, inKeyName, RegSAM.WOW64_64Key, true);
            if (valueName == null)
                valueName = GetValueNames(inHive, inKeyName, RegSAM.WOW64_32Key, true);

            return valueName == null ? null : valueName[0];
            //List<string> valueName = (OSInfo.is64BitOperatingSystem) ? GetValueNames(inHive, inKeyName, RegSAM.WOW64_64Key, true) : GetValueNames(inHive, inKeyName, RegSAM.WOW64_32Key, true);
            //return valueName[0];
        }
        /// <summary>
        /// Method to retrieve the data for the specified value name associated with the opened registry key.
        /// </summary>
        /// <param name="inHive">i.e. RegHive.HKEY_LOCAL_MACHINE</param>
        /// <param name="inKeyName"></param>
        /// <param name="in32or64key"></param>
        /// <param name="keyName">the name of the key to search or empty value to check for the existance of the key</param>
        /// <returns>
        /// the method returns null if the registry key doesn't exists
        /// if we don't specify keyName the method returns "value not set" if the key exists.
        /// if we specify a keyName the keyName value is returned
        /// </returns>
        public static string GetRegKey(UIntPtr inHive, String inKeyName, RegSAM in32or64key, String keyName)
        {
            int keyHandle = 0;
            try
            {
                uint result = RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out keyHandle);
                if (keyName == "(Default)" && result == 0)
                    return "value not set";
                if (result != 0)
                    return null;

                RegistryValueKind type = RegistryValueKind.Unknown;
                int size = 1024;
                IntPtr value = Marshal.AllocHGlobal(size);
                int retVal = RegQueryValueEx(keyHandle, keyName, 0, ref type, value, ref size);

                switch (type)
                {
                    case RegistryValueKind.String:
                        return Marshal.PtrToStringAnsi(value);
                  case RegistryValueKind.ExpandString:
                        return Marshal.PtrToStringAnsi(value);
                    case RegistryValueKind.DWord:
                        return Marshal.ReadInt32(value).ToString();
                    case RegistryValueKind.QWord:
                        return Marshal.ReadInt64(value).ToString();
                }
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (keyHandle != 0) 
                    RegCloseKey(keyHandle);
            }
        }
        /// <summary>
        /// Method to enumerates the values for the specified open registry key.
        /// </summary>
        /// <param name="inHive">i.e. RegHive.HKEY_LOCAL_MACHINE</param>
        /// <param name="inKeyName">Path to registry key</param>
        /// <param name="in32or64key">Indicates that an application on 64-bit Windows should operate on the 32-bit registry view. Example RegSAM.WOW64_32Key</param>
        /// <param name="getOnlyFirstValue">Enumerate only the first registry value - used in ProductInfo.getProductCode()</param>
        /// <returns>Returns a String List with enumerated registry values</returns>
        public static List<string> GetValueNames(UIntPtr inHive, String inKeyName, RegSAM in32or64key, bool getOnlyFirstValue = false)
        {
            int hkey = 0;
            try
            {
                List<string> valueNames = new List<string>();
                uint lResult = RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out hkey);
                if (lResult != 0) 
                    return null;
                //A pointer to a buffer that receives the name of the subkey, including the terminating null character. 
                StringBuilder valueName = new StringBuilder(64);
                //A pointer to a variable that specifies the size of the buffer specified by the lpName parameter, in characters.
                uint lpcchValueName = 64;
                // If getOnlyFirstValue is true we need to enumerate only the first value 
                // Because values are not ordered RegEnumValue may return values in any order.
                if (getOnlyFirstValue)
                {
                    RegEnumValue(hkey, 0, valueName, ref lpcchValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                    valueNames.Add(valueName.ToString());
                }
                // if getOnlyFirstValue is false we need to enumerate all registry values
                else
                {
                    uint dwIndex = 0;
                    // RegEnumValue returns 0 (ERROR_SUCCESS) on success and 259 (ERROR_NO_MORE_ITEMS) 
                    while (RegEnumValue(hkey, dwIndex, valueName, ref lpcchValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero) != 259)
                    {
                        //result = RegEnumValue(hkey, i, valueName, ref lpcchValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                        if (valueName.ToString() != "Patches")
                            valueNames.Add(valueName.ToString());
                        dwIndex++;
                        // lpcchValueName is set to 32 so we need to increment it in order to add the termination char
                        lpcchValueName++;
                    }
                }
                return valueNames;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }
      /*
        static public string GetFirstKeyName(UIntPtr inHive, String inKeyName, RegSAM in32or64key)
        {
            //A handle to an open registry key. 
            int hkey = 0;

            try
            {
                uint lResult = RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.QueryValue | (int)in32or64key, out hkey);
                //uint lResult = RegEnumKeyEx(inHive, inKeyName, 0, (int)RegSAM.EnumerateSubKeys, out hkey);
                if (lResult != 0) 
                    return null;
                //A pointer to a buffer that receives the name of the subkey, including the terminating null character. 
                StringBuilder valueName = new StringBuilder(1024);
                //A pointer to a variable that specifies the size of the buffer specified by the lpName parameter, in characters.
                uint lpcchValueName = 1024;
                int result = RegEnumValue(hkey, 0, valueName, ref lpcchValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                string retVal = valueName.ToString();
                return retVal;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
            finally
            {
                if (hkey != 0) 
                    RegCloseKey(hkey);
            }
        }*/
        /// <summary>
        /// Method to enumerates the values for the specified open registry key.
        /// </summary>
        /// <param name="inHive">Registry Hive to look up</param>
        /// <param name="inKeyName">Path to registry key</param>
        /// <param name="in32or64key">Indicates that an application on 64-bit Windows should operate on the 32-bit registry view.</param>
        /// <param name="getOnlyFirstValue">Enumerate only the first registry value - used in ProductInfo.getProductCode()</param>
        /// <returns>Returns a String List with enumerated registry values</returns>
        public static List<string> GetSubKeyNames(UIntPtr inHive, String inKeyName, RegSAM in32or64key)
        {
            int hkey = 0;
            try
            {
                var subKeyNames = new List<string>();
                uint lResult = RegOpenKeyEx(inHive, inKeyName, 0, (int)RegSAM.EnumerateSubKeys | (int)in32or64key, out hkey);
                if (lResult != 0)
                    return null;
                //A pointer to a buffer that receives the name of the subkey, including the terminating null character. 
                StringBuilder subKeyName = new StringBuilder(64);
                //A pointer to a variable that specifies the size of the buffer specified by the lpName parameter, in characters.
                uint lpcchValueName = 64;
                // if getOnlyFirstValue is false we need to enumerate all registry values
                uint dwIndex = 0;
                // RegEnumValue result returns 0 (ERROR_SUCCESS) on success and 259 (ERROR_NO_MORE_ITEMS) 
                int result = 0;
                
                while ((result = RegEnumKeyEx(hkey, dwIndex, subKeyName, ref lpcchValueName, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero)) != 259)
                {
                    subKeyNames.Add(subKeyName.ToString());
                    dwIndex++;
                    // lpcchValueName now holds the size of the subKeyName so we need to set it back to 64
                    lpcchValueName = 64;
                }
                return subKeyNames;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                return null;
            }
        }
        #endregion

      internal static bool IsKeyExist(UIntPtr uIntPtr, string keyPath, RegSAM in32or64key)
      {
        string key = RegistryWrapper.GetRegKey(RegHive.LocalMachine, keyPath, in32or64key, "(Default)");
        return (key != null && key == "value not set") ? true : false;
      }
    }

    #region Enum RegSam
    public enum RegSAM
    {
        QueryValue = 0x0001,
        SetValue = 0x0002,
        CreateSubKey = 0x0004,
        EnumerateSubKeys = 0x0008,
        Notify = 0x0010,
        CreateLink = 0x0020,
        // Indicates that an application on 64-bit Windows should operate on the 32-bit registry view. 
        // This flag is ignored by 32-bit Windows. For more information, see Accessing an Alternate Registry View.
        // This flag must be combined using the OR operator with the other flags in this table that either query or access registry values.
        WOW64_32Key = 0x0200,
        //Indicates that an application on 64-bit Windows should operate on the 64-bit registry view.
        WOW64_64Key = 0x0100,
        WOW64_Res = 0x0300,
        Read = 0x00020019,
        Write = 0x00020006,
        Execute = 0x00020019,
        AllAccess = 0x000f003f
    }
    #endregion

    public static class RegHive
    {
        public static UIntPtr ClassesRoot = new UIntPtr(0x80000000u);
        public static UIntPtr CurrentUser = new UIntPtr(0x80000001u);
        public static UIntPtr LocalMachine = new UIntPtr(0x80000002u);
        public static UIntPtr Users = new UIntPtr(0x80000003u);
    }
}
