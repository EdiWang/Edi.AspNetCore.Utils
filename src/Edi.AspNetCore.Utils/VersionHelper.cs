using Microsoft.Win32;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Edi.AspNetCore.Utils;

/// <summary>
/// Provides utilities for retrieving application version information and operating system details.
/// </summary>
public static partial class VersionHelper
{
    private static readonly Assembly _entryAssembly = Assembly.GetEntryAssembly();
    private static readonly string _fileVersion = _entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
    private static readonly string _informationalVersion = _entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

    /// <summary>
    /// Compiled regular expression for matching non-stable version keywords.
    /// </summary>
    /// <returns>A regex that matches common pre-release version identifiers.</returns>
    [GeneratedRegex(@"\b(preview|beta|rc|debug|alpha|test|canary|nightly)\b", RegexOptions.IgnoreCase)]
    private static partial Regex NonStableVersionRegex();

    /// <summary>
    /// Gets the basic application version from the assembly file version attribute.
    /// </summary>
    /// <value>
    /// The file version of the entry assembly, or "N/A" if not available.
    /// </value>
    public static string AppVersionBasic => _fileVersion ?? "N/A";

    /// <summary>
    /// Gets the formatted application version with optional git hash information.
    /// </summary>
    /// <value>
    /// The informational version if available, formatted to show a shortened git hash in parentheses.
    /// If the informational version contains a git hash (indicated by a '+' character),
    /// the hash is truncated to 6 characters and displayed in the format: "version (githash)".
    /// Falls back to <see cref="AppVersionBasic"/> if informational version is not available.
    /// </value>
    public static string AppVersion
    {
        get
        {
            if (_informationalVersion is null)
                return AppVersionBasic;

            var plusIndex = _informationalVersion.IndexOf('+');
            if (plusIndex <= 0)
                return _informationalVersion;

            var gitHash = _informationalVersion.AsSpan()[(plusIndex + 1)..];
            var prefix = _informationalVersion.AsSpan()[..plusIndex];

            if (gitHash.Length <= 6)
                return _informationalVersion;

            var gitHashShort = gitHash[..6];
            return gitHashShort.IsEmpty ? AppVersionBasic : $"{prefix} ({gitHashShort})";
        }
    }

    /// <summary>
    /// Determines whether the current application version is a non-stable (pre-release) version.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the version contains keywords indicating a pre-release version 
    /// (preview, beta, rc, debug, alpha, test, canary, nightly); otherwise, <c>false</c>.
    /// </returns>
    public static bool IsNonStableVersion() => NonStableVersionRegex().IsMatch(AppVersion);

    /// <summary>
    /// Attempts to retrieve the full operating system version including build number and update build revision.
    /// </summary>
    /// <returns>
    /// For Windows systems, returns the product name and full version including UBR (Update Build Revision) if available.
    /// For non-Windows systems, returns the standard OS version string.
    /// If registry access fails on Windows, falls back to the standard OS version string.
    /// </returns>
    /// <remarks>
    /// On Windows, this method reads from the registry at "SOFTWARE\Microsoft\Windows NT\CurrentVersion"
    /// to get the ProductName and UBR (Update Build Revision) for a more detailed version string.
    /// Example output: "Microsoft Windows 11 Pro 10.0.22631.4317"
    /// </remarks>
    public static string TryGetFullOSVersion()
    {
        var osVer = Environment.OSVersion;
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return osVer.VersionString;

        try
        {
            var currentVersion = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
            if (currentVersion != null)
            {
                var name = currentVersion.GetValue("ProductName", "Microsoft Windows NT");
                var ubr = currentVersion.GetValue("UBR", string.Empty).ToString();
                if (!string.IsNullOrWhiteSpace(ubr))
                {
                    return $"{name} {osVer.Version.Major}.{osVer.Version.Minor}.{osVer.Version.Build}.{ubr}";
                }
            }
        }
        catch
        {
            return osVer.VersionString;
        }

        return osVer.VersionString;
    }
}
