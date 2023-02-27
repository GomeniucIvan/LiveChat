using System.Text.Json.Serialization;

namespace Smartstore.Web.Common.Api
{
    public class RefreshTokenRequest
    {
        [JsonPropertyName("refreshToken")]
        public string RefreshToken { get; set; }
    }
}
