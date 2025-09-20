using System.Text.RegularExpressions;

namespace Edi.AspNetCore.Utils;

/// <summary>
/// Provides utility methods for detecting runtime environments and parsing environment variables.
/// </summary>
public static class EnvironmentHelper
{
    /// <summary>
    /// Determines whether the application is running on Azure App Service.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the application is running on Azure App Service; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks for the presence of the WEBSITE_SITE_NAME environment variable,
    /// which is automatically set by Azure App Service.
    /// </remarks>
    public static bool IsRunningOnAzureAppService() => !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME"));

    /// <summary>
    /// Determines whether the application is running in a Docker container.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the application is running in a Docker container; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks for the DOTNET_RUNNING_IN_CONTAINER environment variable,
    /// which is set to "true" when running in a Docker container.
    /// </remarks>
    public static bool IsRunningInDocker() => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

    /// <summary>
    /// Retrieves and validates environment tags from the specified environment variable.
    /// </summary>
    /// <param name="environmentVariable">
    /// The name of the environment variable to read tags from. Defaults to "APP_TAGS".
    /// </param>
    /// <returns>
    /// An enumerable collection of valid tags. If no valid tags are found or the environment variable is not set,
    /// returns a collection containing an empty string.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Tags are expected to be comma-separated in the environment variable.
    /// Each tag is validated against a regular expression pattern that allows:
    /// alphanumeric characters, hyphens, hash symbols, at symbols, dollar signs,
    /// parentheses, and square brackets.
    /// </para>
    /// <para>
    /// Invalid tags are filtered out and not included in the result.
    /// Whitespace around tags is automatically trimmed.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Environment variable APP_TAGS = "prod,api-v1,feature#123"
    /// var tags = EnvironmentHelper.GetEnvironmentTags();
    /// // Returns: ["prod", "api-v1", "feature#123"]
    /// 
    /// // Custom environment variable
    /// var customTags = EnvironmentHelper.GetEnvironmentTags("CUSTOM_TAGS");
    /// </code>
    /// </example>
    public static IEnumerable<string> GetEnvironmentTags(string environmentVariable = "APP_TAGS")
    {
        var tagsEnv = Environment.GetEnvironmentVariable(environmentVariable);
        if (string.IsNullOrWhiteSpace(tagsEnv))
        {
            yield return string.Empty;
            yield break;
        }

        var tagRegex = new Regex(@"^[a-zA-Z0-9-#@$()\[\]/]+$");
        var tags = tagsEnv.Split(',');
        foreach (string tag in tags)
        {
            var t = tag.Trim();
            if (tagRegex.IsMatch(t))
            {
                yield return t;
            }
        }
    }
}
