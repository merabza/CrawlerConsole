using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using OneOf;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

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
        OneOf<TaskDto?, Error[]> taskResult = await _apiClient.GetTaskByName(_taskName, cancellationToken);
        if (taskResult.IsT1)
        {
            Error.PrintErrorsOnConsole(taskResult.AsT1);
            return false;
        }

        TaskDto? task = taskResult.AsT0;
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

        OneOf<TaskDto?, Error[]> existingResult = await _apiClient.GetTaskByName(newTaskName, cancellationToken);
        if (existingResult.IsT1)
        {
            Error.PrintErrorsOnConsole(existingResult.AsT1);
            return false;
        }

        if (existingResult.AsT0 is not null)
        {
            StShared.WriteErrorLine($"New Name For Task {newTaskName} is not valid", true);
            return false;
        }

        //სახელის შეცვლა ადგილზე — TaskId და Start Point-ები უცვლელი რჩება
        task.TaskName = newTaskName;
        Option<Error[]> updateResult = await _apiClient.UpdateTask(task, cancellationToken);
        if (updateResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])updateResult);
            return false;
        }

        return true;
    }

    protected override string GetStatus()
    {
        return _taskName;
    }
}
