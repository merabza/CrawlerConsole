using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.Cruders;
using AppCliTools.CliParameters.FieldEditors;
using CrawlerServiceShared.Contracts;
using SystemTools.SharedKernel;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole.Cruders;

public sealed class SchemeCruder : Cruder
{
    private readonly CrawlerServiceApiClient _apiClient;

    public SchemeCruder(CrawlerServiceApiClient apiClient) : base("Scheme", "Schemes")
    {
        _apiClient = apiClient;
        FieldEditors.Add(new BoolFieldEditor(nameof(SchemeDto.SchProhibited)));
    }

    protected override Dictionary<string, ItemData> GetCrudersDictionary()
    {
        return _apiClient.GetSchemesList().GetAwaiter().GetResult().Match(
            schemes => schemes.ToDictionary(k => k.SchName, ItemData (v) => v), failure =>
            {
                failure.Error.PrintErrorsOnConsole();
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
        if (newRecord is not SchemeDto newScheme)
        {
            return;
        }

        Result<SchemeDto?> schemeResult = await _apiClient.GetSchemeByName(recordKey, cancellationToken);
        if (schemeResult.IsFailure)
        {
            schemeResult.Error.PrintErrorsOnConsole();
            return;
        }

        SchemeDto? scheme = schemeResult.Value;
        if (scheme is null)
        {
            StShared.WriteErrorLine($"scheme {recordKey} not found", true);
            return;
        }

        scheme.SchName = newScheme.SchName;

        Result updateResult = await _apiClient.UpdateScheme(scheme, cancellationToken);
        if (updateResult.IsFailure)
        {
            updateResult.Error.PrintErrorsOnConsole();
        }
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not SchemeDto newScheme)
        {
            return;
        }

        Result<SchemeDto> createResult = await _apiClient.CreateScheme(newScheme, cancellationToken);
        if (createResult.IsFailure)
        {
            createResult.Error.PrintErrorsOnConsole();
        }
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        Result deleteResult = await _apiClient.DeleteScheme(recordKey, cancellationToken);
        if (deleteResult.IsFailure)
        {
            deleteResult.Error.PrintErrorsOnConsole();
        }
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new SchemeDto { SchName = recordKey ?? string.Empty };
    }
}
