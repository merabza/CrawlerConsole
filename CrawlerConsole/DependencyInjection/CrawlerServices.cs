using AppCliTools.CliMenu;
using AppCliTools.CliParametersDataEdit;
using AppCliTools.CliTools.Services.MenuBuilder;
using CrawlerConsole.Menu.Batches;
using CrawlerConsole.Menu.CrawlerParametersEdit;
using CrawlerConsole.Menu.Hosts;
using CrawlerConsole.Menu.Schemes;
using CrawlerConsole.Menu.Tasks;
using CrawlerConsoleData.Models;
using CrawlerDbPersistence;
using CrawlerRepoInterfaces;
using CrawlerRepositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibParameters;
using ParametersManagement.LibParameters.DependencyInjection;
using Serilog.Events;
using SystemTools.DependencyInjection;
using SystemTools.SerilogStuff.DependencyInjection;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.DependencyInjection;

namespace CrawlerConsole.DependencyInjection;

public static class CrawlerServices
{
    public static IServiceCollection AddServices(this IServiceCollection services, string appName,
        CrawlerConsoleParameters par, string parametersFileName)
    {
        var databaseServerConnections = new DatabaseServerConnections(par.DatabaseServerConnections);

        (EDatabaseProvider? dataProvider, string? connectionString, int commandTimeout) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(par.DatabaseParameters,
                databaseServerConnections);

        //მონაცემთა ბაზასთან დაკავშირებული სერვისები ემატება მხოლოდ მაშინ, თუ connectionString არსებობს
        //და ბაზასთან დაკავშირება შესაძლებელია. წინააღმდეგ შემთხვევაში პროგრამა მაინც უნდა გაეშვას,
        //რომ მომხმარებელმა შეძლოს ბაზასთან დასაკავშირებელი პარამეტრების შეყვანა.
        if (!string.IsNullOrEmpty(connectionString) && DatabaseConnectionChecker.CheckConnection(par.DatabaseParameters,
                databaseServerConnections, appName, NullLogger.Instance))
        {
            // @formatter:off
            services
                .AddContextByProvider<CrawlerDbContext>(dataProvider, connectionString, commandTimeout)
                .AddSingleton<ICrawlerRepositoryCreatorFactory, CrawlerRepositoryCreatorFactory>()
                .AddScoped<ICrawlerRepository, CrawlerRepository>()

                //მენიუს სტრატეგიები, რომლებიც მონაცემთა ბაზას იყენებენ
                .AddTransient<IMenuCommandFactoryStrategy, HostListCliMenuCommandFactoryStrategy>()
                .AddTransient<IMenuCommandFactoryStrategy, SchemeListCliMenuCommandFactoryStrategy>()
                .AddTransient<IMenuCommandFactoryStrategy, BatchListCliMenuCommandFactoryStrategy>()
                .AddTransient<IMenuCommandListFactoryStrategy, TasksListFactoryStrategy>();
            // @formatter:on
        }

        // @formatter:off
        services
            .AddSerilogLoggerService(LogEventLevel.Information, appName, par.LogFolder)
            .AddHttpClient()

            //.AddMemoryCache()
            //.AddSingleton<MenuParameters>()

            //მენიუს სტრატეგიები, რომლებიც ბაზაზე არ არიან დამოკიდებული და ყოველთვის ხელმისაწვდომია
            .AddTransient<IMenuCommandFactoryStrategy, CrawlerParametersEditorCliMenuCommandFactoryStrategy>()
            .AddTransient<IMenuCommandFactoryStrategy, NewTaskCliMenuCommandFactoryStrategy>()

            //.AddSingleton<IProcesses, Processes>()
            .AddSingleton<IMenuBuilder, CrawlerMenuBuilder>()
            //.AddTransientAllStrategies<IToolCommandFactoryStrategy>(
            //    typeof(CorrectNewDatabaseToolCommandFactoryStrategy).Assembly,
            //    typeof(JetBrainsCleanupCodeRunnerToolCommandFactoryStrategy).Assembly,
            //    typeof(JsonFromProjectDbProjectGetterFactoryStrategy).Assembly,
            //    typeof(GenerateApiRoutesToolCommandFactoryStrategy).Assembly,
            //    typeof(ApplicationSettingsEncoderToolCommandFactoryStrategy).Assembly)
            .AddApplication(x =>
            {
                x.AppName = appName;
            })
            .AddMainParametersManager<ParametersManager>(x =>
            {
                x.ParametersFileName = parametersFileName;
                x.Par = par;
            });

        // @formatter:on
        return services;
    }
}
