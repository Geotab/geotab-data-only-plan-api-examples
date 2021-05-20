using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetFeedDeviceAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetFeedDeviceAsyncExample).Name);

            try
            {
                // Feed parameters.
                // See MyGeotab SDK <a href="https://geotab.github.io/sdk/software/guides/concepts/#result-limits">Result Limits</a> and <a href="https://geotab.github.io/sdk/software/api/reference/#M:Geotab.Checkmate.Database.DataStore.GetFeed1">GetFeed()</a> documentation for information about the feed result limit defined below.
                const int DefaultFeedResultsLimitDevice = 5000;
                int getFeedNumberOfCallsToMake = 5;
                int getFeedSecondsToWaitBetweenCalls = 5;
                long? feedVersion = 0;

                List<Device> deviceCache = new();
                FeedResult<Device> feedResult;

                // Start by populating the deviceCache with a list of all devices.
                ConsoleUtility.LogListItem($"Population of deviceCache started.");
                bool isFirstCall = true;
                bool keepGoing = true;
                while (keepGoing == true)
                {
                    feedResult = await api.GetFeedDeviceAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedDeviceAsync executed:");
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    if (isFirstCall == true)
                    {
                        deviceCache.AddRange(feedResult.Data);
                        isFirstCall = false;
                    }
                    else
                    {
                        // Add new items to the cache, or update existing items with their changed counterparts.
                        foreach (Device feedResultDevice in feedResult.Data)
                        {
                            Device cachedDeviceToUpdate = deviceCache.Where(device => device.Id == feedResultDevice.Id).FirstOrDefault();
                            if (cachedDeviceToUpdate == null)
                            {
                                deviceCache.Add(feedResultDevice);
                            }
                            else
                            {
                                var index = deviceCache.IndexOf(cachedDeviceToUpdate);
                                deviceCache[index] = feedResultDevice;
                            }
                        }
                    }
                    if (feedResult.Data.Count < DefaultFeedResultsLimitDevice)
                    {
                        keepGoing = false;
                    }
                }
                ConsoleUtility.LogListItem($"Population of deviceCache completed.");

                // Execute a GetFeed loop for the prescribed number of iterations, adding new items to the cache, or updating existing items with their changed counterparts.
                for (int getFeedCallNumber = 1; getFeedCallNumber < getFeedNumberOfCallsToMake + 1; getFeedCallNumber++)
                {
                    feedResult = await api.GetFeedDeviceAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedDeviceAsync executed.  Iteration:", getFeedCallNumber.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    // Add new items to the cache, or update existing items with their changed counterparts.
                    foreach (Device feedResultDevice in feedResult.Data)
                    {
                        Device cachedDeviceToUpdate = deviceCache.Where(device => device.Id == feedResultDevice.Id).FirstOrDefault();
                        if (cachedDeviceToUpdate == null)
                        {
                            deviceCache.Add(feedResultDevice);
                        }
                        else
                        {
                            var index = deviceCache.IndexOf(cachedDeviceToUpdate);
                            deviceCache[index] = feedResultDevice;
                        }
                    }
                    // Wait for the prescribed amount of time before making the next GetFeed call.
                    Thread.Sleep(getFeedSecondsToWaitBetweenCalls * 1000);
                }
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetFeedDeviceAsyncExample).Name);
        }
    }
}