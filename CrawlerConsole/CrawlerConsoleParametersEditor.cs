using System.Net.Http;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersApiClientsEdit;
using AppCliTools.CliParametersApiClientsEdit.FieldEditors;
using AppCliTools.CliParametersDataEdit.Cruders;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using CrawlerConsole.Cruders;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibApiClientParameters;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole;

public sealed class CrawlerConsoleParametersEditor : ParametersEditor
{
    public CrawlerConsoleParametersEditor(IParameters parameters,
        IParametersManager parametersManager, ILogger logger, IHttpClientFactory httpClientFactory) : base(
        "CrawlerConsole Parameters Editor", parameters, parametersManager)
    {
        FieldEditors.Add(new FolderPathFieldEditor(nameof(CrawlerConsoleParameters.LogFolder)));
        FieldEditors.Add(new IntFieldEditor(nameof(CrawlerConsoleParameters.LoadPagesMaxCount), 10000));

        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerConsoleParameters.Alphabet),
            "აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ"));

        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerConsoleParameters.ExtraSymbols), "-–"));

        FieldEditors.Add(new DictionaryFieldEditor<PunctuationCruder, PunctuationModel>(
            nameof(CrawlerConsoleParameters.Punctuations), x => new PunctuationCruder(logger, parametersManager, x)));

        FieldEditors.Add(new DictionaryFieldEditor<SmartSchemaCruder, SmartSchema>(
            nameof(CrawlerConsoleParameters.SmartSchemas), x => new SmartSchemaCruder(parametersManager, x)));

        FieldEditors.Add(new DictionaryFieldEditor<FileStorageCruder, FileStorageData>(
            nameof(CrawlerConsoleParameters.FileStorages), x => new FileStorageCruder(logger, parametersManager, x)));

        FieldEditors.Add(new DictionaryFieldEditor<ApiClientCruder, ApiClientSettings>(
            nameof(CrawlerConsoleParameters.ApiClients),
            x => new ApiClientCruder(logger, httpClientFactory, parametersManager, x)));

        //მენეჯმენტის (CRUD) ოპერაციებისთვის გამოსაყენებელი CrawlerService-ის ApiClient-ის არჩევა
        FieldEditors.Add(new ApiClientNameFieldEditor(nameof(CrawlerConsoleParameters.ManagementApiClientName), logger,
            httpClientFactory, parametersManager, true));
    }
}
