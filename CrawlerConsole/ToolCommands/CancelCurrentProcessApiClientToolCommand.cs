using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CrawlerServiceShared.Contracts;
using Microsoft.Extensions.Logging;
using SystemTools.ReCounterContracts;
using SystemTools.SharedKernel;

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
        Result<bool> result = await CrawlerServiceApiClient.CancelCurrentProcess(cancellationToken);
        if (result.IsFailure)
        {
            return ReturnFalseLogErrors(result.Error);
        }

        if (!result.Value)
        {
            //სერვერზე გასაუქმებელი პროცესი არ არის
            Console.WriteLine("No running process to cancel");
            return false;
        }

        //დაველოდოთ, სანამ სერვერზე პროცესი ნამდვილად გაჩერდება
        Console.WriteLine("Waiting for process to stop...");

        for (int i = 0; i < WaitForStopSeconds; i++)
        {
            Result<ProgressData> statusResult =
                await CrawlerServiceApiClient.GetCurrentProcessStatus(cancellationToken);
            if (statusResult.IsFailure)
            {
                return ReturnFalseLogErrors(statusResult.Error);
            }

            if (!statusResult.Value.BoolData.GetValueOrDefault(ReCounterConstants.ProcessRun))
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
