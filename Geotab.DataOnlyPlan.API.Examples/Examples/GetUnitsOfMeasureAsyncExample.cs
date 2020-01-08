using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetUnitsOfMeasureAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetUnitsOfMeasureAsyncExample).Name);

            try
            {
                IList<UnitOfMeasure> unitsOfMeasure = await api.GetUnitsOfMeasureAsync();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetUnitsOfMeasureAsyncExample).Name);
        }
    }
}