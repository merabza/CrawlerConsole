using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerRepoInterfaces;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class EditStartPointCliMenuCommand : CliMenuCommand
{
    private readonly ICrawlerRepository _crawlerRepository;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditStartPointCliMenuCommand(ICrawlerRepository crawlerRepository, string taskName, string startPoint) :
        base("Edit Start Point", EMenuAction.LevelUp, EMenuAction.Reload, taskName)
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

        string? newStartPoint = Inputer.InputText("change Start Point ", _startPoint);
        if (string.IsNullOrWhiteSpace(newStartPoint))
        {
            return ValueTask.FromResult(false);
        }

        if (_startPoint == newStartPoint)
        {
            return ValueTask.FromResult(false); //თუ ცვლილება მართლაც მოითხოვეს
        }

        if (_crawlerRepository.GetStartPoint(task.TaskId, newStartPoint) is not null)
        {
            StShared.WriteErrorLine($"New Start Point {newStartPoint} is not valid", true);
            return ValueTask.FromResult(false);
        }

        startPoint.StartPoint = newStartPoint;
        _crawlerRepository.UpdateStartPoint(startPoint);
        _crawlerRepository.SaveChanges();

        return ValueTask.FromResult(true);
    }
}
