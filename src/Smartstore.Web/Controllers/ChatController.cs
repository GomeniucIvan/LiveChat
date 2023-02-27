using Microsoft.AspNetCore.OData.Routing.Attributes;
using Microsoft.AspNetCore.SignalR;
using Smartstore.Core.Companies.Domain;
using Smartstore.Core.Companies.Dtos;
using Smartstore.Core.Companies.Proc;
using Smartstore.Core.Data;
using Smartstore.Web.Api;
using Smartstore.Web.Models.Laucher;
using Smartstore.Web.Models.System;

namespace Smartstore.Web.Controllers
{
    [ODataRouteComponent("odata/v1/chat")]
    [Route("odata/v1/chat")]
    public class ChatController : WebApiController
    {
        #region Fields

        private readonly SmartDbContext _db;
        private readonly IHubContext<ChatHub> _hubContext;

        #endregion

        #region Ctor

        public ChatController(SmartDbContext db, 
            IHubContext<ChatHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        #endregion

        #region Methods

        [HttpPost("VisitorData")]
        public async Task<IActionResult> InitVisitor()
        {
            var company = _db.Company_GetDetails(companyId: null, companyKey: CompanyKey);
            if (company == null)
            {
                return ApiJson(new GenericApiModel<VisitorInitModel>().Error("Company not found."), HttpContext);
            }

            var model = new VisitorInitModel()
            {
                CompanyId = company.Id,
                VisitorId = VisitorId
            };

            return ApiJson(new GenericApiModel<VisitorInitModel>().Success(model), HttpContext);
        }

        [HttpPost("LauncherMessages")]
        public async Task<IActionResult> Messages()
        {
            IList<CompanyMessageDto> messages = _db.CompanyMessage_GetVisitorList(companyId: CompanyId,
                visitorId: VisitorId,
                companyCustomerId: null,
                visitorCall: true).ToList();

            return ApiJson(new GenericApiModel<IList<CompanyMessageDto>>().Success(messages.ToArray()), HttpContext);
        }

        [HttpPost("SendText")]
        public async Task<IActionResult> SendText([FromBody]LauncherMessageModel model)
        {
            var resultModel = new GenericApiModel<CompanyMessageDto>();
            if (model != null)
            {
                var messageDto = new CompanyMessageDto()
                {
                    Message = model.Message,
                    VisitorId = VisitorId,
                    CompanyCustomerId = null,
                    CompanyId = CompanyId,
                };

                var companyMessageId = _db.CompanyMessage_Insert(messageDto, messageTypeId: MessageTypeEnum.Visitor);
                if (companyMessageId.HasValue)
                {
                    messageDto.Id = companyMessageId.GetValueOrDefault();
                    await _hubContext.Clients.All.SendAsync($"company_{messageDto.CompanyId}_new_message", messageDto);
                }

                return ApiJson(resultModel.Success(messageDto), HttpContext);
            }

            resultModel.IsValid = false;
            return ApiJson(resultModel.Error(), HttpContext);
        } 

        #endregion
    }
}
