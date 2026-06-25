using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using OneOf;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.MenuCommands;

public sealed class EditStartPointCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public EditStartPointCliMenuCommand(CrawlerServiceApiClient apiClient, string taskName, string startPoint) :
        base("Edit Start Point", EMenuAction.LevelUp, EMenuAction.Reload, taskName)
    {
        _apiClient = apiClient;
        _taskName = taskName;
        _startPoint = startPoint;
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
            StShared.WriteErrorLine($"Task with name {_taskName} is not found", true);
            return false;
        }

        OneOf<TaskStartPointDto?, Error[]> startPointResult =
            await _apiClient.GetStartPoint(task.TaskId, _startPoint, cancellationToken);
        if (startPointResult.IsT1)
        {
            Error.PrintErrorsOnConsole(startPointResult.AsT1);
            return false;
        }

        TaskStartPointDto? startPoint = startPointResult.AsT0;
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

        OneOf<TaskStartPointDto?, Error[]> existingResult =
            await _apiClient.GetStartPoint(task.TaskId, newStartPoint, cancellationToken);
        if (existingResult.IsT1)
        {
            Error.PrintErrorsOnConsole(existingResult.AsT1);
            return false;
        }

        if (existingResult.AsT0 is not null)
        {
            StShared.WriteErrorLine($"New Start Point {newStartPoint} is not valid", true);
            return false;
        }

        startPoint.StartPoint = newStartPoint;
        Option<Error[]> updateResult = await _apiClient.UpdateStartPoint(startPoint, cancellationToken);
        if (updateResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])updateResult);
            return false;
        }

        return true;
    }
}
