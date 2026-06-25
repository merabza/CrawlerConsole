using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliMenu;
using AppCliTools.CliParameters.CliMenuCommands;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using CrawlerConsole.MenuCommands;
using CrawlerConsoleData.Models;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.Cruders;

public sealed class BatchCruder : Cruder
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;
    private readonly CrawlerConsoleParameters _par;

    public BatchCruder(ILogger logger, IHttpClientFactory httpClientFactory, CrawlerConsoleParameters par,
        CrawlerServiceApiClient apiClient) : base("Batch", "Batches")
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _par = par;
        _apiClient = apiClient;

        FieldEditors.Add(new BoolFieldEditor(nameof(BatchDto.IsOpen)));
        FieldEditors.Add(new BoolFieldEditor(nameof(BatchDto.AutoCreateNextPart)));
    }

    private List<BatchDto> GetBatches()
    {
        return _apiClient.GetBatchesList().GetAwaiter().GetResult().Match(
            batches => batches,
            errors =>
            {
                Error.PrintErrorsOnConsole(errors);
                return new List<BatchDto>();
            });
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return GetBatches().ToDictionary(k => k.BatchName, ItemData (v) => v);
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return GetCrudersDictionary().ContainsKey(recordKey);
    }

    public override async ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not BatchDto newBatch)
        {
            return;
        }

        OneOf<BatchDto?, Error[]> batchResult = await _apiClient.GetBatchByName(recordKey, cancellationToken);
        if (batchResult.IsT1)
        {
            Error.PrintErrorsOnConsole(batchResult.AsT1);
            return;
        }

        BatchDto? batch = batchResult.AsT0;
        if (batch is null)
        {
            StShared.WriteErrorLine($"batch {recordKey} not found", true);
            return;
        }

        batch.BatchName = newBatch.BatchName;

        Option<Error[]> updateResult = await _apiClient.UpdateBatch(batch, cancellationToken);
        if (updateResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])updateResult);
        }
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not BatchDto newBatch)
        {
            return;
        }

        OneOf<BatchDto, Error[]> createResult = await _apiClient.CreateBatch(newBatch, cancellationToken);
        if (createResult.IsT1)
        {
            Error.PrintErrorsOnConsole(createResult.AsT1);
        }
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        Option<Error[]> deleteResult = await _apiClient.DeleteBatch(recordKey, cancellationToken);
        if (deleteResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])deleteResult);
        }
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new BatchDto { BatchName = recordKey ?? string.Empty };
    }

    public override void FillDetailsSubMenu(CliMenuSet itemSubMenuSet, string itemName)
    {
        base.FillDetailsSubMenu(itemSubMenuSet, itemName);

        Dictionary<string, BatchDto> batches = GetBatches().ToDictionary(k => k.BatchName, v => v);
        BatchDto batch = batches[itemName];

        itemSubMenuSet.AddMenuItem(new BatchTaskCliMenuCommand(_logger, _httpClientFactory, _apiClient, _par, batch));

        var detailsCruder = new HostByBatchCruder(_apiClient, batch);
        var newItemCommand = new NewItemCliMenuCommand(detailsCruder, itemName, $"Create New {detailsCruder.CrudName}");
        itemSubMenuSet.AddMenuItem(newItemCommand);

        List<string> hostNames = detailsCruder.GetHostNamesByBatch();

        foreach (HostSubMenuCliMenuCommand detailListCommand in hostNames.Select(s =>
                     new HostSubMenuCliMenuCommand(detailsCruder, s, itemName, true)))
        {
            itemSubMenuSet.AddMenuItem(detailListCommand);
        }
    }
}
