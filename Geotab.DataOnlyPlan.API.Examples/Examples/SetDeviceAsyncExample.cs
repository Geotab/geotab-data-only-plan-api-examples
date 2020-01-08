using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class SetDeviceAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api, string deviceId)
        {
            ConsoleUtility.LogExampleStarted(typeof(SetDeviceAsyncExample).Name);

            try
            {
                // Set parameter values to apply when adding device.
                string id = deviceId;
                string name = "Vehicle 1 Upd";
                bool enableDeviceBeeping = false;
                bool enableDriverIdentificationReminder = false;
                int driverIdentificationReminderImmobilizeSeconds = 21;
                bool enableBeepOnEngineRpm = false;
                int engineRpmBeepValue = 3001;
                bool enableBeepOnIdle = false;
                int idleMinutesBeepValue = 5;
                bool enableBeepOnSpeeding = false;
                int speedingStartBeepingSpeed = 111;
                int speedingStopBeepingSpeed = 101;
                bool enableBeepBrieflyWhenApprocahingWarningSpeed = false;
                bool enableBeepOnDangerousDriving = false;
                int accelerationWarningThreshold = 24;
                int brakingWarningThreshold = -36;
                int corneringWarningThreshold = 28;
                bool enableBeepWhenSeatbeltNotUsed = false;
                int seatbeltNotUsedWarningSpeed = 12;
                bool enableBeepWhenPassengerSeatbeltNotUsed = false;
                bool beepWhenReversing = false;

                ConsoleUtility.LogInfoStart($"Updating device '{id}' in database '{api.Credentials.Database}'...");

                List<Device> deviceCache = await ExampleUtility.GetAllDevicesAsync(api);
                Device deviceToSet = deviceCache.Where(targetDevice => targetDevice.Id.ToString() == deviceId).First();
                await api.SetDeviceAsync(deviceToSet, name, enableDeviceBeeping, enableDriverIdentificationReminder, driverIdentificationReminderImmobilizeSeconds, enableBeepOnEngineRpm, engineRpmBeepValue, enableBeepOnIdle, idleMinutesBeepValue, enableBeepOnSpeeding, speedingStartBeepingSpeed, speedingStopBeepingSpeed, enableBeepBrieflyWhenApprocahingWarningSpeed, enableBeepOnDangerousDriving, accelerationWarningThreshold, brakingWarningThreshold, corneringWarningThreshold, enableBeepWhenSeatbeltNotUsed, seatbeltNotUsedWarningSpeed, enableBeepWhenPassengerSeatbeltNotUsed, beepWhenReversing);

                ConsoleUtility.LogComplete();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(SetDeviceAsyncExample).Name);
        }
    }
}