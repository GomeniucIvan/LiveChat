using Microsoft.Extensions.Logging;
using SlackAPI;

namespace Smartstore.Utilities
{

    /// <summary>
    /// https://api.slack.com/apps/A025VFBU7QT/oauth? bot access token
    /// </summary>
    public class SlackLoggerSettingsHelper
    {
        public bool EnableSlackLogger { get; set; }
        public string Token { get; set; }
        public bool LogCustomerEmail { get; set; }
        public bool LogCustomerIp { get; set; }

        public bool EnableInformationLevel { get; set; }
        public string InformationLevelChannelName { get; set; }

        public bool EnableWarningLevel { get; set; }
        public string WarningLevelChannelName { get; set; }

        public bool EnableErrorLevel { get; set; }
        public string ErrorLevelChannelName { get; set; }

        public bool EnableCriticalLevel { get; set; }
        public string CriticalLevelChannelName { get; set; }

        //nested
        public string CustomerIp { get; set; }
        public string CustomerEmail { get; set; }
        public string WebsiteHostUrl { get; set; }
        public string CurrentPageUrl { get; set; }
    }

    public static class SlackLogger
    {
        private static SlackLoggerSettingsHelper _slackLoggerSettings = new ();

        public static void Initialize(SlackLoggerSettingsHelper settings)
        {
            _slackLoggerSettings = settings;
        }

        public static void LogMessageToSlack(LogLevel logLevel, Exception ex, string message,
            params object[] args)
        {
            if (!string.IsNullOrEmpty(message) && args.Any())
                message = String.Format(message, args);

            if (_slackLoggerSettings.EnableSlackLogger && !string.IsNullOrEmpty(_slackLoggerSettings.Token))
            {
                var currentDateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm");

                var htmlToSend = $"*Log level:* {logLevel.ToString()} \n";
                htmlToSend += $"*Created on:* `{currentDateTime}` \n";
                htmlToSend += $"*Page:* `{_slackLoggerSettings.CurrentPageUrl}` \n";
                if (_slackLoggerSettings.LogCustomerEmail)
                    htmlToSend += $"*Email:* `{_slackLoggerSettings.CustomerEmail}` \n";

                if (_slackLoggerSettings.LogCustomerIp)
                    htmlToSend += $"*IP address:* `{_slackLoggerSettings.CustomerIp}` \n";

                if (!string.IsNullOrEmpty(message))
                    htmlToSend += $"*Message:* ```{message}``` \n";

                if (ex != null && ex.InnerException != null)
                    htmlToSend += $"*Exception:* `{ex.InnerException}` \n";

                if (string.IsNullOrEmpty(message) && ex == null)
                {
                    return;
                }

                var slackClient = new SlackTaskClient(_slackLoggerSettings.Token);

                //Information type
                if (logLevel == LogLevel.Information && _slackLoggerSettings.EnableInformationLevel &&
                    !string.IsNullOrEmpty(_slackLoggerSettings.InformationLevelChannelName))
                {
                    var task = Task.Factory.StartNew(async () =>
                    {
                        PostMessageResponse response =
                            await slackClient.PostMessageAsync(_slackLoggerSettings.InformationLevelChannelName,
                                htmlToSend);

                        if (response != null && !response.ok)
                        {

                        }
                    }).Unwrap();
                }

                //Warning type
                if (logLevel == LogLevel.Warning && _slackLoggerSettings.EnableWarningLevel &&
                    !string.IsNullOrEmpty(_slackLoggerSettings.WarningLevelChannelName))
                {
                    var task = Task.Factory.StartNew(async () =>
                    {
                        PostMessageResponse response =
                            await slackClient.PostMessageAsync(_slackLoggerSettings.WarningLevelChannelName,
                                htmlToSend);

                        if (response != null && !response.ok)
                        {

                        }
                    }).Unwrap();
                }

                //Error type
                if (logLevel == LogLevel.Error && _slackLoggerSettings.EnableErrorLevel &&
                    !string.IsNullOrEmpty(_slackLoggerSettings.ErrorLevelChannelName))
                {
                    var task = Task.Factory.StartNew(async () =>
                    {
                        PostMessageResponse response =
                            await slackClient.PostMessageAsync(_slackLoggerSettings.ErrorLevelChannelName, htmlToSend);

                        if (response != null && !response.ok)
                        {

                        }
                    }).Unwrap();
                }

                //Critical type
                if (logLevel == LogLevel.Critical && _slackLoggerSettings.EnableCriticalLevel &&
                    !string.IsNullOrEmpty(_slackLoggerSettings.CriticalLevelChannelName))
                {
                    var task = Task.Factory.StartNew(async () =>
                    {
                        PostMessageResponse response =
                            await slackClient.PostMessageAsync(_slackLoggerSettings.CriticalLevelChannelName,
                                htmlToSend);

                        if (response != null && !response.ok)
                        {

                        }
                    }).Unwrap();
                }
            }
        }

        public static void LogMessageToSlackLogger(this ILogger l, LogLevel logLevel, Exception ex, string message,
            params object[] args)
        {
            LogMessageToSlack(logLevel, ex, message, args);
        }
    }
}
