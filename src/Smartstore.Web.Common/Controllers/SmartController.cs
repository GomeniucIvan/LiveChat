using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NuGet.Protocol;
using Smartstore.Core.Localization;
using Smartstore.Core.Logging;

namespace Smartstore.Web.Controllers
{
    [NotifyFilter(Order = 1000)] // Run last (OnResultExecuting)
    public abstract class SmartController : Controller
    {
        protected SmartController()
        {
        }

        public ILogger Logger { get; set; } = NullLogger.Instance;

        public Localizer T { get; set; } = NullLocalizer.Instance;

        public ICommonServices Services { get; set; }

        public SmartDbContext Db
        {
            get => HttpContext.RequestServices.GetService<SmartDbContext>();
        }
    }
}
