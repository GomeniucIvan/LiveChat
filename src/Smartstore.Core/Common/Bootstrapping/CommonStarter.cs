using Autofac;
using Smartstore.Core.Common;
using Smartstore.Core.Common.Services;
using Smartstore.Core.Web;
using Smartstore.Engine.Builders;

namespace Smartstore.Core.Bootstrapping
{
    internal sealed class CommonStarter : StarterBase
    {
        public override void BuildPipeline(RequestPipelineBuilder builder)
        {
            if (builder.ApplicationContext.IsInstalled)
            {
                // Run before bundling middleware
                builder.Configure(StarterOrdering.BeforeStaticFilesMiddleware - 1, app =>
                {
                    app.AddLogger();
                });
            }
        }

        public override void ConfigureContainer(ContainerBuilder builder, IApplicationContext appContext)
        {
            builder.RegisterType<GeoCountryLookup>().As<IGeoCountryLookup>().SingleInstance();
            builder.RegisterType<DateTimeHelper>().As<IDateTimeHelper>().InstancePerLifetimeScope();
            builder.RegisterType<GenericAttributeService>().As<IGenericAttributeService>().InstancePerLifetimeScope();
            builder.RegisterType<DefaultWebHelper>().As<IWebHelper>().InstancePerLifetimeScope();
            builder.RegisterType<UAParserUserAgent>().As<IUserAgent>().InstancePerLifetimeScope();
            builder.RegisterType<PreviewModeCookie>().As<IPreviewModeCookie>().InstancePerLifetimeScope();
        }
    }
}