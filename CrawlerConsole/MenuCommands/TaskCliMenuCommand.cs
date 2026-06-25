using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.MenuCommands;

public sealed class TaskCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _taskName;

    public TaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory, CrawlerServiceApiClient apiClient,
        IParametersManager parametersManager, string taskName) : base("Run this task", EMenuAction.Reload)
    {
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

        if (taskResult.AsT0 is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return false;
        }

        return false;
    }
}
