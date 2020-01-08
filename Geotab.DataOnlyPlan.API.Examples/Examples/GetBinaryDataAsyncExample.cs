using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetBinaryDataAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetBinaryDataAsyncExample).Name);

            IList<BinaryData> binaryData;

            try
            {
                // Get a random device Id for use in the following examples.
                List<Device> deviceCache = await ExampleUtility.GetAllDevicesAsync(api);
                string deviceId = ExampleUtility.GetRandomDeviceId(deviceCache);
                Device deviceForTextMessages = deviceCache.Where(targetDevice => targetDevice.Id.ToString() == deviceId).First();

                // Example: Get all binary data:
                binaryData = await api.GetBinaryDataAsync();

                // Example: Get all CalibrationId binary data for a single device:
                binaryData = await api.GetBinaryDataAsync("CalibrationId", null, null, "", deviceId);

                // Example: Get all CalibrationId binary data for a single device for the past week:
                DateTime fromDate = DateTime.Now - TimeSpan.FromDays(7);
                binaryData = await api.GetBinaryDataAsync("CalibrationId", fromDate, null, "", deviceId);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetBinaryDataAsyncExample).Name);
        }
    }
}