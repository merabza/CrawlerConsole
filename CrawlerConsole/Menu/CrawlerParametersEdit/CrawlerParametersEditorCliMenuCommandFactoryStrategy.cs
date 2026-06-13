using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.Menu.CrawlerParametersEdit;

// ReSharper disable once ClassNeverInstantiated.Global
public class CrawlerParametersEditorCliMenuCommandFactoryStrategy(
    ILogger<CrawlerParametersEditorCliMenuCommandFactoryStrategy> logger,
    IHttpClientFactory httpClientFactory,
    IParametersManager parametersManager,
    IApplication application) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        var parameters = (CrawlerConsoleParameters)parametersManager.Parameters;

        var supportToolsParametersEditor = new CrawlerParametersEditor(application, parameters,
            parametersManager, logger, httpClientFactory);
        return new ParametersEditorListCliMenuCommand(supportToolsParametersEditor);
    }
}
