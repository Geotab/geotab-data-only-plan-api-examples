using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetControllersAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetControllersAsyncExample).Name);

            IList<Controller> controllers;
            Controller controller;

            try
            {
                // Example: Get all controllers:
                controllers = await api.GetControllersAsync();

                // Example: Search for a controller based on id: 
                string controllerId = KnownId.ControllerObdPowertrainId.ToString();
                controllers = await api.GetControllersAsync(controllerId);
                controller = controllers.FirstOrDefault();

                // Example: Search for controllers based on name: 
                string controllerName = "Body (B)";
                controllers = await api.GetControllersAsync("", controllerName);

                // Example: Search for controllers based on sourceId:
                string controllerSourceId = KnownId.SourceObdId.ToString();
                controllers = await api.GetControllersAsync("", "", controllerSourceId);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetControllersAsyncExample).Name);
        }
    }
}