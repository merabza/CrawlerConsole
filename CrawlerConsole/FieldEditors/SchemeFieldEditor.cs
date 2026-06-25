using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCliTools.CliParameters.FieldEditors;
using CrawlerConsole.Cruders;
using CrawlerServiceShared.Contracts;

namespace CrawlerConsole.FieldEditors;

public sealed class SchemeFieldEditor : FieldEditor<string>
{
    private readonly CrawlerServiceApiClient _apiClient;

    // ReSharper disable once ConvertToPrimaryConstructor
    public SchemeFieldEditor(string propertyName, CrawlerServiceApiClient apiClient) : base(propertyName)
    {
        _apiClient = apiClient;
    }

    public override async ValueTask UpdateField(string? recordKey, object recordForUpdate,
        CancellationToken cancellationToken = default)
    {
        var schemeCruder = new SchemeCruder(_apiClient);
        List<string> keys = schemeCruder.GetKeys();
        string? def = keys.Count > 1 ? null : schemeCruder.GetKeys().SingleOrDefault();
        SetValue(recordForUpdate,
            await schemeCruder.GetNameWithPossibleNewName(FieldName, GetValue(recordForUpdate, def), null, true,
                cancellationToken));
    }
}
