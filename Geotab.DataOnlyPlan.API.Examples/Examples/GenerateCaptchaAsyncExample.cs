using System;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GenerateCaptchaAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GenerateCaptchaAsyncExample).Name);

            try
            {
                string id = Guid.NewGuid().ToString();
                string captchaImage = await api.GenerateCaptchaAsync(id);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GenerateCaptchaAsyncExample).Name);
        }
    }
}