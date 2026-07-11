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

public sealed class CancelCurrentProcessApiClientToolCommand : ApiClientToolAction
{
    public const string ActionName = "Cancel Current Process";
    private const int WaitForStopSeconds = 60;

    // ReSharper disable once ConvertToPrimaryConstructor
    public CancelCurrentProcessApiClientToolCommand(ILogger logger, CrawlerServiceApiClient crawlerServiceApiClient) :
        base(logger, ActionName, crawlerServiceApiClient)
    {
    }

    protected override async ValueTask<bool> RunAction(CancellationToken cancellationToken = default)
    {
        OneOf<bool, Error[]> result = await CrawlerServiceApiClient.CancelCurrentProcess(cancellationToken);
        if (result.IsT1)
        {
            return ReturnFalseLogErrors(result.AsT1);
        }

        if (!result.AsT0)
        {
            return false;
        }

        //დაველოდოთ, სანამ სერვერზე პროცესი ნამდვილად გაჩერდება
        Console.WriteLine("Waiting for process to stop...");

        for (int i = 0; i < WaitForStopSeconds; i++)
        {
            OneOf<ProgressData, Error[]> statusResult =
                await CrawlerServiceApiClient.GetCurrentProcessStatus(cancellationToken);
            if (statusResult.IsT1)
            {
                return ReturnFalseLogErrors(statusResult.AsT1);
            }

            if (!statusResult.AsT0.BoolData.GetValueOrDefault(ReCounterConstants.ProcessRun))
            {
                Console.WriteLine("Process stopped");
                return true;
            }

            await Task.Delay(1000, cancellationToken);
        }

        Console.WriteLine($"Process did not stop in {WaitForStopSeconds} seconds");
        return false;
    }
}
