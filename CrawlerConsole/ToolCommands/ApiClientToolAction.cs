using System.Threading;
using System.Threading.Tasks;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using SystemTools.BackgroundTasks;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public /*open*/ class ApiClientToolAction : ToolAction
{
    private readonly ILogger _logger;
    protected readonly CrawlerServiceApiClient CrawlerServiceApiClient;

    protected ApiClientToolAction(ILogger logger, string actionName, CrawlerServiceApiClient crawlerServiceApiClient) :
        base(logger, actionName, null, null)
    {
        _logger = logger;
        CrawlerServiceApiClient = crawlerServiceApiClient;
    }

    //protected ApiClientToolAction(ILogger logger, IHttpClientFactory httpClientFactory, string actionName,
    //    IParameters par, IParametersManager? parametersManager, string description, bool useConsole = false) : base(
    //    logger, actionName, par, parametersManager, description, useConsole)
    //{
    //    _logger = logger;
    //    HttpClientFactory = httpClientFactory;
    //}

    //protected CrawlerServiceApiClient CreateCrawlerServiceApiClient()
    //{
    //    var par = (ApiToolCommandParameters)Par;
    //    return new CrawlerServiceApiClient(_logger, HttpClientFactory, par.Api.Server, par.Api.ApiKey, true);
    //}

    protected bool ReturnFalseLogErrors(Error[] errors)
    {
        _logger.LogError("Action {ToolActionName} finished with errors", ToolActionName);
        Error.PrintErrorsOnConsole(errors);
        return false;
    }

    //გაშვებული პროცესის მონიტორინგის ავტომატური ჩართვა
    protected async ValueTask<bool> RunProcessMonitoring(CancellationToken cancellationToken)
    {
        var processMonitoringToolCommand = new ProcessMonitoringApiClientToolCommand(_logger, CrawlerServiceApiClient);
        return await processMonitoringToolCommand.Run(cancellationToken);
    }
}
