using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using CrawlerConsole.Cruders;
using CrawlerConsoleData.Models;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.Menu.Batches;

// ReSharper disable once ClassNeverInstantiated.Global
public class BatchListCliMenuCommandFactoryStrategy(
    ILogger<BatchListCliMenuCommandFactoryStrategy> logger,
    IHttpClientFactory httpClientFactory,
    IParametersManager parametersManager,
    CrawlerServiceApiClient apiClient) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        var parameters = (CrawlerConsoleParameters)parametersManager.Parameters;
        //პაკეტების რედაქტორი
        var batchCruder = new BatchCruder(logger, httpClientFactory, parameters, apiClient);
        //"Batches"
        return new CruderListCliMenuCommand(batchCruder);
    }
}
