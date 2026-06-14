using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using AppCliTools.LibDataInput;
using CrawlerConsole.ToolCommands;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class TestOnePageCliMenuCommand : ApiCliMenuCommand
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TestOnePageCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, string taskName) : base(parametersManager, taskName, "Test One Page")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (CrawlerConsoleParameters)_parametersManager.Parameters;
        TaskModel task = parameters.GetTaskRequired(_taskName);
        string apiName = parameters.GetApiNameRequired(_taskName);

        ApiToolCommandParameters? apiToolCommandParameters = CreateApiParameters(apiName);
        if (apiToolCommandParameters is null)
        {
            return false;
        }

        string? strUrl = Inputer.InputText("Page for Test", null);
        if (string.IsNullOrWhiteSpace(strUrl))
        {
            StShared.WriteErrorLine("Page for Test is empty", true);
            return false;
        }

        var crawlerRunnerToolAction = new OnePageCrawlerRunnerApiClientToolCommand(_logger, _httpClientFactory,
            apiToolCommandParameters, new Uri(strUrl), task.StartPoints, _taskName);

        return await crawlerRunnerToolAction.Run(cancellationToken);
    }
}
