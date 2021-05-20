using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;
using Geotab.DataOnlyPlan.API.Examples.FleetMonitor.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Represents a <see cref="FaultData"/> or <see cref="StatusData"/> <see cref="Diagnostic"/> that is being tracked for a specific vehicle.  Holds information about the subject vehicle's <see cref="Device"/> and the <see cref="Diagnostic"/> being tracked.  Stores information received via data feed for the subject <see cref="Diagnostic"/> for the subject vehicle.  Writes stored diagnostic data to output files.   
    /// </summary>
    class TrackedDiagnostic
    {
        readonly string faultDataHeader = "Device ID|Device Serial Number|Device Name|FaultData Time|Diagnostic ID|Diagnostic Name|Failure Mode Code|Failure Mode Name|Failure Mode Source|Controller Name|Fault Count|Fault State|Malfunction Lamp Lit|Red Stop Lamp Lit|Amber Warning Lamp Lit|Protect Warning Lamp Lit|DismissedDataTime|Dismissed User";
        readonly string statusDataHeader = "Device ID|Device Serial Number|Device Name|StatusData Time|Diagnostic ID|Name|Source|Value|Units";
        readonly List<FaultData> receivedFaultData = new();
        readonly List<StatusData> receivedStatusData = new();

        /// <summary>
        /// The broad category of diagnostic that a <see cref="TrackedDiagnostic"/> falls under.
        /// </summary>
        public enum DiagnosticCategory { FaultData, StatusData }

        /// <summary>
        /// Creates a <see cref="TrackedDiagnostic"/> instance, determines its <see cref="DiagnosticCategory"/> and sets its <see cref="OutputFilePath"/> based on the type of diagnostic.
        /// </summary>
        /// <param name="device">The <see cref="Device"/> for which to track diagnostic data.</param>
        /// <param name="diagnostic">The <see cref="Diagnostic"/> for which to track data.</param>
        /// <param name="outputFaultDataFilePath">The path of the output file to which data is to be written if the <see cref="Diagnostic"/> is determined to be of the type <see cref="DiagnosticCategory.FaultData"/>.</param>
        /// <param name="outputStatusDataFilePath">The path of the output file to which data is to be written if the <see cref="Diagnostic"/> is determined to be of the type <see cref="DiagnosticCategory.StatusData"/>.</param>
        /// <param name="maximumFileSizeInBytes">The maximum size, in bytes, that an output file can reach before a new output file is created.</param>
        public TrackedDiagnostic(Device device, Diagnostic diagnostic, string outputFaultDataFilePath, string outputStatusDataFilePath, long maximumFileSizeInBytes)
        {
            // Determine the DiagnosticCategory based on the DiagnosticType of the suppled Diagnostic.
            DiagnosticType = (DiagnosticType)diagnostic.DiagnosticType;
            DiagnosticCategoryType = DiagnosticType switch
            {
                DiagnosticType.None => throw new NotSupportedException($"The DiagnosticType '{DiagnosticType}' is not supported."),
                DiagnosticType.Sid => DiagnosticCategory.FaultData,
                DiagnosticType.Pid => DiagnosticCategory.FaultData,
                DiagnosticType.GoDiagnostic => DiagnosticCategory.StatusData,
                DiagnosticType.DataDiagnostic => DiagnosticCategory.StatusData,
                DiagnosticType.SuspectParameter => DiagnosticCategory.FaultData,
                DiagnosticType.ObdFault => DiagnosticCategory.FaultData,
                DiagnosticType.GoFault => DiagnosticCategory.FaultData,
                DiagnosticType.ObdWwhFault => DiagnosticCategory.FaultData,
                DiagnosticType.ProprietaryFault => DiagnosticCategory.FaultData,
                DiagnosticType.LegacyFault => DiagnosticCategory.FaultData,
                _ => throw new NotSupportedException($"The DiagnosticType '{DiagnosticType}' is not supported."),
            };
            DeviceId = device.Id;
            DeviceName = device.Name;
            DeviceSerialNumber = device.SerialNumber;
            DiagnosticId = diagnostic.Id;

            // Set the OutputFilePath based on the type of diagnostic.
            switch (DiagnosticCategoryType)
            {
                case DiagnosticCategory.FaultData:
                    OutputFilePath = outputFaultDataFilePath;
                    break;
                case DiagnosticCategory.StatusData:
                    OutputFilePath = outputStatusDataFilePath;
                    break;
                default:
                    break;
            }

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
        /// The <see cref="Geotab.Checkmate.ObjectModel.Id"/> of the <see cref="Geotab.Checkmate.ObjectModel.Engine.Diagnostic"/> for which this <see cref="TrackedDiagnostic"/> will store data for the associated vehicle.
        /// </summary>
        public Id DiagnosticId { get; }

        /// <summary>
        /// The <see cref="DiagnosticCategory"/> of the <see cref="Geotab.Checkmate.ObjectModel.Engine.Diagnostic"/> represented by this <see cref="TrackedDiagnostic"/>.
        /// </summary>
        public DiagnosticCategory DiagnosticCategoryType { get; }

        /// <summary>
        /// The <see cref="Geotab.Checkmate.ObjectModel.Engine.DiagnosticType"/> of the <see cref="Geotab.Checkmate.ObjectModel.Engine.Diagnostic"/> represented by this <see cref="TrackedDiagnostic"/>.
        /// </summary>
        public DiagnosticType DiagnosticType { get; }

        /// <summary>
        /// The header row conatining column headers to be written to output files if the <see cref="DiagnosticCategory"/> of this <see cref="TrackedDiagnostic"/> is <see cref="DiagnosticCategory.FaultData"/>.
        /// </summary>
        public string FaultDataHeader
        {
            get { return faultDataHeader; }
        }

        /// <summary>
        /// The maximum size, in bytes, that an output file can reach before a new output file is created.
        /// </summary>
        public long MaximumFileSizeInBytes { get; }

        /// <summary>
        /// The path of the output file to which data is currently written by this <see cref="TrackedDiagnostic"/>.
        /// </summary>
        public string OutputFilePath { get; set; }

        /// <summary>
        /// If the <see cref="DiagnosticCategory"/> of this <see cref="TrackedDiagnostic"/> is <see cref="DiagnosticCategory.FaultData"/>, the <see cref="FaultData"/> records received via data feed for the <see cref="Geotab.Checkmate.ObjectModel.Engine.Diagnostic"/> and <see cref="Device"/> associated with this <see cref="TrackedDiagnostic"/> since the last time such records were written to file.
        /// </summary>
        public ReadOnlyCollection<FaultData> ReceivedFaultData
        {
            get { return receivedFaultData.AsReadOnly(); }
        }

        /// <summary>
        /// If the <see cref="DiagnosticCategory"/> of this <see cref="TrackedDiagnostic"/> is <see cref="DiagnosticCategory.StatusData"/>, the <see cref="StatusData"/> records received via data feed for the <see cref="Geotab.Checkmate.ObjectModel.Engine.Diagnostic"/> and <see cref="Geotab.Checkmate.ObjectModel.Device"/> associated with this <see cref="TrackedDiagnostic"/> since the last time such records were written to file.
        /// </summary>
        public ReadOnlyCollection<StatusData> ReceivedStatusData
        {
            get { return receivedStatusData.AsReadOnly(); }
        }

        /// <summary>
        /// The header row conatining column headers to be written to output files if the <see cref="DiagnosticCategory"/> of this <see cref="TrackedDiagnostic"/> is <see cref="DiagnosticCategory.StatusData"/>.
        /// </summary>
        public string StatusDataHeader
        {
            get { return statusDataHeader; }
        }

        /// <summary>
        /// Adds a <see cref="FaultData"/> record to the <see cref="ReceivedFaultData"/> of this <see cref="TrackedDiagnostic"/>.
        /// </summary>
        /// <param name="faultData">The <see cref="FaultData"/> record to be added.</param>
        public void AddData(FaultData faultData)
        {
            // Throw exception if there is an attempt to add FaultData and this TrackedDiagnostic does not represent a Diagnostic of the FaultData category.
            if (DiagnosticCategoryType != DiagnosticCategory.FaultData)
            {
                throw new ArgumentException($"This TrackedDiagnostic represents a Diagnostic of the type '{DiagnosticCategoryType}' and cannot accept FaultData.");
            }

            // Validate to ensure FaultData is for the subject Device and Diagnostic.
            if (faultData.Device.Id != DeviceId)
            {
                throw new ArgumentException($"The supplied FaultData is for a Device with Id '{faultData.Device.Id}' and cannot be added to this TrackedDiagnostic which represents the Device with Id '{DeviceId}'.");
            }
            if (faultData.Diagnostic.Id != DiagnosticId)
            {
                throw new ArgumentException($"The supplied FaultData is for a Diagnostic with Id '{faultData.Diagnostic.Id}' and cannot be added to this TrackedDiagnostic which represents the Diagnostic with Id '{DiagnosticId}'.");
            }
            receivedFaultData.Add(faultData);

            // Update DeviceName with that of the faultData in case the device name has changed (the faultData will have the latest version of the Device since it gets updated with the device cache in the FeedProcessor before this AddData() method is called. 
            if (DeviceName != faultData.Device.Name)
            {
                DeviceName = faultData.Device.Name;
            }
        }

        /// <summary>
        /// Adds a <see cref="StatusData"/> record to the <see cref="ReceivedStatusData"/> of this <see cref="TrackedDiagnostic"/>.
        /// </summary>
        /// <param name="statusData">The <see cref="StatusData"/> record to be added.</param>
        public void AddData(StatusData statusData)
        {
            // Throw exception if there is an attempt to add StatusData and this TrackedDiagnostic does not represent a Diagnostic of the StatusData category.
            if (DiagnosticCategoryType != DiagnosticCategory.StatusData)
            {
                throw new ArgumentException($"This TrackedDiagnostic represents a Diagnostic of the type '{DiagnosticCategoryType}' and cannot accept StatusData.");
            }

            // Validate to ensure StatusData is for the subject Device and Diagnostic.
            if (statusData.Device.Id != DeviceId)
            {
                throw new ArgumentException($"The supplied StatusData is for a Device with Id '{statusData.Device.Id}' and cannot be added to this TrackedDiagnostic which represents the Device with Id '{DeviceId}'.");
            }
            if (statusData.Diagnostic.Id != DiagnosticId)
            {
                throw new ArgumentException($"The supplied StatusData is for a Diagnostic with Id '{statusData.Diagnostic.Id}' and cannot be added to this TrackedDiagnostic which represents the Diagnostic with Id '{DiagnosticId}'.");
            }
            receivedStatusData.Add(statusData);

            // Update DeviceName with that of the statusData in case the device name has changed (the statusData will have the latest version of the Device since it gets updated with the device cache in the FeedProcessor before this AddData() method is called.  
            if (DeviceName != statusData.Device.Name)
            {
                DeviceName = statusData.Device.Name;
            }
        }

        /// <summary>
        /// Writes the <see cref="ReceivedFaultData"/> or <see cref="ReceivedStatusData"/> of this <see cref="TrackedDiagnostic"/> (depending on its <see cref="DiagnosticCategory"/>) to file and then clears the subject data from memory.
        /// </summary>
        /// <returns></returns>
        public async Task WriteDataToFileAsync()
        {
            try
            {
                switch (DiagnosticCategoryType)
                {
                    case DiagnosticCategory.FaultData:
                        if (receivedFaultData.Count > 0)
                        {
                            // Ensure FaultData records are sorted chronologiocally.
                            List<FaultData> sortedFaultData = receivedFaultData.OrderBy(faultData => faultData.DateTime).ToList();
                            // Write FaultData records to file.
                            using (TextWriter fileWriter = new StreamWriter(OutputFilePath, true))
                            {
                                foreach (FaultData faultData in sortedFaultData)
                                {
                                    string failureModeCode = "";
                                    string failureModeName = "";
                                    string failureModeSourceName = "";
                                    FailureMode failureMode = faultData.FailureMode;
                                    if (failureMode != null)
                                    {
                                        failureModeCode = failureMode.Code.ToString();
                                        failureModeName = failureMode.Name;
                                        Source failureModeSource = failureMode.Source;
                                        if (failureModeSource != null)
                                        {
                                            failureModeSourceName = failureModeSource.Name;
                                        }
                                    }
                                    string dismissUserName = "";
                                    string dismissDateTime = "";
                                    User dismissUser = faultData.DismissUser;
                                    if (dismissUser != null)
                                    {
                                        dismissUserName = dismissUser.Name;
                                        dismissDateTime = faultData.DismissDateTime.ToString();
                                    }
                                    await fileWriter.WriteLineAsync($"{DeviceId}|{DeviceSerialNumber}|{DeviceName}|{faultData.DateTime}|{faultData.Diagnostic.Id}|{faultData.Diagnostic.Name}|{failureModeCode}|{failureModeName}|{failureModeSourceName}|{faultData.Controller.Name}|{faultData.Count}|{faultData.FaultState}|{faultData.MalfunctionLamp}|{faultData.RedStopLamp}|{faultData.AmberWarningLamp}|{faultData.ProtectWarningLamp}|{dismissDateTime}|{dismissUserName}");
                                }
                            }
                            receivedFaultData.Clear();
                        }
                        break;
                    case DiagnosticCategory.StatusData:
                        if (receivedStatusData.Count > 0)
                        {
                            // Ensure StatusData records are sorted chronologiocally.
                            List<StatusData> sortedStatusData = receivedStatusData.OrderBy(statusData => statusData.DateTime).ToList();
                            // Write StatusData records to file.
                            using (TextWriter fileWriter = new StreamWriter(OutputFilePath, true))
                            {
                                foreach (StatusData statusData in sortedStatusData)
                                {
                                    await fileWriter.WriteLineAsync($"{DeviceId}|{DeviceSerialNumber}|{DeviceName}|{statusData.DateTime}|{statusData.Diagnostic.Id}|{statusData.Diagnostic.Name}|{statusData.Diagnostic.Source.Name}|{statusData.Data}|{statusData.Diagnostic.UnitOfMeasure.Name}");
                                }
                            }
                            receivedStatusData.Clear();
                        }
                        break;
                    default:
                        break;
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