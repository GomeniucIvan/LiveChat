using System;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Microsoft.OData;
using Newtonsoft.Json;
using SlackAPI;
using Smartstore.Core.Companies.Proc;
using Smartstore.Core.Data;
using Smartstore.Core.Identity.Proc;
using Smartstore.Core.Platform.Identity.Dtos;
using Smartstore.Web.Api.Helpers;
using Smartstore.Web.Common.Api;
using Smartstore.Web.Infrastructure.JwtToken.Extensions;

namespace Smartstore.Web.Api.Security
{
    /// <summary>
    /// Verifies the identity of a user using basic authentication.
    /// Also ensures that requests are sent via HTTPS (except in a development environment).
    /// </summary>
    public sealed class BasicAuthenticationHandler : AuthenticationHandler<BasicAuthenticationOptions>
    {
        internal const string DateHeader = "Api-Date";

        private readonly IWebApiService _apiService;
        private readonly IJwtAuthManager _jwtAuthManager;
        private readonly SmartDbContext _db; 

        public BasicAuthenticationHandler(
            IWebApiService apiService,
            IOptionsMonitor<BasicAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock, 
            IJwtAuthManager jwtAuthManager,
            SmartDbContext db)
            : base(options, logger, encoder, clock)
        {
            _apiService = apiService;
            _jwtAuthManager = jwtAuthManager;
            _db = db;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var state = _apiService.GetState();

            try
            {
                // INFO: for batch requests, these headers are only present in the first response.
                var headers = Response.Headers;
                headers.Add(DateHeader, DateTime.UtcNow.ToString("o"));

                if (!state.IsActive)
                {
                    return Failure(AccessDeniedReason.ApiDisabled);
                }

                if (!Request.IsHttps && Options.SslRequired)
                {
                    return Failure(AccessDeniedReason.SslRequired);
                }

                var currentPath = GetPath(Request?.Path.Value ?? "");

                if (currentPath.StartsWith("/chat"))
                {
                    var initAuthValue = Request?.Headers[JwtClaimTypes.InitAuthorization];
                    var companyIdValue = Request?.Headers[JwtClaimTypes.CompanyId];
                    var visitorIdValue = Request?.Headers[JwtClaimTypes.VisitorId];
                    var companyKey = "";
                    var companyId = "";
                    var visitorId = "";


                    if (string.IsNullOrEmpty(initAuthValue) &&
                        (string.IsNullOrEmpty(companyIdValue) || string.IsNullOrEmpty(visitorIdValue)))
                    {
                        return Failure(AccessDeniedReason.InvalidAuthorizationHeader);
                    }

                    if (!string.IsNullOrEmpty(initAuthValue) && string.IsNullOrEmpty(companyIdValue) &&
                         string.IsNullOrEmpty(visitorIdValue))
                    {
                        try
                        {
                            var deserializedHeader = initAuthValue.ToString().Deserialize<VisitorAuthModel>();
                            if (deserializedHeader == null)
                            {
                                return Failure(AccessDeniedReason.InvalidAuthorizationHeader);
                            }

                            companyKey = deserializedHeader.Key;
                            visitorId = deserializedHeader.VisitorId;
                        }
                        catch (Exception e)
                        {
                            return Failure(AccessDeniedReason.InvalidAuthorizationHeader);
                        }
                    }
                    else
                    {
                        companyId = companyIdValue;
                        visitorId = visitorIdValue;
                    }

                    if (string.IsNullOrEmpty(visitorId))
                    {
                        return Failure(AccessDeniedReason.InvalidAuthorizationHeader);
                    }

                    var visitorClaims = new[]
                    {
                        new Claim(JwtClaimTypes.CompanyKey, companyKey, ClaimValueTypes.String, ClaimsIssuer),
                        new Claim(JwtClaimTypes.CompanyId, companyId, ClaimValueTypes.Integer32, ClaimsIssuer),
                        new Claim(JwtClaimTypes.VisitorId, visitorId, ClaimValueTypes.Integer, ClaimsIssuer)
                    };

                    var visitorPrincipal = new ClaimsPrincipal(new ClaimsIdentity(visitorClaims, Scheme.Name));
                    var visitorTicket = new AuthenticationTicket(visitorPrincipal, Scheme.Name);

                    //$"Authenticated API request using scheme {Scheme.Name}: customer {user.CustomerId}.".Dump();
                    return AuthenticateResult.Success(visitorTicket);
                }

                var rawAuthValue = Request?.Headers[HeaderNames.Authorization];
                if (!AuthenticationHeaderValue.TryParse(rawAuthValue, out var authHeader))
                {
                    //todo change to attribute
                    if (string.Equals("/Login", currentPath) ||
                        string.Equals("/Logout", currentPath) ||
                        string.Equals("/Install", currentPath) ||
                        string.Equals("/Progress", currentPath) ||
                        string.Equals("/Startup", currentPath) ||
                        string.Equals("/Resources", currentPath) ||
                        string.Equals("/Finalize", currentPath))
                    {
                        var nullableClaims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, "", ClaimValueTypes.String, ClaimsIssuer)
                        };

                        var nullablePrincipal = new ClaimsPrincipal(new ClaimsIdentity(nullableClaims, Scheme.Name));
                        var nullableTicket = new AuthenticationTicket(nullablePrincipal, Scheme.Name);

                        //$"Authenticated API request using scheme {Scheme.Name}: customer {user.CustomerId}.".Dump();
                        return AuthenticateResult.Success(nullableTicket);
                    }

                    return Failure(AccessDeniedReason.InvalidAuthorizationHeader);
                }

