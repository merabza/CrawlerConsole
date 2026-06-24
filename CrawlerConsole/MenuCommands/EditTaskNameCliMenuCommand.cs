using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerRepoInterfaces;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class EditTaskNameCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditTaskNameCliMenuCommand(ICrawlerRepository crawlerRepository, string taskName) : base("Edit task Name",
        EMenuAction.LevelUp, EMenuAction.Reload, taskName)
    {
        _crawlerRepository = crawlerRepository;
        _taskName = taskName;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var task = _crawlerRepository.GetTaskByName(_taskName);
        if (task is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return ValueTask.FromResult(false);
        }

        //ამოცანის სახელის რედაქტირება
        string? newTaskName = Inputer.InputText("change  Task Name ", _taskName);
        if (string.IsNullOrWhiteSpace(newTaskName))
        {
            return ValueTask.FromResult(false);
        }

        if (_taskName == newTaskName)
        {
            return ValueTask.FromResult(false); //თუ ცვლილება მართლაც მოითხოვეს
        }

        if (_crawlerRepository.GetTaskByName(newTaskName) is not null)
        {
            StShared.WriteErrorLine($"New Name For Task {newTaskName} is not valid", true);
            return ValueTask.FromResult(false);
        }

        //სახელის შეცვლა ადგილზე — TaskId და Start Point-ები უცვლელი რჩება
        task.TaskName = newTaskName;
        _crawlerRepository.UpdateTask(task);
        _crawlerRepository.SaveChanges();

        return ValueTask.FromResult(true);
    }

    protected override string GetStatus()
    {
        return _taskName;
    }
}
