using System.Net.Http;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.Services.MenuBuilder;
using CrawlerConsole.Menu.Batches;
using CrawlerConsole.Menu.CrawlerParametersEdit;
using CrawlerConsole.Menu.Hosts;
using CrawlerConsole.Menu.Schemes;
using CrawlerConsole.Menu.Tasks;
using CrawlerConsoleData.Models;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        //მენეჯმენტის (CRUD) ოპერაციებზე დამოკიდებული სერვისები ემატება მხოლოდ მაშინ, თუ მითითებულია
        //მენეჯმენტის ApiClient-ი (Server). წინააღმდეგ შემთხვევაში პროგრამა მაინც უნდა გაეშვას,
        //რომ მომხმარებელმა შეძლოს შესაბამისი პარამეტრების შეყვანა.
        if (ManagementApiClientResolver.TryResolve(par, out string server, out string? apiKey))
        {
            // @formatter:off
            services
                .AddScoped(sp => new CrawlerServiceApiClient(
                    sp.GetRequiredService<ILogger<CrawlerServiceApiClient>>(),
                    sp.GetRequiredService<IHttpClientFactory>(), server, apiKey, true))

                //მენიუს სტრატეგიები, რომლებიც მონაცემთა ბაზას CrawlerService-ის გავლით იყენებენ
                .AddTransient<IMenuCommandFactoryStrategy, HostListCliMenuCommandFactoryStrategy>()
                .AddTransient<IMenuCommandFactoryStrategy, SchemeListCliMenuCommandFactoryStrategy>()
                .AddTransient<IMenuCommandFactoryStrategy, BatchListCliMenuCommandFactoryStrategy>()
                .AddTransient<IMenuCommandFactoryStrategy, NewTaskCliMenuCommandFactoryStrategy>()
                .AddTransient<IMenuCommandListFactoryStrategy, TasksListFactoryStrategy>();
            // @formatter:on
        }

        // @formatter:off
        services
            .AddSerilogLoggerService(LogEventLevel.Warning, appName, par.LogFolder)
            .AddHttpClient()

            //მენიუს სტრატეგიები, რომლებიც სერვისზე არ არიან დამოკიდებული და ყოველთვის ხელმისაწვდომია
            .AddTransient<IMenuCommandFactoryStrategy, CrawlerParametersEditorCliMenuCommandFactoryStrategy>()

            .AddSingleton<IMenuBuilder, CrawlerMenuBuilder>()
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
