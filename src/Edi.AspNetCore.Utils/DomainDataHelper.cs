namespace Edi.AspNetCore.Utils;

/// <summary>
/// Provides helper methods for managing application domain data storage and retrieval.
/// </summary>
public static class DomainDataHelper
{
    /// <summary>
    /// Sets a key-value pair in the current application domain's data store.
    /// </summary>
    /// <param name="key">The unique string identifier for the data.</param>
    /// <param name="value">The object to store in the application domain.</param>
    /// <remarks>
    /// This method stores data that is accessible across the entire application domain.
    /// The data persists for the lifetime of the application domain.
    /// </remarks>
    public static void SetAppDomainData(string key, object value)
    {
        AppDomain.CurrentDomain.SetData(key, value);
    }

    /// <summary>
    /// Retrieves a value from the current application domain's data store with type safety.
    /// </summary>
    /// <typeparam name="T">The expected type of the stored data.</typeparam>
    /// <param name="key">The unique string identifier for the data to retrieve.</param>
    /// <param name="defaultValue">The value to return if the key is not found or the data is null. Defaults to the default value of type T.</param>
    /// <returns>The stored value cast to type T, or the default value if the key is not found or the data is null.</returns>
    /// <remarks>
    /// This method performs a direct cast to the specified type T. Ensure the stored data is compatible with the expected type to avoid cast exceptions.
    /// </remarks>
    /// <exception cref="InvalidCastException">Thrown when the stored data cannot be cast to the specified type T.</exception>
    public static T GetAppDomainData<T>(string key, T defaultValue = default(T))
    {
        object data = AppDomain.CurrentDomain.GetData(key);
        if (data == null)
        {
            return defaultValue;
        }

        return (T)data;
    }
}
