using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetVersionInformationAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetVersionInformationAsyncExample).Name);

            try
            {
                VersionInformation versionInformation = await api.GetVersionInformationAsync();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetVersionInformationAsyncExample).Name);
        }
    }
}