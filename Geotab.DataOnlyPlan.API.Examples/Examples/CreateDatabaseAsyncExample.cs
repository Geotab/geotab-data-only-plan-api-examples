using System;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class CreateDatabaseAsyncExample
    {
        /// <summary>
        /// IMPORTANT: This example should be used sparingly - only when a new database is actually required!
        /// </summary>
        /// <param name="api"></param>
        /// <returns></returns>
        public static async Task<string> Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(CreateDatabaseAsyncExample).Name);

            string createDatabaseResult = "";

            try
            {
                // Confirm user wishes to proceed with database creation.
                ConsoleUtility.LogWarning("By proceeding, you will be requesting the creation of a new MyGeotab database.");
                string input = ConsoleUtility.GetUserInput("'y' to confirm you wish to create a new database, or 'n' to cancel.");
                if (input != "y")
                {
                    ConsoleUtility.LogInfo("Cancelled CreateDatabaseAsync example.");
                    return createDatabaseResult;
                }

                // Re-authenticate against "my.geotab.com".
                ConsoleUtility.LogInfoStart("Reauthenticating against my.geotab.com...");
                await api.AuthenticateAsync("my.geotab.com", "", api.Credentials.UserName, api.Credentials.Password);
                ConsoleUtility.LogComplete();

                string database = "";

                // Generate a database name and ensure that it is not already used.
                bool databaseExists = true;
                while (databaseExists == true)
                {
                    database = Guid.NewGuid().ToString().Replace("-", "");
                    databaseExists = await api.DatabaseExistsAsync(database);
                }

                ConsoleUtility.LogInfoStartMultiPart($"Creating database named '{database}'.", $"THIS MAY TAKE SEVERAL MINUTES...", Common.ConsoleColorForWarnings);

                // Set parameter values for CreateDatabaseAsync call.
                string username = api.Credentials.UserName;
                string password = api.Credentials.Password;
                string companyName = "Customer XYZ Ltd.";
                string firstName = "John";
                string lastName = "Smith";
                string phoneNumber = "+1 (555) 123-4567";
                string resellerName = "Reseller 123 Inc.";
                int fleetSize = 1;
                bool signUpForNews = false;
                string timeZoneId = "America/Toronto";
                string comments = "some comments";

                // Create database.
                createDatabaseResult = await api.CreateDatabaseAsync(database, username, password, companyName, firstName, lastName, phoneNumber, resellerName, fleetSize, signUpForNews, timeZoneId, comments);
                ConsoleUtility.LogComplete();

                // Get the server and database information for the new database.
                string[] serverAndDatabase = (createDatabaseResult).Split('/');
                string server = serverAndDatabase.First();
                string createdDatabase = serverAndDatabase.Last();
                ConsoleUtility.LogInfo($"Created database '{createdDatabase}' on server '{server}'.");

                // Authenticate against the new database so that additional api calls (the 'Add', 'Set' and 'Remove' ones in particular) can be executed.
                ConsoleUtility.LogInfoStart($"Authenticating against '{createdDatabase}' database...");
                await api.AuthenticateAsync(server, createdDatabase, api.Credentials.UserName, api.Credentials.Password);
                ConsoleUtility.LogComplete();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(CreateDatabaseAsyncExample).Name);
            return createDatabaseResult;
        }
    }
}