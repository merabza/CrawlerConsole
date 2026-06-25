using AppCliTools.CliMenu;
using CrawlerConsole.MenuCommands;
using CrawlerServiceShared.Contracts;

namespace CrawlerConsole.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewTaskCliMenuCommandFactoryStrategy(CrawlerServiceApiClient apiClient) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        //ახალი ამოცანის შექმნა
        return new NewTaskCliMenuCommand(apiClient);
    }
}
