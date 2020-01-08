using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetTimeZonesAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetTimeZonesAsyncExample).Name);

            try
            {
                IList<Geotab.Checkmate.ObjectModel.TimeZoneInfo> timeZones = await api.GetTimeZonesAsync();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetTimeZonesAsyncExample).Name);
        }
    }
}