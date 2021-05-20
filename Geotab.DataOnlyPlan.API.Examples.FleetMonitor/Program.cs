using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.FleetMonitor.Utilities;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// The main program.  Handles authentication to a MyGeotab database, loads configuration information and instantiates the <see cref="DatabaseWorker"/>, which is responsible for execution of  application logic. 
    /// </summary>
    class Program
    {
        const string ArgNameVehicleTrackingOption = "VehicleTrackingOption";
        const string ArgNameSpecificVehiclesToTrack = "SpecificVehiclesToTrack";
        const string ArgNameDiagnosticsToTrack = "DiagnosticsToTrack";
        const string ArgNameFeedStartOption = "FeedStartOption";
        const string ArgNameFeedStartSpecificTimeUTC = "FeedStartSpecificTimeUTC";
        const string ArgNameFeedIntervalSeconds = "FeedIntervalSeconds";
        const string ArgNameOutputFolder = "OutputFolder";
        const string ArgNameMaximumFileSizeMB = "MaximumFileSizeMB";
        const int ShortestAllowedFeedIntervalSeconds = 1;
               
        static async Task Main()
        {
            IList<ConfigItem> configItems;
            bool trackSpecificVehicles = false;
            IList<Device> devicesToTrack = null;
            Common.FeedStartOption feedStartOption = Common.FeedStartOption.CurrentTime;
            DateTime feedStartSpecificTimeUTC = DateTime.MinValue;
            int feedIntervalSeconds = 60;
            IList<Diagnostic> diagnosticsToTrack = null;
            string parentOutputFolder;
            string outputFolder;
            int maximumFileSizeBytes = 1024000;
            GeotabDataOnlyPlanAPI api;
            
            string username = "";
            string password = "";
            string server = "my.geotab.com";
            string database = "";
            string title = "";

            try
            {
                // Set title.
                title = AppDomain.CurrentDomain.FriendlyName.Replace(".", " ");
                Console.Title = title;
                ConsoleUtility.LogUtilityStartup(title);

                // Request MyGeotab credentials and database name.
                server = ConsoleUtility.GetUserInput($"MyGeotab server");
                database = ConsoleUtility.GetUserInput($"Database to run examples against.").ToLower();
                username = ConsoleUtility.GetUserInput($"MyGeotab username");
                password = ConsoleUtility.GetUserInputMasked($"MyGeotab password");

                // Create Geotab Data-Only Plan API instance and authenticate.
                api = new GeotabDataOnlyPlanAPI(server, database, username, password);
                ConsoleUtility.LogInfoStart("Authenticating...");
                await api.AuthenticateAsync();
                ConsoleUtility.LogOk();

                // Load configuration information from the config file.
                configItems = GetConfigItems("configuration");

                // Validate output folder and create subfolder for output files.
                parentOutputFolder = configItems.Where(configItem => configItem.Key == ArgNameOutputFolder).FirstOrDefault().Value;
                if (!Directory.Exists(parentOutputFolder))
                {
                    throw new ArgumentException($"The specified output folder, '{parentOutputFolder}', does not exist.");
                }
                DirectoryInfo directoryInfo = new(parentOutputFolder);
                string subfolderName = $"Output_{DateTime.Now:yyyyMMdd_HHmmss}";
                _ = directoryInfo.CreateSubdirectory(subfolderName);
                outputFolder = Path.Combine(directoryInfo.FullName, subfolderName);

                // Validate and set maximum file size.
                string maxFileSizeMBString = configItems.Where(configItem => configItem.Key == ArgNameMaximumFileSizeMB).FirstOrDefault().Value;
                if (int.TryParse(maxFileSizeMBString, out int maxFileSizeMB))
                {
                    if (maxFileSizeMB > 0)
                    {
                        maximumFileSizeBytes = maxFileSizeMB * Common.MegabyteToByteMultiplier;
                    }
                }

                // Get the vehicle tracking option.
                string vehicleTrackingOption = configItems.Where(configItem => configItem.Key == ArgNameVehicleTrackingOption).FirstOrDefault().Value;
                switch (vehicleTrackingOption)
                {
                    case nameof(Common.VehicleTrackingOption.Reporting):
                        trackSpecificVehicles = false;
                        break;
                    case nameof(Common.VehicleTrackingOption.Specific):
                        trackSpecificVehicles = true;
                        break;
                    default:
                        break;
                }

                // If specific vehicles are to be tracked, validate the supplied list of device IDs against the current database.  Discard any supplied items that are not valid device IDs.  If no valid device IDs are supplied, switch back to tracking vehicles that are currently reporting data. 
                if (trackSpecificVehicles == true)
                {
                    ConsoleUtility.LogInfo("Validating SpecificVehiclesToTrack...");
                    string vehicleListSupplied = configItems.Where(configItem => configItem.Key == ArgNameSpecificVehiclesToTrack).FirstOrDefault().Value;
                    string[] vehicleList = vehicleListSupplied.Split(",");
                    devicesToTrack = new List<Device>();
                    IList<Device> devices = await GetAllDevicesAsync(api);
                    for (int vehicleListIndex = 0; vehicleListIndex < vehicleList.Length; vehicleListIndex++)
                    {
                        string vehicleDeviceId = vehicleList[vehicleListIndex];
                        Device checkedDevice = devices.Where(device => device.Id.ToString() == vehicleDeviceId).FirstOrDefault();
                        if (checkedDevice == null)
                        {
                            ConsoleUtility.LogListItem($"Note - The following is not a valid device Id", $"{vehicleDeviceId}", Common.ConsoleColorForUnchangedData, Common.ConsoleColorForErrors);
                            continue;
                        }
                        devicesToTrack.Add(checkedDevice);
                    }
                    if (devicesToTrack.Count == 0)
                    {
                        ConsoleUtility.LogWarning($"No valid device IDs have been entered. Switching to tracking of vehicles that are currently reporting data.");
                        trackSpecificVehicles = false;
                    }
                }

                // Get diagnostics to be tracked.
                ConsoleUtility.LogInfo("Validating DiagnosticsToTrack...");
                string diagnosticListSupplied = configItems.Where(configItem => configItem.Key == ArgNameDiagnosticsToTrack).FirstOrDefault().Value;
                string[] diagnosticList = diagnosticListSupplied.Split(",");
                diagnosticsToTrack = new List<Diagnostic>();
                IList<Diagnostic> diagnostics = await GetAllDiagnosticsAsync(api);
                for (int diagnosticListIndex = 0; diagnosticListIndex < diagnosticList.Length; diagnosticListIndex++)
                {
                    string diagnosticId = diagnosticList[diagnosticListIndex];
                    Diagnostic checkedDiagnostic = diagnostics.Where(diagnostic => diagnostic.Id.ToString() == diagnosticId).FirstOrDefault();
                    if (checkedDiagnostic == null)
                    {
                        ConsoleUtility.LogListItem($"Note - The following is not a valid diagnostic Id", $"{diagnosticId}", Common.ConsoleColorForUnchangedData, Common.ConsoleColorForErrors);
                        continue;
                    }
                    diagnosticsToTrack.Add(checkedDiagnostic);
                }
                if (diagnosticsToTrack.Count == 0)
                {
                    ConsoleUtility.LogWarning($"No valid diagnostic IDs have been entered. As such, no diagnostics will be tracked.");
                }

                // Get the feed start option.
                string feedStartOptionString = configItems.Where(configItem => configItem.Key == ArgNameFeedStartOption).FirstOrDefault().Value;
                switch (feedStartOptionString)
                {
                    case nameof(Common.FeedStartOption.CurrentTime):
                        feedStartOption = Common.FeedStartOption.CurrentTime;
                        break;
                    case nameof(Common.FeedStartOption.FeedResultToken):
                        feedStartOption = Common.FeedStartOption.FeedResultToken;
                        break;
                    case nameof(Common.FeedStartOption.SpecificTime):
                        string feedStartSpecificTimeUTCString = configItems.Where(configItem => configItem.Key == ArgNameFeedStartSpecificTimeUTC).FirstOrDefault().Value;
                        if (DateTime.TryParse(feedStartSpecificTimeUTCString, null, System.Globalization.DateTimeStyles.RoundtripKind, out feedStartSpecificTimeUTC) == false)
                        {
                            ConsoleUtility.LogWarning($"The value of '{feedStartSpecificTimeUTCString}' specified for FeedStartSpecificTimeUTC is invalid. As such, the current date and time will be used instead.");
                            feedStartOption = Common.FeedStartOption.CurrentTime;
                        }
                        else
                        {
                            feedStartOption = Common.FeedStartOption.SpecificTime;
                        }
                        break;
                    default:
                        break;
                }

                // Get the feed interval.
                string feedIntervalSecondsString = configItems.Where(configItem => configItem.Key == ArgNameFeedIntervalSeconds).FirstOrDefault().Value;
                if (int.TryParse(feedIntervalSecondsString, out int feedIntervalSecondsInt))
                {
                    if (feedIntervalSecondsInt < ShortestAllowedFeedIntervalSeconds)
                    {
                        ConsoleUtility.LogListItem($"Note - The specified FeedIntervalSeconds value of '{feedIntervalSecondsString}' is less then the shortest allowed value of '{ShortestAllowedFeedIntervalSeconds}'.  FeedIntervalSeconds will be set to:", ShortestAllowedFeedIntervalSeconds.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForErrors);
                        feedIntervalSeconds = ShortestAllowedFeedIntervalSeconds;
                    }
                    else
                    {
                        feedIntervalSeconds = feedIntervalSecondsInt;
                    }
                }
                else
                {
                    ConsoleUtility.LogListItem($"Note - The specified FeedIntervalSeconds value of '{feedIntervalSecondsString}' is invalid.  FeedIntervalSeconds will be set to:", ShortestAllowedFeedIntervalSeconds.ToString(), Common.ConsoleColorForUnchangedData, Common.ConsoleColorForErrors);
                    feedIntervalSeconds = ShortestAllowedFeedIntervalSeconds;
                }

                // Instantiate a DatabaseWorker to start processing the data feeds.
                bool continuous = true;
                Worker worker = new DatabaseWorker(username, password, database, server, parentOutputFolder, outputFolder, maximumFileSizeBytes, feedIntervalSeconds, feedStartOption, feedStartSpecificTimeUTC, trackSpecificVehicles, devicesToTrack, diagnosticsToTrack);
                var cancellationToken = new CancellationTokenSource();
                Task task = Task.Run(async () => await worker.DoWorkAsync(continuous), cancellationToken.Token);
                if (continuous && Console.ReadLine() != null)
                {
                    worker.RequestStop();
                    cancellationToken.Cancel();
                }
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }
            finally
            {
                ConsoleUtility.LogUtilityShutdown(title);
                _ = Console.ReadKey();
            }
        }

        /// <summary>
        /// Prompts the user to input the path to a config file and then loads the contents of the config file into a list of <see cref="ConfigItem"/> objects.
        /// </summary>
        /// <param name="configFilePathPromptMessage">A description of the file type being sought (e.g. '<c>config</c>').  For use in user prompts.</param>
        /// <returns>A list of <see cref="ConfigItem"/> objects.</returns>
        static IList<ConfigItem> GetConfigItems(string configFilePathPromptMessage)
        {
            // Get the config file path.
            string configFilePath = ConsoleUtility.GetUserInputFilePath(configFilePathPromptMessage);

            // Load the config file contents.
            ConsoleUtility.LogInfoStart($"Loading configuration information from file '{configFilePath}'...");
            IList<ConfigItem> configItems = null;
            using (FileStream configFile = File.OpenRead(configFilePath))
            {
                configItems = configFile.CsvToList<ConfigItem>(null, 0, 0, '|');
            }
            if (!configItems.Any())
            {
                throw new Exception($"No configuration information was loaded from the CSV file '{configFilePath}'.");
            }
            ConsoleUtility.LogComplete();
            return configItems;
        }

        /// <summary>
        /// Get a list of all <see cref="Device"/> objects in the currently-authenticated database.
        /// </summary>
        /// <param name="api">An authenticated <see cref="GeotabDataOnlyPlanAPI"/> object.</param>
        /// <returns></returns>
        static async Task<List<Device>> GetAllDevicesAsync(Geotab.DataOnlyPlan.API.GeotabDataOnlyPlanAPI api)
        {
            const int DefaultFeedResultsLimitDevice = 5000;
            List<Device> allDevices = new();
            long? feedVersion = 0;
            FeedResult<Device> feedResult;
            bool keepGoing = true;

            while (keepGoing == true)
            {
                feedResult = await api.GetFeedDeviceAsync(feedVersion);
                feedVersion = feedResult.ToVersion;
                allDevices.AddRange(feedResult.Data);
                if (feedResult.Data.Count < DefaultFeedResultsLimitDevice)
                {
                    keepGoing = false;
                }
            }
            return allDevices;
        }

        /// <summary>
        /// Get a list of all <see cref="Diagnostic"/> objects in the currently-authenticated database.
        /// </summary>
        /// <param name="api">An authenticated <see cref="GeotabDataOnlyPlanAPI"/> object.</param>
        /// <returns></returns>
        static async Task<List<Diagnostic>> GetAllDiagnosticsAsync(Geotab.DataOnlyPlan.API.GeotabDataOnlyPlanAPI api)
        {
            const int DefaultFeedResultsLimitDiagnostic = 50000;
            List<Diagnostic> allDiagnostics = new();
            long? feedVersion = 0;
            FeedResult<Diagnostic> feedResult;
            bool keepGoing = true;

            while (keepGoing == true)
            {
                feedResult = await api.GetFeedDiagnosticAsync(feedVersion);
                feedVersion = feedResult.ToVersion;
                allDiagnostics.AddRange(feedResult.Data);
                if (feedResult.Data.Count < DefaultFeedResultsLimitDiagnostic)
                {
                    keepGoing = false;
                }
            }
            return allDiagnostics;
        }
    }
}