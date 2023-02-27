using Smartstore.Core.Configuration;

namespace Smartstore.Core.Common.Settings
{
    /// <summary>
    /// Represents settings of the slack
    /// </summary>
    public class SlackLoggerSettings : ISettings
    {
        /// <summary>
        /// Gets or sets logger working
        /// </summary>
        public bool EnableSlackLogger { get; set; }

        /// <summary>
        /// Gets or sets secret token
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// Gets or sets logging customer email
        /// </summary>
        public bool LogCustomerEmail { get; set; }

        /// <summary>
        /// Gets or sets logging customer ip
        /// </summary>
        public bool LogCustomerIp { get; set; }

        /// <summary>
        /// Gets or sets new customer entries
        /// </summary>
        public bool LogNewCustomerAccess { get; set; }

        /// <summary>
        /// Gets or sets activity for information level
        /// </summary>
        public bool EnableInformationLevel { get; set; }

        /// <summary>
        /// Gets or sets information level channel name
        /// </summary>
        public string InformationLevelChannelName { get; set; }

        /// <summary>
        /// Gets or sets activity for warning level
        /// </summary>
        public bool EnableWarningLevel { get; set; }

        /// <summary>
        /// Gets or sets warning level channel name
        /// </summary>
        public string WarningLevelChannelName { get; set; }

        /// <summary>
        /// Gets or sets activity for error level
        /// </summary>
        public bool EnableErrorLevel { get; set; }

        /// <summary>
        /// Gets or sets error level channel name
        /// </summary>
        public string ErrorLevelChannelName { get; set; }

        /// <summary>
        /// Gets or sets activity for critical level
        /// </summary>
        public bool EnableCriticalLevel { get; set; }

        /// <summary>
        /// Gets or sets critical level channel name
        /// </summary>
        public string CriticalLevelChannelName { get; set; }
    }
}
