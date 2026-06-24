using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParametersApiClientsEdit.FieldEditors;
using CrawlerDbModels;
using CrawlerRepoInterfaces;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.Cruders;

public sealed class TaskCruder : Cruder
{
    private readonly ICrawlerRepository _crawlerRepository;

    public TaskCruder(ILogger logger, IHttpClientFactory httpClientFactory, IParametersManager parametersManager,
        ICrawlerRepository crawlerRepository) : base("Task", "Tasks")
    {
        _crawlerRepository = crawlerRepository;
        FieldEditors.Add(new ApiClientNameFieldEditor(nameof(TaskModel.ApiName), logger, httpClientFactory,
            parametersManager));
    }

    public static TaskCruder Create(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager, ICrawlerRepository crawlerRepository)
    {
        return new TaskCruder(logger, httpClientFactory, parametersManager, crawlerRepository);
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return _crawlerRepository.GetTasksList().ToDictionary(k => k.TaskName, ItemData (v) => v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return GetCrudersDictionary().ContainsKey(recordKey);
    }

    public override ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not TaskModel newTask)
        {
            return ValueTask.CompletedTask;
        }

        TaskModel task = _crawlerRepository.GetTaskByName(recordKey) ?? throw new Exception("task is null");

        task.TaskName = newTask.TaskName;
        task.ApiName = newTask.ApiName;
        _crawlerRepository.UpdateTask(task);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not TaskModel newTask)
        {
            return ValueTask.CompletedTask;
        }

        //ახალი ჩანაწერი იქმნება სუფთა ობიექტებით, რომ rename-ის დროს (Remove + Add) Start Point-ები შენარჩუნდეს
        var task = new TaskModel
        {
            TaskName = recordKey,
            ApiName = newTask.ApiName,
            StartPoints = newTask.StartPoints.Select(sp => new TaskStartPoint { StartPoint = sp.StartPoint }).ToList()
        };
        _crawlerRepository.CreateTask(task);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ValueTask RemoveRecordWithKey(string recordKey, CancellationToken cancellationToken = default)
    {
        TaskModel task = _crawlerRepository.GetTaskByName(recordKey) ?? throw new Exception("task is null");
        _crawlerRepository.DeleteTask(task);

        _crawlerRepository.SaveChanges();
        return ValueTask.CompletedTask;
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TaskModel { TaskName = recordKey ?? string.Empty };
    }
}
