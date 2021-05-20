using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Represents a vehicle that is being tracked.  Holds information about the subject vehicle's device, stores GPS and diagnostic information received from data feeds for the subject vehicle, and orchestrates writing of such data to output files.
    /// </summary>
    class TrackedVehicle
    {
        const string DateFormatStringForOutputFilenames = "yyyyMMddHHmmss";

        /// <summary>
        /// Creates a <see cref="TrackedVehicle"/> instance, validates existence of the output folder, determines filenames for output GPS, fault data and status data files, creates a new <see cref="Geotab.DataOnlyPlan.API.Examples.FleetMonitor.TrackedGpsData"/> instance, and creates a new list to store <see cref="Geotab.DataOnlyPlan.API.Examples.FleetMonitor.TrackedDiagnostic"/> objects.
        /// </summary>
        /// <param name="device">The <see cref="Geotab.Checkmate.ObjectModel.Device"/> associated with the vehicle to be tracked.</param>
        /// <param name="outputFolder">The path of the folder into which output data files are to be written.</param>
        /// <param name="maximumFileSizeInBytes">The maximum size, in bytes, that an output file can reach before a new output file is created.</param>
        public TrackedVehicle(Device device, string outputFolder, long maximumFileSizeInBytes)
        {
            // Validate outputFolder.
            if (!Directory.Exists(outputFolder))
            {
                throw new ArgumentException($"The specified folder, '{outputFolder}', does not exist.");
            }

            Device = device;
            DeviceId = Device.Id;

            // Generate names of files to which data associated with the TrackedVehicle will be written.
            DateTime startTime = DateTime.Now;
            string gpsDataFilename = $"GPS Data - DeviceID {DeviceId} FileID {startTime.ToString(DateFormatStringForOutputFilenames)}.csv";
            GpsDataFilePath = Path.Combine(outputFolder, gpsDataFilename);
            string faultDataFilename = $"Fault Data - DeviceID {DeviceId} FileID {startTime.ToString(DateFormatStringForOutputFilenames)}.csv";
            FaultDataFilePath = Path.Combine(outputFolder, faultDataFilename);
            string statusDataFilename = $"Status Data - DeviceID {DeviceId} FileID {startTime.ToString(DateFormatStringForOutputFilenames)}.csv";
            StatusDataFilePath = Path.Combine(outputFolder, statusDataFilename);

            MaximumFileSizeInBytes = maximumFileSizeInBytes;
            TrackedDiagnostics = new List<TrackedDiagnostic>();
            TrackedGpsData = new TrackedGpsData(Device, GpsDataFilePath, maximumFileSizeInBytes);
        }

        /// <summary>
        /// The <see cref="Geotab.Checkmate.ObjectModel.Device"/> associated with the vehicle to be tracked.
        /// </summary>
        public Device Device { get; }

        /// <summary>
        /// The <see cref="Geotab.Checkmate.ObjectModel.Id"/> of the <see cref="Geotab.Checkmate.ObjectModel.Device"/> associated with the vehicle to be tracked.
        /// </summary>
        public Id DeviceId { get; }

        /// <summary>
        /// The path of the file to which to write fault data associated with the <see cref="TrackedVehicle"/>.
        /// </summary>
        public string FaultDataFilePath { get; set; }

        /// <summary>
        /// The path of the file to which to write GPS data associated with the <see cref="TrackedVehicle"/>.
        /// </summary>
        public string GpsDataFilePath { get; set; }

        /// <summary>
        /// Indicates whether any new <see cref="Geotab.Checkmate.ObjectModel.Engine.FaultData"/> records associated with any of this <see cref="TrackedVehicle"/>'s <see cref="TrackedDiagnostics"/> have been received since the last time such records were written to file.
        /// </summary>
        public bool HasNewFaultDataRecords
        {
            get
            {
                return TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.ReceivedFaultData.Any()).Any();
            }
        }

        /// <summary>
        /// Indicates whether any new <see cref="Geotab.Checkmate.ObjectModel.LogRecord"/> records associated with this <see cref="TrackedVehicle"/>'s <see cref="TrackedGpsData"/> have been received since the last time such records were written to file.
        /// </summary>
        public bool HasNewGpsLogRecords
        {
            get
            {
                return TrackedGpsData.ReceivedLogRecords.Any();
            }
        }

        /// <summary>
        /// Indicates whether any new <see cref="Geotab.Checkmate.ObjectModel.Engine.StatusData"/> records associated with any of this <see cref="TrackedVehicle"/>'s <see cref="TrackedDiagnostics"/> have been received since the last time such records were written to file.
        /// </summary>
        public bool HasNewStatusDataRecords
        {
            get
            {
                return TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.ReceivedStatusData.Any()).Any();
            }
        }

        /// <summary>
        /// The maximum size, in bytes, that an output file can reach before a new output file is created.
        /// </summary>
        public long MaximumFileSizeInBytes { get; }

        /// <summary>
        /// The path of the file to which to write status data associated with this <see cref="TrackedVehicle"/>.
        /// </summary>
        public string StatusDataFilePath { get; set; }

        /// <summary>
        /// A list of <see cref="Geotab.DataOnlyPlan.API.Examples.FleetMonitor.TrackedDiagnostic"/> objects to store fault data and/or status data records received from data feeds for the vehicle represented by this <see cref="TrackedVehicle"/>.  There will be one <see cref="Geotab.DataOnlyPlan.API.Examples.FleetMonitor.TrackedDiagnostic"/> object for each diagnostic being tracked.
        /// </summary>
        public List<TrackedDiagnostic> TrackedDiagnostics { get; }

        /// <summary>
        /// A <see cref="Geotab.DataOnlyPlan.API.Examples.FleetMonitor.TrackedGpsData"/> object to store GPS data records received via data feed for the vehicle represented by this <see cref="TrackedVehicle"/>.
        /// </summary>
        public TrackedGpsData TrackedGpsData { get; }

        /// <summary>
        /// Triggers the writing to file of any GPS, fault, or status data received for this <see cref="TrackedVehicle"/> since the last time such records were written to file.  Includes logic to split output files into new files when the <see cref="MaximumFileSizeInBytes"/> has been reached.
        /// </summary>
        /// <returns></returns>
        public async Task WriteDataToFileAsync()
        {
            DateTime startTime = DateTime.Now;
            string newFilenameSuffix = $"{startTime.ToString(DateFormatStringForOutputFilenames)}.csv";
            int filenamePrefixLength;
            string outputFilePathPrefix;

            // If the GPS data file for the current TrackedVehicle has reached the maximum allowed size, change the TrackedGpsData OutputFilePath so that data will be written to a new file.
            if (TrackedGpsData.ReceivedLogRecords.Any())
            {
                FileInfo gpsDataOutputFileInfo = new(GpsDataFilePath);
                if (gpsDataOutputFileInfo.Exists && gpsDataOutputFileInfo.Length >= MaximumFileSizeInBytes)
                {
                    filenamePrefixLength = GpsDataFilePath.Length - newFilenameSuffix.Length;
                    outputFilePathPrefix = GpsDataFilePath.Substring(0, filenamePrefixLength);
                    GpsDataFilePath = $"{outputFilePathPrefix}{newFilenameSuffix}";
                    TrackedGpsData.OutputFilePath = GpsDataFilePath;
                }
            }

            // If there are any TrackedDiagnostics, execute logic to split files that have exceeded the maximum allowed size and add headers to the new files (headers need to be added here because all FaultData diagnostics are written to a single file and all StatusData diagnostics are written to a single file).
            if (TrackedDiagnostics.Any())
            {
                // FaultData:
                if (TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.DiagnosticCategoryType == TrackedDiagnostic.DiagnosticCategory.FaultData).Any())
                {
                    // If the FaultData file for the current TrackedVehicle does not yet exist, or if it exists and has reached the maximum allowed size, change the FaultDataFilePath and set the OutputFilePath of all TrackedDiagnostics that represent FaultData so that data will be written to a new file. Also, create the new file and write the header if there is any data to write.
                    FileInfo faultDataFileInfo = new(FaultDataFilePath);
                    if (faultDataFileInfo.Exists == false || faultDataFileInfo.Length >= MaximumFileSizeInBytes)
                    {
                        filenamePrefixLength = FaultDataFilePath.Length - newFilenameSuffix.Length;
                        outputFilePathPrefix = FaultDataFilePath.Substring(0, filenamePrefixLength);
                        FaultDataFilePath = $"{outputFilePathPrefix}{newFilenameSuffix}";
                        bool createFileAndWriteHeader = false;
                        foreach (TrackedDiagnostic trackedDiagnostic in TrackedDiagnostics)
                        {
                            if (trackedDiagnostic.DiagnosticCategoryType == TrackedDiagnostic.DiagnosticCategory.FaultData)
                            {
                                trackedDiagnostic.OutputFilePath = FaultDataFilePath;
                            }
                            if (trackedDiagnostic.ReceivedFaultData.Any())
                            {
                                createFileAndWriteHeader = true;
                            }
                        }
                        if (createFileAndWriteHeader == true)
                        {
                            using (TextWriter fileWriter = new StreamWriter(FaultDataFilePath, true))
                            {
                                TrackedDiagnostic trackedFaultDataDiagnostic = TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.DiagnosticCategoryType == TrackedDiagnostic.DiagnosticCategory.FaultData).First();
                                fileWriter.WriteLine(trackedFaultDataDiagnostic.FaultDataHeader);
                            }
                        }
                    }
                }
                // StatusData:
                if (TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.DiagnosticCategoryType == TrackedDiagnostic.DiagnosticCategory.StatusData).Any())
                {
                    // If the StatusData file for the current TrackedVehicle does not yet exist, or if it exists and has reached the maximum allowed size, change the StatusDataFilePath and set the OutputFilePath of all TrackedDiagnostics that represent StatusData so that data will be written to a new file. Also, create the new file and write the header if there is any data to write.
                    FileInfo statusDataFileInfo = new(StatusDataFilePath);
                    if (statusDataFileInfo.Exists == false || statusDataFileInfo.Length >= MaximumFileSizeInBytes)
                    {
                        filenamePrefixLength = StatusDataFilePath.Length - newFilenameSuffix.Length;
                        outputFilePathPrefix = StatusDataFilePath.Substring(0, filenamePrefixLength);
                        StatusDataFilePath = $"{outputFilePathPrefix}{newFilenameSuffix}";
                        bool createFileAndWriteHeader = false;
                        foreach (TrackedDiagnostic trackedDiagnostic in TrackedDiagnostics)
                        {
                            if (trackedDiagnostic.DiagnosticCategoryType == TrackedDiagnostic.DiagnosticCategory.StatusData)
                            {
                                trackedDiagnostic.OutputFilePath = StatusDataFilePath;
                            }
                            if (trackedDiagnostic.ReceivedStatusData.Any())
                            {
                                createFileAndWriteHeader = true;
                            }
                        }
                        if (createFileAndWriteHeader == true)
                        {
                            using (TextWriter fileWriter = new StreamWriter(StatusDataFilePath, true))
                            {
                                TrackedDiagnostic trackedStatusDataDiagnostic = TrackedDiagnostics.Where(trackedDiagnostic => trackedDiagnostic.DiagnosticCategoryType == TrackedDiagnostic.DiagnosticCategory.StatusData).First();
                                fileWriter.WriteLine(trackedStatusDataDiagnostic.StatusDataHeader);
                            }
                        }
                    }
                }

                // Write received data to file.
                await TrackedGpsData.WriteDataToFileAsync();
                foreach (TrackedDiagnostic trackedDiagnostic in TrackedDiagnostics)
                {
                    await trackedDiagnostic.WriteDataToFileAsync();
                }
            }
        }
    }
}