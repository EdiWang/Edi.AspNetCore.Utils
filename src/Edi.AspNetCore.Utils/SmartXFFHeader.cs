using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Edi.AspNetCore.Utils;

public static class SmartXFFHeaderExtensions
{
    private const int MaxHeaderNameLength = 40;
    private const string ForwardedHeadersSection = "ForwardedHeaders";
    private const string HeaderNameKey = "HeaderName";
    private const string KnownProxiesKey = "KnownProxies";

    public static void UseSmartXFFHeader(this WebApplication app)
    {
        var fho = new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        };

        ConfigureForwardedForHeaderName(app, fho);
        ConfigureKnownProxies(app, fho);

        app.UseForwardedHeaders(fho);
    }

    private static void ConfigureForwardedForHeaderName(WebApplication app, ForwardedHeadersOptions fho)
    {
        var headerName = app.Configuration[$"{ForwardedHeadersSection}:{HeaderNameKey}"];
        if (!string.IsNullOrWhiteSpace(headerName))
        {
            if (headerName.Length > MaxHeaderNameLength || !IsValidHeaderName(headerName))
            {
                app.Logger.LogWarning("XFF header name '{HeaderName}' is invalid, it will not be applied", headerName);
            }
            else
            {
                fho.ForwardedForHeaderName = headerName;
                DomainDataHelper.SetAppDomainData("ForwardedHeaders_HeaderName", headerName);
            }
        }
    }

    private static void ConfigureKnownProxies(WebApplication app, ForwardedHeadersOptions fho)
    {
        var knownProxies = app.Configuration.GetSection($"{ForwardedHeadersSection}:{KnownProxiesKey}").Get<string[]>();
        if (knownProxies is { Length: > 0 })
        {
            if (EnvironmentHelper.IsRunningInDocker())
            {
                app.Logger.LogWarning("Running in Docker, skip adding 'KnownProxies'.");
            }
            else
            {
                fho.ForwardLimit = null;
                fho.KnownProxies.Clear();

                foreach (var ip in knownProxies)
                {
                    if (IPAddress.TryParse(ip, out var ipAddress))
                    {
                        fho.KnownProxies.Add(ipAddress);
                    }
                    else
                    {
                        app.Logger.LogWarning("Invalid IP address '{IpAddress}' in KnownProxies configuration.", ip);
                    }
                }

                app.Logger.LogInformation("Added known proxies ({Count}): {Proxies}", knownProxies.Length, System.Text.Json.JsonSerializer.Serialize(knownProxies));
            }
        }
        else
        {
# if NET10_0
            fho.KnownIPNetworks.Add(new(IPAddress.Any, 0));
            fho.KnownIPNetworks.Add(new(IPAddress.IPv6Any, 0));
#else
            fho.KnownNetworks.Add(new(IPAddress.Any, 0));
            fho.KnownNetworks.Add(new(IPAddress.IPv6Any, 0));
#endif
        }
    }

    private static bool IsValidHeaderName(string headerName)
    {
        if (string.IsNullOrEmpty(headerName))
        {
            return false;
        }

        // Check if header name conforms to the standard which allows:
        // - Any ASCII character from 'a' to 'z' and 'A' to 'Z'
        // - Digits from '0' to '9'
        // - Special characters: '!', '#', '$', '%', '&', ''', '*', '+', '-', '.', '^', '_', '`', '|', '~'
        return headerName.All(c =>
            c is >= 'a' and <= 'z' ||
            c is >= 'A' and <= 'Z' ||
            c is >= '0' and <= '9' ||
            c == '!' || c == '#' || c == '$' || c == '%' || c == '&' || c == '\'' ||
            c == '*' || c == '+' || c == '-' || c == '.' || c == '^' || c == '_' ||
            c == '`' || c == '|' || c == '~');
    }
}
