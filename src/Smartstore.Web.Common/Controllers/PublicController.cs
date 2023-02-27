namespace Smartstore.Web.Controllers
{
    //[Authorize]
    //[AuthorizeAccess]
    //[TrackActivity(Order = 100)]
    [SaveChanges(typeof(SmartDbContext), Order = int.MaxValue)]
    public class PublicController : SmartController
    {
    }
}
