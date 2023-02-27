using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData;
using Microsoft.OData.UriParser;

namespace Smartstore.Web.Api
{
    /// <summary>
    /// Smart base controller class for OData endpoints.
    /// </summary>
    /// <remarks>
    /// - ActionResult<T> vs. IActionResult: IActionResult is used when multiple return types are possible.
    /// For ActionResult<T> ProducesResponseTypeAttribute's type property can be excluded.
    /// - Explicit "From" parameter bindings are required otherwise Swagger will describe them as "query" params by default.
    /// </remarks>
    /// <summary>
    /// Smart base controller class for OData endpoints.
    /// </summary>
    [Authorize(AuthenticationSchemes = "Api"), IgnoreAntiforgeryToken]
    public abstract class SmartODataController : ODataController
    {
        private SmartDbContext _db;

        protected SmartDbContext Db
        {
            get => _db ??= HttpContext.RequestServices.GetService<SmartDbContext>();
        }

        #region Utilities

        /// <summary>
        /// Gets related keys from an OData Uri.
        /// </summary>
        /// <returns>Dictionary with key property names and values.</returns>
        protected IReadOnlyDictionary<string, object> GetRelatedKeys(Uri uri)
        {
            Guard.NotNull(uri, nameof(uri));

            var feature = HttpContext.ODataFeature();
            //var serviceRoot = new Uri(new Uri(feature.BaseAddress), feature.RoutePrefix);
            var serviceRoot = new Uri(feature.BaseAddress);
            var parser = new ODataUriParser(feature.Model, serviceRoot, uri, feature.Services);

            parser.Resolver ??= new UnqualifiedODataUriResolver { EnableCaseInsensitive = true };
            //parser.UrlKeyDelimiter = ODataUrlKeyDelimiter.Slash;

            var path = parser.ParsePath();
            var segment = path.OfType<KeySegment>().FirstOrDefault();

            if (segment is null)
            {
                return new Dictionary<string, object>(capacity: 0);
            }

            return new Dictionary<string, object>(segment.Keys, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Creates an absolute OData entity URL.
        /// Typically used for <see cref="CreatedODataResult{TEntity}"/> to create the location response header.
        /// </summary>
        /// <param name="id">Entity identifier.</param>
        /// <returns>Absolute OData URL.</returns>
        /// <example>https://www.my-store/odata/v1/Addresses(85382)</example>
        protected string BuildUrl(int id)
        {
            Guard.NotZero(id);

            var routePrefix = Request.ODataFeature().RoutePrefix;
            var controller = Request.RouteValues.GetControllerName();
            var url = UriHelper.BuildAbsolute(Request.Scheme, Request.Host, Request.PathBase);

            return $"{url.EnsureEndsWith('/')}{routePrefix.EnsureEndsWith('/')}{controller}({id})";
        }

        protected ODataErrorResult ErrorResult(
            Exception ex = null,
            string message = null,
            int statusCode = StatusCodes.Status422UnprocessableEntity)
        {
            if (ex != null && ex is ODataErrorException oex)
            {
                return ODataErrorResult(oex.Error);
            }

            return ODataErrorResult(new()
            {
                ErrorCode = statusCode.ToString(),
                Message = message ?? ex.Message,
                InnerError = ex != null ? new ODataInnerError(ex) : null
            });
        }

        protected NotFoundODataResult NotFound(int id, string entityName = null)
            => NotFound($"Cannot find {entityName} entity with identifier {id}.");

        /// <summary>
        /// Returns <see cref="ODataErrorResult"/> with status <see cref="StatusCodes.Status403Forbidden"/>
        /// and the message that the current operation is not allowed on this endpoint.
        /// </summary>
        /// <param name="extraInfo">Extra info to append to the message.</param>
        protected ODataErrorResult Forbidden(string extraInfo = null)
            => ErrorResult(null, $"{Request.Method} on {Request.Path} is not allowed.".Grow(extraInfo, " "), StatusCodes.Status403Forbidden);

        #endregion
    }
}
