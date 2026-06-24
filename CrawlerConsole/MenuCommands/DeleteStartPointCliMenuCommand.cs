using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerRepoInterfaces;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class DeleteStartPointCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteStartPointCliMenuCommand(ICrawlerRepository crawlerRepository, string taskName, string startPoint) :
        base("Delete Start Point", EMenuAction.LevelUp)
    {
        _crawlerRepository = crawlerRepository;
        _taskName = taskName;
        _startPoint = startPoint;
    }

    protected override ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        var task = _crawlerRepository.GetTaskByName(_taskName);
        if (task is null)
        {
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return ValueTask.FromResult(false);
        }

        var startPoint = _crawlerRepository.GetStartPoint(task.TaskId, _startPoint);
        if (startPoint is null)
        {
            StShared.WriteErrorLine($"Start Point {_startPoint} in Task {_taskName} is not found", true);
            return ValueTask.FromResult(false);
        }

        if (!Inputer.InputBool($"This will Delete Start Point {_startPoint}. are you sure?", false, false))
        {
            return ValueTask.FromResult(false);
        }

        _crawlerRepository.DeleteStartPoint(startPoint);
        _crawlerRepository.SaveChanges();

        return ValueTask.FromResult(true);
    }
}
