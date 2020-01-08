using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;
using Geotab.DataOnlyPlan.API.Examples.FleetMonitor.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// The "brain" of the application.  Coordinates implementation of configuration items, instantiates <see cref="FeedParameters"/> and <see cref="FeedProcessor"/> objects, and iteratively triggers retrieval of data via feeds, followed by the writing of data to output files (and summary data to the console window).  
    /// </summary>
    class DatabaseWorker : Worker
    {
        const string FaultDataTokenFilename = "FaultData Token.txt";
        const string GpsTokenFilename = "GPS Token.txt";
        const string StatusDataTokenFilename = "StatusData Token.txt";
        readonly string DataFeedTokenFolder;
        readonly string OutputFolder;
        readonly string faultDataTokenFilePath;
        readonly string gpsTokenFilePath;
        readonly string statusDataTokenFilePath;
        readonly long MaximumFileSizeInBytes;
        readonly int FeedIntervalSeconds;
        readonly FeedParameters feedParameters;
        readonly FeedProcessor feedProcessor;
        readonly bool TrackSpecificVehicles;
        readonly List<Device> DevicesToTrack;
        readonly List<Diagnostic> DiagnosticsToTrack;
        readonly List<TrackedVehicle> TrackedVehicles;
        int iterationNumber;
        readonly bool useStatusDataFeed;
        readonly bool useFaultDataFeed;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseWorker"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="password">The password.</param>
        /// <param name="database">The database.</param>
        /// <param name="server">The server.</param>
        /// <param name="dataFeedTokenFolder">The folder where data feed token files are to be written.</param>
        /// <param name="outputFolder">The folder where output files are to be written.</param>
        /// <param name="maximumFileSizeInBytes">The maximum size, in bytes, that a file may reach before a new file is started.</param>
        /// <param name="feedIntervalSeconds">The number of seconds to wait, after processing a batch of feed results, before executing the next iteration of GetFeed() calls.</param>
        /// <param name="feedStartOption">The <see cref="Common.FeedStartOption" /> to use.</param>
        /// <param name="feedStartSpecificTimeUTC">If <paramref name="feedStartOption"/> is set to <see cref="Common.FeedStartOption.SpecificTime"/>, the date and time at which to start the data feeds.</param>
        /// <param name="trackSpecificVehicles">Whether to track specific vehicles or vehicles that are reporting data.</param>
        /// <param name="devicesToTrack">If <paramref name="trackSpecificVehicles"/> is <c>true</c>, the list of <see cref="Device"/>s to track.</param>
        /// <param name="diagnosticsToTrack">The <see cref="Diagnostic"/>s that are to be tracked.</param>
        public DatabaseWorker(string user, string password, string database, string server, string dataFeedTokenFolder, string outputFolder, long maximumFileSizeInBytes, int feedIntervalSeconds, Common.FeedStartOption feedStartOption, DateTime? feedStartSpecificTimeUTC = null, bool trackSpecificVehicles = false, IList<Device> devicesToTrack = null, IList<Diagnostic> diagnosticsToTrack = null)
            : base()
        {
            // Validate input.
            if (trackSpecificVehicles == true && (devicesToTrack == null || devicesToTrack.Count == 0))
            {
                throw new ArgumentException($"'trackSpecificVehicles' is set to 'true', but 'devicesToTrack' is null or empty.");
            }

            DataFeedTokenFolder = dataFeedTokenFolder;
            OutputFolder = outputFolder;
            MaximumFileSizeInBytes = maximumFileSizeInBytes;
            TrackSpecificVehicles = trackSpecificVehicles;
            DevicesToTrack = (List<Device>)devicesToTrack;
            DiagnosticsToTrack = (List<Diagnostic>)diagnosticsToTrack;
            
            // Determine whether to use StatusData and/or FaultData feeds based on the diagnostics, if any, that are to be tracked.
            if (DiagnosticsToTrack != null && DiagnosticsToTrack.Count > 0)
            {
                if (DiagnosticsToTrack.Where(diagnosticToTrack => diagnosticToTrack.DiagnosticType == DiagnosticType.Sid || diagnosticToTrack.DiagnosticType == DiagnosticType.Pid || diagnosticToTrack.DiagnosticType == DiagnosticType.SuspectParameter || diagnosticToTrack.DiagnosticType == DiagnosticType.ObdFault || diagnosticToTrack.DiagnosticType == DiagnosticType.GoFault || diagnosticToTrack.DiagnosticType == DiagnosticType.ObdWwhFault || diagnosticToTrack.DiagnosticType == DiagnosticType.ProprietaryFault || diagnosticToTrack.DiagnosticType == DiagnosticType.LegacyFault).Any())
                {
                    useFaultDataFeed = true;
                }
                if (DiagnosticsToTrack.Where(diagnosticToTrack => diagnosticToTrack.DiagnosticType == DiagnosticType.GoDiagnostic || diagnosticToTrack.DiagnosticType == DiagnosticType.DataDiagnostic).Any())
                {
                    useStatusDataFeed = true;
                }
            }

            // Build token file paths.
            faultDataTokenFilePath = Path.Combine(DataFeedTokenFolder, FaultDataTokenFilename);
            gpsTokenFilePath = Path.Combine(DataFeedTokenFolder, GpsTokenFilename);
            statusDataTokenFilePath = Path.Combine(DataFeedTokenFolder, StatusDataTokenFilename);

            // If feeds are to be started based on feed result token, read previously-written token values from their respective files. 
            long faultDataToken = 0;
            long gpsToken = 0;
            long statusDataToken = 0;
            if (feedStartOption == Common.FeedStartOption.FeedResultToken)
            {
                if (File.Exists(faultDataTokenFilePath))
                {
                    using (StreamReader faultDataTokenFileReader = new StreamReader(faultDataTokenFilePath))
                    {
                        String faultDataTokenString = faultDataTokenFileReader.ReadToEnd();
                        long.TryParse(faultDataTokenString, out faultDataToken);
                    }
                }
                if (File.Exists(gpsTokenFilePath))
                {
                    using (StreamReader gpsTokenFileReader = new StreamReader(gpsTokenFilePath))
                    {
                        String gpsTokenString = gpsTokenFileReader.ReadToEnd();
                        long.TryParse(gpsTokenString, out gpsToken);
                    }
                }
                if (File.Exists(statusDataTokenFilePath))
                {
                    using (StreamReader statusDataTokenFileReader = new StreamReader(statusDataTokenFilePath))
                    {
                        String statusDataTokenString = statusDataTokenFileReader.ReadToEnd();
                        long.TryParse(statusDataTokenString, out statusDataToken);
                    }
                }
            }

            // Instantiate FeedParameters and FeedProcessor objects.
            feedParameters = new FeedParameters(gpsToken, statusDataToken, faultDataToken, feedStartOption, feedStartSpecificTimeUTC);
            FeedIntervalSeconds = feedIntervalSeconds;
            feedProcessor = new FeedProcessor(server, database, user, password, useFaultDataFeed, useStatusDataFeed);
            TrackedVehicles = new List<TrackedVehicle>();
        }

        /// <summary>
        /// Creates a <see cref="TrackedVehicle"/> object to represent and hold feed data for the subject <see cref="Device"/>.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> to be represented.</param>
        /// <returns></returns>
        TrackedVehicle CreateTrackedVehicle(Device device)
        {
            TrackedVehicle trackedVehicle = new TrackedVehicle(device, OutputFolder, MaximumFileSizeInBytes);
            if (DiagnosticsToTrack != null && DiagnosticsToTrack.Count > 0)
            {
                // Add TrackedDiagnostics.
                foreach (Diagnostic diagnosticToTrack in DiagnosticsToTrack)
                {
                    TrackedDiagnostic trackedDiagnostic = new TrackedDiagnostic(trackedVehicle.Device, diagnosticToTrack, trackedVehicle.FaultDataFilePath, trackedVehicle.StatusDataFilePath, MaximumFileSizeInBytes);
                    trackedVehicle.TrackedDiagnostics.Add(trackedDiagnostic);
                }
            }
            TrackedVehicles.Add(trackedVehicle);
            return trackedVehicle;
        }

        /// <summary>
        /// Gets the <see cref="TrackedVehicle"/> representing the subject <see cref="Device"/> from the <see cref="TrackedVehicles"/> list.  If not found, a new <see cref="TrackedVehicle"/> will be created, added to the <see cref="TrackedVehicles"/> list and returned under the following conditions: 1 - if specific vehicles are being tracked and the subject <see cref="Device"/> represents one of those vehicles; 2 - if vehicles are to be added to the <see cref="TrackedVehicles"/> list dynamically as they report data. 
        /// </summary>
        /// <param name="device">The <see cref="Device"/> for which to get the associated <see cref="TrackedVehicle"/>.</param>
        /// <returns></returns>
        TrackedVehicle GetTrackedVehicle(Device device)
        {
            TrackedVehicle trackedVehicleToReturn = null;

            // Try to find the TrackedVehicle.
            List<TrackedVehicle> listWithTargetTrackedVehicle = TrackedVehicles.Where(trackedVehicle => trackedVehicle.DeviceId == device.Id).ToList();
            if (listWithTargetTrackedVehicle.Any())
            {
                trackedVehicleToReturn = listWithTargetTrackedVehicle.First();
            }
            if (trackedVehicleToReturn == null)
            {
                if (TrackSpecificVehicles == true)
                {
                    // If tracking specific vehicles and the subject Device is in the list of DevicesToTrack, create the TrackedVehicle.
                    if (DevicesToTrack.Where(deviceToTrack => deviceToTrack.Id == device.Id).Any())
                    {
                        trackedVehicleToReturn = CreateTrackedVehicle(device);
                    }
                }
                else
                {
                    // If all vehicles that are reporting data are being tracked, create the TrackedVehicle.
                    trackedVehicleToReturn = CreateTrackedVehicle(device);
                }
            }
            return trackedVehicleToReturn;
        }

        /// <summary>
        /// Processes the feed results.  First, <see cref="LogRecord"/>s are added to <see cref="TrackedGpsData"/> of <see cref="TrackedVehicle"/>s and <see cref="StatusData"/>/<see cref="FaultData"/> records are added to <see cref="TrackedDiagnostic"/>s of <see cref="TrackedVehicle"/>s.  Then, the newly-received data for the affected <see cref="TrackedVehicle"/>s is written to file(s) (and summary info is written to the console window). 
        /// </summary>
        /// <param name="results">The <see cref="FeedResultData"/> containing new data received from the data feeds.</param>
        /// <returns></returns>
        async Task ProcessFeedResultsAsync(FeedResultData results)
        {
            try
            {
                // For each received LogRecord, if the LogRecord is for a vehicle that is being tracked, add the LogRecord to the TrackedVehicle's TrackedGpsData.  
                foreach (LogRecord logRecord in results.GpsRecords)
                {
                    TrackedVehicle trackedVehicleToUpdate = GetTrackedVehicle(logRecord.Device);
                    if (trackedVehicleToUpdate != null)
                    {
                        TrackedGpsData trackedGpsData = trackedVehicleToUpdate.TrackedGpsData;
                        trackedGpsData.AddData(logRecord);
                    }
                }

                if (useStatusDataFeed == true)
                {
                    // For each received StatusData, if the StatusData represents a Diagnostic that is being tracked and the StatusData is for a vehicle that is being tracked, add the StatusData to the TrackedVehicle's TrackedDiagnostics.
                    foreach (StatusData statusData in results.StatusData)
                    {
                        if (!DiagnosticsToTrack.Where(diagnostic => diagnostic.Id == statusData.Diagnostic.Id).Any())
                        {
                            continue;
                        }
                        TrackedVehicle trackedVehicleToUpdate = GetTrackedVehicle(statusData.Device);
                        if (trackedVehicleToUpdate != null)
                        {
                            TrackedDiagnostic trackedDiagnosticToUpdate = trackedVehicleToUpdate.TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.DiagnosticId == statusData.Diagnostic.Id).First();
                            trackedDiagnosticToUpdate.AddData(statusData);
                        }
                    }
                }
                if (useFaultDataFeed == true)
                {
                    // For each received FaultData, if the FaultData represents a Diagnostic that is being tracked and the FaultData is for a vehicle that is being tracked, add the FaultData to the TrackedVehicle's TrackedDiagnostics.
                    foreach (FaultData faultData in results.FaultData)
                    {
                        if (!DiagnosticsToTrack.Where(diagnostic => diagnostic.Id == faultData.Diagnostic.Id).Any())
                        {
                            continue;
                        }
                        TrackedVehicle trackedVehicleToUpdate = GetTrackedVehicle(faultData.Device);
                        if (trackedVehicleToUpdate != null)
                        {
                            TrackedDiagnostic trackedDiagnosticToUpdate = trackedVehicleToUpdate.TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.DiagnosticId == faultData.Diagnostic.Id).First();
                            trackedDiagnosticToUpdate.AddData(faultData);
                        }
                    }

                }
                WriteFeedResultStatsToConsole();
                foreach (TrackedVehicle trackedVehicle in TrackedVehicles)
                {
                    await trackedVehicle.WriteDataToFileAsync();
                }
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }
        }

        /// <summary>
        /// Executes a "feed iteration" whereby data is retrieved via feed(s), feed result tokens are stored and persisted to file, feed results are processed (including writing data to output files and displaying summary data in the console window), and a delay of the configured duration is applied.  This method is called iteratively via the inherited <see cref="Worker.DoWorkAsync(bool)"/> method of the base <see cref="Worker"/> class.
        /// </summary>
        /// <returns></returns>
        public async override Task WorkActionAsync()
        {
            iterationNumber += 1;
            ConsoleUtility.LogSeparator2();
            ConsoleUtility.LogInfoMultiPart("Iteration:", iterationNumber.ToString(), Common.ConsoleColorForUnchangedData);

            // Execute the feed calls and get the results for processing.
            FeedResultData feedResultData = await feedProcessor.GetFeedDataAsync(feedParameters);
            
            // Write the feed result token (toVersion) values to file.
            using (StreamWriter faultDataTokenFileWriter = new StreamWriter(faultDataTokenFilePath))
            {
                faultDataTokenFileWriter.Write(feedParameters.LastFaultDataToken);
            }
            using (StreamWriter gpsTokenFileWriter = new StreamWriter(gpsTokenFilePath))
            {
                gpsTokenFileWriter.Write(feedParameters.LastGpsDataToken);
            }
            using (StreamWriter statusDataTokenFileWriter = new StreamWriter(statusDataTokenFilePath))
            {
                statusDataTokenFileWriter.Write(feedParameters.LastStatusDataToken);
            }

            // Process the feed results.
            await ProcessFeedResultsAsync(feedResultData);

            // Wait for the configured duration before executing the process again.
            ConsoleUtility.LogListItem($"Waiting for {FeedIntervalSeconds.ToString()} second(s) before starting next iteration...");
            await Task.Delay(TimeSpan.FromSeconds(FeedIntervalSeconds));
        }

        /// <summary>
        /// Writes updates to the console window for the current feed iteration including lists of <see cref="Device"/>s for which new <see cref="LogRecord"/>s, <see cref="StatusData"/> records and <see cref="FaultData"/> records have been received.
        /// </summary>
        public void WriteFeedResultStatsToConsole()
        {
            StringBuilder vehicleListStringBuilder;
            string vehicleList;
            List<TrackedVehicle> trackedVehiclesWithNewData;
            // Build and display list of devices for which there are GPS log updates.
            trackedVehiclesWithNewData = TrackedVehicles.Where(trackedVehicle => trackedVehicle.HasNewGpsLogRecords == true).ToList();
            if (trackedVehiclesWithNewData != null && trackedVehiclesWithNewData.Any())
            {
                vehicleListStringBuilder = new StringBuilder();
                foreach (TrackedVehicle trackedVehicle in trackedVehiclesWithNewData)
                {
                    vehicleListStringBuilder.Append($"{trackedVehicle.DeviceId.ToString()}, ");
                }
                vehicleList = vehicleListStringBuilder.ToString();
                vehicleList = vehicleList.Substring(0, vehicleList.Length - 2);
                ConsoleUtility.LogListItem("GPS log updates received for devices:", vehicleList, Common.ConsoleColorForListItems, Common.ConsoleColorForChangedData);
            }
            // Build and display list of devices for which there are StatusData updates.
            trackedVehiclesWithNewData = TrackedVehicles.Where(trackedVehicle => trackedVehicle.HasNewStatusDataRecords == true).ToList();
            if (trackedVehiclesWithNewData != null && trackedVehiclesWithNewData.Any())
            {
                vehicleListStringBuilder = new StringBuilder();
                foreach (TrackedVehicle trackedVehicle in trackedVehiclesWithNewData)
                {
                    vehicleListStringBuilder.Append($"{trackedVehicle.DeviceId.ToString()}, ");
                }
                vehicleList = vehicleListStringBuilder.ToString();
                vehicleList = vehicleList.Substring(0, vehicleList.Length - 2);
                ConsoleUtility.LogListItem("StatusData updates received for devices:", vehicleList, Common.ConsoleColorForListItems, Common.ConsoleColorForChangedData);
            }
            // Build and display list of devices for which there are FaultData updates.
            trackedVehiclesWithNewData = TrackedVehicles.Where(trackedVehicle => trackedVehicle.HasNewFaultDataRecords == true).ToList();
            if (trackedVehiclesWithNewData != null && trackedVehiclesWithNewData.Any())
            {
                vehicleListStringBuilder = new StringBuilder();
                foreach (TrackedVehicle trackedVehicle in trackedVehiclesWithNewData)
                {
                    vehicleListStringBuilder.Append($"{trackedVehicle.DeviceId.ToString()}, ");
                }
                vehicleList = vehicleListStringBuilder.ToString();
                vehicleList = vehicleList.Substring(0, vehicleList.Length - 2);
                ConsoleUtility.LogListItem("FaultData updates received for devices:", vehicleList, Common.ConsoleColorForListItems, Common.ConsoleColorForChangedData);
            }
        }
    }
}