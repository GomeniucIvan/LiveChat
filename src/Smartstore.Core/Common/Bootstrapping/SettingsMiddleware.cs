using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Smartstore.ComponentModel;
using Smartstore.Core.Common.Settings;
using Smartstore.Core.Stores;
using Smartstore.Core.Web;
using Smartstore.Utilities;

namespace Smartstore.Core.Common
{
    public static class SlackLoggerBuilderExtensions
    {
        /// <summary>
        /// Uses media middleware
        /// </summary>
        public static IApplicationBuilder AddLogger(this IApplicationBuilder app)
        {
            Guard.NotNull(app, nameof(app));

            //todo ivan , find alternative
            app.UseMiddleware<SettingsMiddleware>();

            return app;
        }
    }

    //todo ivan, find better way to initialize settings on startup before static files
    public class SettingsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IApplicationContext _appContext;

        private static SlackLoggerSettingsHelper _slackSettings;

        public SettingsMiddleware(
            RequestDelegate next, 
            IApplicationContext appContext)
        {
            _next = next;
            _appContext = appContext;
        }

        public async Task Invoke(
            HttpContext context,
            SlackLoggerSettings slackLoggerSettings,
            IStoreContext storeContext,
            IWorkContext workingContext,
            IWebHelper webHelper)
        {
            if (_slackSettings == null)
            {
                var mapper = MapperFactory.GetMapper<SlackLoggerSettings, SlackLoggerSettingsHelper>();
                var model = await mapper.MapAsync(slackLoggerSettings);

                var configSlackSettings = _appContext.AppConfiguration.Slack;

                if (configSlackSettings.LogError == true)
                {
                    model.EnableSlackLogger = true;
                    model.EnableErrorLevel = true;
                    model.EnableCriticalLevel = true;
                    model.CriticalLevelChannelName = configSlackSettings.Channel;
                    model.ErrorLevelChannelName = configSlackSettings.Channel;
                    model.Token = configSlackSettings.Token;
                }

                _slackSettings = model;
                var currentCustomer = workingContext?.CurrentCustomer;

                model.CustomerIp = webHelper.GetClientIpAddress()?.ToString();
                model.CustomerEmail = currentCustomer?.Email;
                model.CurrentPageUrl = webHelper.GetCurrentPageUrl(true);
                model.WebsiteHostUrl = webHelper.GetStoreLocation();

                SlackLogger.Initialize(model);

            }
            await _next(context);
        }

        public static void ResetSlackLogger()
        {
            _slackSettings = null;
        }
    }
}
