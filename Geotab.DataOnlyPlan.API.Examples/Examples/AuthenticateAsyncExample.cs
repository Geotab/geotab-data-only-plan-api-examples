using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class AuthenticateAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(AuthenticateAsyncExample).Name);

            try
            {
                string server = ConsoleUtility.GetUserInput("server").ToLower();
                string database = ConsoleUtility.GetUserInput("database").ToLower();
                string username = ConsoleUtility.GetUserInput("username");
                string password = ConsoleUtility.GetUserInputMasked("password");
                await api.AuthenticateAsync(server, database, username, password);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(AuthenticateAsyncExample).Name);
        }
    }
}