using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Smartstore.Core.Data;
using Smartstore.Core.Localization;
using Smartstore.Core.Web;
using Smartstore.Data;
using Smartstore.Data.Caching;
using Smartstore.Data.Hooks;
using Smartstore.Diagnostics;

namespace Smartstore.Core.Identity
{
    public partial class CustomerService : ICustomerService
    {
        #region Raw SQL

        private static bool? _hasForumTables;

        private async Task<bool> HasForumTables()
            => _hasForumTables ??= await _db.DataProvider.HasTableAsync("Forums_Post");

        private async Task<string> GetSqlGuestCustomerIds(string paramClauses)
        {
            string sql;

            if (await HasForumTables())
            {
                sql = $@"SELECT c.Id FROM Customer c
LEFT OUTER JOIN CustomerContent AS cc ON c.Id = cc.CustomerId
WHERE c.Email IS NULL AND c.IsSystemAccount = 0{paramClauses}
AND (NOT EXISTS (SELECT 1 AS x FROM CustomerContent AS cc1 WHERE c.Id = cc1.CustomerId))
ORDER BY c.Id";
            }
            else
            {
                sql = $@"SELECT c.Id FROM Customer c
LEFT OUTER JOIN CustomerContent AS cc ON c.Id = cc.CustomerId
WHERE c.Email IS NULL AND c.IsSystemAccount = 0{paramClauses}
AND (NOT EXISTS (SELECT 1 AS x FROM CustomerContent AS cc1 WHERE c.Id = cc1.CustomerId))
ORDER BY c.Id";
            }

            return _db.DataProvider.ApplyPaging(sql, 0, 20000);
        }

        private static string GetSqlDeleteGenericAttributes(string sqlGuestCustomerIds)
        {
            return $@"DELETE g FROM GenericAttribute g
INNER JOIN (
{sqlGuestCustomerIds}
) sub_c on g.EntityId = sub_c.Id
WHERE KeyGroup = 'Customer'";
        }

        private static string GetSqlDeleteGuestCustomers(string sqlGuestCustomerIds)
        {
            return $@"DELETE c FROM Customer c
INNER JOIN (
{sqlGuestCustomerIds}
) sub_c on c.Id = sub_c.Id";
        }

        #endregion

        private readonly SmartDbContext _db;
        private readonly UserManager<Customer> _userManager;
        private readonly IWebHelper _webHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUserAgent _userAgent;
        private readonly IChronometer _chronometer;

        private Customer _authCustomer;
        private bool _authCustomerResolved;

        public CustomerService(
            SmartDbContext db,
            UserManager<Customer> userManager,
            IWebHelper webHelper,
            IHttpContextAccessor httpContextAccessor,
            IUserAgent userAgent,
            IChronometer chronometer)
        {
            _db = db;
            _userManager = userManager;
            _webHelper = webHelper;
            _httpContextAccessor = httpContextAccessor;
            _userAgent = userAgent;
            _chronometer = chronometer;
        }

        public Localizer T { get; set; } = NullLocalizer.Instance;
        public ILogger Logger { get; set; } = NullLogger.Instance;

        #region Guest customers

        public virtual async Task<Customer> CreateGuestCustomerAsync(Guid? customerGuid = null)
        {
            var customer = new Customer
            {
                CustomerGuid = customerGuid ?? Guid.NewGuid(),
                Active = true,
                CreatedOnUtc = DateTime.UtcNow,
                LastActivityDateUtc = DateTime.UtcNow,
            };

            // Add to 'Guests' role
            var guestRole = await GetRoleBySystemNameAsync(SystemCustomerRoleNames.Guests);
            if (guestRole == null)
            {
                throw new SmartException("'Guests' role could not be loaded");
            }

            using (new DbContextScope(_db, minHookImportance: HookImportance.Essential))
            {
                // Non-essential hooks should NOT react to the insertion of a guest customer record.
                // We want to prevent cache key lock recursion flaws this way: because a hook can trigger
                // actions which - in rare cases, e.g. in a singleton scope - may result in a new guest customer
                // entity inserted to the database, which would now result in calling the source hook again
                // (if it handled the Customer entity).

                // Ensure that entities are saved to db in any case
                customer.CustomerRoleMappings.Add(new CustomerRoleMapping { CustomerId = customer.Id, CustomerRoleId = guestRole.Id });
                _db.Customers.Add(customer);

                await _db.SaveChangesAsync();

                var clientIdent = _webHelper.GetClientIdent();
                if (clientIdent.HasValue())
                {
                    customer.GenericAttributes.ClientIdent = clientIdent;
                    await _db.SaveChangesAsync();
                }
            }

            //Logger.DebugFormat("Guest account created for anonymous visitor. Id: {0}, ClientIdent: {1}", customer.CustomerGuid, clientIdent ?? "n/a");

            return customer;
        }

