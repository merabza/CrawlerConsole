using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerRepoInterfaces;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class DeleteTaskCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteTaskCliMenuCommand(ICrawlerRepository crawlerRepository, string taskName) : base("Delete Task",
        EMenuAction.LevelUp)
    {
        _crawlerRepository = crawlerRepository;
        _taskName = taskName;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var task = _crawlerRepository.GetTaskByName(_taskName);
        if (task is null)
        {
            StShared.WriteErrorLine($"Task {_taskName} not found", true);
            return ValueTask.FromResult(false);
        }

        if (!Inputer.InputBool($"This will Delete  Task {_taskName}. are you sure?", false, false))
        {
            return ValueTask.FromResult(false);
        }

        //ამოცანის წაშლა ბაზიდან Start Point-ებთან ერთად (cascade)
        _crawlerRepository.DeleteTask(task);
        _crawlerRepository.SaveChanges();

        return ValueTask.FromResult(true);
    }
}
