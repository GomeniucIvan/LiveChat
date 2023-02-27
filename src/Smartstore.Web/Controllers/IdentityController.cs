using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Smartstore.Core.Identity;
using Smartstore.Web.Api;
using Smartstore.Web.Common.Api;
using Smartstore.Web.Models.Identity;
using Smartstore.Web.Models.System;

namespace Smartstore.Web.Controllers
{
    public class IdentityController : WebApiController
    {
        #region Fields

        private readonly UserManager<Customer> _userManager;
        private readonly SignInManager<Customer> _signInManager;
        private readonly IJwtAuthManager _jwtAuthManager;

        #endregion

        #region Ctor

        public IdentityController(UserManager<Customer> userManager,
            SignInManager<Customer> signInManager,
            IJwtAuthManager jwtAuthManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtAuthManager = jwtAuthManager;
        }

        #endregion

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody]LoginModel model)
        {
            var customer = await _userManager.FindByEmailAsync(model.Email.TrimSafe());

            var result = await _signInManager.PasswordSignInAsync(customer, model.Password, true, lockoutOnFailure: false);

            var resultModel = new GenericApiModel<LoginModel>();

            if (result.Succeeded)
            {
                await Services.EventPublisher.PublishAsync(new CustomerSignedInEvent { Customer = customer });

                var claims = new[]
                {
                    new Claim(JwtClaimTypes.Guid, customer.CustomerGuid.ToString()),
                    new Claim(JwtClaimTypes.CustomerId, customer.Id.ToString()),
                    new Claim(ClaimTypes.Name, model.Email),
                    new Claim(JwtClaimTypes.Password, customer.Password),
                };

                var jwtResult = _jwtAuthManager.GenerateTokens(model.Email, claims, DateTime.Now);
                Logger.LogInformation($"User [{model.Email}] logged in the system.");

                var loginModel = new LoginModel
                {
                    Email = model.Email,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString,
                    IsValid = true
                };

                resultModel.IsValid = true;
                resultModel.Data = loginModel;
                return ApiJson(resultModel, HttpContext);
            }
            else
            {
                resultModel.Message = T("App.Login.WrongCredentials");
                return ApiJson(resultModel, HttpContext);
            }
        }

        [HttpPost("RefreshToken")]
        [Authorize]
        [Produces(Json)]
        public async Task<ActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var userName = User.Identity?.Name;
                Logger.LogInformation($"User [{userName}] is trying to refresh JWT token.");

                if (string.IsNullOrWhiteSpace(request.RefreshToken))
                {
                    return Unauthorized();
                }

                var accessToken = await HttpContext.GetTokenAsync("Bearer", "access_token");
                var jwtResult = _jwtAuthManager.Refresh(request.RefreshToken, accessToken, DateTime.Now);
                Logger.LogInformation($"User [{userName}] has refreshed JWT token.");

                var loginModel = new LoginModel
                {
                    Email = userName,
                    AccessToken = jwtResult.AccessToken,
                    RefreshToken = jwtResult.RefreshToken.TokenString,
                    IsValid = true
                };

                return Ok(loginModel);
            }
            catch (SecurityTokenException e)
            {
                return Unauthorized(e.Message); 
            }
        }
    }
}
