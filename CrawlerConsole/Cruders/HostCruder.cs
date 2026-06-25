using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using CrawlerServiceShared.Contracts;
using LanguageExt;
using OneOf;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole.Cruders;

public sealed class HostCruder : Cruder
{
    private readonly CrawlerServiceApiClient _apiClient;

    public HostCruder(CrawlerServiceApiClient apiClient) : base("Host", "Hosts")
    {
        _apiClient = apiClient;
        FieldEditors.Add(new TextFieldEditor(nameof(HostDto.HostName)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return _apiClient.GetHostsList().GetAwaiter().GetResult().Match(
            hosts => hosts.ToDictionary(k => k.HostName, ItemData (v) => v),
            errors =>
            {
                Error.PrintErrorsOnConsole(errors);
                return new Dictionary<string, ItemData>();
            });
    }

    public override bool ContainsRecordWithKey(string recordKey)
    {
        return GetCrudersDictionary().ContainsKey(recordKey);
    }

    public override async ValueTask UpdateRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not HostDto newHost)
        {
            return;
        }

        OneOf<HostDto?, Error[]> hostResult = await _apiClient.GetHostByName(recordKey, cancellationToken);
        if (hostResult.IsT1)
        {
            Error.PrintErrorsOnConsole(hostResult.AsT1);
            return;
        }

        HostDto? host = hostResult.AsT0;
        if (host is null)
        {
            StShared.WriteErrorLine($"host {recordKey} not found", true);
            return;
        }

        host.HostName = newHost.HostName;

        Option<Error[]> updateResult = await _apiClient.UpdateHost(host, cancellationToken);
        if (updateResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])updateResult);
        }
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not HostDto newHost)
        {
            return;
        }

        OneOf<HostDto, Error[]> createResult = await _apiClient.CreateHost(newHost, cancellationToken);
        if (createResult.IsT1)
        {
            Error.PrintErrorsOnConsole(createResult.AsT1);
        }
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        Option<Error[]> deleteResult = await _apiClient.DeleteHost(recordKey, cancellationToken);
        if (deleteResult.IsSome)
        {
            Error.PrintErrorsOnConsole((Error[])deleteResult);
        }
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new HostDto { HostName = string.Empty };
    }

    public override bool CheckValidation(ItemData item)
    {
        if (item is not HostDto newHost)
        {
            StShared.WriteErrorLine("Invalid Host Data", true);
            return false;
        }

        var re = new Regex(@"[-a-zA-Z0-9]{1,256}\.([-a-zA-Z0-9]{1,256}\.)*[a-zA-Z0-9()]{1,6}");
        Match m = re.Match(newHost.HostName);
        if (m is { Success: true, Groups.Count: 2 } && m.Groups[0].Value == newHost.HostName)
        {
            return true;
        }

        StShared.WriteErrorLine($"Invalid Host Name {newHost.HostName}.", true);
        return false;
    }
}
