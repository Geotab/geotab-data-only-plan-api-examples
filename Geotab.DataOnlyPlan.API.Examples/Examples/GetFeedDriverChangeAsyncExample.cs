using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetFeedDriverChangeAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetFeedDriverChangeAsyncExample).Name);

            try
            {
                // Feed parameters.
                // See MyGeotab SDK <a href="https://geotab.github.io/sdk/software/guides/concepts/#result-limits">Result Limits</a> and <a href="https://geotab.github.io/sdk/software/api/reference/#M:Geotab.Checkmate.Database.DataStore.GetFeed1">GetFeed()</a> documentation for information about the feed result limit defined below.
                const int DefaultFeedResultsLimitDriverChange = 50000;
                int getFeedNumberOfCallsToMake = 5;
                int getFeedSecondsToWaitBetweenCalls = 5;
                long? feedVersion = 0;

                List<DriverChange> driverChangeCache = new List<DriverChange>();
                FeedResult<DriverChange> feedResult;

                // Start by populating the driverChangeCache with a list of all driverChanges.
                ConsoleUtility.LogListItem($"Population of driverChangeCache started.");
                bool isFirstCall = true;
                bool keepGoing = true;
                while (keepGoing == true)
                {
                    feedResult = await api.GetFeedDriverChangeAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedDriverChangeAsync executed:");
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    if (isFirstCall == true)
                    {
                        driverChangeCache.AddRange(feedResult.Data);
                        isFirstCall = false;
                    }
                    else
                    {
                        // Add new items to the cache, or update existing items with their changed counterparts.
                        foreach (DriverChange feedResultDriverChange in feedResult.Data)
                        {
                            DriverChange cachedDriverChangeToUpdate = driverChangeCache.Where(driverChange => driverChange.Id == feedResultDriverChange.Id).FirstOrDefault();
                            if (cachedDriverChangeToUpdate == null)
                            {
                                driverChangeCache.Add(feedResultDriverChange);
                            }
                            else
                            {
                                var index = driverChangeCache.IndexOf(cachedDriverChangeToUpdate);
                                driverChangeCache[index] = feedResultDriverChange;
                            }
                        }
                    }
                    if (feedResult.Data.Count < DefaultFeedResultsLimitDriverChange)
                    {
                        keepGoing = false;
                    }
                }
                ConsoleUtility.LogListItem($"Population of driverChangeCache completed.");

                // Execute a GetFeed loop for the prescribed number of iterations, adding new items to the cache, or updating existing items with their changed counterparts.
                for (int getFeedCallNumber = 1; getFeedCallNumber < getFeedNumberOfCallsToMake + 1; getFeedCallNumber++)
                {
                    feedResult = await api.GetFeedDriverChangeAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedDriverChangeAsync executed.  Iteration:", getFeedCallNumber.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    // Add new items to the cache, or update existing items with their changed counterparts.
                    foreach (DriverChange feedResultDriverChange in feedResult.Data)
                    {
                        DriverChange cachedDriverChangeToUpdate = driverChangeCache.Where(driverChange => driverChange.Id == feedResultDriverChange.Id).FirstOrDefault();
                        if (cachedDriverChangeToUpdate == null)
                        {
                            driverChangeCache.Add(feedResultDriverChange);
                        }
                        else
                        {
                            var index = driverChangeCache.IndexOf(cachedDriverChangeToUpdate);
                            driverChangeCache[index] = feedResultDriverChange;
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

            ConsoleUtility.LogExampleFinished(typeof(GetFeedDriverChangeAsyncExample).Name);
        }
    }
}