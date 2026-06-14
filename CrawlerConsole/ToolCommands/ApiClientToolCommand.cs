using System.Net.Http;
using AppCliTools.CliParameters;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public /*open*/ class ApiClientToolCommand : ToolCommand
{
    private readonly ILogger _logger;
    protected readonly IHttpClientFactory HttpClientFactory;

    protected ApiClientToolCommand(ILogger logger, IHttpClientFactory httpClientFactory, string actionName,
        ParametersManager parametersManager, string description, bool useConsole = false) : base(logger, actionName,
        parametersManager, description, useConsole)
    {
        _logger = logger;
        HttpClientFactory = httpClientFactory;
    }

    protected ApiClientToolCommand(ILogger logger, IHttpClientFactory httpClientFactory, string actionName,
        IParameters par, IParametersManager? parametersManager, string description, bool useConsole = false) : base(
        logger, actionName, par, parametersManager, description, useConsole)
    {
        _logger = logger;
        HttpClientFactory = httpClientFactory;
    }

    protected CrawlerServiceApiClient CreateCrawlerServiceApiClient()
    {
        var par = (ApiToolCommandParameters)Par;
        return new CrawlerServiceApiClient(_logger, HttpClientFactory, par.Api.Server, par.Api.ApiKey, true);
    }

    protected bool ReturnFalseLogErrors(Error[] errors)
    {
        _logger.LogError("Action {ToolActionName} finished with errors", ToolActionName);
        Error.PrintErrorsOnConsole(errors);
        return false;
    }
}
