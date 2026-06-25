using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using CrawlerConsole.MenuCommands;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class TasksListFactoryStrategy(
    ILogger<TasksListFactoryStrategy> logger,
    IHttpClientFactory httpClientFactory,
    IParametersManager parametersManager,
    CrawlerServiceApiClient apiClient) : IMenuCommandListFactoryStrategy
{
    public List<CliMenuCommand> CreateMenuCommandsList()
    {
        return apiClient.GetTasksList().GetAwaiter().GetResult().Match(
            tasks => tasks.OrderBy(o => o.TaskName)
                .Select(task => new TaskSubMenuCliMenuCommand(logger, httpClientFactory, parametersManager, apiClient,
                    task.TaskName)).Cast<CliMenuCommand>().ToList(),
            errors =>
            {
                Error.PrintErrorsOnConsole(errors);
                return new List<CliMenuCommand>();
            });
    }
}
