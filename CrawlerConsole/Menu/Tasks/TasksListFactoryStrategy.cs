using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using CrawlerConsole.MenuCommands;
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
        return crawlerRepository.GetTasksList().OrderBy(o => o.TaskName)
            .Select(task => new TaskSubMenuCliMenuCommand(logger, httpClientFactory, parametersManager,
                crawlerRepository, task.TaskName)).Cast<CliMenuCommand>().ToList();
    }
}
