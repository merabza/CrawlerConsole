using System;
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

        Option<Error[]> clearRawWordsByLemmasResult = await aiClient.TestOnePage(
            new TestOnePageRequest { Url = _strUrl, StartPoints = _startPoints.ToList(), TaskName = _taskName },
            cancellationToken);

        return clearRawWordsByLemmasResult.IsNone || ReturnFalseLogErrors((Error[])clearRawWordsByLemmasResult);
    }
}
