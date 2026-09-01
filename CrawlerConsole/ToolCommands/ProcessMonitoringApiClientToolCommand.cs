using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using SystemTools.ReCounterContracts;
using SystemTools.SharedKernel;

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
        Result<ProgressData> statusResult =
            await CrawlerServiceApiClient.GetCurrentProcessStatus(cancellationToken);
        if (statusResult.IsFailure)
        {
            return ReturnFalseLogErrors(statusResult.Error);
        }

        if (!statusResult.Value.BoolData.GetValueOrDefault(ReCounterConstants.ProcessRun))
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

        bool stoppedByEscape = false;
        while (ProcessMonitoringManager.Instance.ProcessIsRunning)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo ch = Console.ReadKey(true);
                if (ch.Key == ConsoleKey.Escape)
                {
                    stoppedByEscape = true;
                    break;
                }
            }

            await Task.Delay(1000, cancellationToken);
        }

        bool stopMessagesResult = await CrawlerServiceApiClient.StopMessages(cancellationToken);

        if (stoppedByEscape)
        {
            Console.WriteLine("Monitoring stopped, but the process on the server continues");
        }

        return stopMessagesResult;
    }
}
