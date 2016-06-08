using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace LRDetect
{
  sealed class WinTrust
  {
    static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
    // GUID of the action to perform
    const string WINTRUST_ACTION_GENERIC_VERIFY_V2 = "{00AAC56B-CD44-11d0-8CC2-00C04FC295EE}";

    [DllImport("wintrust.dll", ExactSpelling = true, SetLastError = false, CharSet = CharSet.Unicode)]
    static extern WinVerifyTrustResult WinVerifyTrust(
        [In] IntPtr hwnd,
        [In] [MarshalAs(UnmanagedType.LPStruct)] Guid pgActionID,
        [In] WinTrustData pWVTData
    );

    // call WinTrust.WinVerifyTrust() to check embedded file signature
    static WinVerifyTrustResult VerifyEmbeddedSignature(string fileName)
    {
      WinVerifyTrustResult result;
      try
      {
        //If OS is 64 bit, we want to disable the WOW64 redirection, as LRDetect is 32bit process and it will be redirected to 
        //C:\Windows\SystemWOW64 instead of C:\Windows\System32 folder
        Helper.Wow64DisableWow64FsRedirection();

        WinTrustData wtd = new WinTrustData(fileName);
        Guid guidAction = new Guid(WINTRUST_ACTION_GENERIC_VERIFY_V2);
        result = WinVerifyTrust(INVALID_HANDLE_VALUE, guidAction, wtd);
        Logger.Debug("WinVerifyTrustResut: " + result.ToString());
      }
      finally
      {
        Helper.Wow64RevertWow64FsRedirection();
      }
      return result;
    }

    public static string IsFileSignedInfo(string fileName)
    {
      WinVerifyTrustResult result = VerifyEmbeddedSignature(fileName);
      return result == WinVerifyTrustResult.NO_ERROR ? Html.Yes : Html.cNo + " " + result.ToString();
    }

    WinTrust() { }

  }

  #region enum WinVerifyTrustResult
  enum WinVerifyTrustResult : uint
  {
    NO_ERROR = 0, //"The operation completed successfully."
    ERROR_FILE_NOT_FOUND = 0x00000002,
    TRUST_E_PROVIDER_UNKNOWN = 0x800b0001,           // Trust provider is not recognized on this system
    TRUST_E_ACTION_UNKNOWN = 0x800b0002,         // Trust provider does not support the specified action
    TRUST_E_SUBJECT_FORM_UNKNOWN = 0x800b0003,        // Trust provider does not support the form specified for the subject
    TRUST_E_SUBJECT_NOT_TRUSTED = 0x800b0004,         // Subject failed the specified verification action
    TRUST_E_NOSIGNATURE = 0x800B0100,         // TRUST_E_NOSIGNATURE - File was not signed
    TRUST_E_EXPLICIT_DISTRUST = 0x800B0111,   // Signer's certificate is in the Untrusted Publishers store
    TRUST_E_BAD_DIGEST = 0x80096010,    // TRUST_E_BAD_DIGEST - file was probably corrupt
    CERT_E_EXPIRED = 0x800B0101,        // CERT_E_EXPIRED - Signer's certificate was expired
    CERT_E_REVOKED = 0x800B010C,     // CERT_E_REVOKED Subject's certificate was revoked
    CERT_E_UNTRUSTEDROOT = 0x800B0109,          // CERT_E_UNTRUSTEDROOT - A certification chain processed correctly but terminated in a root certificate that is not trusted by the trust provider.
    CRYPT_E_FILE_ERROR = 0x80092003 //CRYPT_E_FILE_ERROR,"An error occurred while reading or writing to a file."
  }
  #endregion

  #region structure WinTrustData
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    class WinTrustData
    {
      UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustData));
      IntPtr PolicyCallbackData = IntPtr.Zero;
      IntPtr SIPClientData = IntPtr.Zero;
      // required: UI choice
      WinTrustDataUIChoice UIChoice = WinTrustDataUIChoice.None;
      // required: certificate revocation check options
      WinTrustDataRevocationChecks RevocationChecks = WinTrustDataRevocationChecks.None;
      // required: which structure is being passed in?
      WinTrustDataChoice UnionChoice = WinTrustDataChoice.File;
      // individual file
      IntPtr FileInfoPtr;
      WinTrustDataStateAction StateAction = WinTrustDataStateAction.Ignore;
      IntPtr StateData = IntPtr.Zero;
      String URLReference = null;
      WinTrustDataProvFlags ProvFlags = WinTrustDataProvFlags.RevocationCheckChainExcludeRoot;
      WinTrustDataUIContext UIContext = WinTrustDataUIContext.Execute;

      // constructor for silent WinTrustDataChoice.File check
      public WinTrustData(string fileName)
      {
        // On Win7SP1+, don't allow MD2 or MD4 signatures
        if ((Environment.OSVersion.Version.Major > 6) ||
            ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor > 1)) ||
            ((Environment.OSVersion.Version.Major == 6) && (Environment.OSVersion.Version.Minor == 1) && !String.IsNullOrEmpty(Environment.OSVersion.ServicePack)))
        {
          ProvFlags |= WinTrustDataProvFlags.DisableMD2andMD4;
        }

        WinTrustFileInfo wtfiData = new WinTrustFileInfo(fileName);
        FileInfoPtr = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(WinTrustFileInfo)));
        Marshal.StructureToPtr(wtfiData, FileInfoPtr, false);

      }

      ~WinTrustData()
      {
        Marshal.FreeCoTaskMem(FileInfoPtr);
      }
    }
    #endregion

  #region WinTrustData struct field enums
  enum WinTrustDataUIChoice : uint
  {
    All = 1,
    None = 2,
    NoBad = 3,
    NoGood = 4
  }

  enum WinTrustDataRevocationChecks : uint
  {
    None = 0x00000000,
    WholeChain = 0x00000001
  }

  enum WinTrustDataChoice : uint
  {
    File = 1,
    Catalog = 2,
    Blob = 3,
    Signer = 4,
    Certificate = 5
  }

  enum WinTrustDataStateAction : uint
  {
    Ignore = 0x00000000,
    Verify = 0x00000001,
    Close = 0x00000002,
    AutoCache = 0x00000003,
    AutoCacheFlush = 0x00000004
  }

  [FlagsAttribute]
  enum WinTrustDataProvFlags : uint
  {
    UseIe4TrustFlag = 0x00000001,
    NoIe4ChainFlag = 0x00000002,
    NoPolicyUsageFlag = 0x00000004,
    RevocationCheckNone = 0x00000010,
    RevocationCheckEndCert = 0x00000020,
    RevocationCheckChain = 0x00000040,
    RevocationCheckChainExcludeRoot = 0x00000080,
    SaferFlag = 0x00000100,        // Used by software restriction policies. Should not be used.
    HashOnlyFlag = 0x00000200,
    UseDefaultOsverCheck = 0x00000400,
    LifetimeSigningFlag = 0x00000800,
    CacheOnlyUrlRetrieval = 0x00001000,      // affects CRL retrieval and AIA retrieval
    DisableMD2andMD4 = 0x00002000      // Win7 SP1+: Disallows use of MD2 or MD4 in the chain except for the root 
  }

  enum WinTrustDataUIContext : uint
  {
    Execute = 0,
    Install = 1
  }
  #endregion

  #region WinTrustFileInfo structures
  /// <summary>
  /// The WINTRUST_FILE_INFO structure is used when calling WinVerifyTrust to verify an individual file.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  class WinTrustFileInfo
  {
    //Count of bytes in this structure.
    UInt32 StructSize = (UInt32)Marshal.SizeOf(typeof(WinTrustFileInfo));

    // required, full path and file name of the file to be verified. 
    IntPtr pszFilePath;

    // Optional. File handle to the open file to be verified. This handle must be to a file that has at least read permission. This member can be set to NULL.
    IntPtr hFile = IntPtr.Zero;

    //Optional. Pointer to a GUID structure that specifies the subject type. This member can be set to NULL.
    IntPtr pgKnownSubject = IntPtr.Zero;

    public WinTrustFileInfo(string filePath)
    {
      pszFilePath = Marshal.StringToCoTaskMemAuto(filePath);
    }

    ~WinTrustFileInfo()
    {
      Marshal.FreeCoTaskMem(pszFilePath);
    }
  }
  #endregion

  
}
