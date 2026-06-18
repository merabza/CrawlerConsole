using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
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

        //კითხვის დასმა-არდასმა აქ, კონსოლის მხარეს გადაწყდება; პასუხი ენდპოინტს პარამეტრად გადაეცემა
        OneOf<CrawlerPreCheckResult, Error[]> preCheckResult =
            await aiClient.PreCheck(_batchName, null, cancellationToken);
        if (preCheckResult.IsT1)
        {
            return ReturnFalseLogErrors(preCheckResult.AsT1);
        }

        int newPartsCreateLimit = 0;
        if (!preCheckResult.AsT0.AutoCreateNextPart)
        {
            newPartsCreateLimit = Inputer.InputInt(
                $"Opened part not found for batch {_batchName}. Auto-create new parts count (0 = no, -1 = unlimited)",
                -1);
        }

        Option<Error[]> runBatchResult = await aiClient.RunBatch(_batchName, newPartsCreateLimit, cancellationToken);

        return runBatchResult.IsNone || ReturnFalseLogErrors((Error[])runBatchResult);
    }
}
