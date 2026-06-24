using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using CrawlerRepoInterfaces;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class TaskCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly string _taskName;

    public TaskCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ICrawlerRepository crawlerRepository, IParametersManager parametersManager, string taskName) : base(
        "Run this task", EMenuAction.Reload)
    {
        _crawlerRepository = crawlerRepository;
        _taskName = taskName;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var task = _crawlerRepository.GetTaskByName(_taskName);
        if (task == null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(false);
    }
}
