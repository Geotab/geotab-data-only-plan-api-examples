using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class AddDriverChangeAsyncExample
    {
        public static async Task<string> Run(GeotabDataOnlyPlanAPI api, string deviceId, string userId)
        {
            ConsoleUtility.LogExampleStarted(typeof(AddDriverChangeAsyncExample).Name);

            string addedDriverChangeId = "";

            try
            {
                // Set parameter values to apply when adding driver change.
                DateTime dateTime = DateTime.Now;
                List<Device> deviceCache = await ExampleUtility.GetAllDevicesAsync(api);
                List<User> userCache = await ExampleUtility.GetAllUsersAsync(api);
                Device device = deviceCache.Where(targetDevice => targetDevice.Id.ToString() == deviceId).First();
                Driver driver = userCache.Where(targetUser => targetUser.Id.ToString() == userId).First() as Driver;

                DriverChangeType driverChangeType = DriverChangeType.Driver;

                ConsoleUtility.LogInfoStart($"Adding driverChange of type '{driverChangeType}' for driver '{driver.Id}' and device '{device.Id}' to database '{api.Credentials.Database}'...");
                               
                addedDriverChangeId = await api.AddDriverChangeAsync(dateTime, device, driver, driverChangeType);

                ConsoleUtility.LogComplete();
                ConsoleUtility.LogInfo($"Added driverChange Id: {addedDriverChangeId}");
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(AddDriverChangeAsyncExample).Name);
            return addedDriverChangeId;
        }
    }
}