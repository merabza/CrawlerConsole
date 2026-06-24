using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerDbModels;
using CrawlerRepoInterfaces;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class NewTaskCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;

    //ახალი აპლიკაციის ამოცანის შექმნა

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewTaskCliMenuCommand(ICrawlerRepository crawlerRepository) : base("New Task", EMenuAction.Reload)
    {
        _crawlerRepository = crawlerRepository;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        //ამოცანის შექმნის პროცესი დაიწყო
        Console.WriteLine("Create new Task started");

        //ახალი ამოცანის სახელის შეტანა პროგრამაში
        string? newTaskName = Inputer.InputText("New Task Name", null);
        if (string.IsNullOrEmpty(newTaskName))
        {
            return ValueTask.FromResult(false);
        }

        //გადავამოწმოთ ხომ არ არსებობს იგივე სახელით სხვა ამოცანა.
        if (_crawlerRepository.GetTaskByName(newTaskName) is not null)
        {
            StShared.WriteErrorLine(
                $"Task with Name {newTaskName} is already exists. cannot create task with this name. ", true);
            return ValueTask.FromResult(false);
        }

        //ახალი ამოცანის შექმნა და ჩაწერა ბაზაში
        _crawlerRepository.CreateTask(new TaskModel { TaskName = newTaskName });
        _crawlerRepository.SaveChanges();

        //ყველაფერი კარგად დასრულდა
        return ValueTask.FromResult(true);
    }
}
