using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class RemoveUserAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api, string userId)
        {
            ConsoleUtility.LogExampleStarted(typeof(RemoveUserAsyncExample).Name);

            try
            {
                ConsoleUtility.LogInfoStart($"Removing user '{userId}' from database '{api.Credentials.Database}'...");

                List<User> userCache = await ExampleUtility.GetAllUsersAsync(api);
                User userToRemove = userCache.Where(targetUser => targetUser.Id.ToString() == userId).First();
                await api.RemoveUserAsync(userToRemove);

                ConsoleUtility.LogComplete();
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(RemoveUserAsyncExample).Name);
        }
    }
}