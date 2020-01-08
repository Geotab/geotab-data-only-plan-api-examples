using System;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;
using Geotab.DataOnlyPlan.API.Examples.FleetMonitor.Utilities;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Contains the latest data tokens for use during the next <see cref="FeedProcessor.GetFeedDataAsync"/> call.
    /// </summary>
    class FeedParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeedParameters"/> class.
        /// </summary>
        /// <param name="lastGpsDataToken">The latest <see cref="LogRecord" /> token</param>
        /// <param name="lastStatusDataToken">The latest <see cref="StatusData" /> token</param>
        /// <param name="lastFaultDataToken">The latest <see cref="FaultData" /> token</param>
        /// <param name="feedStartOption">The <see cref="Common.FeedStartOption" /> to use.</param>
        /// <param name="feedStartSpecificTimeUTC">If <paramref name="feedStartOption"/> is set to <see cref="Common.FeedStartOption.SpecificTime" />, the date and time at which to start the data feeds.</param>
        public FeedParameters(long? lastGpsDataToken, long? lastStatusDataToken, long? lastFaultDataToken, Common.FeedStartOption feedStartOption, DateTime? feedStartSpecificTimeUTC = null)
        {
            LastGpsDataToken = lastGpsDataToken;
            LastStatusDataToken = lastStatusDataToken;
            LastFaultDataToken = lastFaultDataToken;
            FeedStartOption = feedStartOption;
            if (feedStartSpecificTimeUTC == null)
            {
                FeedStartSpecificTimeUTC = DateTime.MinValue;
            }
            else if (feedStartSpecificTimeUTC > DateTime.UtcNow)
            {
                ConsoleUtility.LogWarning($"The value of '{feedStartSpecificTimeUTC}' specified for FeedStartSpecificTimeUTC is in the future. As such, the current date and time will be used instead.");
                FeedStartSpecificTimeUTC = DateTime.UtcNow;
            }
            else
            {
                FeedStartSpecificTimeUTC = (DateTime)feedStartSpecificTimeUTC;            
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="FeedStartOption" /> to use.
        /// </summary>
        public Common.FeedStartOption FeedStartOption { get; set;  }

        /// <summary>
        /// Gets the date and time at which to start the data feeds if <paramref name="feedStartOption"/> is set to <see cref="FeedStartOption.SpecificTime" />.
        /// </summary>
        public DateTime FeedStartSpecificTimeUTC { get; }

        /// <summary>
        /// Gets or sets the latest <see cref="FaultData" /> token.
        /// </summary>
        /// <value>
        /// The last fault data token.
        /// </value>
        public long? LastFaultDataToken { get; set; }

        /// <summary>
        /// Gets or sets the latest <see cref="LogRecord" /> token.
        /// </summary>
        /// <value>
        /// The last GPS data token.
        /// </value>
        public long? LastGpsDataToken { get; set; }

        /// <summary>
        /// Gets or sets the latest <see cref="StatusData" /> token.
        /// </summary>
        /// <value>
        /// The last status data token.
        /// </value>
        public long? LastStatusDataToken { get; set; }
    }
}