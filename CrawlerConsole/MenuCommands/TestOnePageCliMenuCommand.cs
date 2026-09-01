using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using AppCliTools.LibDataInput;
using CrawlerConsole.ToolCommands;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class TestOnePageCliMenuCommand : ApiCliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly CrawlerServiceApiClient _crawlerServiceApiClient;
    private readonly ILogger _logger;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TestOnePageCliMenuCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient,
        IParametersManager parametersManager, CrawlerServiceApiClient apiClient, string taskName) : base(
        parametersManager, taskName, "Test One Page")
    {
        _logger = logger;
        _crawlerServiceApiClient = crawlerServiceApiClient;
        _apiClient = apiClient;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        Result<TaskDto?> taskResult = await _apiClient.GetTaskByName(_taskName, cancellationToken);
        if (taskResult.IsFailure)
        {
            taskResult.Error.PrintErrorsOnConsole();
            return false;
        }

        TaskDto? task = taskResult.Value;
        if (task is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} does not exists", true);
            return false;
        }

        //if (string.IsNullOrWhiteSpace(task.ApiName))
        //{
        //    StShared.WriteErrorLine($"Server does not specified for task {_taskName}", true);
        //    return false;
        //}

        //ApiToolCommandParameters? apiToolCommandParameters = CreateApiParameters(task.ApiName);
        //if (apiToolCommandParameters is null)
        //{
        //    return false;
        //}

        string? strUrl = Inputer.InputText("Page for Test", null);
        if (string.IsNullOrWhiteSpace(strUrl))
        {
            StShared.WriteErrorLine("Page for Test is empty", true);
            return false;
        }

        //StartPoints-ებს სერვისი თვითონ აიღებს ბაზიდან task name-ით
        var crawlerRunnerToolAction = new OnePageCrawlerRunnerApiClientToolCommand(_logger,
            _crawlerServiceApiClient, new Uri(strUrl), _taskName);

        return await crawlerRunnerToolAction.Run(cancellationToken);
    }
}
