using System;
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

public sealed class OnePageCrawlerRunnerApiClientToolCommand : ApiClientToolCommand
{
    public const string ActionName = "Clear RawWordsByLemmas";
    private readonly IEnumerable<string> _startPoints;
    private readonly string _strUrl;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public OnePageCrawlerRunnerApiClientToolCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        ApiToolCommandParameters par, Uri url, IEnumerable<string> startPoints, string taskName) : base(logger,
        httpClientFactory, ActionName, par, null, ActionName, true)
    {
        _strUrl = url.ToString();
        _startPoints = startPoints;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        CrawlerServiceApiClient aiClient = CreateCrawlerServiceApiClient();

        //კითხვის დასმა-არდასმა აქ, კონსოლის მხარეს გადაწყდება; პასუხები ენდპოინტს პარამეტრად გადაეცემა
        OneOf<CrawlerPreCheckResult, Error[]> preCheckResult =
            await aiClient.PreCheck(_taskName, _strUrl, cancellationToken);
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

        Option<Error[]> testOnePageResult = await aiClient.TestOnePage(
            new TestOnePageRequest
            {
                Url = _strUrl,
                StartPoints = _startPoints.ToList(),
                TaskName = _taskName,
                DeleteContentForReanalyze = deleteContentForReanalyze,
                NewPartsCreateLimit = newPartsCreateLimit
            }, cancellationToken);

        return testOnePageResult.IsNone || ReturnFalseLogErrors((Error[])testOnePageResult);
    }
}
