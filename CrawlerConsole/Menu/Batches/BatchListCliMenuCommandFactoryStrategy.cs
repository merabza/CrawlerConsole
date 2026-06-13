using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using CrawlerConsole.Cruders;
using CrawlerConsoleData.Models;
using CrawlerRepoInterfaces;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.Menu.Batches;

// ReSharper disable once ClassNeverInstantiated.Global
public class BatchListCliMenuCommandFactoryStrategy(
    ILogger<BatchListCliMenuCommandFactoryStrategy> logger,
    IHttpClientFactory httpClientFactory,
    IParametersManager parametersManager,
    ICrawlerRepository crawlerRepository) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        var parameters = (CrawlerConsoleParameters)parametersManager.Parameters;
        //პაკეტების რედაქტორი
        var batchCruder = new BatchCruder(logger, httpClientFactory, parameters, crawlerRepository);
        //"Batches"
        return new CruderListCliMenuCommand(batchCruder);
    }
}