                if (authHeader.Scheme.IsEmpty())
                {
                    return Failure(AccessDeniedReason.InvalidAuthorizationHeader);
                }

                ClaimsPrincipal claimsPrincipal = null;

                try
                {
                    var (principalToken, jwtSecurityToken) = _jwtAuthManager.DecodeJwtToken(token: authHeader.Scheme);
                    claimsPrincipal = principalToken;
                }
                catch (Exception e)
                {
                    return Failure(AccessDeniedReason.ExpiredAuthorizationHeader); 
                }

                if (claimsPrincipal?.Identity == null)
                {
                    return Failure(AccessDeniedReason.InvalidAuthorizationHeader); 
                }

                var email = claimsPrincipal?.Identity.Name;
                
                if (email.IsEmpty())
                {
                    return Failure(AccessDeniedReason.UserUnknown);                  
                }

                var workingCompanyIdStringHeader = Request?.Headers[JwtClaimTypes.CompanyId];
                var workingCompanyIdString = workingCompanyIdStringHeader.ToString();

                CustomerApiDto user = _db.Customer_ApiDetails(email: email);

                if (user == null)
                {
                    return Failure(AccessDeniedReason.UserUnknown);
                }

                if (!user.Active)
                {
                    return Failure(AccessDeniedReason.UserDisabled);
                }

                var password = claimsPrincipal?.Claims.GetStringClaimValue(JwtClaimTypes.Password);

                if (user.Password != password)
                {
                    return Failure(AccessDeniedReason.InvalidCredentials);
                }

                if (!user.CompanyIds.Any() || !string.IsNullOrEmpty(workingCompanyIdString) && !user.CompanyIds.Any(v => v.ToString() == workingCompanyIdString))
                {
                    return Failure(AccessDeniedReason.InvalidCompany);
                }

                var selectedCompanyIdString = string.IsNullOrEmpty(workingCompanyIdString) ? user.CompanyIds.FirstOrDefault().ToString() : workingCompanyIdString;
                var updatedUser = _db.Customer_ApiDetails(email: email, updateLastLoginDate: true);

                Request.HttpContext.Items[MaxApiQueryOptions.Key] = new MaxApiQueryOptions
                {
                    MaxTop = state.MaxTop,
                    MaxExpansionDepth = state.MaxExpansionDepth
                };

                headers.CacheControl = "no-cache";

                var claims = new[]
                {
                    new Claim(JwtClaimTypes.CustomerId, user.Id.ToString(), ClaimValueTypes.Integer32, ClaimsIssuer),
                    new Claim(JwtClaimTypes.CompanyId, selectedCompanyIdString, ClaimValueTypes.Integer32, ClaimsIssuer)
                };

                var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                //$"Authenticated API request using scheme {Scheme.Name}: customer {user.CustomerId}.".Dump();
                return AuthenticateResult.Success(ticket);
            }
            catch (Exception ex)
            {
                return Failure(ex.Message, ex);
            }
        }

        protected string GetPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }
            path = path.Replace("/odata/v1", "", StringComparison.CurrentCultureIgnoreCase);
            return path;
        }

        protected override Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            // INFO: status code and response headers already set in HandleAuthenticateAsync.
            return Task.CompletedTask;
        }

        private AuthenticateResult Failure(AccessDeniedReason reason)
        {
            return Failure(CreateMessage(reason));
        }

        private AuthenticateResult Failure(string message, Exception ex = null, int statusCode = Status401Unauthorized)
        {
            Response.StatusCode = statusCode;

            var odataError = new ODataError
            {
                ErrorCode = statusCode.ToString(),
                Message = message,
                InnerError = ex != null ? new ODataInnerError(ex) : null
            };

            var odataEx = new ODataErrorException(message, ex, odataError);

            odataEx.Data["JsonContent"] = odataError.ToString();

            // Let the ErrorController handle ODataErrorException.
            Response.HttpContext.Features.Set<IExceptionHandlerPathFeature>(new ODataExceptionHandlerPathFeature(odataEx, Request));

            var nullableClaims = new[]
            {
                new Claim(JwtClaimTypes.ErrorMessage, message, ClaimValueTypes.String, ClaimsIssuer),
                new Claim(JwtClaimTypes.ForceLogOut, "true", ClaimValueTypes.Boolean, ClaimsIssuer),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(nullableClaims, Scheme.Name));
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }

        private string CreateMessage(AccessDeniedReason reason)
        {
            string msg = null;

            switch (reason)
            {
                case AccessDeniedReason.ApiDisabled:
                    msg = "Web API is disabled.";
                    break;
                case AccessDeniedReason.SslRequired:
                    msg = "Web API requests require SSL.";
                    break;
                case AccessDeniedReason.InvalidAuthorizationHeader:
                    msg = "Missing or invalid authorization header. Must have the format 'Token'.";
                    break;
                case AccessDeniedReason.ExpiredAuthorizationHeader:
                    msg = "The authentication token has expired. Please log in again.";
                    break;
                case AccessDeniedReason.InvalidCredentials:
                    msg = $"The credentials sent for user with server key do not match.";
                    break;
                case AccessDeniedReason.UserUnknown:
                    msg = $"Unknown user. The token does not exist.";
                    break;
                case AccessDeniedReason.UserDisabled:
                    msg = $"API is disabled for you.";
                    break;
            }

            return msg;
        }
    }
}
