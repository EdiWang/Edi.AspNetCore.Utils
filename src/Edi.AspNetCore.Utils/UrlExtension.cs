using System.Net;

namespace Edi.AspNetCore.Utils;

/// <summary>
/// Provides extension methods for URL manipulation and validation.
/// </summary>
public static class UrlExtension
{
    /// <summary>
    /// Specifies the URL scheme types for validation.
    /// </summary>
    public enum UrlScheme
    {
        /// <summary>
        /// HTTP scheme only.
        /// </summary>
        Http,
        
        /// <summary>
        /// HTTPS scheme only.
        /// </summary>
        Https,
        
        /// <summary>
        /// Both HTTP and HTTPS schemes.
        /// </summary>
        All
    }

    /// <summary>
    /// Validates whether the specified string is a valid URL with the given scheme.
    /// </summary>
    /// <param name="url">The URL string to validate.</param>
    /// <param name="urlScheme">The URL scheme to validate against. Default is <see cref="UrlScheme.All"/>.</param>
    /// <returns><c>true</c> if the URL is valid and matches the specified scheme; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when an invalid <paramref name="urlScheme"/> value is provided.</exception>
    public static bool IsValidUrl(this string url, UrlScheme urlScheme = UrlScheme.All)
    {
        var isValidUrl = Uri.TryCreate(url, UriKind.Absolute, out var uriResult);
        if (!isValidUrl) return false;

        isValidUrl &= urlScheme switch
        {
            UrlScheme.All => uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == Uri.UriSchemeHttp,
            UrlScheme.Https => uriResult.Scheme == Uri.UriSchemeHttps,
            UrlScheme.Http => uriResult.Scheme == Uri.UriSchemeHttp,
            _ => throw new ArgumentOutOfRangeException(nameof(urlScheme), urlScheme, null)
        };
        return isValidUrl;
    }

    /// <summary>
    /// Combines a base URL with a path segment, ensuring proper formatting with forward slashes.
    /// </summary>
    /// <param name="url">The base URL to combine with the path.</param>
    /// <param name="path">The path segment to append to the URL.</param>
    /// <returns>A properly formatted URL with the path appended.</returns>
    /// <exception cref="ArgumentNullException">Thrown when either <paramref name="url"/> or <paramref name="path"/> is null or whitespace.</exception>
    public static string CombineUrl(this string url, string path)
    {
        if (string.IsNullOrWhiteSpace(url) || string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentNullException();
        }

        url = url.Trim();
        path = path.Trim();

        return url.TrimEnd('/') + "/" + path.TrimStart('/');
    }

    /// <summary>
    /// Determines whether the specified URI represents a localhost address.
    /// </summary>
    /// <param name="uri">The URI to check for localhost.</param>
    /// <returns><c>true</c> if the URI is a localhost address; otherwise, <c>false</c>.</returns>
    /// <remarks>
    /// This method checks for various localhost representations including:
    /// <list type="bullet">
    /// <item><description>Loopback addresses (localhost, 127.0.0.1, [::1])</description></item>
    /// <item><description>Local machine hostname</description></item>
    /// <item><description>Local IP addresses assigned to the machine</description></item>
    /// </list>
    /// </remarks>
    public static bool IsLocalhostUrl(this Uri uri)
    {
        try
        {
            if (uri.IsLoopback)
            {
                // localhost, 127.0.0.1, [::1]
                return true;
            }

            // Get the local host name and compare it with the URL host
            string localHostName = Dns.GetHostName();
            if (uri.Host.Equals(localHostName, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Get local IP addresses and compare them with the URL host
            IPAddress[] localIPs = Dns.GetHostAddresses(Dns.GetHostName());
            return localIPs.Any(addr => uri.Host.Equals(addr.ToString()));
        }
        catch (UriFormatException)
        {
            return false;
        }
    }
}