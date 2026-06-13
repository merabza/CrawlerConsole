using System.Net.Http;
using AppCliTools.CliParameters;
using AppCliTools.CliParameters.FieldEditors;
using AppCliTools.CliParametersDataEdit.Cruders;
using AppCliTools.CliParametersDataEdit.FieldEditors;
using AppCliTools.CliParametersEdit.Cruders;
using CrawlerConsole.Cruders;
using CrawlerConsoleData.Models;
using Microsoft.Extensions.Logging;
using ParametersManagement.LibDatabaseParameters;
using ParametersManagement.LibFileParameters.Models;
using ParametersManagement.LibParameters;
using SystemTools.SystemToolsShared;

namespace CrawlerConsole;

public sealed class CrawlerParametersEditor : ParametersEditor
{
    public CrawlerParametersEditor(IApplication application, IParameters parameters,
        IParametersManager parametersManager, ILogger logger, IHttpClientFactory httpClientFactory) : base(
        "Crawler Parameters Editor", parameters, parametersManager)
    {
        FieldEditors.Add(new FolderPathFieldEditor(nameof(CrawlerConsoleParameters.LogFolder)));

        //FieldEditors.Add(new DatabaseServerConnectionNameFieldEditor(logger, httpClientFactory,
        //    nameof(CrawlerConsoleParameters.DatabaseConnectionName), parametersManager, true));

        //FieldEditors.Add(new IntFieldEditor(nameof(CrawlerConsoleParameters.CommandTimeOut), 10000));
        FieldEditors.Add(new IntFieldEditor(nameof(CrawlerConsoleParameters.LoadPagesMaxCount), 10000));
        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerConsoleParameters.Alphabet),
            "აბგდევზთიკლმნოპჟრსტუფქღყშჩცძწჭხჯჰ"));
        FieldEditors.Add(new TextFieldEditor(nameof(CrawlerConsoleParameters.ExtraSymbols), "-–"));

        //FieldEditors.Add(new PunctuationsFieldEditor(nameof(CrawlerConsoleParameters.Punctuations), parametersManager,
        //    logger));

        FieldEditors.Add(new DictionaryFieldEditor<PunctuationCruder, PunctuationModel>(
            nameof(CrawlerConsoleParameters.Punctuations), logger, parametersManager));

        //FieldEditors.Add(new DatabaseServerConnectionsFieldEditor(logger, httpClientFactory, parametersManager,
        //    nameof(CrawlerConsoleParameters.DatabaseServerConnections)));

        FieldEditors.Add(new DictionaryFieldEditor<DatabaseServerConnectionCruder, DatabaseServerConnectionData>(
            nameof(CrawlerConsoleParameters.DatabaseServerConnections), logger, httpClientFactory, parametersManager));

        FieldEditors.Add(new DatabaseParametersFieldEditor(application, logger, httpClientFactory,
            nameof(CrawlerConsoleParameters.DatabaseParameters), parametersManager));

        FieldEditors.Add(
            new DictionaryFieldEditor<SmartSchemaCruder, SmartSchema>(nameof(CrawlerConsoleParameters.SmartSchemas),
                parametersManager));

        FieldEditors.Add(
            new DictionaryFieldEditor<FileStorageCruder, FileStorageData>(nameof(CrawlerConsoleParameters.FileStorages),
                logger, parametersManager));
    }
}
