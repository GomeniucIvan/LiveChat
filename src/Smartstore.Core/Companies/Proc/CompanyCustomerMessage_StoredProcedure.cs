using Smartstore.Core.Companies.Domain;
using Smartstore.Core.Companies.Dtos;
using Smartstore.Core.Data;
using Smartstore.Core.Data.Bootstrapping;

namespace Smartstore.Core.Companies.Proc
{
    public static class CompanyMessage_StoredProcedure
    {
        public static IList<CompanyMessageDto> CompanyMessage_GetList(this SmartDbContext db,
            int companyId,
            int pageSize = 50)
        {
            var pCompanyIdDbParameter = db.DataProvider.CreateIntParameter("CompanyId", companyId);
            var pPageSizeDbParameter = db.DataProvider.CreateIntParameter("PageSize", pageSize);

            return db.ExecStoreProcedure<CompanyMessageDto>($"{nameof(CompanyMessage)}_GetList",
                pCompanyIdDbParameter,
                pPageSizeDbParameter).ToList();
        }


        public static IList<CompanyMessageDto> CompanyMessage_GetVisitorList(this SmartDbContext db,
            int companyId,
            int? visitorId,
            int? companyCustomerId,
            bool visitorCall = false)
        {
            var pCompanyIdDbParameter = db.DataProvider.CreateIntParameter("CompanyId", companyId);
            var pVisitorIdDbParameter = db.DataProvider.CreateIntParameter("VisitorId", visitorId);
            var pCompanyCustomerIdDbParameter = db.DataProvider.CreateIntParameter("CompanyCustomerId", companyCustomerId);
            var pVisitorCallDbParameter = db.DataProvider.CreateBooleanParameter("VisitorCall", visitorCall);

            return db.ExecStoreProcedure<CompanyMessageDto>($"{nameof(CompanyMessage)}_GetVisitorList",
                pCompanyIdDbParameter,
                pVisitorIdDbParameter,
                pCompanyCustomerIdDbParameter,
                pVisitorCallDbParameter).ToList();
        }

        public static int? CompanyMessage_Insert(this SmartDbContext db,
            CompanyMessageDto model,
            MessageTypeEnum messageType)
        {
            var pCompanyIdDbParameter = db.DataProvider.CreateIntParameter("CompanyId", model.CompanyId);
            var pCompanyCustomerIdDbParameter = db.DataProvider.CreateIntParameter("CompanyCustomerId", model.CompanyCustomerId);
            var pVisitorIdDbParameter = db.DataProvider.CreateIntParameter("VisitorId", model.VisitorId);
            var pMessageDbParameter = db.DataProvider.CreateParameter("Message", model.Message);
            var pMessageTypeIdDbParameter = db.DataProvider.CreateIntParameter("MessageTypeId", (int)messageType);

            return db.ExecStoreProcedure<int?>($"{nameof(CompanyMessage)}_Insert",
                pCompanyIdDbParameter,
                pCompanyCustomerIdDbParameter,
                pVisitorIdDbParameter,
                pMessageDbParameter,
                pMessageTypeIdDbParameter).FirstOrDefault();
        }
    }
}
