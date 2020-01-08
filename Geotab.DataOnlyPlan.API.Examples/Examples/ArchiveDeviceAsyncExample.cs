using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class ArchiveDeviceAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api, string deviceId)
        {
            ConsoleUtility.LogExampleStarted(typeof(ArchiveDeviceAsyncExample).Name);

            try
            {
                ConsoleUtility.LogInfoStart($"Archiving device '{deviceId}' in database '{api.Credentials.Database}'...");

                List<Device> deviceCache = await ExampleUtility.GetAllDevicesAsync(api);
                Device deviceToArchive = deviceCache.Where(targetDevice => targetDevice.Id.ToString() == deviceId).First();
                await api.ArchiveDeviceAsync(deviceToArchive);

                ConsoleUtility.LogComplete();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(ArchiveDeviceAsyncExample).Name);
        }
    }
}