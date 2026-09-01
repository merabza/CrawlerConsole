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

public sealed class RunBatchCliMenuCommand : ApiCliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly CrawlerServiceApiClient _crawlerServiceApiClient;
    private readonly ILogger _logger;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunBatchCliMenuCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient,
        IParametersManager parametersManager, CrawlerServiceApiClient apiClient, string taskName) : base(
        parametersManager, taskName, "Run Batch")
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

        string? batchName = Inputer.InputText("Batch Name", null);
        if (string.IsNullOrWhiteSpace(batchName))
        {
            StShared.WriteErrorLine("Batch Name is empty", true);
            return false;
        }

        var crawlerRunnerToolAction = new RunBatchApiClientToolCommand(_logger, _crawlerServiceApiClient, batchName);

        return await crawlerRunnerToolAction.Run(cancellationToken);
    }
}
