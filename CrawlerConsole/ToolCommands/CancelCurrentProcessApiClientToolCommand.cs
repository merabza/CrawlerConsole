using System.Threading;
using System.Threading.Tasks;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public sealed class CancelCurrentProcessApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Cancel Current Process";

    // ReSharper disable once ConvertToPrimaryConstructor
    public CancelCurrentProcessApiClientToolCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient) :
        base(logger, ActionName, crawlerServiceApiClient)
    {
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        OneOf<bool, Error[]> result = await CrawlerServiceApiClient.CancelCurrentProcess(cancellationToken);
        return result.IsT0 ? result.AsT0 : ReturnFalseLogErrors(result.AsT1);
    }
}
