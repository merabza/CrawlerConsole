using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class NewStartPointCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewStartPointCliMenuCommand(CrawlerServiceApiClient apiClient, string taskName) : base("New Start Point",
        EMenuAction.Reload)
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
            StShared.WriteErrorLine($"Task with name {_taskName} not found", true);
            return false;
        }

        //სტარტ პოინტის შექმნის პროცესი დაიწყო
        Console.WriteLine("Create new Start Point started");

        //ახალი სტარტ პოინტის შეტანა პროგრამაში
        string? newStartPoint = Inputer.InputText("New Start Point", null);
        if (string.IsNullOrWhiteSpace(newStartPoint))
        {
            return false;
        }

        //გადავამოწმოთ ხომ არ არსებობს იგივე სტარტ პოინტი
        Result<TaskStartPointDto?> existingResult =
            await _apiClient.GetStartPoint(task.TaskId, newStartPoint, cancellationToken);
        if (existingResult.IsFailure)
        {
            existingResult.Error.PrintErrorsOnConsole();
            return false;
        }

        if (existingResult.Value is not null)
        {
            StShared.WriteErrorLine(
                $"Start Point with Name {newStartPoint} is already exists. cannot create Start Point with this name. ",
                true);
            return false;
        }

        //ახალი სტარტ პოინტის ჩაწერა ბაზაში
        Result<TaskStartPointDto> addResult = await _apiClient.AddStartPoint(
            new AddStartPointRequest { TaskId = task.TaskId, StartPoint = newStartPoint }, cancellationToken);
        if (addResult.IsFailure)
        {
            addResult.Error.PrintErrorsOnConsole();
            return false;
        }

        MenuAction = EMenuAction.Reload;
        return true;
    }
}
