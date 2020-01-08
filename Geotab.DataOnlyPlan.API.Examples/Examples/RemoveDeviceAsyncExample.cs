using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class RemoveDeviceAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api, string deviceId)
        {
            ConsoleUtility.LogExampleStarted(typeof(RemoveDeviceAsyncExample).Name);

            try
            {
                ConsoleUtility.LogInfoStart($"Removing device '{deviceId}' from database '{api.Credentials.Database}'...");

                List<Device> deviceCache = await ExampleUtility.GetAllDevicesAsync(api);
                Device deviceToRemove = deviceCache.Where(targetDevice => targetDevice.Id.ToString() == deviceId).First();
                await api.RemoveDeviceAsync(deviceToRemove);

                ConsoleUtility.LogComplete();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(RemoveDeviceAsyncExample).Name);
        }
    }
}