        public virtual Task<Customer> FindGuestCustomerByClientIdentAsync(string clientIdent = null, int maxAgeSeconds = 60)
        {
            if (_httpContextAccessor.HttpContext == null || _userAgent.IsBot || _userAgent.IsPdfConverter)
            {
                return Task.FromResult<Customer>(null);
            }

            using (_chronometer.Step("FindGuestCustomerByClientIdent"))
            {
                clientIdent = clientIdent.NullEmpty() ?? _webHelper.GetClientIdent();
                if (clientIdent.IsEmpty())
                {
                    return Task.FromResult<Customer>(null);
                }

                var dateFrom = DateTime.UtcNow.AddSeconds(-maxAgeSeconds);

                var query = from a in _db.GenericAttributes.AsNoTracking()
                            join c in _db.Customers on a.EntityId equals c.Id into Customers
                            from c in Customers.DefaultIfEmpty()
                            where c.LastActivityDateUtc >= dateFrom
                                && c.Email == null
                                && a.KeyGroup == "Customer"
                                && a.Key == "ClientIdent"
                                && a.Value == clientIdent
                            select c;

                return query
                    .IncludeCustomerRoles()
                    .FirstOrDefaultAsync();
            }
        }

        public virtual async Task<int> DeleteGuestCustomersAsync(
            DateTime? registrationFrom,
            DateTime? registrationTo,
            bool onlyWithoutShoppingCart,
            CancellationToken cancelToken = default)
        {
            var paramClauses = new StringBuilder(200);
            var parameters = new List<object>();
            var numberOfDeletedCustomers = 0;
            var numberOfDeletedAttributes = 0;
            var pIndex = 0;

            if (registrationFrom.HasValue)
            {
                paramClauses.AppendFormat(" AND @p{0} <= c.CreatedOnUtc", pIndex++);
                parameters.Add(registrationFrom.Value);
            }
            if (registrationTo.HasValue)
            {
                paramClauses.AppendFormat(" AND @p{0} >= c.CreatedOnUtc", pIndex++);
                parameters.Add(registrationTo.Value);
            }

            var sqlGuestCustomerIds = await GetSqlGuestCustomerIds(paramClauses.ToString());
            var sqlGenericAttributes = GetSqlDeleteGenericAttributes(sqlGuestCustomerIds);
            var sqlGuestCustomers = GetSqlDeleteGuestCustomers(sqlGuestCustomerIds);

            // Delete generic attributes.
            while (true)
            {
                var numDeleted = await _db.Database.ExecuteSqlRawAsync(sqlGenericAttributes, parameters.ToArray(), cancelToken);
                if (numDeleted <= 0)
                {
                    break;
                }

                numberOfDeletedAttributes += numDeleted;
            }

            // Delete guest customers.
            while (true)
            {
                var numDeleted = await _db.Database.ExecuteSqlRawAsync(sqlGuestCustomers, parameters.ToArray(), cancelToken);
                if (numDeleted <= 0)
                {
                    break;
                }

                numberOfDeletedCustomers += numDeleted;
            }

            Logger.Debug("Deleted {0} guest customers including {1} generic attributes.", numberOfDeletedCustomers, numberOfDeletedAttributes);

            return numberOfDeletedCustomers;
        }

        #endregion

        #region Customers

        public virtual Customer GetCustomerBySystemName(string systemName, bool tracked = true)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = _db.Customers
                .IncludeCustomerRoles()
                .ApplyTracking(tracked)
                .AsCaching()
                .Where(x => x.SystemName == systemName)
                .OrderBy(x => x.Id);

            return query.FirstOrDefault();
        }

        public virtual Task<Customer> GetCustomerBySystemNameAsync(string systemName, bool tracked = true)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return Task.FromResult((Customer)null);

            var query = _db.Customers
                .IncludeCustomerRoles()
                .ApplyTracking(tracked)
                .AsCaching()
                .Where(x => x.SystemName == systemName)
                .OrderBy(x => x.Id);

            return query.FirstOrDefaultAsync();
        }

        public virtual async Task<Customer> GetAuthenticatedCustomerAsync()
        {
            if (!_authCustomerResolved)
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                {
                    return null;
                }

                var principal = await EnsureAuthentication(httpContext);

                if (principal?.Identity.IsAuthenticated == true)
                {
                    _authCustomer = await _userManager.GetUserAsync(principal);
                }

                _authCustomerResolved = true;
            }

            if (_authCustomer == null || !_authCustomer.Active || _authCustomer.Deleted || !_authCustomer.IsRegistered())
            {
                return null;
            }

            return _authCustomer;
        }

        /// <summary>
        /// Ensures that the authentication handler runs (even before the authentication middleware)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private static async Task<ClaimsPrincipal> EnsureAuthentication(HttpContext context)
        {
            var authenticationFeature = context.Features.Get<IAuthenticationFeature>();
            if (authenticationFeature == null)
            {
                // The middleware did not run yet
                var result = await context.AuthenticateAsync();
                if (result.Succeeded)
                {
                    return result.Principal;
                }
            }

            return context.User;
        }

        #endregion

        #region Roles

        public virtual CustomerRole GetRoleBySystemName(string systemName, bool tracked = true)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return null;

            var query = _db.CustomerRoles
                .ApplyTracking(tracked)
                .AsCaching()
                .Where(x => x.SystemName == systemName)
                .OrderBy(x => x.Id);

            return query.FirstOrDefault();
        }

        public virtual Task<CustomerRole> GetRoleBySystemNameAsync(string systemName, bool tracked = true)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                return Task.FromResult((CustomerRole)null);

            var query = _db.CustomerRoles
                .ApplyTracking(tracked)
                .AsCaching()
                .Where(x => x.SystemName == systemName)
                .OrderBy(x => x.Id);

            return query.FirstOrDefaultAsync();
        }

        #endregion
    }
}
