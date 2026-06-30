using System;
using System.Linq;
using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliMenu.CliMenuCommands;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.LibDataInput;
using CrawlerConsole.Cruders;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.MenuCommands;

public sealed class TaskSubMenuCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly IParametersManager _parametersManager;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public TaskSubMenuCliMenuCommand(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, CrawlerServiceApiClient apiClient, string taskName) : base(taskName,
        EMenuAction.LoadSubMenu)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _parametersManager = parametersManager;
        _apiClient = apiClient;
        _taskName = taskName;
    }

    public override CliMenuSet GetSubMenu()
    {
        var taskSubMenuSet = new CliMenuSet($" Task => {Name}");

        var deleteTaskCommand = new DeleteTaskCliMenuCommand(_apiClient, Name);
        taskSubMenuSet.AddMenuItem(deleteTaskCommand);

        taskSubMenuSet.AddMenuItem(new EditTaskNameCliMenuCommand(_apiClient, Name));

        //პროექტის პარამეტრი
        var taskCruder = TaskCruder.Create(_apiClient);
        var editCommand = new EditItemAllFieldsInSequenceCliMenuCommand(taskCruder, _taskName);
        taskSubMenuSet.AddMenuItem(editCommand);

        taskCruder.FillDetailsSubMenu(taskSubMenuSet, _taskName);

        taskSubMenuSet.AddMenuItem(new TaskCliMenuCommand(_logger, _httpClientFactory, _apiClient, _parametersManager,
            Name));

        taskSubMenuSet.AddMenuItem(new RunTaskCliMenuCommand(_logger, _apiClient, _parametersManager,
            _apiClient, Name));

        taskSubMenuSet.AddMenuItem(new RunBatchCliMenuCommand(_logger, _apiClient, _parametersManager,
            _apiClient, Name));

        taskSubMenuSet.AddMenuItem(new TestOnePageCliMenuCommand(_logger, _apiClient, _parametersManager,
            _apiClient, Name));

        taskSubMenuSet.AddMenuItem(new ProcessMonitoringCliMenuCommand(_logger, _apiClient, _parametersManager, Name));

        taskSubMenuSet.AddMenuItem(new CancelCurrentProcessCliMenuCommand(_logger, _apiClient, _parametersManager,
            Name));

        var newStartPointCommand = new NewStartPointCliMenuCommand(_apiClient, Name);
        taskSubMenuSet.AddMenuItem(newStartPointCommand);

        OneOf<TaskDto?, Error[]> taskResult = _apiClient.GetTaskByName(Name).GetAwaiter().GetResult();
        TaskDto? task = taskResult.IsT0 ? taskResult.AsT0 : null;
        if (task is not null)
        {
            foreach (TaskStartPointDto startPoint in task.StartPoints.OrderBy(o => o.StartPoint))
            {
                taskSubMenuSet.AddMenuItem(new StartPointSubMenuCliMenuCommand(_apiClient, Name, startPoint.StartPoint));
            }
        }

        string key = ConsoleKey.Escape.Value().ToUpperInvariant();
        taskSubMenuSet.AddMenuItem(key, new ExitToMainMenuCliMenuCommand("Exit to Main menu", null), key.Length);

        return taskSubMenuSet;
    }
}
