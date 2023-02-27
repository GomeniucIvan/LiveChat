using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using NuGet.Protocol;
using Smartstore.ComponentModel;
using Smartstore.Core.Localization;
using Smartstore.Web.Common.Api;
using Smartstore.Web.Infrastructure.JwtToken.Extensions;

namespace Smartstore.Web.Api
{
    [ODataRouteComponent("odata/v1")]
    [Route("odata/v1")]
    [ApiExplorerSettings(GroupName = "webapi1")]
    public abstract class WebApiController: SmartODataController
    {
        //BasicAuthenticationHandler

        #region Fields

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ICommonServices Services { get; set; }
        public ILogger Logger { get; set; } = NullLogger.Instance;

        #endregion

        #region Claims

        protected int? CustomerId
        {
            get
            {
                var httpContext = HttpContext;
                var userContext = httpContext.User;
                var customerIdString = userContext.Claims.GetStringClaimValue(JwtClaimTypes.CustomerId);
                if (!string.IsNullOrEmpty(customerIdString))
                {
                    return int.Parse(customerIdString);
                }

                return null;
            }
        }

        protected int? VisitorId
        {
            get
            {
                var httpContext = HttpContext;
                var userContext = httpContext.User;
                var customerIdString = userContext.Claims.GetStringClaimValue(JwtClaimTypes.VisitorId);
                if (!string.IsNullOrEmpty(customerIdString))
                {
                    return int.Parse(customerIdString);
                }

                return 0;
            }
        }

        protected string CompanyKey
        {
            get
            {
                var httpContext = HttpContext;
                var userContext = httpContext.User;
                return userContext.Claims.GetStringClaimValue(JwtClaimTypes.CompanyKey);
            }
        }

        protected int CompanyId
        {
            get
            {
                var httpContext = HttpContext;
                var userContext = httpContext.User;
                var customerIdString = userContext.Claims.GetStringClaimValue(JwtClaimTypes.CompanyId);
                if (!string.IsNullOrEmpty(customerIdString))
                {
                    return int.Parse(customerIdString);
                }

                return 0;
            }
        }

        #endregion

        #region Helpers

        protected virtual IActionResult ApiJson(object model, HttpContext httpContext)
        {
            //CommonController not require validation/redirect
            if (model is not null && httpContext is not null)
            {
                var forceLogOut = httpContext.User.Claims.GetStringClaimValue(JwtClaimTypes.ForceLogOut);
                if (forceLogOut.ToBool(defaultValue: false))
                {
                    // Get the type of the object
                    Type objectType = model.GetType();

                    // Create a new property
                    PropertyInfo newProperty = objectType.GetProperty(JwtClaimTypes.ForceLogOut) ?? objectType.GetProperty(JwtClaimTypes.ForceLogOut, BindingFlags.Instance | BindingFlags.Public);

                    if (newProperty is not null)
                    {
                        // Set the value of the new property
                        newProperty.SetValue(model, true);
                    }
                }

                var errorMessage = httpContext.User.Claims.GetStringClaimValue(JwtClaimTypes.ErrorMessage);
                if (!errorMessage.IsEmpty())
                {
                    // Get the type of the object
                    Type objectType = model.GetType();

                    // Create a new property
                    PropertyInfo newProperty = objectType.GetProperty(JwtClaimTypes.ErrorMessage) ?? objectType.GetProperty(JwtClaimTypes.ErrorMessage, BindingFlags.Instance | BindingFlags.Public);

                    if (newProperty is not null)
                    {
                        // Set the value of the new property
                        newProperty.SetValue(model, errorMessage);
                    }
                }
            }

            return Ok(model.ToJson());
        }

        #endregion
    }
}
