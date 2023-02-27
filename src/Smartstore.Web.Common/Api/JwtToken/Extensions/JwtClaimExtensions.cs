using System.Security.Claims;

namespace Smartstore.Web.Infrastructure.JwtToken.Extensions
{
    public static class JwtClaimExtensions
    {
        public static string GetStringClaimValue(this IEnumerable<Claim> claims, string claimName)
        {
            if (claims == null)
            {
                return "";
            }

            var claimValue = claims.FirstOrDefault(v => v.Type == claimName)?.Value;
            return claimValue;
        }
    }
}
