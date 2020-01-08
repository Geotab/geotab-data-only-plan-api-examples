using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetUnitOfMeasureAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetUnitOfMeasureAsyncExample).Name);

            try
            {
                string unitOfMeasureId = KnownId.UnitOfMeasureKilometersPerHourId.ToString();
                UnitOfMeasure unitOfMeasure = await api.GetUnitOfMeasureAsync(unitOfMeasureId);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetUnitOfMeasureAsyncExample).Name);
        }
    }
}