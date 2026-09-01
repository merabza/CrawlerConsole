using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliTools.Services.MenuBuilder;
using CrawlerConsole.Menu;
using CrawlerConsole.Menu.Batches;
using CrawlerConsole.Menu.Hosts;
using CrawlerConsole.Menu.Schemes;
using CrawlerConsole.Menu.Tasks;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibParameters;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;
using SystemTools.TestApiContracts;

namespace CrawlerConsole;

public sealed class CrawlerMenuBuilder(
    IServiceProvider serviceProvider,
    IParametersManager parametersManager,
    IHttpClientFactory httpClientFactory,
    ILogger<CrawlerMenuBuilder> logger) : IMenuBuilder
{
    public Task<CliMenuSet?> BuildMainMenu()
    {
        try
        {
            List<string> excludeList = [];

            if (CheckConnection())
            {
                return Task.FromResult(CliMenuSetFactory.CreateMenuSet("Main Menu",
                    [.. MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList)], serviceProvider, true));
            }

            excludeList.Add(nameof(HostListCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(SchemeListCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(BatchListCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(NewTaskCliMenuCommandFactoryStrategy));
            excludeList.Add(nameof(TasksListFactoryStrategy));

            //მთავარი მენიუს ჩატვირთვა
            return Task.FromResult(CliMenuSetFactory.CreateMenuSet("Main Menu",
                [.. MenuData.MainMenuCommandFactoryStrategyNames.Except(excludeList)], serviceProvider, true));
        }
        catch (Exception exception)
        {
            return Task.FromException<CliMenuSet?>(exception);
        }
    }

    private bool CheckConnection()
    {
        try
        {
            Console.WriteLine("Checking management service configuration...");

            var parameters = (CrawlerConsoleParameters)parametersManager.Parameters;

            if (!ManagementApiClientResolver.TryResolve(parameters, out string server, out _)) { return false; }

            //კლიენტის შექმნა ვერსიის შესამოწმებლად
            var apiClient = new TestApiClient(logger, httpClientFactory, server, true);

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();
            Result<bool> testConnectionResult = apiClient.TestConnection(token).Result;

            if (testConnectionResult.IsSuccess)
            {
                return testConnectionResult.Value;
            }

            testConnectionResult.Error.PrintErrorsOnConsole();
            return false;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
            return false;
        }
        catch
        {
            StShared.WriteErrorLine(
                "ErrorOmd when checked connection. Start server part, or configure connection to right server", true,
                null, false);
            return false;
        }
    }
}
