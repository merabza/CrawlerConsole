using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.Cruders;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.Cruders;

public sealed class HostByBatchCruder : Cruder
{
    private readonly CrawlerServiceApiClient _apiClient;
    private readonly BatchDto _batch;

    // ReSharper disable once ConvertToPrimaryConstructor
    public HostByBatchCruder(CrawlerServiceApiClient apiClient, BatchDto batch) : base("Host Name", "Host Names")
    {
        _apiClient = apiClient;
        _batch = batch;
    }

    public List<string> GetHostNamesByBatch()
    {
        return _apiClient.GetHostStartUrlNamesByBatch(_batch.BatchName).GetAwaiter().GetResult().Match(
            names => names,
            errors =>
            {
                Error.PrintErrorsOnConsole(errors);
                return new List<string>();
            });
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return GetHostNamesByBatch().ToDictionary(k => k, ItemData (v) => new TextItemData { Text = v });
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new TextItemData();
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return GetHostNamesByBatch().Contains(recordKey);
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        var uri = new Uri(recordKey);
        Option<Error[]> removeResult =
            await _apiClient.RemoveHostByBatch(_batch.BatchName, uri.Scheme, uri.Host, cancellationToken);
        if (removeResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])removeResult);
        }
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        var uri = new Uri(recordKey);
        Option<Error[]> addResult = await _apiClient.AddHostByBatch(
            new HostByBatchRequest { BatchName = _batch.BatchName, SchemeName = uri.Scheme, HostName = uri.Host },
            cancellationToken);
        if (addResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])addResult);
        }
    }
}
