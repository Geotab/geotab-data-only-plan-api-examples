using System;
using System.Threading;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetFeedTripAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetFeedTripAsyncExample).Name);

            try
            {
                // Feed parameters.
                int getFeedNumberOfCallsToMake = 5;
                int getFeedSecondsToWaitBetweenCalls = 5;
                DateTime getFeedStartTime = DateTime.UtcNow - TimeSpan.FromDays(1);
                // See MyGeotab SDK <a href="https://geotab.github.io/sdk/software/guides/concepts/#result-limits">Result Limits</a> and <a href="https://geotab.github.io/sdk/software/api/reference/#M:Geotab.Checkmate.Database.DataStore.GetFeed1">GetFeed()</a> documentation for information about the feed result limit defined below.
                int getFeedresultsLimit = 50000;

                long? feedVersion;
                FeedResult<Trip> feedResult;

                // Make initial GetFeed call using the "seed" time.  The returned toVersion will be used as the fromVersion to start the subsequent GetFeed loop.
                feedResult = await api.GetFeedTripAsync(getFeedStartTime, getFeedresultsLimit);
                feedVersion = feedResult.ToVersion;

                // Log results to console.
                Console.WriteLine($"Initial feed start time: {getFeedStartTime.ToString()}");
                Console.WriteLine($"Initial FeedResult ToVersion: {feedVersion.ToString()}");
                Console.WriteLine($"Initial FeedResult Records: {feedResult.Data.Count.ToString()}");
                if (feedResult.Data.Count > 0)
                {
                    Console.WriteLine($"Initial FeedResult first record DateTime: {feedResult.Data[0].DateTime.ToString()}");
                    Console.WriteLine($"Initial FeedResult last record DateTime: {feedResult.Data[feedResult.Data.Count - 1].DateTime.ToString()}");
                }

                // Execute a GetFeed loop for the prescribed number of iterations, setting the fromVersion on the first iteration to the toVersion that was returned by the initial GetFeed call.
                for (int getFeedCallNumber = 1; getFeedCallNumber < getFeedNumberOfCallsToMake + 1; getFeedCallNumber++)
                {
                    // Make GetFeed call.
                    feedResult = await api.GetFeedTripAsync(feedVersion, getFeedresultsLimit);
                    feedVersion = feedResult.ToVersion;

                    // Log results to console.
                    Console.WriteLine($"Feed iteration: {getFeedCallNumber.ToString()}");
                    Console.WriteLine($"Feed iteration: {getFeedCallNumber.ToString()} FeedResult ToVersion: {feedVersion.ToString()}");
                    Console.WriteLine($"Feed iteration: {getFeedCallNumber.ToString()} FeedResult Records: {feedResult.Data.Count.ToString()}");
                    if (feedResult.Data.Count > 0)
                    {
                        Console.WriteLine($"Feed iteration: {getFeedCallNumber.ToString()} FeedResult first record DateTime: {feedResult.Data[0].DateTime.ToString()}");
                        Console.WriteLine($"Feed iteration: {getFeedCallNumber.ToString()} FeedResult last record DateTime: {feedResult.Data[feedResult.Data.Count - 1].DateTime.ToString()}");
                    }
                    // Wait for the prescribed amount of time before making the next GetFeed call.
                    Thread.Sleep(getFeedSecondsToWaitBetweenCalls * 1000);
                }
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(GetFeedTripAsyncExample).Name);
        }
    }
}