using Humanizer.Localisation;
using Microsoft.AspNetCore.Http;
using NuGet.Protocol;
using Smartstore.Core.Localization;
using Smartstore.Core.Localization.Routing;
using Smartstore.Core.Platform.Identity.Dtos;
using Smartstore.Core.Platform.Localization.Proc;
using Smartstore.Core.Seo;
using Smartstore.Web.Api;
using Smartstore.Web.Models.Common;
using Smartstore.Web.Models.System;
using System;

namespace Smartstore.Web.Controllers
{
    public class CommonController : WebApiController
    {
        #region Fields

        private readonly SeoSettings _seoSettings;
        private readonly ILocalizationService _localizationService;
        private readonly IApplicationContext _appContext;

        #endregion

        #region Ctor

        public CommonController(SeoSettings seoSettings,
            ILocalizationService localizationService,
            IApplicationContext appContext)
        {
            _seoSettings = seoSettings;
            _localizationService = localizationService;
            _appContext = appContext;
        }

        #endregion

        #region Methods

        [HttpGet("Startup")]
        public IActionResult Startup()
        {
            var titleRoute = "";
            var defaultTitle = _seoSettings.GetLocalizedSetting(x => x.MetaTitle).Value;

            switch (_seoSettings.PageTitleSeoAdjustment)
            {
                case PageTitleSeoAdjustment.PagenameAfterStorename:
                    titleRoute = "{0}" + _seoSettings.PageTitleSeparator + defaultTitle;
                    break;
                case PageTitleSeoAdjustment.StorenameAfterPagename:
                default:
                    titleRoute = defaultTitle + _seoSettings.PageTitleSeparator + "{0}";
                    break;
            }

            //todo remove
            var customer = Services.WorkContext.CurrentCustomer;

            var model = new StartupModel()
            {
                DefaultTitle = defaultTitle,
                RouteTitle = titleRoute,
                IsRegistered = customer.IsRegistered(),
                IsAdmin = customer.IsRegistered() && customer.IsAdmin(),
                CompanyId = CompanyId.ZeroToNull()
            };

            if (CustomerId.GetValueOrDefault(0) > 0)
            {
                CustomerWebContextDto customerDto = Services.WorkContext.CurrentCustomerById(CustomerId.GetValueOrDefault());
                if (customerDto != null)
                {
                    model.IsRegistered = customerDto.IsRegistered();
                    model.IsAdmin = customer.IsRegistered() && customer.IsAdmin();
                }
            }

            return ApiJson(new GenericApiModel<StartupModel>().Success(model), httpContext: null);
        }

        [LocalizedRoute("Resources")]
        public async Task<IActionResult> Resources()
        {
            var resources = await _localizationService.GetPublicResourcesAsync();
            return ApiJson(new GenericApiModel<IList<LocaleStringResourceDto>>().Success(resources), httpContext: null);
        }

        [LocalizedRoute("SecondaryAsideList")]
        public async Task<IActionResult> SecondaryAsideList()
        {
            var controllerName = ControllerName;
            var list = new List<VmSecondaryMenuItem>();

            if (controllerName.EqualsNoCase("Settings"))
            {
                list.Add(new VmSecondaryMenuItem
                {
                    Text = T("App.Navigation.Settings.Account"),
                    SVGClass = "arrow-trending-lines-outline",
                    Url = "/settings/general"
                });
            }

            return ApiJson(new GenericApiModel<IList<VmSecondaryMenuItem>>().Success(list.ToArray()), HttpContext);
        }

        #endregion
    }
}
