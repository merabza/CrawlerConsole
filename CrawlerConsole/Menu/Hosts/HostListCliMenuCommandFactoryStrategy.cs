using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using CrawlerConsole.Cruders;
using CrawlerRepoInterfaces;

namespace CrawlerConsole.Menu.Hosts;

// ReSharper disable once ClassNeverInstantiated.Global
public class HostListCliMenuCommandFactoryStrategy(ICrawlerRepository crawlerRepository) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        //ჰოსტების რედაქტორი
        var hostCruder = new HostCruder(crawlerRepository);
        //"Hosts"
        return new CruderListCliMenuCommand(hostCruder);
    }
}
