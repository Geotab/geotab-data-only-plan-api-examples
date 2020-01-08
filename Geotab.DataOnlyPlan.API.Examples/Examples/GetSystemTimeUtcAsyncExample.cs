using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetSystemTimeUtcAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetSystemTimeUtcAsyncExample).Name);

            try
            {
                DateTime systemTimeUtc = await api.GetSystemTimeUtcAsync();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetSystemTimeUtcAsyncExample).Name);
        }
    }
}