using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.MenuCommands;

public sealed class EditStartPointCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditStartPointCliMenuCommand(CrawlerServiceApiClient apiClient, string taskName, string startPoint) : base(
        "Edit Start Point", EMenuAction.LevelUp, EMenuAction.Reload, taskName)
    {
        _apiClient = apiClient;
        _taskName = taskName;
        _startPoint = startPoint;
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
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return false;
        }

        Result<TaskStartPointDto?> startPointResult =
            await _apiClient.GetStartPoint(task.TaskId, _startPoint, cancellationToken);
        if (startPointResult.IsFailure)
        {
            startPointResult.Error.PrintErrorsOnConsole();
            return false;
        }

        TaskStartPointDto? startPoint = startPointResult.Value;
        if (startPoint is null)
        {
            StShared.WriteErrorLine($"Start Point {_startPoint} in Task {_taskName} is not found", true);
            return false;
        }

        string? newStartPoint = Inputer.InputText("change Start Point ", _startPoint);
        if (string.IsNullOrWhiteSpace(newStartPoint))
        {
            return false;
        }

        if (_startPoint == newStartPoint)
        {
            return false; //თუ ცვლილება მართლაც მოითხოვეს
        }

        Result<TaskStartPointDto?> existingResult =
            await _apiClient.GetStartPoint(task.TaskId, newStartPoint, cancellationToken);
        if (existingResult.IsFailure)
        {
            existingResult.Error.PrintErrorsOnConsole();
            return false;
        }

        if (existingResult.Value is not null)
        {
            StShared.WriteErrorLine($"New Start Point {newStartPoint} is not valid", true);
            return false;
        }

        startPoint.StartPoint = newStartPoint;
        Result updateResult = await _apiClient.UpdateStartPoint(startPoint, cancellationToken);
        if (updateResult.IsFailure)
        {
            updateResult.Error.PrintErrorsOnConsole();
            return false;
        }

        return true;
    }
}
