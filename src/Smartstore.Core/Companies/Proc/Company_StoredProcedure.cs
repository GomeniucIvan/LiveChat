using Smartstore.Core.Companies.Domain;
using Smartstore.Core.Companies.Dtos;
using Smartstore.Core.Data;
using Smartstore.Core.Data.Bootstrapping;

namespace Smartstore.Core.Companies.Proc;

public static class Customer_StoredProcedure
{
    public static CompanyDto Company_GetDetails(this SmartDbContext db,
        int? companyId,
        string companyKey = "")
    {
        var pCompanyIdDbParameter = db.DataProvider.CreateIntParameter("CompanyId", companyId);
        var pCompanyKeyDbParameter = db.DataProvider.CreateParameter("CompanyKey", companyKey);

        return db.ExecStoreProcedure<CompanyDto>($"{nameof(Company)}_GetDetails",
            pCompanyIdDbParameter,
            pCompanyKeyDbParameter).FirstOrDefault();
    }
}