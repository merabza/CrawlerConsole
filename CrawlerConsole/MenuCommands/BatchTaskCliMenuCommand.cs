using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using CrawlerConsoleData.Models;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;

namespace CrawlerConsole.MenuCommands;

public sealed class BatchTaskCliMenuCommand : CliMenuCommand
{
    //private readonly BatchDto _batch;
    //private readonly CrawlerServiceApiClient _apiClient;
    //private readonly IHttpClientFactory _httpClientFactory;

    //private readonly ILogger _logger;
    //private readonly CrawlerConsoleParameters _par;

    public BatchTaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        CrawlerServiceApiClient apiClient, CrawlerConsoleParameters par, BatchDto batch) : base("Run this batch",
        EMenuAction.Reload)
    {
        //_logger = logger;
        //_httpClientFactory = httpClientFactory;
        //_par = par;
        //_batch = batch;
        //_apiClient = apiClient;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(false);
    }
}
