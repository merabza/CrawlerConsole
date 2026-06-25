using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using OneOf;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

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
        OneOf<TaskDto?, Error[]> taskResult = await _apiClient.GetTaskByName(_taskName, cancellationToken);
        if (taskResult.IsT1)
        {
            Error.PrintErrorsOnConsole(taskResult.AsT1);
            return false;
        }

        TaskDto? task = taskResult.AsT0;
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
        OneOf<TaskStartPointDto?, Error[]> existingResult =
            await _apiClient.GetStartPoint(task.TaskId, newStartPoint, cancellationToken);
        if (existingResult.IsT1)
        {
            Error.PrintErrorsOnConsole(existingResult.AsT1);
            return false;
        }

        if (existingResult.AsT0 is not null)
        {
            StShared.WriteErrorLine(
                $"Start Point with Name {newStartPoint} is already exists. cannot create Start Point with this name. ",
                true);
            return false;
        }

        //ახალი სტარტ პოინტის ჩაწერა ბაზაში
        OneOf<TaskStartPointDto, Error[]> addResult = await _apiClient.AddStartPoint(
            new AddStartPointRequest { TaskId = task.TaskId, StartPoint = newStartPoint }, cancellationToken);
        if (addResult.IsT1)
        {
            Error.PrintErrorsOnConsole(addResult.AsT1);
            return false;
        }

        MenuAction = EMenuAction.Reload;
        return true;
    }
}
