using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.ReCounterContracts;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.ToolCommands;

public sealed class ProcessMonitoringApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Process Monitoring";

    // ReSharper disable once ConvertToPrimaryConstructor
    public ProcessMonitoringApiClientToolCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient) :
        base(logger, ActionName, crawlerServiceApiClient)
    {
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        OneOf<ProgressData, Error[]> statusResult =
            await CrawlerServiceApiClient.GetCurrentProcessStatus(cancellationToken);
        if (statusResult.IsT1)
        {
            return ReturnFalseLogErrors(statusResult.AsT1);
        }

        if (!statusResult.AsT0.BoolData.GetValueOrDefault(ReCounterConstants.ProcessRun))
        {
            Console.WriteLine("Process is not running");
            return true;
        }

        ProcessMonitoringManager.Instance.ProcessIsRunning = true;

        if (!await CrawlerServiceApiClient.RunMessages(cancellationToken))
        {
            return false;
        }

        Console.WriteLine("Press Escape for Stop monitoring");

        while (ProcessMonitoringManager.Instance.ProcessIsRunning)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo ch = Console.ReadKey(true);
                if (ch.Key == ConsoleKey.Escape)
                {
                    break;
                }
            }

            await Task.Delay(1000, cancellationToken);
        }

        return await CrawlerServiceApiClient.StopMessages(cancellationToken);
    }
}
