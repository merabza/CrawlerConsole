using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using CrawlerConsole.ToolCommands;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.MenuCommands;

public sealed class RunTaskCliMenuCommand : ApiCliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunTaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, CrawlerServiceApiClient apiClient, string taskName) : base(
        parametersManager, taskName, "Run Task")
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

        //StartPoints-ებს სერვისი თვითონ აიღებს ბაზიდან task name-ით
        var crawlerRunnerToolAction = new RunTaskApiClientToolCommand(_logger, _httpClientFactory,
            apiToolCommandParameters, _taskName);

        return await crawlerRunnerToolAction.Run(cancellationToken);
    }
}
