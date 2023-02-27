using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Routing.Attributes;
using NuGet.Protocol;
using Smartstore.Core.Installation;
using Smartstore.Threading;
using Smartstore.Web.Api;
using Smartstore.Web.Models.System;

namespace Smartstore.Web.Controllers
{
    [ODataRouteComponent("odata/v1/")]
    [Route("odata/v1/")]
    public class InstallController : SmartController
    {
        private readonly IInstallationService _installService;
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IApplicationContext _appContext;
        private readonly IAsyncState _asyncState;

        public InstallController(
            IHostApplicationLifetime hostApplicationLifetime,
            IApplicationContext appContext,
            IAsyncState asyncState)
        {
            _installService = EngineContext.Current.Scope.ResolveOptional<IInstallationService>();
            _hostApplicationLifetime = hostApplicationLifetime;
            _appContext = appContext;
            _asyncState = asyncState;
        }

        private string T(string resourceName)
        {
            return _installService.GetResource(resourceName);
        }

        [HttpGet("Install")]
        public async Task<IActionResult> Install()
        {
            var model = new GenericApiModel<InstallationModel>();
            model.IsValid = true;

            if (_appContext.IsInstalled)
            {
                model.Data = new InstallationModel()
                {
                    IsInstalled = _appContext.IsInstalled
                };

                return Ok(model.ToJson());
            }

            model.Data = new InstallationModel()
            {
                //AdminEmail = T("AdminEmailValue")
            };

            return Ok(model.ToJson());
        }

        [HttpPost("Install")]
        public async Task<JsonResult> Install([FromBody]InstallationModel model)
        {
            if (!ModelState.IsValid)
            {
                var result = new InstallationResult();
                ModelState.SelectMany(x => x.Value.Errors).Each(x => result.Errors.Add(x.ErrorMessage));
                return new JsonResult(result);
            }
            else
            {
                var result = await _installService.InstallAsync(model, HttpContext.RequestServices.AsLifetimeScope());
                return new JsonResult(result);
            }
        }

        [HttpPost("Progress")]
        public async Task<JsonResult> Progress()
        {
            var progress = await _asyncState.GetAsync<InstallationResult>();
            return new JsonResult(progress);
        }

        [HttpPost("Finalize")]
        public async Task<IActionResult> Finalize(bool restart)
        {
            await _asyncState.RemoveAsync<InstallationResult>();

            if (restart)
            {
                _hostApplicationLifetime.StopApplication();
            }

            return new JsonResult(new { Success = true });
        }
    }
}
