using System.Collections.Generic;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Contains the results of a set of data feed calls.
    /// </summary>
    class FeedResultData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedResultData"/> class.
        /// </summary>
        public FeedResultData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedResultData"/> class.
        /// </summary>
        /// <param name="gpsRecords">The <see cref="LogRecord" />s returned by the server.</param>
        /// <param name="statusData">The <see cref="StatusData" /> returned by the server.</param>
        /// <param name="faultData">The <see cref="FaultData" /> returned by the server.</param>
        public FeedResultData(IList<LogRecord> gpsRecords, IList<StatusData> statusData, IList<FaultData> faultData)
        {
            GpsRecords = gpsRecords;
            StatusData = statusData;
            FaultData = faultData;
        }

        /// <summary>
        /// Gets or sets the <see cref="FaultData" /> collection.
        /// </summary>
        /// <value>
        /// The fault data.
        /// </value>
        public IList<FaultData> FaultData { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LogRecord"/> collection.
        /// </summary>
        public IList<LogRecord> GpsRecords { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StatusData" /> collection.
        /// </summary>
        /// <value>
        /// The status data.
        /// </value>
        public IList<StatusData> StatusData { get; set; }
    }
}