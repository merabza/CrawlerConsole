using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using CrawlerConsole.ToolCommands;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.MenuCommands;

public sealed class ProcessMonitoringCliMenuCommand : ApiCliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly ILogger _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ProcessMonitoringCliMenuCommand(ILogger logger, CrawlerServiceApiClient apiClient,
        IParametersManager parametersManager, string taskName) : base(parametersManager, taskName,
        ProcessMonitoringApiClientToolCommand.ActionName, EMenuAction.ReloadWithoutPause)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var processMonitoringToolCommand = new ProcessMonitoringApiClientToolCommand(_logger, _apiClient);

        return await processMonitoringToolCommand.Run(cancellationToken);
    }
}
