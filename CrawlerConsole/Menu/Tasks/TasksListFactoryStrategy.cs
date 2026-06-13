using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using CrawlerConsole.MenuCommands;
using CrawlerConsoleData.Models;
using CrawlerRepoInterfaces;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class TasksListFactoryStrategy(
    ILogger<TasksListFactoryStrategy> logger,
    IHttpClientFactory httpClientFactory,
    IParametersManager parametersManager,
    ICrawlerRepository crawlerRepository) : IMenuCommandListFactoryStrategy
{
    public List<CliMenuCommand> CreateMenuCommandsList()
    {
        var parameters = (CrawlerConsoleParameters)parametersManager.Parameters;

        return parameters.Tasks.OrderBy(o => o.Key)
            .Select(kvp => new TaskSubMenuCliMenuCommand(logger, httpClientFactory, parametersManager,
                crawlerRepository, kvp.Key)).Cast<CliMenuCommand>().ToList();
    }
}
