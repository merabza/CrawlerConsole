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

public sealed class ClearTaskFetchedDataCliMenuCommand : CliMenuCommand
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly string _taskName;

    // ReSharper disable once ConvertToPrimaryConstructor
    public ClearTaskFetchedDataCliMenuCommand(CrawlerServiceApiClient apiClient, string taskName) : base(
        "Clear Fetched Data", EMenuAction.Reload)
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

        if (!Inputer.InputBool(
                $"This will Delete all data fetched by Task {_taskName}. The Task itself will remain. are you sure?",
                false, false))
        {
            return false;
        }

        //ტასკის მიერ მოქაჩული ინფორმაციის გასუფთავება ბაზაში (Batch თავისი შვილებით და ექსკლუზიური Urls-ები)
        Option<Error[]> clearResult = await _apiClient.ClearTaskFetchedData(_taskName, cancellationToken);
        if (clearResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])clearResult);
            return false;
        }

        return true;
    }
}