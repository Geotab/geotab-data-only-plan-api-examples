using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Geotab.Checkmate.ObjectModel;
using Geotab.DataOnlyPlan.API.Examples.FleetMonitor.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Stores <see cref="LogRecord"/> data received via data feed for a vehicle (<see cref="Device"/>) and writes the stored data to output files.
    /// </summary>
    class TrackedGpsData
    {
        const string GpsDataHeader = "Device ID|Device Serial Number|Device Name|GPS Time|Latitude|Longitude|Speed";
        readonly List<LogRecord> receivedLogRecords = new();
        bool outputFileCreated;

        /// <summary>
        /// Creates a <see cref="TrackedGpsData"/> instance.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> for which to track GPS data.</param>
        /// <param name="outputFilePath">The path of the output file to which data is to be written.</param>
        /// <param name="maximumFileSizeInBytes">The maximum size, in bytes, that an output file can reach before a new output file is created.</param>
        public TrackedGpsData(Device device, string outputFilePath, long maximumFileSizeInBytes)
        {
            DeviceId = device.Id;
            DeviceName = device.Name;
            DeviceSerialNumber = device.SerialNumber;
            OutputFilePath = outputFilePath;
            MaximumFileSizeInBytes = maximumFileSizeInBytes;
        }

        /// <summary>
        /// The <see cref="Geotab.Checkmate.ObjectModel.Id"/> of the <see cref="Geotab.Checkmate.ObjectModel.Device"/> associated with the vehicle for which this <see cref="TrackedDiagnostic"/> will store data.
        /// </summary>
        public Id DeviceId { get; }

        /// <summary>
        /// The name of the <see cref="Geotab.Checkmate.ObjectModel.Device"/> associated with the vehicle for which this <see cref="TrackedDiagnostic"/> will store data.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// The serial number of the <see cref="Geotab.Checkmate.ObjectModel.Device"/> associated with the vehicle for which this <see cref="TrackedDiagnostic"/> will store data.
        /// </summary>
        public string DeviceSerialNumber { get; }

        /// <summary>
        /// The maximum size, in bytes, that an output file can reach before a new output file is created.
        /// </summary>
        public long MaximumFileSizeInBytes { get; }

        /// <summary>
        /// The path of the output file to which data is currently written by this <see cref="TrackedGpsData"/>.
        /// </summary>
        public string OutputFilePath { get; set; }

        /// <summary>
        /// The <see cref="LogRecord"/> records received via data feed for the <see cref="Device"/> associated with this <see cref="TrackedDiagnostic"/> since the last time such records were written to file.
        /// </summary>
        public ReadOnlyCollection<LogRecord> ReceivedLogRecords
        {
            get { return receivedLogRecords.AsReadOnly(); }
        }

        /// <summary>
        /// Adds a <see cref="LogRecord"/> record to the <see cref="ReceivedLogRecords"/> of this <see cref="TrackedGpsData"/>.
        /// </summary>
        /// <param name="logRecord">The <see cref="LogRecord"/> record to be added.</param>
        public void AddData(LogRecord logRecord)
        {
            // Validate to ensure LogRecord is for the subject Device.
            if (logRecord.Device.Id != DeviceId)
            {
                throw new ArgumentException($"The supplied LogRecord is for a Device with Id '{logRecord.Device.Id}' and cannot be added to this TrackedGpsData which represents the Device with Id '{DeviceId}'.");
            }

            receivedLogRecords.Add(logRecord);

            // Update DeviceName with that of the logRecord in case the device name has changed (the logRecord will have the latest version of the Device since it gets updated with the device cache in the FeedProcessor before this AddData() method is called.  
            if (DeviceName != logRecord.Device.Name)
            {
                DeviceName = logRecord.Device.Name;
            }
        }

        /// <summary>
        /// Writes the <see cref="ReceivedLogRecords"/> of this <see cref="TrackedGpsData"/> to file and then clears the subject data from memory.
        /// </summary>
        public async Task WriteDataToFileAsync()
        {
            try
            {
                if (receivedLogRecords.Count > 0)
                {
                    // Ensure log records are sorted chronologiocally.
                    List<LogRecord> sortedLogRecords = receivedLogRecords.OrderBy(logRecord => logRecord.DateTime).ToList();
                    // Write log records to file.
                    using (TextWriter fileWriter = new StreamWriter(OutputFilePath, true))
                    {
                        if (outputFileCreated == false)
                        {
                            fileWriter.WriteLine(GpsDataHeader);
                            outputFileCreated = true;
                        }
                        foreach (LogRecord logRecord in sortedLogRecords)
                        {
                            await fileWriter.WriteLineAsync($"{DeviceId}|{DeviceSerialNumber}|{DeviceName}|{logRecord.DateTime}|{logRecord.Latitude}|{logRecord.Longitude}|{logRecord.Speed}");
                        }
                    }
                    receivedLogRecords.Clear();
                }
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
                if (ex is IOException)
                {
                    // Possiable system out of memory exception or file lock. Sleep for a minute and continue.
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
            }
        }
    }
}