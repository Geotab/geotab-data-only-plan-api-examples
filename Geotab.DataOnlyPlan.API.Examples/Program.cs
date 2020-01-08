using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    class Program
    {
        enum GeotabDataOnlyPlanAPIExample
        {
            AddDeviceAsync, AddDriverChangeAsync, AddTextMessageAsync, AddUserAsync, ArchiveDeviceAsync, AuthenticateAsync, CreateDatabaseAsync, DatabaseExistsAsync, GenerateCaptchaAsync, GetBinaryDataAsync, GetControllersAsync, GetCountOfDeviceAsync, GetCountOfUserAsync, GetFailureModesAsync, GetFeedCustomDataAsync, GetFeedDeviceAsyncExample, GetFeedDiagnosticAsyncExample, GetFeedDriverChangeAsyncExample, GetFeedFaultDataAsync, GetFeedLogRecordAsync, GetFeedIoxAddOnAsync, GetFeedStatusDataAsync, GetFeedTripAsync, GetFeedUserAsyncExample, GetFlashCodesAsync, GetSourceAsync, GetSourcesAsync, GetSystemTimeUtcAsync, GetTimeZonesAsync, GetUnitOfMeasureAsync, GetUnitsOfMeasureAsync, GetVersionAsync, GetVersionInformationAsync, RemoveDeviceAsync, RemoveDriverChangeAsync, RemoveUserAsync, SetDeviceAsync, SetUserAsync
        }

        static async Task Main()
        {
            GeotabDataOnlyPlanAPI api;
            string username;
            string password;
            string server = "my.geotab.com";
            string database = "";
            string title = "";

            string lastCreateDatabaseResult;
            string lastAddedDeviceServer = "";
            string lastAddedDeviceDatabase = "";
            string lastAddedDeviceId = "";
            string lastAddedUserServer = "";
            string lastAddedUserDatabase = "";
            string lastAddedUserId = "";
            string lastAddedDriverChangeServer = "";
            string lastAddedDriverChangeDatabase = "";
            string lastAddedDriverChangeId = "";

            try
            {
                // Set title.
                title = AppDomain.CurrentDomain.FriendlyName.Replace(".", " ");
                Console.Title = title;
                ConsoleUtility.LogUtilityStartup(title);

                // Request MyGeotab credentials and database name.
                server = ConsoleUtility.GetUserInput($"MyGeotab server");
                database = ConsoleUtility.GetUserInput($"Database to run examples against.").ToLower();
                username = ConsoleUtility.GetUserInput($"MyGeotab username");
                password = ConsoleUtility.GetUserInputMasked($"MyGeotab password");

                // Create Geotab Data-Only Plan API instance and authenticate.
                api = new GeotabDataOnlyPlanAPI(server, database, username, password);
                ConsoleUtility.LogInfoStart("Authenticating...");
                await api.AuthenticateAsync();
                ConsoleUtility.LogOk();

                bool exit = false;
                while (exit == false)
                {
                    // List the examples that the user may select from.
                    ConsoleUtility.LogExamplesMenuHeader();
                    IList<KeyValuePair<int, string>> listItems = new List<KeyValuePair<int, string>>();
                    string[] GeotabDataOnlyPlanAPIExamples = Enum.GetNames(typeof(GeotabDataOnlyPlanAPIExample));
                    for (int GeotabDataOnlyPlanAPIExampleId = 0; GeotabDataOnlyPlanAPIExampleId < GeotabDataOnlyPlanAPIExamples.Length; GeotabDataOnlyPlanAPIExampleId++)
                    {
                        listItems.Add(new KeyValuePair<int, string>(GeotabDataOnlyPlanAPIExampleId, GeotabDataOnlyPlanAPIExamples[GeotabDataOnlyPlanAPIExampleId]));
                    }
                    ConsoleUtility.LogListItems(listItems, Common.ConsoleColorForListItemIds, Common.ConsoleColorForListItems);

                    // Get user to select which example to run.
                    bool exampleSelected = false;
                    while (!exampleSelected)
                    {
                        exampleSelected = true;
                        string input = ConsoleUtility.GetUserInput("number of the example to run (from the above list), or 'x' to quit.");

                        if (input == "x" || input == "X")
                        {
                            exit = true;
                            break;
                        }

                        if (int.TryParse(input, out int selection))
                        {
                            switch (selection)
                            {
                                case (int)GeotabDataOnlyPlanAPIExample.AddDeviceAsync:
                                    lastAddedDeviceId = await AddDeviceAsyncExample.Run(api);
                                    lastAddedDeviceServer = server;
                                    lastAddedDeviceDatabase = database;
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.AddDriverChangeAsync:
                                    if (lastAddedDeviceDatabase == database && lastAddedDeviceId != "" && lastAddedUserDatabase == database && lastAddedUserId != "")
                                    {
                                        lastAddedDriverChangeId = await AddDriverChangeAsyncExample.Run(api, lastAddedDeviceId, lastAddedUserId);
                                        lastAddedDriverChangeServer = server;
                                        lastAddedDriverChangeDatabase = database;
                                    }
                                    else if (lastAddedDeviceId == "" || lastAddedUserId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' and 'AddUserAsync' examples must be run before the 'AddDriverChangeAsync' example can be run.");
                                    }
                                    else
                                    {
                                        if (lastAddedDeviceDatabase != lastAddedUserDatabase)
                                        {
                                            ConsoleUtility.LogError($"The 'AddDeviceAsync' example was last run against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server and added the device '{lastAddedDeviceId}'.  The 'AddUserAsync' example was last run against the '{lastAddedUserDatabase}' database on the '{lastAddedUserServer}' server and added the user '{lastAddedUserId}'.  The 'AddDeviceAsync' and 'AddUserAsync' examples must be run on the same database before the 'AddDriverChangeAsync' example can be run.");
                                        }
                                    }
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.AddTextMessageAsync:
                                    if (lastAddedDeviceDatabase == database && lastAddedDeviceId != "" && lastAddedUserDatabase == database && lastAddedUserId != "")
                                    {
                                        await AddTextMessageAsyncExample.Run(api, lastAddedDeviceId, lastAddedUserId);
                                    }
                                    else if (lastAddedDeviceId == "" || lastAddedUserId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' and 'AddUserAsync' examples must be run before the 'AddTextMessageAsync' example can be run.");
                                    }
                                    else
                                    {
                                        if (lastAddedDeviceDatabase != lastAddedUserDatabase)
                                        {
                                            ConsoleUtility.LogError($"The 'AddDeviceAsync' example was last run against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server and added the device '{lastAddedDeviceId}'.  The 'AddUserAsync' example was last run against the '{lastAddedUserDatabase}' database on the '{lastAddedUserServer}' server and added the user '{lastAddedUserId}'.  The 'AddDeviceAsync' and 'AddUserAsync' examples must be run on the same database before the 'AddTextMessageAsync' example can be run.");
                                        }
                                    }
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.AddUserAsync:
                                    lastAddedUserId = await AddUserAsyncExample.Run(api);
                                    lastAddedUserServer = server;
                                    lastAddedUserDatabase = database;
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.ArchiveDeviceAsync:
                                    if (lastAddedDeviceDatabase == database && lastAddedDeviceId != "")
                                    {
                                        await ArchiveDeviceAsyncExample.Run(api, lastAddedDeviceId);
                                    }
                                    else if (lastAddedDeviceId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' example must be run before the 'ArchiveDeviceAsync' example can be run.");
                                    }
                                    else if (lastAddedDeviceDatabase != database)
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' example was last run against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server and added the device '{lastAddedDeviceId}'.  Please run the 'AuthenticateAsync' example to authenticate against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server before running the 'ArchiveDeviceAsync' example.");
                                    }
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.AuthenticateAsync:
                                    await AuthenticateAsyncExample.Run(api);
                                    database = api.Credentials.Database;
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.CreateDatabaseAsync:
                                    lastCreateDatabaseResult = await CreateDatabaseAsyncExample.Run(api);

                                    // Get the server and database information for the new database.
                                    string[] serverAndDatabase = (lastCreateDatabaseResult).Split('/');
                                    server = serverAndDatabase.First();
                                    database = serverAndDatabase.Last();
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.DatabaseExistsAsync:
                                    await DatabaseExistsAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GenerateCaptchaAsync:
                                    await GenerateCaptchaAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetBinaryDataAsync:
                                    await GetBinaryDataAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetControllersAsync:
                                    await GetControllersAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetCountOfDeviceAsync:
                                    await GetCountOfDeviceAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetCountOfUserAsync:
                                    await GetCountOfUserAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFailureModesAsync:
                                    await GetFailureModesAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedCustomDataAsync:
                                    await GetFeedCustomDataAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedDeviceAsyncExample:
                                    await GetFeedDeviceAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedDiagnosticAsyncExample:
                                    await GetFeedDiagnosticAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedDriverChangeAsyncExample:
                                    await GetFeedDriverChangeAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedFaultDataAsync:
                                    await GetFeedFaultDataAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedLogRecordAsync:
                                    await GetFeedLogRecordAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedIoxAddOnAsync:
                                    await GetFeedIoxAddOnAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedStatusDataAsync:
                                    await GetFeedStatusDataAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedTripAsync:
                                    await GetFeedTripAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFeedUserAsyncExample:
                                    await GetFeedUserAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetFlashCodesAsync:
                                    await GetFlashCodesAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetSourceAsync:
                                    await GetSourceAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetSourcesAsync:
                                    await GetSourcesAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetSystemTimeUtcAsync:
                                    await GetSystemTimeUtcAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetTimeZonesAsync:
                                    await GetTimeZonesAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetUnitOfMeasureAsync:
                                    await GetUnitOfMeasureAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetUnitsOfMeasureAsync:
                                    await GetUnitsOfMeasureAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetVersionAsync:
                                    await GetVersionAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.GetVersionInformationAsync:
                                    await GetVersionInformationAsyncExample.Run(api);
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.RemoveDriverChangeAsync:
                                    if (lastAddedDriverChangeDatabase == database && lastAddedDriverChangeId != "")
                                    {
                                        await RemoveDriverChangeAsyncExample.Run(api, lastAddedDriverChangeId);
                                        lastAddedDriverChangeDatabase = "";
                                        lastAddedDriverChangeId = "";
                                    }
                                    else if (lastAddedDriverChangeId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddDriverChangeAsync' example must be run before the 'RemoveDriverChangeAsync' example can be run.");
                                    }
                                    else if (lastAddedDriverChangeDatabase != database)
                                    {
                                        ConsoleUtility.LogError($"The 'AddDriverChangeAsync' example was last run against the '{lastAddedDriverChangeDatabase}' database on the '{lastAddedDriverChangeServer}' server and added the DriverChange '{lastAddedDriverChangeId}'.  Please run the 'AuthenticateAsync' example to authenticate against the '{lastAddedDriverChangeDatabase}' database on the '{lastAddedDriverChangeServer}' server before running the 'RemoveDriverChangeAsync' example.");
                                    }
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.RemoveDeviceAsync:
                                    if (lastAddedDeviceDatabase == database && lastAddedDeviceId != "")
                                    {
                                        await RemoveDeviceAsyncExample.Run(api, lastAddedDeviceId);
                                        lastAddedDeviceDatabase = "";
                                        lastAddedDeviceId = "";
                                    }
                                    else if (lastAddedDeviceId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' example must be run before the 'RemoveDeviceAsync' example can be run.");
                                    }
                                    else if (lastAddedDeviceDatabase != database)
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' example was last run against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server and added the device '{lastAddedDeviceId}'.  Please run the 'AuthenticateAsync' example to authenticate against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server before running the 'RemoveDeviceAsync' example.");
                                    }
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.RemoveUserAsync:
                                    if (lastAddedUserDatabase == database && lastAddedUserId != "")
                                    {
                                        await RemoveUserAsyncExample.Run(api, lastAddedUserId);
                                        lastAddedUserDatabase = "";
                                        lastAddedUserId = "";
                                    }
                                    else if (lastAddedUserId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddUserAsync' example must be run before the 'RemoveUserAsync' example can be run.");
                                    }
                                    else if (lastAddedUserDatabase != database)
                                    {
                                        ConsoleUtility.LogError($"The 'AddUserAsync' example was last run against the '{lastAddedUserDatabase}' database on the '{lastAddedUserServer}' server and added the user '{lastAddedUserId}'.  Please run the 'AuthenticateAsync' example to authenticate against the '{lastAddedUserDatabase}' database on the '{lastAddedUserServer}' server before running the 'RemoveUserAsync' example.");
                                    }
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.SetDeviceAsync:
                                    if (lastAddedDeviceDatabase == database && lastAddedDeviceId != "")
                                    {
                                        await SetDeviceAsyncExample.Run(api, lastAddedDeviceId);
                                    }
                                    else if (lastAddedDeviceId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' example must be run before the 'SetDeviceAsync' example can be run.");
                                    }
                                    else if (lastAddedDeviceDatabase != database)
                                    {
                                        ConsoleUtility.LogError($"The 'AddDeviceAsync' example was last run against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server and added the device '{lastAddedDeviceId}'.  Please run the 'AuthenticateAsync' example to authenticate against the '{lastAddedDeviceDatabase}' database on the '{lastAddedDeviceServer}' server before running the 'SetDeviceAsync' example.");
                                    }
                                    break;
                                case (int)GeotabDataOnlyPlanAPIExample.SetUserAsync:
                                    if (lastAddedUserDatabase == database && lastAddedUserId != "")
                                    {
                                        await SetUserAsyncExample.Run(api, lastAddedUserId);
                                    }
                                    else if (lastAddedUserId == "")
                                    {
                                        ConsoleUtility.LogError($"The 'AddUserAsync' example must be run before the 'SetUserAsync' example can be run.");
                                    }
                                    else if (lastAddedUserDatabase != database)
                                    {
                                        ConsoleUtility.LogError($"The 'AddUserAsync' example was last run against the '{lastAddedUserDatabase}' database on the '{lastAddedUserServer}' server and added the user '{lastAddedUserId}'.  Please run the 'AuthenticateAsync' example to authenticate against the '{lastAddedUserDatabase}' database on the '{lastAddedUserServer}' server before running the 'SetUserAsync' example.");
                                    }
                                    break;
                                default:
                                    exampleSelected = false;
                                    ConsoleUtility.LogError($"The value '{input}' is not valid.");
                                    break;
                            }
                        }
                        else
                        {
                            exampleSelected = false;
                            ConsoleUtility.LogError($"The value '{input}' is not valid.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }
            finally
            {
                ConsoleUtility.LogUtilityShutdown(title);
                Console.ReadKey();
            }
        }
    }
}