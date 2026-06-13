using System;
using System.Runtime.CompilerServices;
using System.Threading;
using AppCliTools.CliParametersDataEdit;
using AppCliTools.CliParametersDataEdit.Models;
using DatabaseTools.DbTools;
using DatabaseTools.DbTools.Models;
using DatabaseTools.DbToolsFactory;
using LanguageExt;
using Microsoft.Extensions.Logging;
using OneOf;
using ParametersManagement.LibDatabaseParameters;
using SystemTools.SystemToolsShared;
using SystemTools.SystemToolsShared.Errors;

namespace CrawlerConsole;

public static class DatabaseConnectionChecker
{
    //მოწმდება, შესაძლებელია თუ არა მონაცემთა ბაზასთან დაკავშირება მითითებული პარამეტრებით.
    //ეს შემოწმება გამოიყენება ორ ადგილას: სერვისების რეგისტრაციისას (CrawlerServices.AddServices)
    //და მთავარი მენიუს აგებისას (CrawlerMenuBuilder).
    public static bool CheckConnection(DatabaseParameters? databaseParameters,
        DatabaseServerConnections databaseServerConnections, string appName, ILogger logger)
    {
        if (databaseParameters is null)
        {
            Console.WriteLine("databaseParameters is null");
            return false;
        }

        (EDatabaseProvider? dataProvider, string? connectionString, _) =
            DbConnectionFactory.GetDataProviderConnectionStringCommandTimeOut(databaseParameters,
                databaseServerConnections);

        if (dataProvider is null || connectionString is null)
        {
            Console.WriteLine("dataProvider is null || connectionString is null");
            return false;
        }

        try
        {
            DbConnectionParameters? dbConnectionParameters =
                DbConnectionFactory.GetDbConnectionParameters(dataProvider.Value, connectionString);
            if (dbConnectionParameters is null)
            {
                Console.WriteLine("dbConnectionParameters is null");
                return false;
            }

            // ReSharper disable once using
            // ReSharper disable once DisposableConstructor
            using var cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;
            token.ThrowIfCancellationRequested();

            switch (dataProvider.Value)
            {
                case EDatabaseProvider.SqlServer:

                    if (dbConnectionParameters is not SqlServerConnectionParameters databaseServerConnectionData)
                    {
                        Console.WriteLine("databaseServerConnectionData is null");
                        return false;
                    }

                    //მოისინჯოს ბაზასთან დაკავშირება.
                    //თუ დაკავშირება ვერ მოხერხდა, გამოვიდეს ამის შესახებ შეტყობინება და შევთავაზოთ მონაცემების შეყვანის გაგრძელება, ან გაჩერება
                    //აქ გამოიყენება ბაზასთან პირდაპირ დაკავშირება ვებაგენტის გარეშე,
                    //რადგან სწორედ ასეთი ტიპის კავშირების რედაქტორია ეს.
                    if (string.IsNullOrWhiteSpace(databaseServerConnectionData.ServerAddress) ||
                        string.IsNullOrWhiteSpace(databaseServerConnectionData.DatabaseName))
                    {
                        Console.WriteLine("databaseServerConnectionData parameters is not valid");
                        return false;
                    }

                    OneOf<DbAuthSettingsBase, Error[]> dbAuthSettingsCreateResult = DbAuthSettingsCreator.Create(
                        databaseServerConnectionData.WindowsNtIntegratedSecurity,
                        databaseServerConnectionData.ServerUser, databaseServerConnectionData.ServerPass, true);

                    if (dbAuthSettingsCreateResult.IsT1)
                    {
                        Error.PrintErrorsOnConsole(dbAuthSettingsCreateResult.AsT1);
                        return false;
                    }

                    DbClient? dc = DbClientFactory.GetDbClient(logger, true, dataProvider.Value,
                        databaseServerConnectionData.ServerAddress, dbAuthSettingsCreateResult.AsT0,
                        databaseServerConnectionData.TrustServerCertificate, appName,
                        databaseServerConnectionData.DatabaseName);

                    if (dc is null)
                    {
                        Console.WriteLine("Database client does not created. dc is null");
                        return false;
                    }

                    Option<Error[]> testConnectionResult = dc.TestConnection(true, token).Result;
                    if (testConnectionResult.IsNone)
                    {
                        return true;
                    }

                    Error.PrintErrorsOnConsole((Error[])testConnectionResult);

                    Console.WriteLine("Database test connection failed");
                    break;

                case EDatabaseProvider.SqLite:
                    return
                        false; //აქ ფაილის შემოწმება არის გასაკეთებელი. ჭეშმარიტი დაბრუნდეს, თუ ფაილი არსებობს და იხსნება
                case EDatabaseProvider.None:
                case EDatabaseProvider.OleDb:
                case EDatabaseProvider.WebAgent:
                    break;
                default:
                    throw new SwitchExpressionException("Unsupported database provider.");
            }

            return false;
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Operation was canceled.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in CheckConnection");
            return false;
        }

        return false;
    }
}
