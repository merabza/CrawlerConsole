using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public sealed class OnePageCrawlerRunnerApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Clear RawWordsByLemmas";
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
        OneOf<CrawlerPreCheckResult, Error[]> preCheckResult =
            await CrawlerServiceApiClient.PreCheck(_taskName, _strUrl, cancellationToken);
        if (preCheckResult.IsT1)
        {
            return ReturnFalseLogErrors(preCheckResult.AsT1);
        }

        CrawlerPreCheckResult preCheck = preCheckResult.AsT0;

        bool deleteContentForReanalyze = preCheck.PageAlreadyAnalyzed && Inputer.InputBool(
            $"The page {_strUrl} already analyzed. Do you wont to delete Content data for reanalyze", true, false);

        int newPartsCreateLimit = !preCheck.HasOpenPart && !preCheck.AutoCreateNextPart &&
                                  Inputer.InputBool("Opened part not found, Create new?", true, false)
            ? 1
            : 0;

        Option<Error[]> testOnePageResult = await CrawlerServiceApiClient.TestOnePage(
            new TestOnePageRequest
            {
                Url = _strUrl,
                TaskName = _taskName,
                DeleteContentForReanalyze = deleteContentForReanalyze,
                NewPartsCreateLimit = newPartsCreateLimit
            }, cancellationToken);

        return testOnePageResult.IsNone || ReturnFalseLogErrors((Error[])testOnePageResult);
    }
}
