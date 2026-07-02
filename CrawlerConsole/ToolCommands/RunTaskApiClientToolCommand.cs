using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public sealed class RunTaskApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Run Task";
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
        OneOf<CrawlerPreCheckResult, Error[]> preCheckResult =
            await CrawlerServiceApiClient.PreCheck(_taskName, null, cancellationToken);
        if (preCheckResult.IsT1)
        {
            return ReturnFalseLogErrors(preCheckResult.AsT1);
        }

        int newPartsCreateLimit = 0;
        if (!preCheckResult.AsT0.AutoCreateNextPart)
        {
            newPartsCreateLimit = Inputer.InputInt(
                $"Opened part not found for batch {_taskName}. Auto-create new parts count (0 = no, -1 = unlimited)",
                0);
        }

        Option<Error[]> runTaskResult = await CrawlerServiceApiClient.RunTask(
            new RunTaskRequest { TaskName = _taskName, NewPartsCreateLimit = newPartsCreateLimit }, cancellationToken);

        if (runTaskResult.IsSome)
        {
            return ReturnFalseLogErrors((Error[])runTaskResult);
        }

        //ამოცანა გაეშვა, ავტომატურად ჩავრთოთ მონიტორინგი
        return await RunProcessMonitoring(cancellationToken);
    }
}
