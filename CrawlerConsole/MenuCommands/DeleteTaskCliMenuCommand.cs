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

public sealed class DeleteTaskCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public DeleteTaskCliMenuCommand(CrawlerServiceApiClient apiClient, string taskName) : base("Delete Task",
        EMenuAction.LevelUp)
    {
        _apiClient = apiClient;
        _taskName = taskName;
    }

    protected override async ValueTask<bool> RunBody(CancellationToken cancellationToken = default)
    {
        OneOf<TaskDto?, Error[]> taskResult = await _apiClient.GetTaskByName(_taskName, cancellationToken);
        if (taskResult.IsT1)
        {
            Error.PrintErrorsOnConsole(taskResult.AsT1);
            return false;
        }

        if (taskResult.AsT0 is null)
        {
            StShared.WriteErrorLine($"Task {_taskName} not found", true);
            return false;
        }

        if (!Inputer.InputBool($"This will Delete  Task {_taskName}. are you sure?", false, false))
        {
            return false;
        }

        //ამოცანის წაშლა ბაზიდან Start Point-ებთან ერთად (cascade)
        Option<Error[]> deleteResult = await _apiClient.DeleteTask(_taskName, cancellationToken);
        if (deleteResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])deleteResult);
            return false;
        }

        return true;
    }
}
