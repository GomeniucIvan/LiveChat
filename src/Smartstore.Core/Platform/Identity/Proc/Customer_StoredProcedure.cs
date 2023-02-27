using Smartstore.Core.Data;
using Smartstore.Core.Data.Bootstrapping;
using Smartstore.Core.Platform.Identity.Dtos;

namespace Smartstore.Core.Identity.Proc;

public static class Customer_StoredProcedure
{
    public static CustomerApiDto Customer_ApiDetails(this SmartDbContext db,
        string email,
         bool updateLastLoginDate = false)
    {
        var pEmailDbParameter = db.DataProvider.CreateParameter("Email", email);
        var pUpdateLastLoginDateDbParameter = db.DataProvider.CreateParameter("UpdateLastLoginDate", updateLastLoginDate);

        return db.ExecStoreProcedure<CustomerApiDto>($"{nameof(Customer)}_ApiDetails",
            pEmailDbParameter,
            pUpdateLastLoginDateDbParameter).FirstOrDefault();
    }

    public static CustomerWebContextDto Customer_WebContext(this SmartDbContext db,
        int customerId)
    {
        var pCustomerIdDbParameter = db.DataProvider.CreateIntParameter("CustomerId", customerId);

        return db.ExecStoreProcedure<CustomerWebContextDto>($"{nameof(Customer)}_WebContext",
            pCustomerIdDbParameter).FirstOrDefault();
    }
}