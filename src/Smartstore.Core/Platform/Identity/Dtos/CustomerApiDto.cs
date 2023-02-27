using Smartstore.Extensions;

namespace Smartstore.Core.Platform.Identity.Dtos
{
    public class CustomerApiDto
    {
        public int Id { get; set; }
        public Guid CustomerGuid { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool Active { get; set; }
        public string CompanyIdsString { get; set; }
        public IList<int> CompanyIds => CompanyIdsString.ToListOfInt();
    }
}
