namespace Smartstore.Web.Controllers
{
    public class TestController : PublicController
    {
        [HttpGet("/test/Chat")]
        public ActionResult Chat()
        {
            return View();
        }
    }
}
