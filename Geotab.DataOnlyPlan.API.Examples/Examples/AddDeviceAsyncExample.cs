using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class AddDeviceAsyncExample
    {
        public static async Task<string> Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(AddDeviceAsyncExample).Name);

            string addedDeviceId = "";

            try
            {
                // Set parameter values to apply when adding device.
                string serialNumber = ConsoleUtility.GetUserInput("serial number of device to be added");
                string name = "Vehicle 1";
                bool enableDeviceBeeping = true;
                bool enableDriverIdentificationReminder = true;
                int driverIdentificationReminderImmobilizeSeconds = 20;
                bool enableBeepOnEngineRpm = true;
                int engineRpmBeepValue = 3000;
                bool enableBeepOnIdle = true;
                int idleMinutesBeepValue = 4;
                bool enableBeepOnSpeeding = true;
                int speedingStartBeepingSpeed = 110;
                int speedingStopBeepingSpeed = 100;
                bool enableBeepBrieflyWhenApprocahingWarningSpeed = true;
                bool enableBeepOnDangerousDriving = true;
                int accelerationWarningThreshold = 23;
                int brakingWarningThreshold = -35;
                int corneringWarningThreshold = 27;
                bool enableBeepWhenSeatbeltNotUsed = true;
                int seatbeltNotUsedWarningSpeed = 11;
                bool enableBeepWhenPassengerSeatbeltNotUsed = true;
                bool beepWhenReversing = true;

                ConsoleUtility.LogInfoStart($"Adding device with serial number '{serialNumber}' to database '{api.Credentials.Database}'...");

                addedDeviceId = await api.AddDeviceAsync(serialNumber, name, enableDeviceBeeping, enableDriverIdentificationReminder, driverIdentificationReminderImmobilizeSeconds, enableBeepOnEngineRpm, engineRpmBeepValue, enableBeepOnIdle, idleMinutesBeepValue, enableBeepOnSpeeding, speedingStartBeepingSpeed, speedingStopBeepingSpeed, enableBeepBrieflyWhenApprocahingWarningSpeed, enableBeepOnDangerousDriving, accelerationWarningThreshold, brakingWarningThreshold, corneringWarningThreshold, enableBeepWhenSeatbeltNotUsed, seatbeltNotUsedWarningSpeed, enableBeepWhenPassengerSeatbeltNotUsed, beepWhenReversing);

                ConsoleUtility.LogComplete();
                ConsoleUtility.LogInfo($"Added device Id: {addedDeviceId}");
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(AddDeviceAsyncExample).Name);
            return addedDeviceId;
        }
    }
}