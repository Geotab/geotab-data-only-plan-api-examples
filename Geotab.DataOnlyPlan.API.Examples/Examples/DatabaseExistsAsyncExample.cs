using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class DatabaseExistsAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(DatabaseExistsAsyncExample).Name);

            try
            {
                string databaseName = "SomeDatabaseName";
                bool databaseExists = await api.DatabaseExistsAsync(databaseName);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(DatabaseExistsAsyncExample).Name);
        }
    }
}