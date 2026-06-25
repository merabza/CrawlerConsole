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

public sealed class NewTaskCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;

    //ახალი აპლიკაციის ამოცანის შექმნა

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewTaskCliMenuCommand(CrawlerServiceApiClient apiClient) : base("New Task", EMenuAction.Reload)
    {
        _apiClient = apiClient;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        //ამოცანის შექმნის პროცესი დაიწყო
        Console.WriteLine("Create new Task started");

        //ახალი ამოცანის სახელის შეტანა პროგრამაში
        string? newTaskName = Inputer.InputText("New Task Name", null);
        if (string.IsNullOrEmpty(newTaskName))
        {
            return false;
        }

        //გადავამოწმოთ ხომ არ არსებობს იგივე სახელით სხვა ამოცანა.
        OneOf<TaskDto?, Error[]> existingResult = await _apiClient.GetTaskByName(newTaskName, cancellationToken);
        if (existingResult.IsT1)
        {
            Error.PrintErrorsOnConsole(existingResult.AsT1);
            return false;
        }

        if (existingResult.AsT0 is not null)
        {
            StShared.WriteErrorLine(
                $"Task with Name {newTaskName} is already exists. cannot create task with this name. ", true);
            return false;
        }

        //ახალი ამოცანის შექმნა და ჩაწერა ბაზაში
        OneOf<TaskDto, Error[]> createResult =
            await _apiClient.CreateTask(new TaskDto { TaskName = newTaskName }, cancellationToken);
        if (createResult.IsT1)
        {
            Error.PrintErrorsOnConsole(createResult.AsT1);
            return false;
        }

        //ყველაფერი კარგად დასრულდა
        return true;
    }
}
