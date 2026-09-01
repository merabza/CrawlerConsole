using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;

namespace CrawlerConsole.ToolCommands;

public sealed class RunTaskApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Run Task";

    //პროგრესის შეტყობინებების გაგზავნებს შორის მინიმალური დაყოვნება წამებში
    private const int ProgressDelaySeconds = 1;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunTaskApiClientToolCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient, string taskName)
        : base(logger, ActionName, crawlerServiceApiClient)
    {
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        //CrawlerServiceApiClient aiClient = CreateCrawlerServiceApiClient();

        //კითხვის დასმა-არდასმა აქ, კონსოლის მხარეს გადაწყდება; პასუხი ენდპოინტს პარამეტრად გადაეცემა
        Result<CrawlerPreCheckResult> preCheckResult =
            await CrawlerServiceApiClient.PreCheck(_taskName, null, cancellationToken);
        if (preCheckResult.IsFailure)
        {
            return ReturnFalseLogErrors(preCheckResult.Error);
        }

        int newPartsCreateLimit = 0;
        if (!preCheckResult.Value.AutoCreateNextPart)
        {
            newPartsCreateLimit = Inputer.InputInt(
                $"Opened part not found for batch {_taskName}. Auto-create new parts count (0 = no, -1 = unlimited)",
                0);
        }

        Result runTaskResult = await CrawlerServiceApiClient.RunTask(
            new RunTaskRequest
            {
                TaskName = _taskName,
                NewPartsCreateLimit = newPartsCreateLimit,
                ProgressDelaySeconds = ProgressDelaySeconds
            }, cancellationToken);

        if (runTaskResult.IsFailure)
        {
            return ReturnFalseLogErrors(runTaskResult.Error);
        }

        //ამოცანა გაეშვა, ავტომატურად ჩავრთოთ მონიტორინგი
        return await RunProcessMonitoring(cancellationToken);
    }
}
