using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using CrawlerConsole.Cruders;
using CrawlerServiceShared.Contracts;

namespace CrawlerConsole.Menu.Hosts;

// ReSharper disable once ClassNeverInstantiated.Global
public class HostListCliMenuCommandFactoryStrategy(CrawlerServiceApiClient apiClient) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        //ჰოსტების რედაქტორი
        var hostCruder = new HostCruder(apiClient);
        //"Hosts"
        return new CruderListCliMenuCommand(hostCruder);
    }
}
