using Microsoft.AspNetCore.Http;

namespace Edi.AspNetCore.Utils;

/// <summary>
/// Provides utilities for extracting client IP addresses from HTTP requests in ASP.NET Core applications.
/// Handles various proxy headers and forwarded IP scenarios commonly used by load balancers and CDNs.
/// </summary>
public static class ClientIPHelper
{
    /// <summary>
    /// Extracts the client's IP address from the HTTP context, considering various proxy headers.
    /// </summary>
    /// <param name="context">The HTTP context containing request information.</param>
    /// <returns>
    /// The client's IP address as a string, or null if the context or connection is invalid.
    /// Returns the first valid public IP address found in proxy headers, otherwise returns the remote IP address.
    /// </returns>
    /// <remarks>
    /// This method checks for forwarded headers in order of preference:
    /// <list type="bullet">
    /// <item><description>X-Azure-ClientIP (Azure Front Door)</description></item>
    /// <item><description>CF-Connecting-IP (Cloudflare)</description></item>
    /// <item><description>X-Forwarded-For (Standard proxy header)</description></item>
    /// <item><description>X-Real-IP (Nginx proxy)</description></item>
    /// <item><description>X-Client-IP (Apache proxy)</description></item>
    /// <item><description>True-Client-IP (Akamai and Cloudflare Enterprise)</description></item>
    /// <item><description>HTTP_X_FORWARDED_FOR (IIS)</description></item>
    /// <item><description>HTTP_CLIENT_IP (Alternative)</description></item>
    /// </list>
    /// If no valid public IP is found in headers, falls back to the connection's remote IP address.
    /// </remarks>
    public static string GetClientIP(HttpContext context)
    {
        if (context?.Connection?.RemoteIpAddress == null)
            return null;

        var existingHeaderName = DomainDataHelper.GetAppDomainData<string>("ForwardedHeaders_HeaderName");
        if (existingHeaderName != null)
        {
            return context?.Connection?.RemoteIpAddress.ToString();
        }

        // Check for forwarded headers in order of preference
        var forwardedHeaders = new[]
        {
            "X-Azure-ClientIP",        // Azure Front Door
            "CF-Connecting-IP",        // Cloudflare
            "X-Forwarded-For",         // Standard proxy header
            "X-Real-IP",               // Nginx proxy
            "X-Client-IP",             // Apache proxy
            "True-Client-IP",          // Akamai and Cloudflare Enterprise
            "HTTP_X_FORWARDED_FOR",    // IIS
            "HTTP_CLIENT_IP"           // Alternative
        };

        foreach (var header in forwardedHeaders)
        {
            var headerValue = context.Request.Headers[header].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                // Handle comma-separated IPs (X-Forwarded-For can contain multiple IPs)
                var ips = headerValue.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var ip in ips)
                {
                    var trimmedIp = ip.Trim();
                    if (IsValidPublicIP(trimmedIp))
                    {
                        return trimmedIp;
                    }
                }
            }
        }

        // Fallback to connection remote IP
        var remoteIp = context.Connection.RemoteIpAddress?.ToString();
        return IsValidPublicIP(remoteIp) ? remoteIp : remoteIp;
    }

    /// <summary>
    /// Validates whether the specified IP address is a valid public IP address.
    /// </summary>
    /// <param name="ipAddress">The IP address string to validate.</param>
    /// <returns>
    /// <c>true</c> if the IP address is a valid public IP address; otherwise, <c>false</c>.
    /// Returns <c>false</c> for private, loopback, link-local, and other special-use IP addresses.
    /// </returns>
    /// <remarks>
    /// For IPv4, excludes the following private ranges:
    /// <list type="bullet">
    /// <item><description>10.0.0.0/8 (Private network)</description></item>
    /// <item><description>172.16.0.0/12 (Private network)</description></item>
    /// <item><description>192.168.0.0/16 (Private network)</description></item>
    /// <item><description>169.254.0.0/16 (Link-local)</description></item>
    /// <item><description>127.0.0.0/8 (Loopback)</description></item>
    /// <item><description>0.0.0.0/8 (This network)</description></item>
    /// </list>
    /// For IPv6, excludes link-local, site-local, multicast, loopback, and unspecified addresses.
    /// </remarks>
    private static bool IsValidPublicIP(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress) || !System.Net.IPAddress.TryParse(ipAddress, out var ip))
            return false;

        // Exclude private IP ranges and special addresses
        var bytes = ip.GetAddressBytes();

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // IPv4 private ranges
            return !(
                bytes[0] == 10 ||                                    // 10.0.0.0/8
                bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31 || // 172.16.0.0/12
                bytes[0] == 192 && bytes[1] == 168 ||              // 192.168.0.0/16
                bytes[0] == 169 && bytes[1] == 254 ||              // 169.254.0.0/16 (link-local)
                bytes[0] == 127 ||                                   // 127.0.0.0/8 (loopback)
                bytes[0] == 0                                        // 0.0.0.0/8
            );
        }

        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            // IPv6 private/special ranges
            return !(
                ip.IsIPv6LinkLocal ||
                ip.IsIPv6SiteLocal ||
                ip.IsIPv6Multicast ||
                System.Net.IPAddress.IsLoopback(ip) ||
                ip.Equals(System.Net.IPAddress.IPv6Any) ||
                ip.Equals(System.Net.IPAddress.IPv6None)
            );
        }

        return false;
    }
}
