using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using Microsoft.Extensions.Logging;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public sealed class RunTaskApiClientToolCommand : ApiClientToolCommand
{
    public const string ActionName = "Run Task";
    private readonly IEnumerable<string> _startPoints;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunTaskApiClientToolCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ApiToolCommandParameters par, IEnumerable<string> startPoints, string taskName) : base(logger,
        httpClientFactory, ActionName, par, null, ActionName, true)
    {
        _startPoints = startPoints;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        CrawlerServiceApiClient aiClient = CreateCrawlerServiceApiClient();

        Option<Error[]> runTaskResult = await aiClient.RunTask(
            new RunTaskRequest { StartPoints = _startPoints.ToList(), TaskName = _taskName }, cancellationToken);

        return runTaskResult.IsNone || ReturnFalseLogErrors((Error[])runTaskResult);
    }
}
