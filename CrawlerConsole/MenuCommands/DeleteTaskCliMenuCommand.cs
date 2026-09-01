using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.LibDataInput;
using CrawlerServiceShared.Contracts;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

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
        Result<TaskDto?> taskResult = await _apiClient.GetTaskByName(_taskName, cancellationToken);
        if (taskResult.IsFailure)
        {
            taskResult.Error.PrintErrorsOnConsole();
            return false;
        }

        if (taskResult.Value is null)
        {
            StShared.WriteErrorLine($"Task {_taskName} not found", true);
            return false;
        }

        if (!Inputer.InputBool($"This will Delete  Task {_taskName}. are you sure?", false, false))
        {
            return false;
        }

        //ამოცანის წაშლა ბაზიდან Start Point-ებთან ერთად (cascade)
        Result deleteResult = await _apiClient.DeleteTask(_taskName, cancellationToken);
        if (deleteResult.IsFailure)
        {
            deleteResult.Error.PrintErrorsOnConsole();
            return false;
        }

        return true;
    }
}
