using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetFeedUserAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetFeedUserAsyncExample).Name);

            try
            {
                // Feed parameters.
                // See MyGeotab SDK <a href="https://geotab.github.io/sdk/software/guides/concepts/#result-limits">Result Limits</a> and <a href="https://geotab.github.io/sdk/software/api/reference/#M:Geotab.Checkmate.Database.DataStore.GetFeed1">GetFeed()</a> documentation for information about the feed result limit defined below.
                const int DefaultFeedResultsLimitUser = 5000;
                int getFeedNumberOfCallsToMake = 5;
                int getFeedSecondsToWaitBetweenCalls = 5;
                long? feedVersion = 0;

                List<User> userCache = new List<User>();
                FeedResult<User> feedResult;

                // Start by populating the userCache with a list of all users.
                ConsoleUtility.LogListItem($"Population of userCache started.");
                bool isFirstCall = true;
                bool keepGoing = true;
                while (keepGoing == true)
                {
                    feedResult = await api.GetFeedUserAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedUserAsync executed:");
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    if (isFirstCall == true)
                    {
                        userCache.AddRange(feedResult.Data);
                        isFirstCall = false;
                    }
                    else
                    {
                        // Add new items to the cache, or update existing items with their changed counterparts.
                        foreach (User feedResultUser in feedResult.Data)
                        {
                            User cachedUserToUpdate = userCache.Where(user => user.Id == feedResultUser.Id).FirstOrDefault();
                            if (cachedUserToUpdate == null)
                            {
                                userCache.Add(feedResultUser);
                            }
                            else
                            {
                                var index = userCache.IndexOf(cachedUserToUpdate);
                                userCache[index] = feedResultUser;
                            }
                        }
                    }
                    if (feedResult.Data.Count < DefaultFeedResultsLimitUser)
                    {
                        keepGoing = false;
                    }
                }
                ConsoleUtility.LogListItem($"Population of userCache completed.");

                // Execute a GetFeed loop for the prescribed number of iterations, adding new items to the cache, or updating existing items with their changed counterparts.
                for (int getFeedCallNumber = 1; getFeedCallNumber < getFeedNumberOfCallsToMake + 1; getFeedCallNumber++)
                {
                    feedResult = await api.GetFeedUserAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedUserAsync executed.  Iteration:", getFeedCallNumber.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    // Add new items to the cache, or update existing items with their changed counterparts.
                    foreach (User feedResultUser in feedResult.Data)
                    {
                        User cachedUserToUpdate = userCache.Where(user => user.Id == feedResultUser.Id).FirstOrDefault();
                        if (cachedUserToUpdate == null)
                        {
                            userCache.Add(feedResultUser);
                        }
                        else
                        {
                            var index = userCache.IndexOf(cachedUserToUpdate);
                            userCache[index] = feedResultUser;
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

            ConsoleUtility.LogExampleFinished(typeof(GetFeedUserAsyncExample).Name);
        }
    }
}