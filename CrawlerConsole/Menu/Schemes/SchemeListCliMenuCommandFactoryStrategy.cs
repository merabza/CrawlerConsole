using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using CrawlerConsole.Cruders;
using CrawlerServiceShared.Contracts;

namespace CrawlerConsole.Menu.Schemes;

// ReSharper disable once ClassNeverInstantiated.Global
public class SchemeListCliMenuCommandFactoryStrategy(CrawlerServiceApiClient apiClient) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        var schemeCruder = new SchemeCruder(apiClient);
        return new CruderListCliMenuCommand(schemeCruder);
    }
}
