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

public sealed class RunBatchApiClientToolCommand : ApiClientToolCommand
{
    public const string ActionName = "Run Batch";
    private readonly string _batchName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunBatchApiClientToolCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ApiToolCommandParameters par, string batchName) : base(logger,
        httpClientFactory, ActionName, par, null, ActionName, true)
    {
        _batchName = batchName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        CrawlerServiceApiClient aiClient = CreateCrawlerServiceApiClient();

        Option<Error[]> runBatchResult = await aiClient.RunBatch(_batchName, cancellationToken);

        return runBatchResult.IsNone || ReturnFalseLogErrors((Error[])runBatchResult);
    }
}
