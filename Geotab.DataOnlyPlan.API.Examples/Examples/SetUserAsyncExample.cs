using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class SetUserAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api, string userId)
        {
            ConsoleUtility.LogExampleStarted(typeof(SetUserAsyncExample).Name);

            try
            {
                // Update a driver with keys to a NON-driver.
                // Set parameter values to apply when adding device.
                string id = userId;
                DateTime activeTo = new(2037, 1, 31);
                string comment = "Driver with keys updated to NON-driver";
                string designation = "Driver 2 Upd";
                string employeeNo = "Employee 2 Upd";
                string firstName = "John Upd";
                bool isDriver = false;
                string lastName = "Smith2 Upd";
                string name = "jsmith2Upd";
                string password2 = "Password1!Upd";

                ConsoleUtility.LogInfoStart($"Updating user '{id}' in database '{api.Credentials.Database}'...");

                await api.SetUserAsync(id, activeTo, comment, designation, employeeNo, firstName, isDriver, lastName, name, password2);

                ConsoleUtility.LogComplete();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(SetUserAsyncExample).Name);
        }
    }
}