using AppCliTools.CliMenu;
using CrawlerConsole.MenuCommands;
using ParametersManagement.LibParameters;

namespace CrawlerConsole.Menu.Tasks;

// ReSharper disable once ClassNeverInstantiated.Global
public class NewTaskCliMenuCommandFactoryStrategy(IParametersManager parametersManager) : IMenuCommandFactoryStrategy
{
    public CliMenuCommand CreateMenuCommand()
    {
        //ახალი ამოცანის შექმნა
        return new NewTaskCliMenuCommand(parametersManager);
    }
}
