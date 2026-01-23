using System.Net;

namespace Edi.AspNetCore.Utils;

/// <summary>
/// Provides security utilities for ASP.NET Core applications.
/// </summary>
public class SecurityHelper
{
    /// <summary>
    /// Sanitizes a URL by validating it against common security threats such as open redirects,
    /// localhost access, and private IP addresses.
    /// </summary>
    /// <param name="rawUrl">The raw URL to sterilize.</param>
    /// <returns>
    /// The sanitized URL if valid; otherwise, returns "#" for invalid or potentially dangerous URLs.
    /// </returns>
    public static string SterilizeLink(string rawUrl)
    {
        bool IsUnderLocalSlash()
        {
            // Allows "/" or "/foo" but not "//" or "/\".
            if (rawUrl[0] == '/')
            {
                // url is exactly "/"
                if (rawUrl.Length == 1)
                {
                    return true;
                }

                // url doesn't start with "//" or "/\"
                return rawUrl[1] is not '/' and not '\\';
            }

            return false;
        }

        string invalidReturn = "#";
        if (string.IsNullOrWhiteSpace(rawUrl))
        {
            return invalidReturn;
        }

        if (!rawUrl.IsValidUrl())
        {
            return IsUnderLocalSlash() ? rawUrl : invalidReturn;
        }

        var uri = new Uri(rawUrl);
        if (uri.IsLoopback)
        {
            // localhost, 127.0.0.1
            return invalidReturn;
        }

        if (uri.HostNameType == UriHostNameType.IPv4)
        {
            // Disallow LAN IP (e.g. 192.168.0.1, 10.0.0.1)
            if (IsPrivateIP(uri.Host))
            {
                return invalidReturn;
            }
        }

        return rawUrl;
    }

    /// <summary>
    /// Test an IPv4 address is LAN or not.
    /// </summary>
    /// <param name="ip">IPv4 address</param>
    /// <returns>bool</returns>
    public static bool IsPrivateIP(string ip) => IPAddress.Parse(ip).GetAddressBytes() switch
    {
        // Regex.IsMatch(ip, @"(^127\.)|(^10\.)|(^172\.1[6-9]\.)|(^172\.2[0-9]\.)|(^172\.3[0-1]\.)|(^192\.168\.)")
        // Regex has bad performance, this is better

        var x when x[0] is 192 && x[1] is 168 => true,
        var x when x[0] is 10 => true,
        var x when x[0] is 127 => true,
        var x when x[0] is 172 && x[1] is >= 16 and <= 31 => true,
        _ => false
    };
}
