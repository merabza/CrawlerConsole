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
using ParametersManagement.LibParameters;

namespace CrawlerConsole;

public sealed class CrawlerMenuBuilder(IServiceProvider serviceProvider, IParametersManager parametersManager)
    : IMenuBuilder
{
    public CliMenuSet BuildMainMenu()
    {
        List<string> excludeList = [];

        if (CheckConnection())
        {
            return CliMenuSetFactory.CreateMenuSet("Main Menu",
                MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList).ToList(), serviceProvider, true);
        }

        excludeList.Add(nameof(HostListCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(SchemeListCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(BatchListCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(NewTaskCliMenuCommandFactoryStrategy));
        excludeList.Add(nameof(TasksListFactoryStrategy));

        //მთავარი მენიუს ჩატვირთვა
        return CliMenuSetFactory.CreateMenuSet("Main Menu",
            MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList).ToList(), serviceProvider, true);
    }

    private bool CheckConnection()
    {
        Console.WriteLine("Checking management service configuration...");

        var parameters = (CrawlerConsoleParameters)parametersManager.Parameters;

        return ManagementApiClientResolver.TryResolve(parameters, out _, out _);
    }
}
