using CrawlerConsoleData.Models;
using ParametersManagement.LibApiClientParameters;

namespace CrawlerConsole;

//განსაზღვრავს, რომელი CrawlerService-ის ApiClient-ი (Server+ApiKey) გამოიყენება მენეჯმენტის (CRUD) ოპერაციებისთვის.
//კავშირი იღება ManagementApiClientName-ით არსებული ApiClients ლექსიკონიდან.
internal static class ManagementApiClientResolver
{
    public static bool TryResolve(CrawlerConsoleParameters parameters, out string server, out string? apiKey)
    {
        server = string.Empty;
        apiKey = null;

        if (string.IsNullOrWhiteSpace(parameters.ManagementApiClientName))
        {
            return false;
        }

        if (!parameters.ApiClients.TryGetValue(parameters.ManagementApiClientName, out ApiClientSettings? settings))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(settings.Server))
        {
            return false;
        }

        server = settings.Server;
        apiKey = settings.ApiKey;
        return true;
    }
}
