using System;
using System.IO;
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

                string filePath = "C:\\TEMP";
                if (!Directory.Exists(filePath))
                {
                    filePath = ConsoleUtility.GetUserInputDirectory();
                }
                string outputFilePath = $"{filePath}\\GeotabDataOnlyPlanAPI_CAPTCHA_{id}.jpg";

                var result = await api.GenerateCaptchaAsync(id, outputFilePath);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GenerateCaptchaAsyncExample).Name);
        }
    }
}