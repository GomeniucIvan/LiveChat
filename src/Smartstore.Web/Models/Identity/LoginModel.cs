namespace Smartstore.Web.Models.Identity
{
    public class LoginModel : ModelBase
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool IsValid { get; set; }
    }
}
