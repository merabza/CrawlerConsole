using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParametersApiClientsEdit.CliMenuCommands;
using AppCliTools.CliParametersApiClientsEdit.Parameters;
using AppCliTools.LibDataInput;
using CrawlerConsole.ToolCommands;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class RunBatchCliMenuCommand : ApiCliMenuCommand
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public RunBatchCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, string taskName) : base(parametersManager, taskName, "Run Batch")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var parameters = (CrawlerConsoleParameters)_parametersManager.Parameters;
        string apiName = parameters.GetApiNameRequired(_taskName);

        ApiToolCommandParameters? apiToolCommandParameters = CreateApiParameters(apiName);
        if (apiToolCommandParameters is null)
        {
            return false;
        }

        string? batchName = Inputer.InputText("Batch Name", null);
        if (string.IsNullOrWhiteSpace(batchName))
        {
            StShared.WriteErrorLine("Batch Name is empty", true);
            return false;
        }

        var crawlerRunnerToolAction = new RunBatchApiClientToolCommand(_logger, _httpClientFactory,
            apiToolCommandParameters, batchName);

        return await crawlerRunnerToolAction.Run(cancellationToken);
    }
}
