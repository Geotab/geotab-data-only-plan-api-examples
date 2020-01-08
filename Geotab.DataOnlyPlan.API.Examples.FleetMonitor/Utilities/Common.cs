using System;
using System.Collections.Generic;
using System.Text;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor.Utilities
{
    /// <summary>
    /// Constants and Enums shared by multiple classes.
    /// </summary>
    public static class Common
    {
        public const ConsoleColor ConsoleColorDefault = ConsoleColor.White;
        public const ConsoleColor ConsoleColorForChangedData = ConsoleColor.Green;
        public const ConsoleColor ConsoleColorForErrors = ConsoleColor.Red;
        public const ConsoleColor ConsoleColorForListItemIds = ConsoleColor.Green;
        public const ConsoleColor ConsoleColorForListItems = ConsoleColor.White;
        public const ConsoleColor ConsoleColorForSuccess = ConsoleColor.Green;
        public const ConsoleColor ConsoleColorForUserInput = ConsoleColor.Cyan;
        public const ConsoleColor ConsoleColorForUserPrompts = ConsoleColor.Yellow;
        public const ConsoleColor ConsoleColorForUnchangedData = ConsoleColor.DarkGray;
        public const ConsoleColor ConsoleColorForWarnings = ConsoleColor.Magenta;

        public enum FeedStartOption { CurrentTime, SpecificTime, FeedResultToken }
        public const int MegabyteToByteMultiplier = 1024000;
        public enum VehicleTrackingOption { Reporting, Specific }
    }
}