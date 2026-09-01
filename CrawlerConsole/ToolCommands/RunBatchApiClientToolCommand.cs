using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;

namespace CrawlerConsole.ToolCommands;

public sealed class RunBatchApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Run Batch";

    //პროგრესის შეტყობინებების გაგზავნებს შორის მინიმალური დაყოვნება წამებში
    private const int ProgressDelaySeconds = 1;
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
        Result<CrawlerPreCheckResult> preCheckResult =
            await CrawlerServiceApiClient.PreCheck(_batchName, null, cancellationToken);
        if (preCheckResult.IsFailure)
        {
            return ReturnFalseLogErrors(preCheckResult.Error);
        }

        int newPartsCreateLimit = 0;
        if (!preCheckResult.Value.AutoCreateNextPart)
        {
            newPartsCreateLimit = Inputer.InputInt(
                $"Opened part not found for batch {_batchName}. Auto-create new parts count (0 = no, -1 = unlimited)",
                0);
        }

        Result runBatchResult = await CrawlerServiceApiClient.RunBatch(_batchName, newPartsCreateLimit,
            ProgressDelaySeconds, cancellationToken);

        if (runBatchResult.IsFailure)
        {
            return ReturnFalseLogErrors(runBatchResult.Error);
        }

        //ბაჩი გაეშვა, ავტომატურად ჩავრთოთ მონიტორინგი
        return await RunProcessMonitoring(cancellationToken);
    }
}
