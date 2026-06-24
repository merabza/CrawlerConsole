using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using AppCliTools.LibDataInput;
using CrawlerConsole.ToolCommands;
using CrawlerRepoInterfaces;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class TestOnePageCliMenuCommand : ApiCliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TestOnePageCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, ICrawlerRepository crawlerRepository, string taskName) : base(
        parametersManager, taskName, "Test One Page")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _crawlerRepository = crawlerRepository;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var task = _crawlerRepository.GetTaskByName(_taskName);
        if (task is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} does not exists", true);
            return false;
        }

        if (string.IsNullOrWhiteSpace(task.ApiName))
        {
            StShared.WriteErrorLine($"Server does not specified for task {_taskName}", true);
            return false;
        }

        ApiToolCommandParameters? apiToolCommandParameters = CreateApiParameters(task.ApiName);
        if (apiToolCommandParameters is null)
        {
            return false;
        }

        string? strUrl = Inputer.InputText("Page for Test", null);
        if (string.IsNullOrWhiteSpace(strUrl))
        {
            StShared.WriteErrorLine("Page for Test is empty", true);
            return false;
        }

        //StartPoints-ებს სერვისი თვითონ აიღებს ბაზიდან task name-ით
        var crawlerRunnerToolAction = new OnePageCrawlerRunnerApiClientToolCommand(_logger, _httpClientFactory,
            apiToolCommandParameters, new Uri(strUrl), _taskName);

        return await crawlerRunnerToolAction.Run(cancellationToken);
    }
}
