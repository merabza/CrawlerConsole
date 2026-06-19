using System;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliMenu.CliMenuCommands;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.LibDataInput;
using CrawlerConsole.Cruders;
using CrawlerConsoleData.Models;
using CrawlerRepoInterfaces;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.MenuCommands;

public sealed class TaskSubMenuCliMenuCommand : CliMenuCommand
{
    //private readonly ICrawlerRepositoryCreatorFactory _crawlerRepositoryCreatorFactory;
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TaskSubMenuCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, ICrawlerRepository crawlerRepository, string taskName) : base(taskName,
        EMenuAction.LoadSubMenu)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _crawlerRepository = crawlerRepository;
        _taskName = taskName;
    }

    public override CliMenuSet GetSubMenu()
    {
        var taskSubMenuSet = new CliMenuSet($" Task => {Name}");

        var deleteTaskCommand = new DeleteTaskCliMenuCommand(_parametersManager, Name);
        taskSubMenuSet.AddMenuItem(deleteTaskCommand);

        taskSubMenuSet.AddMenuItem(new EditTaskNameCliMenuCommand(_parametersManager, Name));

        //პროექტის პარამეტრი
        var taskCruder = TaskCruder.Create(_logger, _httpClientFactory, _parametersManager);
        var editCommand = new EditItemAllFieldsInSequenceCliMenuCommand(taskCruder, _taskName);
        taskSubMenuSet.AddMenuItem(editCommand);

        taskCruder.FillDetailsSubMenu(taskSubMenuSet, _taskName);

        taskSubMenuSet.AddMenuItem(new TaskCliMenuCommand(_logger, _httpClientFactory, _crawlerRepository,
            _parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new RunTaskCliMenuCommand(_logger, _httpClientFactory, _parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new RunBatchCliMenuCommand(_logger, _httpClientFactory, _parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new TestOnePageCliMenuCommand(_logger, _httpClientFactory, _parametersManager,
            Name));

        var parameters = (CrawlerConsoleParameters)_parametersManager.Parameters;

        TaskModel? task = parameters.GetTask(Name);
        var newStartPointCommand = new NewStartPointCliMenuCommand(_parametersManager, Name);
        taskSubMenuSet.AddMenuItem(newStartPointCommand);

        if (task?.StartPoints != null)
        {
            foreach (string startPoint in task.StartPoints.OrderBy(o => o))
            {
                taskSubMenuSet.AddMenuItem(new StartPointSubMenuCliMenuCommand(_parametersManager, Name, startPoint));
            }
        }

        string key = ConsoleKey.Escape.Value().ToUpperInvariant();
        taskSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to Main menu", null), key.Length);

        return taskSubMenuSet;
    }
}
