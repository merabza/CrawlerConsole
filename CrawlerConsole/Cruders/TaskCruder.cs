using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using CrawlerServiceShared.Contracts;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.Cruders;

public sealed class TaskCruder : Cruder
{
    private readonly CrawlerServiceApiClient _apiClient;

    private TaskCruder(CrawlerServiceApiClient apiClient) : base("Task", "Tasks")
    {
        _apiClient = apiClient;
    }

    public static TaskCruder Create(CrawlerServiceApiClient apiClient)
    {
        return new TaskCruder(apiClient);
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return _apiClient.GetTasksList().GetAwaiter().GetResult().Match(
            tasks => tasks.ToDictionary(k => k.TaskName, ItemData (v) => v), failure =>
            {
                failure.Error.PrintErrorsOnConsole();
                return new Dictionary<string, ItemData>();
            });
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return GetCrudersDictionary().ContainsKey(recordKey);
    }

    public override async ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not TaskDto newTask)
        {
            return;
        }

        Result<TaskDto?> taskResult = await _apiClient.GetTaskByName(recordKey, cancellationToken);
        if (taskResult.IsFailure)
        {
            taskResult.Error.PrintErrorsOnConsole();
            return;
        }

        TaskDto? task = taskResult.Value;
        if (task is null)
        {
            StShared.WriteErrorLine($"task {recordKey} not found", true);
            return;
        }

        task.TaskName = newTask.TaskName;

        Result updateResult = await _apiClient.UpdateTask(task, cancellationToken);
        if (updateResult.IsFailure)
        {
            updateResult.Error.PrintErrorsOnConsole();
        }
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not TaskDto newTask)
        {
            return;
        }

        //ახალი ჩანაწერი იქმნება სუფთა ობიექტებით, რომ rename-ის დროს (Remove + Add) Start Point-ები შენარჩუნდეს
        var task = new TaskDto
        {
            TaskName = recordKey,
            StartPoints =
                [.. newTask.StartPoints.Select(sp => new TaskStartPointDto { StartPoint = sp.StartPoint })]
        };

        Result<TaskDto> createResult = await _apiClient.CreateTask(task, cancellationToken);
        if (createResult.IsFailure)
        {
            createResult.Error.PrintErrorsOnConsole();
        }
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        Result deleteResult = await _apiClient.DeleteTask(recordKey, cancellationToken);
        if (deleteResult.IsFailure)
        {
            deleteResult.Error.PrintErrorsOnConsole();
        }
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TaskDto { TaskName = recordKey ?? string.Empty };
    }
}
