using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class AddUserAsyncExample
    {
        public static async Task<string> Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(AddUserAsyncExample).Name);

            string addedUserId = "";

            try
            {
                // Add a user that is a driver with a key.
                // Set parameter values to apply when adding user.
                List<Key> keys = new();
                Key key = new(DriverKeyType.CustomNfc, null, "1234567890");
                keys.Add(key);

                string comment = "User added as driver with key.";
                string designation = "Driver 2";
                string employeeNo = "Employee 2";
                string firstName = "John";
                bool isDriver = true;
                string lastName = "Smith2";
                string name = "jsmith2";
                string password2 = "Password1!";
                string licenseNumber = "ABC123";
                string licenseProvinceOrState = "ON";

                ConsoleUtility.LogInfoStart($"Adding user with username '{name}' to database '{api.Credentials.Database}'...");

                addedUserId = await api.AddUserAsync(comment, designation, employeeNo, firstName, isDriver, lastName, name, password2, keys, licenseNumber, licenseProvinceOrState);

                ConsoleUtility.LogComplete();
                ConsoleUtility.LogInfo($"Added user Id: {addedUserId}");
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(AddUserAsyncExample).Name);
            return addedUserId;
        }
    }
}