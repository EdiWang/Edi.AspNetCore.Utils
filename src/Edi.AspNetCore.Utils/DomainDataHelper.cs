namespace Edi.AspNetCore.Utils;

public static class DomainDataHelper
{
    public static void SetAppDomainData(string key, object value)
    {
        AppDomain.CurrentDomain.SetData(key, value);
    }

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
