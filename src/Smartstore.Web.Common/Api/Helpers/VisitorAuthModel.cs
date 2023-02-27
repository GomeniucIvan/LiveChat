using Newtonsoft.Json;

namespace Smartstore.Web.Api.Helpers
{
    public class VisitorAuthModel
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("visitorId")]
        public string VisitorId { get; set; }
    }
}
