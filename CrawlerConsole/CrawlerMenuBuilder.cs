using System;
using System.Collections.Generic;
using System.Linq;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.Services.MenuBuilder;
using CrawlerConsole.Menu;
using CrawlerConsole.Menu.Batches;
using CrawlerConsole.Menu.Hosts;
using CrawlerConsole.Menu.Schemes;
using CrawlerConsole.Menu.Tasks;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole;

public sealed class CrawlerMenuBuilder : IMenuBuilder
{
    private readonly IApplication _application;
    private readonly ILogger<CrawlerMenuBuilder> _logger;
    private readonly IParametersManager _parametersManager;
    private readonly IServiceProvider _serviceProvider;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CrawlerMenuBuilder(IServiceProvider serviceProvider, IParametersManager parametersManager,
        ILogger<CrawlerMenuBuilder> logger, IApplication application)
    {
        _serviceProvider = serviceProvider;
        _parametersManager = parametersManager;
        _logger = logger;
        _application = application;
    }

    public CliMenuSet BuildMainMenu()
    {
        List<string> excludeList = [];

        if (!CheckConnection())
        {
            excludeList.Add(nameof(HostListCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(SchemeListCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(BatchListCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(NewTaskCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(TasksListFactoryStrategy));
        }

        //მთავარი მენიუს ჩატვირთვა
        return CliMenuSetFactory.CreateMenuSet("Main Menu",
            MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList).ToList(), _serviceProvider, true);
    }

    private bool CheckConnection()
    {
        Console.WriteLine("Checking connection to database...");

        var parameters = (CrawlerConsoleParameters)_parametersManager.Parameters;
        var databaseServerConnections = new DatabaseServerConnections(parameters.DatabaseServerConnections);

        return DatabaseConnectionChecker.CheckConnection(parameters.DatabaseParameters, databaseServerConnections,
            _application.AppName, _logger);
    }
}
