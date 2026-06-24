using System;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerRepoInterfaces;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class NewStartPointCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public NewStartPointCliMenuCommand(ICrawlerRepository crawlerRepository, string taskName) : base("New Start Point",
        EMenuAction.Reload)
    {
        _crawlerRepository = crawlerRepository;
        _taskName = taskName;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var task = _crawlerRepository.GetTaskByName(_taskName);
        if (task is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} not found", true);
            return ValueTask.FromResult(false);
        }

        //სტარტ პოინტის შექმნის პროცესი დაიწყო
        Console.WriteLine("Create new Start Point started");

        //ახალი სტარტ პოინტის შეტანა პროგრამაში
        string? newStartPoint = Inputer.InputText("New Start Point", null);
        if (string.IsNullOrWhiteSpace(newStartPoint))
        {
            return ValueTask.FromResult(false);
        }

        //გადავამოწმოთ ხომ არ არსებობს იგივე სტარტ პოინტი
        if (_crawlerRepository.GetStartPoint(task.TaskId, newStartPoint) is not null)
        {
            StShared.WriteErrorLine(
                $"Start Point with Name {newStartPoint} is already exists. cannot create Start Point with this name. ",
                true);
            return ValueTask.FromResult(false);
        }

        //ახალი სტარტ პოინტის ჩაწერა ბაზაში
        _crawlerRepository.AddStartPoint(task.TaskId, newStartPoint);
        _crawlerRepository.SaveChanges();

        MenuAction = EMenuAction.Reload;
        return ValueTask.FromResult(true);
    }
}
