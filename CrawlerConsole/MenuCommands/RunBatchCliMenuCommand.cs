using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using AppCliTools.LibDataInput;
using CrawlerConsole.ToolCommands;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.MenuCommands;

public sealed class RunBatchCliMenuCommand : ApiCliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunBatchCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, CrawlerServiceApiClient apiClient, string taskName) : base(
        parametersManager, taskName, "Run Batch")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _apiClient = apiClient;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        OneOf<TaskDto?, Error[]> taskResult = await _apiClient.GetTaskByName(_taskName, cancellationToken);
        if (taskResult.IsT1)
        {
            Error.PrintErrorsOnConsole(taskResult.AsT1);
            return false;
        }

        TaskDto? task = taskResult.AsT0;
        if (task is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} does not exists", true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(task.ApiName))
        {
            StShared.WriteErrorLine($"Server does not specified for task {_taskName}", true);
            return false;
        }

        ApiToolCommandParameters? apiToolCommandParameters = CreateApiParameters(task.ApiName);
        if (apiToolCommandParameters is null)
        {
            return false;
        }

        string? batchName = Inputer.InputText("Batch Name", null);
        if (string.IsNullOrWhiteSpace(batchName))
        {
            StShared.WriteErrorLine("Batch Name is empty", true);
            return false;
        }

        var crawlerRunnerToolAction = new RunBatchApiClientToolCommand(_logger, _httpClientFactory,
            apiToolCommandParameters, batchName);

        return await crawlerRunnerToolAction.Run(cancellationToken);
    }
}
