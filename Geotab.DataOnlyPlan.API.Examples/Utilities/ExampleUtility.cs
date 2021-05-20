using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Geotab.Checkmate.ObjectModel;
using Geotab.Checkmate.ObjectModel.Engine;

namespace Geotab.DataOnlyPlan.API.Examples.Utilities
{
    /// <summary>
    /// Contains methods used by the various examples.
    /// </summary>
    public class ExampleUtility
    {
        /// <summary>
        /// Get a list of all <see cref="Device"/> objects in the currently-authenticated database.
        /// </summary>
        /// <param name="api">An authenticated <see cref="GeotabDataOnlyPlanAPI"/> object.</param>
        /// <returns></returns>
        public static async Task<List<Device>> GetAllDevicesAsync(Geotab.DataOnlyPlan.API.GeotabDataOnlyPlanAPI api)
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
        public static async Task<List<Diagnostic>> GetAllDiagnosticsAsync(Geotab.DataOnlyPlan.API.GeotabDataOnlyPlanAPI api)
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

        /// <summary>
        /// Get a list of all <see cref="DriverChange"/> objects in the currently-authenticated database.
        /// </summary>
        /// <param name="api">An authenticated <see cref="GeotabDataOnlyPlanAPI"/> object.</param>
        /// <returns></returns>
        public static async Task<List<DriverChange>> GetAllDriverChangesAsync(Geotab.DataOnlyPlan.API.GeotabDataOnlyPlanAPI api)
        {
            const int DefaultFeedResultsLimitDriverChange = 50000;
            List<DriverChange> allDriverChanges = new();
            long? feedVersion = 0;
            FeedResult<DriverChange> feedResult;
            bool keepGoing = true;

            while (keepGoing == true)
            {
                feedResult = await api.GetFeedDriverChangeAsync(feedVersion);
                feedVersion = feedResult.ToVersion;
                allDriverChanges.AddRange(feedResult.Data);
                if (feedResult.Data.Count < DefaultFeedResultsLimitDriverChange)
                {
                    keepGoing = false;
                }
            }
            return allDriverChanges;
        }

        /// <summary>
        /// Get a list of all <see cref="User"/> objects in the currently-authenticated database.
        /// </summary>
        /// <param name="api">An authenticated <see cref="GeotabDataOnlyPlanAPI"/> object.</param>
        /// <returns></returns>
        public static async Task<List<User>> GetAllUsersAsync(Geotab.DataOnlyPlan.API.GeotabDataOnlyPlanAPI api)
        {
            const int DefaultFeedResultsLimitUser = 5000;
            List<User> allUsers = new();
            long? feedVersion = 0;
            FeedResult<User> feedResult;
            bool keepGoing = true;

            while (keepGoing == true)
            {
                feedResult = await api.GetFeedUserAsync(feedVersion);
                feedVersion = feedResult.ToVersion;
                allUsers.AddRange(feedResult.Data);
                if (feedResult.Data.Count < DefaultFeedResultsLimitUser)
                {
                    keepGoing = false;
                }
            }
            return allUsers;
        }

        /// <summary>
        /// Gets the Id of a random device.
        /// </summary>
        /// <param name="devices">This list of devices from which to get a random Id.</param>
        /// <returns></returns>
        public static string GetRandomDeviceId(IList<Device> devices)
        {
            string deviceId;
            if (devices.Any())
            {
                var random = new Random();
                int index = random.Next(devices.Count);
                deviceId = devices[index].Id.ToString();
            }
            else
            {
                throw new Exception("No devices found.");
            }
            return deviceId;
        }

        /// <summary>
        /// Gets the name of a random device.
        /// </summary>
        /// <param name="devices">This list of devices from which to get a random name.</param>
        /// <returns></returns>
        public static string GetRandomDeviceName(IList<Device> devices)
        {
            string deviceName;
            if (devices.Any())
            {
                var random = new Random();
                int index = random.Next(devices.Count);
                deviceName = devices[index].Name;
            }
            else
            {
                throw new Exception("No devices found.");
            }
            return deviceName;
        }

        /// <summary>
        /// Gets the serial number of a random device.
        /// </summary>
        /// <param name="devices">This list of devices from which to get a random serial number.</param>
        /// <returns></returns>
        public static string GetRandomDeviceSerialNumber(IList<Device> devices)
        {
            string deviceSerialNumber;
            if (devices.Any())
            {
                var random = new Random();
                int index = random.Next(devices.Count);
                deviceSerialNumber = devices[index].SerialNumber;
            }
            else
            {
                throw new Exception("No devices found.");
            }
            return deviceSerialNumber;
        }

        /// <summary>
        /// Gets the Id of a random driverChange.
        /// </summary>
        /// <param name="driverChanges">This list of driverChanges from which to get a random Id.</param>
        /// <returns></returns>
        public static string GetRandomDriverChangeId(IList<DriverChange> driverChanges)
        {
            string driverChangeId;
            if (driverChanges.Any())
            {
                var random = new Random();
                int index = random.Next(driverChanges.Count);
                driverChangeId = driverChanges[index].Id.ToString();
            }
            else
            {
                throw new Exception("No driverChanges found.");
            }
            return driverChangeId;
        }

        /// <summary>
        /// Gets the FirstName of a random user.
        /// </summary>
        /// <param name="users">This list of users from which to get a random first name.</param>
        /// <returns></returns>
        public static string GetRandomUserFirstName(IList<User> users)
        {
            string userFirstName;
            if (users.Any())
            {
                var random = new Random();
                int index = random.Next(users.Count);
                userFirstName = users[index].FirstName;
            }
            else
            {
                throw new Exception("No users found.");
            }
            return userFirstName;
        }

        /// <summary>
        /// Gets the Id of a random user.
        /// </summary>
        /// <param name="users">This list of users from which to get a random Id.</param>
        /// <returns></returns>
        public static string GetRandomUserId(IList<User> users)
        {
            string userId;
            if (users.Any())
            {
                var random = new Random();
                int index = random.Next(users.Count);
                userId = users[index].Id.ToString();
            }
            else
            {
                throw new Exception("No users found.");
            }
            return userId;
        }
    }
}