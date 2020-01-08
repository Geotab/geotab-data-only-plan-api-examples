using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetCountOfDeviceAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetCountOfDeviceAsyncExample).Name);

            try
            {
                int deviceCount = await api.GetCountOfDeviceAsync();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetCountOfDeviceAsyncExample).Name);
        }
    }
}