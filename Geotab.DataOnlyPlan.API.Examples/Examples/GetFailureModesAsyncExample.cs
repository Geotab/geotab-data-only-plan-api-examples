using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetFailureModesAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetFailureModesAsyncExample).Name);

            IList<FailureMode> failureModes;
            FailureMode failureMode;

            try
            {
                // Example: Get all failureModes:
                failureModes = await api.GetFailureModesAsync();

                // Example: Search for a failureMode based on id: 
                if (failureModes.Any())
                {
                    string failureModeId = failureModes.FirstOrDefault().Id.ToString();
                    failureModes = await api.GetFailureModesAsync(failureModeId);
                    failureMode = failureModes.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetFailureModesAsyncExample).Name);
        }
    }
}