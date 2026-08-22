using System.Collections.Generic;
using System.Linq;
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
            schemes => schemes.ToDictionary(k => k.SchName, ItemData (v) => v), errors =>
            {
                ErrorOmd.PrintErrorsOnConsole(errors);
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

        OneOf<SchemeDto?, ErrorOmd[]> schemeResult = await _apiClient.GetSchemeByName(recordKey, cancellationToken);
        if (schemeResult.IsT1)
        {
            ErrorOmd.PrintErrorsOnConsole(schemeResult.AsT1);
            return;
        }

        SchemeDto? scheme = schemeResult.AsT0;
        if (scheme is null)
        {
            StShared.WriteErrorLine($"scheme {recordKey} not found", true);
            return;
        }

        scheme.SchName = newScheme.SchName;

        Option<ErrorOmd[]> updateResult = await _apiClient.UpdateScheme(scheme, cancellationToken);
        if (updateResult.IsSome)
        {
            ErrorOmd.PrintErrorsOnConsole((ErrorOmd[])updateResult);
        }
    }

    protected override async ValueTask AddRecordWithKey(string recordKey, ItemData newRecord,
        CancellationToken cancellationToken = default)
    {
        if (newRecord is not SchemeDto newScheme)
        {
            return;
        }

        OneOf<SchemeDto, ErrorOmd[]> createResult = await _apiClient.CreateScheme(newScheme, cancellationToken);
        if (createResult.IsT1)
        {
            ErrorOmd.PrintErrorsOnConsole(createResult.AsT1);
        }
    }

    protected override async ValueTask RemoveRecordWithKey(string recordKey,
        CancellationToken cancellationToken = default)
    {
        Option<ErrorOmd[]> deleteResult = await _apiClient.DeleteScheme(recordKey, cancellationToken);
        if (deleteResult.IsSome)
        {
            ErrorOmd.PrintErrorsOnConsole((ErrorOmd[])deleteResult);
        }
    }

    protected override ItemData CreateNewItem(string? recordKey, ItemData? defaultItemData)
    {
        return new SchemeDto { SchName = recordKey ?? string.Empty };
    }
}
