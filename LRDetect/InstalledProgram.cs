using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LRDetect
{
  public class InstalledProgram : IComparable<InstalledProgram>, IEquatable<InstalledProgram>
  {
    public string DisplayName { get { return displayName; } set { displayName = value; } }
    string displayName;

    public string DisplayVersion { get { return displayVersion; } set { displayVersion = value; } }
    string displayVersion;

    public string UninstallString { get { return uninstallString; } set { uninstallString = value; } }
    string uninstallString;

    public string InstallDate { get { return installDate; } set { installDate = value; } }
    string installDate;

    public static string uninstallerKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    public override string ToString()
    {
      return String.Format("{0} {1}", Html.B(DisplayName), displayVersion);
    }

    public InstalledProgram(string ProgramDisplayName)
    {
      displayName = ProgramDisplayName;
    }

    public InstalledProgram(string ProgramDisplayName, string ProgramVersion, string ProgramUnisntallString, string InstallDateString)
    {
      displayName = ProgramDisplayName;
      displayVersion = ProgramVersion;
      uninstallString = ProgramUnisntallString;
      installDate = InstallDateString;
    }

    //Sorting function, required by IComparable interface
    public int CompareTo(InstalledProgram other)
    {
      return String.Compare(DisplayName, other.DisplayName);
    }

    //Equality function, required by IEquatable interface
    bool IEquatable<InstalledProgram>.Equals(InstalledProgram other)
    {
      return (DisplayName == other.DisplayName && DisplayVersion == other.DisplayVersion) ? true : false;
    }
  }
}
