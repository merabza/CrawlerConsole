using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using CrawlerConsole.ToolCommands;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.MenuCommands;

public sealed class CancelCurrentProcessCliMenuCommand : ApiCliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly ILogger _logger;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CancelCurrentProcessCliMenuCommand(ILogger logger, CrawlerServiceApiClient apiClient,
        IParametersManager parametersManager, string taskName) : base(parametersManager, taskName,
        CancelCurrentProcessApiClientToolCommand.ActionName)
    {
        _logger = logger;
        _apiClient = apiClient;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var cancelCurrentProcessToolCommand = new CancelCurrentProcessApiClientToolCommand(_logger, _apiClient);

        return await cancelCurrentProcessToolCommand.Run(cancellationToken);
    }
}
