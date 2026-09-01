using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class EditTaskNameCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditTaskNameCliMenuCommand(CrawlerServiceApiClient apiClient, string taskName) : base("Edit task Name",
        EMenuAction.LevelUp, EMenuAction.Reload, taskName)
    {
        _apiClient = apiClient;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        Result<TaskDto?> taskResult = await _apiClient.GetTaskByName(_taskName, cancellationToken);
        if (taskResult.IsFailure)
        {
            taskResult.Error.PrintErrorsOnConsole();
            return false;
        }

        TaskDto? task = taskResult.Value;
        if (task is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return false;
        }

        //ამოცანის სახელის რედაქტირება
        string? newTaskName = Inputer.InputText("change  Task Name ", _taskName);
        if (string.IsNullOrWhiteSpace(newTaskName))
        {
            return false;
        }

        if (_taskName == newTaskName)
        {
            return false; //თუ ცვლილება მართლაც მოითხოვეს
        }

        Result<TaskDto?> existingResult = await _apiClient.GetTaskByName(newTaskName, cancellationToken);
        if (existingResult.IsFailure)
        {
            existingResult.Error.PrintErrorsOnConsole();
            return false;
        }

        if (existingResult.Value is not null)
        {
            StShared.WriteErrorLine($"New Name For Task {newTaskName} is not valid", true);
            return false;
        }

        //სახელის შეცვლა ადგილზე — TaskId და Start Point-ები უცვლელი რჩება
        task.TaskName = newTaskName;
        Result updateResult = await _apiClient.UpdateTask(task, cancellationToken);
        if (updateResult.IsFailure)
        {
            updateResult.Error.PrintErrorsOnConsole();
            return false;
        }

        return true;
    }

    protected override string GetStatus()
    {
        return _taskName;
    }
}
