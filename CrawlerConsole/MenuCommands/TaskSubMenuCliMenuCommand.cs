using System;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliMenu.CliMenuCommands;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.LibDataInput;
using CrawlerConsole.Cruders;
using CrawlerRepoInterfaces;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.MenuCommands;

public sealed class TaskSubMenuCliMenuCommand : CliMenuCommand
{
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

        var deleteTaskCommand = new DeleteTaskCliMenuCommand(_crawlerRepository, Name);
        taskSubMenuSet.AddMenuItem(deleteTaskCommand);

        taskSubMenuSet.AddMenuItem(new EditTaskNameCliMenuCommand(_crawlerRepository, Name));

        //პროექტის პარამეტრი
        var taskCruder = TaskCruder.Create(_logger, _httpClientFactory, _parametersManager, _crawlerRepository);
        var editCommand = new EditItemAllFieldsInSequenceCliMenuCommand(taskCruder, _taskName);
        taskSubMenuSet.AddMenuItem(editCommand);

        taskCruder.FillDetailsSubMenu(taskSubMenuSet, _taskName);

        taskSubMenuSet.AddMenuItem(new TaskCliMenuCommand(_logger, _httpClientFactory, _crawlerRepository,
            _parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new RunTaskCliMenuCommand(_logger, _httpClientFactory, _parametersManager,
            _crawlerRepository, Name));

        taskSubMenuSet.AddMenuItem(new RunBatchCliMenuCommand(_logger, _httpClientFactory, _parametersManager,
            _crawlerRepository, Name));

        taskSubMenuSet.AddMenuItem(new TestOnePageCliMenuCommand(_logger, _httpClientFactory, _parametersManager,
            _crawlerRepository, Name));

        var newStartPointCommand = new NewStartPointCliMenuCommand(_crawlerRepository, Name);
        taskSubMenuSet.AddMenuItem(newStartPointCommand);

        var task = _crawlerRepository.GetTaskByName(Name);
        if (task is not null)
        {
            foreach (var startPoint in task.StartPoints.OrderBy(o => o.StartPoint))
            {
                taskSubMenuSet.AddMenuItem(new StartPointSubMenuCliMenuCommand(_crawlerRepository, Name,
                    startPoint.StartPoint));
            }
        }

        string key = ConsoleKey.Escape.Value().ToUpperInvariant();
        taskSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to Main menu", null), key.Length);

        return taskSubMenuSet;
    }
}
