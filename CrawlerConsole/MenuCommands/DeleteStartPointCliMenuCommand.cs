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

public sealed class DeleteStartPointCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _startPoint;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteStartPointCliMenuCommand(CrawlerServiceApiClient apiClient, string taskName, string startPoint) :
        base("Delete Start Point", EMenuAction.LevelUp)
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

        if (startPointResult.AsT0 is null)
        {
            StShared.WriteErrorLine($"Start Point {_startPoint} in Task {_taskName} is not found", true);
            return false;
        }

        if (!Inputer.InputBool($"This will Delete Start Point {_startPoint}. are you sure?", false, false))
        {
            return false;
        }

        Option<Error[]> deleteResult = await _apiClient.DeleteStartPoint(task.TaskId, _startPoint, cancellationToken);
        if (deleteResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])deleteResult);
            return false;
        }

        return true;
    }
}
