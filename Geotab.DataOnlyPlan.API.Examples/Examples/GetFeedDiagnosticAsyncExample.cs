using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class GetFeedDiagnosticAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api)
        {
            ConsoleUtility.LogExampleStarted(typeof(GetFeedDiagnosticAsyncExample).Name);

            try
            {
                // Feed parameters.
                // See MyGeotab SDK <a href="https://geotab.github.io/sdk/software/guides/concepts/#result-limits">Result Limits</a> and <a href="https://geotab.github.io/sdk/software/api/reference/#M:Geotab.Checkmate.Database.DataStore.GetFeed1">GetFeed()</a> documentation for information about the feed result limit defined below.
                const int DefaultFeedResultsLimitDiagnostic = 50000;
                int getFeedNumberOfCallsToMake = 5;
                int getFeedSecondsToWaitBetweenCalls = 5;
                long? feedVersion = 0;

                List<Diagnostic> diagnosticCache = new List<Diagnostic>();
                FeedResult<Diagnostic> feedResult;

                // Start by populating the diagnosticCache with a list of all diagnostics.
                ConsoleUtility.LogListItem($"Population of diagnosticCache started.");
                bool isFirstCall = true;
                bool keepGoing = true;
                while (keepGoing == true)
                {
                    feedResult = await api.GetFeedDiagnosticAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedDiagnosticAsync executed:");
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    if (isFirstCall == true)
                    {
                        diagnosticCache.AddRange(feedResult.Data);
                        isFirstCall = false;
                    }
                    else
                    {
                        // Add new items to the cache, or update existing items with their changed counterparts.
                        foreach (Diagnostic feedResultDiagnostic in feedResult.Data)
                        {
                            Diagnostic cachedDiagnosticToUpdate = diagnosticCache.Where(diagnostic => diagnostic.Id == feedResultDiagnostic.Id).FirstOrDefault();
                            if (cachedDiagnosticToUpdate == null)
                            {
                                diagnosticCache.Add(feedResultDiagnostic);
                            }
                            else
                            {
                                var index = diagnosticCache.IndexOf(cachedDiagnosticToUpdate);
                                diagnosticCache[index] = feedResultDiagnostic;
                            }
                        }
                    }
                    if (feedResult.Data.Count < DefaultFeedResultsLimitDiagnostic)
                    {
                        keepGoing = false;
                    }
                }
                ConsoleUtility.LogListItem($"Population of diagnosticCache completed.");

                // Execute a GetFeed loop for the prescribed number of iterations, adding new items to the cache, or updating existing items with their changed counterparts.
                for (int getFeedCallNumber = 1; getFeedCallNumber < getFeedNumberOfCallsToMake + 1; getFeedCallNumber++)
                {
                    feedResult = await api.GetFeedDiagnosticAsync(feedVersion);
                    feedVersion = feedResult.ToVersion;
                    ConsoleUtility.LogListItem("GetFeedDiagnosticAsync executed.  Iteration:", getFeedCallNumber.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult ToVersion:", feedVersion.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    ConsoleUtility.LogListItem("FeedResult Records:", feedResult.Data.Count.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForSuccess);
                    // Add new items to the cache, or update existing items with their changed counterparts.
                    foreach (Diagnostic feedResultDiagnostic in feedResult.Data)
                    {
                        Diagnostic cachedDiagnosticToUpdate = diagnosticCache.Where(diagnostic => diagnostic.Id == feedResultDiagnostic.Id).FirstOrDefault();
                        if (cachedDiagnosticToUpdate == null)
                        {
                            diagnosticCache.Add(feedResultDiagnostic);
                        }
                        else
                        {
                            var index = diagnosticCache.IndexOf(cachedDiagnosticToUpdate);
                            diagnosticCache[index] = feedResultDiagnostic;
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

            ConsoleUtility.LogExampleFinished(typeof(GetFeedDiagnosticAsyncExample).Name);
        }
    }
}