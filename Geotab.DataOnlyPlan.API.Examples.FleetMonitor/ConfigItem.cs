﻿using System.ComponentModel.DataAnnotations;

namespace Geotab.DataOnlyPlan.API.Examples.FleetMonitor
{
    /// <summary>
    /// Object containing a key-value pair read from a configuration file.
    /// </summary>
    public class ConfigItem
    {
        /// <summary>
        /// The key.
        /// </summary>
        /// <value></value>
        [Display(Name = "Key", Order = 1)]
        public string Key { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        /// <value></value>
        [Display(Name = "Value", Order = 2)]
        public string Value { get; set; }
    }
}
