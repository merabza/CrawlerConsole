using AppCliTools.CliMenu;
using CrawlerConsole.MenuCommands;
using CrawlerRepoInterfaces;

namespace CrawlerConsole.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewTaskCliMenuCommandFactoryStrategy(ICrawlerRepository crawlerRepository) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        //ახალი ამოცანის შექმნა
        return new NewTaskCliMenuCommand(crawlerRepository);
    }
}
