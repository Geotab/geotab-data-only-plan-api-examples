using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geotab.DataOnlyPlan.API.Examples.Utilities;
using Geotab.Checkmate.ObjectModel;

namespace Geotab.DataOnlyPlan.API.Examples
{
    static class AddTextMessageAsyncExample
    {
        public static async Task Run(GeotabDataOnlyPlanAPI api, string deviceId, string userId)
        {
            ConsoleUtility.LogExampleStarted(typeof(AddTextMessageAsyncExample).Name);

            try
            {
                List<Device> deviceCache = await ExampleUtility.GetAllDevicesAsync(api);
                List<User> userCache = await ExampleUtility.GetAllUsersAsync(api);
                Device deviceForTextMessages = deviceCache.Where(targetDevice => targetDevice.Id.ToString() == deviceId).First();
                User userForTextMessages = userCache.Where(targetUser => targetUser.Id.ToString() == userId).First();

                /**
                 * Example: Add basic text message:
                 */

                // Set-up the message content.
                TextContent messageContent = new("Testing: Geotab API example text message", false);

                // Construct the text message.
                DateTime utcNow = DateTime.UtcNow;
                TextMessage basicTextMessage = new(null, null, utcNow, utcNow, deviceForTextMessages, userForTextMessages, messageContent, true, null, null, null, null, null);

                // Add the text message. MyGeotab will take care of the actual sending.
                string addedTextMessageId = await api.AddTextMessageAsync(basicTextMessage);


                /**
                 * Example: Add location message:
                 * Note: A location message is a message with a location. A series of location messages can be sent in succession to comprise a route.  A clear message can be sent to clear any previous location messages.
                 */

                // Set up message and GPS location
                LocationContent clearStopsContent = new("Testing: Geotab API example clear all stops message", "Reset Stops", 0, 0);
                // Construct a "Clear Previous Stops" message
                TextMessage clearMessage = new(deviceForTextMessages, userForTextMessages, clearStopsContent, true);
                // Add the clear stops text message, Geotab will take care of the sending process.
                string addedClearMessageId = await api.AddTextMessageAsync(clearMessage);
                // Set up message and GPS location
                LocationContent withGPSLocation = new("Testing: Geotab API example location message", "Geotab", 43.452879, -79.701648);
                // Construct the location text message.
                TextMessage locationMessage = new(deviceForTextMessages, userForTextMessages, withGPSLocation, true);
                // Add the text message, Geotab will take care of the sending process.
                string addedLocationMessageId = await api.AddTextMessageAsync(locationMessage);


                /**
                 * Example: IoXOutput Message
                 */
                IoxOutputContent ioxOutputContent = new(true);
                TextMessage ioxOutputMessage = new(deviceForTextMessages, userForTextMessages, ioxOutputContent, true);
                string addedIoxOutputMessageId = await api.AddTextMessageAsync(ioxOutputMessage);


                /**
                 * Example: MimeContent Message
                 */
                string messageString = "Secret Message!";
                byte[] bytes = Encoding.ASCII.GetBytes(messageString);
                TimeSpan binaryDataPacketDelay = new(0, 0, 0);
                MimeContent mimeContent = new("multipart/byteranges", bytes, binaryDataPacketDelay, null);
                TextMessage mimeContentTextMessage = new(deviceForTextMessages, userForTextMessages, mimeContent, true);
                string addedMimeContentTextMessageId = await api.AddTextMessageAsync(mimeContentTextMessage);


                /**
                 * Example: GoTalk Message
                 */
                GoTalkContent goTalkContent = new("You're following too closely!");
                TextMessage goTalkMessage = new(deviceForTextMessages, userForTextMessages, goTalkContent, true);
                string addedGoTalkMessageId = await api.AddTextMessageAsync(goTalkMessage);
            }
            catch (Exception ex)
            {
                ConsoleUtility.LogError(ex);
            }

            ConsoleUtility.LogExampleFinished(typeof(AddTextMessageAsyncExample).Name);
        }
    }
}