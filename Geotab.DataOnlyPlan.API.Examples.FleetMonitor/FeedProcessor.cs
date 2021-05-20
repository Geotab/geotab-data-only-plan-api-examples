using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;
using Exception = System.Exception;
using Geotab.DataOnlyPlan.API.Examples.FleetMonitor.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Retrieves data from a MyGeotab database via multiple data feeds.  Also maintains caches of "lookup" data and "hydrates" objects returned via data feed by fully-populating certain child objects that are returned with only the Id properties populated.
    /// </summary>
    class FeedProcessor
    {
        const int CacheRepopulationIntervalMinutes = 5;
        const int DefaultFeedResultsLimitDevice = 5000;
        const int DefaultFeedResultsLimitDiagnostic = 50000;
        readonly GeotabDataOnlyPlanAPI api;
        readonly bool UseStatusDataFeed;
        readonly bool UseFaultDataFeed;
        long? lastDeviceFeedVersion = 0;
        long? lastDiagnosticFeedVersion = 0;
        readonly IDictionary<Id, Controller> controllerCache;
        readonly IDictionary<Id, Device> deviceCache;
        readonly IDictionary<Id, Diagnostic> diagnosticCache;
        readonly IDictionary<Id, FailureMode> failureModeCache;
        readonly IDictionary<Id, UnitOfMeasure> unitOfMeasureCache;
        DateTime nextCacheRepopulationTime = DateTime.MinValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedProcessor"/> class.
        /// </summary>
        /// <param name="server">The Geotab server address.</param>
        /// <param name="database">The database.</param>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="useFaultDataFeed">Indicates whether to use the <see cref="Geotab.Checkmate.ObjectModel.Engine.FaultData"/> feed.</param>
        /// <param name="useStatusDataFeed">Indicates whether to use the <see cref="Geotab.Checkmate.ObjectModel.Engine.StatusData"/> feed.</param>
        public FeedProcessor(string server, string database, string user, string password, bool useFaultDataFeed, bool useStatusDataFeed)
            : this(new GeotabDataOnlyPlanAPI(server, database, user, password), useFaultDataFeed, useStatusDataFeed)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedProcessor"/> class.
        /// </summary>
        /// <param name="api">The <see cref="GeotabDataOnlyPlanAPI"/> instance to use.</param>
        /// <param name="useFaultDataFeed">Indicates whether to use the <see cref="Geotab.Checkmate.ObjectModel.Engine.FaultData"/> feed.</param>
        /// <param name="useStatusDataFeed">Indicates whether to use the <see cref="Geotab.Checkmate.ObjectModel.Engine.StatusData"/> feed.</param>
        public FeedProcessor(GeotabDataOnlyPlanAPI api, bool useFaultDataFeed, bool useStatusDataFeed)
        {
            this.api = api;
            _ = api.AuthenticateAsync();
            controllerCache = new Dictionary<Id, Controller>();
            deviceCache = new Dictionary<Id, Device>();
            diagnosticCache = new Dictionary<Id, Diagnostic>();
            failureModeCache = new Dictionary<Id, FailureMode>();
            unitOfMeasureCache = new Dictionary<Id, UnitOfMeasure>();
            UseFaultDataFeed = useFaultDataFeed;
            UseStatusDataFeed = useStatusDataFeed;
        }

        /// <summary>
        /// Returns a fully-populated <see cref="Controller"/> from the cache if it exists, otherwise gets it from the server and adds it to the cache before returning it.
        /// </summary>
        /// <param name="controller">The <see cref="Controller"/> to populate.</param>
        /// <returns>A fully-populated <see cref="Controller"/>.</returns>
        async Task<Controller> GetControllerAsync(Controller controller)
        {
            if (controller == null || controller is NoController)
            {
                return NoController.Value;
            }
            Id id = controller.Id;
            if (controllerCache.TryGetValue(id, out controller))
            {
                return controller;
            }
            IList<Controller> returnedControllers = await api.GetControllersAsync(id.ToString());
            if (!returnedControllers.Any())
            {
                return null;
            }
            controller = returnedControllers.First();
            controllerCache.Add(id, controller);
            return controller;
        }

        /// <summary>
        /// Returns a fully-populated <see cref="Device"/> from the cache.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> to populate.</param>
        /// <returns>A fully-populated <see cref="Device"/>.</returns>
        Device GetDevice(Device device)
        {
            if (device == null || device is NoDevice)
            {
                return NoDevice.Value;
            }

            if (deviceCache.TryGetValue(device.Id, out Device deviceToReturn))
            {
                return deviceToReturn;
            }
            else
            {
                return NoDevice.Value;
            }
        }

        /// <summary>
        /// Returns a fully-populated <see cref="Diagnostic"/> from the cache.
        /// </summary>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> to populate.</param>
        /// <returns>A fully-populated <see cref="Diagnostic"/>.</returns>
        Diagnostic GetDiagnostic(Diagnostic diagnostic)
        {
            if (diagnostic == null || diagnostic is NoDiagnostic)
            {
                return NoDiagnostic.Value;
            }

            if (diagnosticCache.TryGetValue(diagnostic.Id, out Diagnostic diagnosticToReturn))
            {
                return diagnosticToReturn;
            }
            else
            {
                return NoDiagnostic.Value;
            }
        }

        /// <summary>
        /// Returns a fully populated <see cref="FailureMode"/> from the cache if it exists, otherwise gets it from the server and adds it to the cache before returning it.
        /// </summary>
        /// <param name="failureMode">The <see cref="FailureMode"/> to populate.</param>
        /// <returns>A fully-populated <see cref="FailureMode"/>.</returns>
        async Task<FailureMode> GetFailureModeAsync(FailureMode failureMode)
        {
            if (failureMode == null || failureMode is NoFailureMode)
            {
                return NoFailureMode.Value;
            }
            Id id = failureMode.Id;
            if (failureModeCache.TryGetValue(id, out failureMode))
            {
                return failureMode;
            }
            IList<FailureMode> returnedFailureModes = await api.GetFailureModesAsync(id.ToString());
            if (!returnedFailureModes.Any())
            {
                return null;
            }
            failureMode = returnedFailureModes.First();
            failureModeCache.Add(id, failureMode);
            return failureMode;
        }

        /// <summary>
        /// Requests <see cref="LogRecord"/>, <see cref="FaultData"/> and <see cref="StatusData"/> records via data feeds.  Then, updates local caches of "lookup" data.  Finally, iterates through the returned objects, "hydrating" important child objects using their fully-populated counterparts in the caches.
        /// </summary>
        /// <param name="feedParameters">Contains the latest data tokens and collections to be used in the next set of data feed calls.</param>
        /// <returns><see cref="FeedResultData"/></returns>
        public async Task<FeedResultData> GetFeedDataAsync(FeedParameters feedParameters)
        {
            FeedResultData feedResults = new(new List<LogRecord>(), new List<StatusData>(), new List<FaultData>());
            FeedResult<LogRecord> feedLogRecordData;
            FeedResult<StatusData> feedStatusData = null;
            FeedResult<FaultData> feedFaultData = null;

            try
            {
                if (feedParameters.FeedStartOption == Common.FeedStartOption.CurrentTime || feedParameters.FeedStartOption == Common.FeedStartOption.SpecificTime)
                {
                    // If the feeds are to be started at the current date/time or at a specific date/time, get the appropriate DateTime value and then use it to populate the fromDate parameter when making the GetFeed() calls.
                    DateTime feedStartTime = DateTime.UtcNow;
                    if (feedParameters.FeedStartOption == Common.FeedStartOption.SpecificTime)
                    {
                        feedStartTime = feedParameters.FeedStartSpecificTimeUTC;
                    }
                    feedLogRecordData = await api.GetFeedLogRecordAsync(feedStartTime);
                    ConsoleUtility.LogListItem("GPS log records received:", feedLogRecordData.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedLogRecordData.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
                    if (UseStatusDataFeed == true)
                    {
                        feedStatusData = await api.GetFeedStatusDataAsync(feedStartTime);
                        ConsoleUtility.LogListItem("StatusData records received:", feedStatusData.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedStatusData.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
                    }
                    if (UseFaultDataFeed == true)
                    {
                        feedFaultData = await api.GetFeedFaultDataAsync(feedStartTime);
                        ConsoleUtility.LogListItem("FaultData records received:", feedFaultData.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedFaultData.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
                    }
                    // Switch to FeedResultToken for subsequent calls.
                    feedParameters.FeedStartOption = Common.FeedStartOption.FeedResultToken;
                }
                else
                {
                    // If the feeds are to be called based on feed result token, use the tokens returned in the toVersion of previous GetFeed() calls (or loaded from file, if continuing where processing last left-off) to populate the fromVersion parameter when making the next GetFeed() calls. 
                    feedLogRecordData = await api.GetFeedLogRecordAsync(feedParameters.LastGpsDataToken);
                    ConsoleUtility.LogListItem("GPS log records received:", feedLogRecordData.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedLogRecordData.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
                    if (UseStatusDataFeed == true)
                    {
                        feedStatusData = await api.GetFeedStatusDataAsync(feedParameters.LastStatusDataToken);
                        ConsoleUtility.LogListItem("StatusData records received:", feedStatusData.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedStatusData.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
                    }
                    if (UseFaultDataFeed == true)
                    {
                        feedFaultData = await api.GetFeedFaultDataAsync(feedParameters.LastFaultDataToken);
                        ConsoleUtility.LogListItem("FaultData records received:", feedFaultData.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedFaultData.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
                    }
                }

                // Update local caches of "lookup" data.  
                if (DateTime.UtcNow > nextCacheRepopulationTime)
                {
                    // "Feedless" caches are for object types not available via data feed in the MyGeotab API.  In this case, it is necessary to updates all of the objects of each type with every update.  Since these lookup data objects are not frequently-changing (if they were, they would be accessible via data feed), these caches are only updated on a specified interval instead of on every call to this GetFeedDataAsync() method in order to avoid unnecessary processing.
                    await UpdateAllFeedlessCachesAsync();
                    nextCacheRepopulationTime = DateTime.UtcNow.AddMinutes(CacheRepopulationIntervalMinutes);
                }
                // For object types supported by the MyGeotab API data feed, the local caches can be updated every time this GetFeedDataAsync() method is executed bacause only new or changed objects are returned.
                await UpdateDeviceCacheAsync();
                await UpdateDiagnosticCacheAsync();

                // Use the local caches to "hydrate" child objects of objects returned via data feed.
                feedParameters.LastGpsDataToken = feedLogRecordData.ToVersion;
                foreach (LogRecord logRecord in feedLogRecordData.Data)
                {
                    // Populate relevant LogRecord fields.
                    logRecord.Device = GetDevice(logRecord.Device);
                    feedResults.GpsRecords.Add(logRecord);
                }
                if (UseStatusDataFeed == true)
                {
                    feedParameters.LastStatusDataToken = feedStatusData.ToVersion;
                    foreach (StatusData statusData in feedStatusData.Data)
                    {
                        // Populate relevant StatusData fields.
                        statusData.Device = GetDevice(statusData.Device);
                        statusData.Diagnostic = GetDiagnostic(statusData.Diagnostic);
                        feedResults.StatusData.Add(statusData);
                    }
                }
                if (UseFaultDataFeed == true)
                {
                    feedParameters.LastFaultDataToken = feedFaultData.ToVersion;
                    foreach (FaultData faultData in feedFaultData.Data)
                    {
                        // Populate relevant FaultData fields.
                        faultData.Device = GetDevice(faultData.Device);
                        faultData.Diagnostic = GetDiagnostic(faultData.Diagnostic);
                        faultData.Controller = await GetControllerAsync(faultData.Controller);
                        faultData.FailureMode = await GetFailureModeAsync(faultData.FailureMode);
                        feedResults.FaultData.Add(faultData);
                    }
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = Common.ConsoleColorForErrors;
                Console.WriteLine(e.Message);
                Console.ForegroundColor = Common.ConsoleColorDefault;
                if (e is HttpRequestException)
                {
                    await Task.Delay(5000);
                }
                if (e is DbUnavailableException)
                {
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
            return feedResults;
        }

        /// <summary>
        /// Updates all caches that are not populated via data feeds.  To be called periodically since Get() calls will return all entities - as opposed to GetFeed() calls, which return only the entities that have changed since the last feed version.
        /// </summary>
        /// <returns></returns>
        async Task UpdateAllFeedlessCachesAsync()
        {
            await UpdateControllerCacheAsync();
            await UpdateFailureModeCacheAsync();
            await UpdateUnitOfMeasureCacheAsync();
        }

        /// <summary>
        /// Updates the <see cref="Controller"/> cache.  All <see cref="Controller"/>s are obtained using a Get() call.  Then, <see cref="Controller"/>s that are already in the cache are replaced and any <see cref="Controller"/>s not in the cache are added to the cache.
        /// </summary>
        /// <returns></returns>
        async Task UpdateControllerCacheAsync()
        {
            ConsoleUtility.LogInfoStart("Updating Controller cache...");

            List<Controller> returnedControllers = await api.GetControllersAsync() as List<Controller>;
            foreach (Controller returnedController in returnedControllers)
            {
                if (controllerCache.ContainsKey(returnedController.Id))
                {
                    controllerCache[returnedController.Id] = returnedController;
                }
                else
                {
                    controllerCache.Add(returnedController.Id, returnedController);
                }
            }

            ConsoleUtility.LogComplete(Common.ConsoleColorForUnchangedData);
            ConsoleUtility.LogListItem($"Controller cache records added/updated:", returnedControllers.Count.ToString(), Common.ConsoleColorForListItems, (returnedControllers.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
        }

        /// <summary>
        /// Updates the <see cref="Device"/> cache.  All new or changed <see cref="Device"/>s are obtained using a GetFeed() call.  Then, <see cref="Device"/>s that are already in the cache are replaced and any <see cref="Device"/>s not in the cache are added to the cache.
        /// </summary>
        /// <returns></returns>
        async Task UpdateDeviceCacheAsync()
        {
            ConsoleUtility.LogInfoStart("Updating Device cache...");

            // Populate the deviceCache, adding new items and updating existing items with their changed counterparts from the database.  Repeat execution of the GetFeedDeviceAsync method until no more results are returned to ensure that the cache is complete and up-to-date.
            FeedResult<Device> feedResult = null;
            bool keepGoing = true;
            while (keepGoing == true)
            {
                feedResult = await api.GetFeedDeviceAsync(lastDeviceFeedVersion);
                lastDeviceFeedVersion = feedResult.ToVersion;
                foreach (Device feedResultDevice in feedResult.Data)
                {
                    if (deviceCache.ContainsKey(feedResultDevice.Id))
                    {
                        deviceCache[feedResultDevice.Id] = feedResultDevice;
                    }
                    else
                    {
                        deviceCache.Add(feedResultDevice.Id, feedResultDevice);
                    }
                }
                if (feedResult.Data.Count < DefaultFeedResultsLimitDevice)
                {
                    keepGoing = false;
                }
            }

            ConsoleUtility.LogComplete(Common.ConsoleColorForUnchangedData);
            ConsoleUtility.LogListItem($"Device cache records added/updated:", feedResult.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedResult.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
        }

        /// <summary>
        /// Updates the <see cref="Diagnostic"/> cache.  All new or changed <see cref="Diagnostic"/>s are obtained using a GetFeed() call.  Then, the <see cref="Controller"/> and <see cref="UnitOfMeasure"/> child objects are "hydrated" with fully-populated cached counterparts. Finally, <see cref="Diagnostic"/>s that are already in the cache are replaced and any <see cref="Diagnostic"/>s not in the cache are added to the cache.
        /// </summary>
        async Task UpdateDiagnosticCacheAsync()
        {
            ConsoleUtility.LogInfoStart("Updating Diagnostic cache...");

            // Populate the diagnosticCache, adding new items and updating existing items with their changed counterparts from the database.  Repeat execution of the GetFeedDiagnosticAsync method until no more results are returned to ensure that the cache is complete and up-to-date.
            FeedResult<Diagnostic> feedResult = null;
            bool keepGoing = true;
            while (keepGoing == true)
            {
                feedResult = await api.GetFeedDiagnosticAsync(lastDiagnosticFeedVersion);
                lastDiagnosticFeedVersion = feedResult.ToVersion;
                foreach (Diagnostic feedResultDiagnostic in feedResult.Data)
                {
                    // Hydrate Controller and UnitOfMeasure objects.
                    Controller controller = feedResultDiagnostic.Controller;
                    if (controller == null)
                    {
                        feedResultDiagnostic.Controller = NoController.Value;
                    }
                    else if (!controller.Equals(NoController.Value))
                    {
                        feedResultDiagnostic.Controller = controllerCache[controller.Id];
                    }
                    UnitOfMeasure unitOfMeasure = feedResultDiagnostic.UnitOfMeasure;
                    if (unitOfMeasure != null)
                    {
                        feedResultDiagnostic.UnitOfMeasure = unitOfMeasureCache[unitOfMeasure.Id];
                    }

                    // Add or update.
                    if (diagnosticCache.ContainsKey(feedResultDiagnostic.Id))
                    {
                        diagnosticCache[feedResultDiagnostic.Id] = feedResultDiagnostic;
                    }
                    else
                    {
                        diagnosticCache.Add(feedResultDiagnostic.Id, feedResultDiagnostic);
                    }
                }
                if (feedResult.Data.Count < DefaultFeedResultsLimitDiagnostic)
                {
                    keepGoing = false;
                }
            }
            ConsoleUtility.LogComplete(Common.ConsoleColorForUnchangedData);
            ConsoleUtility.LogListItem($"Diagnostics added/updated:", feedResult.Data.Count.ToString(), Common.ConsoleColorForListItems, (feedResult.Data.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
        }

        /// <summary>
        /// Updates the <see cref="FailureMode"/> cache.  All <see cref="FailureMode"/>s are obtained using a Get() call.  Then, <see cref="FailureMode"/>s that are already in the cache are replaced and any <see cref="FailureMode"/>s not in the cache are added to the cache.
        /// </summary>
        /// <returns></returns>
        async Task UpdateFailureModeCacheAsync()
        {
            ConsoleUtility.LogInfoStart("Updating FailureMode cache...");

            List<FailureMode> returnedFailureModes = await api.GetFailureModesAsync() as List<FailureMode>;
            foreach (FailureMode returnedFailureMode in returnedFailureModes)
            {
                if (failureModeCache.ContainsKey(returnedFailureMode.Id))
                {
                    failureModeCache[returnedFailureMode.Id] = returnedFailureMode;
                }
                else
                {
                    failureModeCache.Add(returnedFailureMode.Id, returnedFailureMode);
                }
            }

            ConsoleUtility.LogComplete(Common.ConsoleColorForUnchangedData);
            ConsoleUtility.LogListItem($"FailureMode cache records added/updated:", returnedFailureModes.Count.ToString(), Common.ConsoleColorForListItems, (returnedFailureModes.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
        }

        /// <summary>
        /// Updates the <see cref="UnitOfMeasure"/> cache.  All <see cref="UnitOfMeasure"/> objects are obtained using a Get() call.  Then, <see cref="UnitOfMeasure"/> objects that are already in the cache are replaced and any <see cref="UnitOfMeasure"/> objects not in the cache are added to the cache.
        /// </summary>
        /// <returns></returns>
        async Task UpdateUnitOfMeasureCacheAsync()
        {
            ConsoleUtility.LogInfoStart("Updating UnitsOfMeasure cache...");

            List<UnitOfMeasure> returnedUnitsOfMeasure = await api.GetUnitsOfMeasureAsync() as List<UnitOfMeasure>;
            foreach (UnitOfMeasure returnedUnitOfMeasure in returnedUnitsOfMeasure)
            {
                if (unitOfMeasureCache.ContainsKey(returnedUnitOfMeasure.Id))
                {
                    unitOfMeasureCache[returnedUnitOfMeasure.Id] = returnedUnitOfMeasure;
                }
                else
                {
                    unitOfMeasureCache.Add(returnedUnitOfMeasure.Id, returnedUnitOfMeasure);
                }
            }

            ConsoleUtility.LogComplete(Common.ConsoleColorForUnchangedData);
            ConsoleUtility.LogListItem($"UnitsOfMeasure cache records added/updated:", returnedUnitsOfMeasure.Count.ToString(), Common.ConsoleColorForListItems, (returnedUnitsOfMeasure.Count > 0) ? Common.ConsoleColorForChangedData : Common.ConsoleColorForUnchangedData);
        }
    }
}