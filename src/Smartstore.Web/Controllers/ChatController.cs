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

        [HttpPost("Data")]
        public async Task<IActionResult> Data()
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

        [HttpPost("Messages")]
        public async Task<IActionResult> Messages()
        {
            IList<CompanyMessageDto> messages = _db.CompanyMessage_GetVisitorList(companyId: CompanyId,
                visitorId: VisitorId,
                companyCustomerId: null,
                visitorCall: true,
                newMessagesCount: out int newMessagesCount).ToList();

            var response = new VisitorResponseDto()
            {
                Messages = messages,
                NewMessagesCount = newMessagesCount
            };

            return ApiJson(new GenericApiModel<VisitorResponseDto>().Success(response), HttpContext);
        }

        [HttpPost("MarkReadMessages")]
        public async Task<IActionResult> MarkReadMessages()
        {
            int messagesCount = _db.CompanyMessage_MarkRead(companyId: CompanyId,
                visitorId: VisitorId.GetValueOrDefault(),
                companyCustomerId: null,
                visitorCall: true);

            return ApiJson(new GenericApiModel<bool>().Success(true), HttpContext);
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody]LauncherMessageModel model)
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

                var companyMessageId = _db.CompanyMessage_Insert(messageDto, messageType: MessageTypeEnum.Visitor);
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

        [HttpPost("Typing")]
        public async Task<IActionResult> Typing()
        {
            await _hubContext.Clients.All.SendAsync($"visitor_{CompanyId}_{VisitorId}_typing");
            return ApiJson(null, HttpContext);
        }

        #endregion
    }
}
