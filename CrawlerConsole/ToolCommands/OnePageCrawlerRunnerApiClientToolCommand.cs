using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using SystemTools.SharedKernel;

namespace CrawlerConsole.ToolCommands;

public sealed class OnePageCrawlerRunnerApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Clear RawWordsByLemmas";

    //პროგრესის შეტყობინებების გაგზავნებს შორის მინიმალური დაყოვნება წამებში
    private const int ProgressDelaySeconds = 1;
    private readonly string _strUrl;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OnePageCrawlerRunnerApiClientToolCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient,
        Uri url, string taskName) : base(logger, ActionName, crawlerServiceApiClient)
    {
        _strUrl = url.ToString();
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        //კითხვის დასმა-არდასმა აქ, კონსოლის მხარეს გადაწყდება; პასუხები ენდპოინტს პარამეტრად გადაეცემა
        Result<CrawlerPreCheckResult> preCheckResult =
            await CrawlerServiceApiClient.PreCheck(_taskName, _strUrl, cancellationToken);
        if (preCheckResult.IsFailure)
        {
            return ReturnFalseLogErrors(preCheckResult.Error);
        }

        CrawlerPreCheckResult preCheck = preCheckResult.Value;

        bool deleteContentForReanalyze = preCheck.PageAlreadyAnalyzed && Inputer.InputBool(
            $"The page {_strUrl} already analyzed. Do you wont to delete Content data for reanalyze", true, false);

        int newPartsCreateLimit = preCheck is { HasOpenPart: false, AutoCreateNextPart: false } &&
                                  Inputer.InputBool("Opened part not found, Create new?", true, false)
            ? 1
            : 0;

        Result testOnePageResult = await CrawlerServiceApiClient.TestOnePage(
            new TestOnePageRequest
            {
                Url = _strUrl,
                TaskName = _taskName,
                DeleteContentForReanalyze = deleteContentForReanalyze,
                NewPartsCreateLimit = newPartsCreateLimit,
                ProgressDelaySeconds = ProgressDelaySeconds
            }, cancellationToken);

        return testOnePageResult.IsSuccess || ReturnFalseLogErrors(testOnePageResult.Error);
    }
}
