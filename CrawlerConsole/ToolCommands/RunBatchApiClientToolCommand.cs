using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public sealed class RunBatchApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Run Batch";
    private readonly string _batchName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunBatchApiClientToolCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient,
        string batchName) : base(logger, ActionName, crawlerServiceApiClient)
    {
        _batchName = batchName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        //კითხვის დასმა-არდასმა აქ, კონსოლის მხარეს გადაწყდება; პასუხი ენდპოინტს პარამეტრად გადაეცემა
        OneOf<CrawlerPreCheckResult, Error[]> preCheckResult =
            await CrawlerServiceApiClient.PreCheck(_batchName, null, cancellationToken);
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

        Option<Error[]> runBatchResult =
            await CrawlerServiceApiClient.RunBatch(_batchName, newPartsCreateLimit, cancellationToken);

        if (runBatchResult.IsSome)
        {
            return ReturnFalseLogErrors((Error[])runBatchResult);
        }

        //ბაჩი გაეშვა, ავტომატურად ჩავრთოთ მონიტორინგი
        return await RunProcessMonitoring(cancellationToken);
    }
}
