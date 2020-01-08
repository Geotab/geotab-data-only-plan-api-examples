using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class RemoveDriverChangeAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api, string driverChangeId)
        {
            ConsoleUtility.LogExampleStarted(typeof(RemoveDriverChangeAsyncExample).Name);

            try
            {
                ConsoleUtility.LogInfoStart($"Removing driverChange '{driverChangeId}' from database '{api.Credentials.Database}'...");

                IList<DriverChange> driverChanges = await ExampleUtility.GetAllDriverChangesAsync(api);
                DriverChange driverChangeToRemove = driverChanges.Where(targetDriverChange => targetDriverChange.Id.ToString() == driverChangeId).First();
                await api.RemoveDriverChangeAsync(driverChangeToRemove);

                ConsoleUtility.LogComplete();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(RemoveDriverChangeAsyncExample).Name);
        }
    }
}