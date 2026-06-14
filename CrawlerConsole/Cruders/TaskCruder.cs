using System.Collections.Generic;
using System.Net.Http;
using AppCliTools.CliParameters;
using AppCliTools.CliParametersApiClientsEdit.FieldEditors;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.Cruders;

public sealed class TaskCruder : ParCruder<TaskModel>
{
    public TaskCruder(ILogger logger, IHttpClientFactory httpClientFactory, IParametersManager parametersManager,
        Dictionary<string, TaskModel> currentValuesDictionary) : base(parametersManager, currentValuesDictionary,
        "Task", "Tasks")
    {
        FieldEditors.Add(new ApiClientNameFieldEditor(nameof(TaskModel.ApiName), logger, httpClientFactory,
            ParametersManager));
        //FieldEditors.Add(new EnumFieldEditor<EApiType>(nameof(TaskModel.ApiType), EApiType.ProGrammarGe, true));
        //FieldEditors.Add(new EnumFieldEditor<ECountType>(nameof(TaskModel.CountType), ECountType.Full, true));
        //FieldEditors.Add(new TextFieldEditor(nameof(TaskModel.InstanceName), Environment.MachineName.Capitalize()));
    }

    public static TaskCruder Create(ILogger logger, IHttpClientFactory httpClientFactory,
        IParametersManager parametersManager)
    {
        var parameters = (CrawlerConsoleParameters)parametersManager.Parameters;
        return new TaskCruder(logger, httpClientFactory, parametersManager, parameters.Tasks);
    }
}
