﻿using Microsoft.AspNetCore.SignalR;
using Smartstore.Core.Companies.Domain;
using Smartstore.Core.Companies.Dtos;
using Smartstore.Core.Companies.Proc;
using Smartstore.Core.Data;
using Smartstore.Web.Api;
using Smartstore.Web.Models.Conversation;
using Smartstore.Web.Models.System;

namespace Smartstore.Web.Controllers
{
    public class ConversationController : WebApiController
    {
        #region Fields

        private readonly SmartDbContext _db;
        private readonly IHubContext<ChatHub> _hubContext;

        #endregion

        #region Ctor

        public ConversationController(SmartDbContext db, 
            IHubContext<ChatHub> hubContext)
        {
            _db = db;
            _hubContext = hubContext;
        }

        #endregion

        #region Methods

        [HttpPost("Messages")]
        public async Task<IActionResult> Messages()
        {
            IList<CompanyMessageDto> messages = _db.CompanyMessage_GetList(companyId: CompanyId).ToList();

            return ApiJson(new GenericApiModel<IList<CompanyMessageDto>>().Success(messages.ToArray()), HttpContext);
        }

        [HttpPost("VisitorMessages")]
        public async Task<IActionResult> VisitorMessages()
        {
            IList<CompanyMessageDto> messages = _db.CompanyMessage_GetVisitorList(companyId: CompanyId,
                visitorId: VisitorId,
                companyCustomerId: CustomerId).ToList();

            return ApiJson(new GenericApiModel<IList<CompanyMessageDto>>().Success(messages.ToArray()), HttpContext);
        }

        [HttpPost("SendMessage")]
        public async Task<IActionResult> SendMessage([FromBody]MessageModel model)
        {
            var resultModel = new GenericApiModel<CompanyMessageDto>();
            if (model != null)
            {
                var messageDto = new CompanyMessageDto()
                {
                    Message = model.Message,
                    VisitorId = VisitorId,
                    CompanyCustomerId = CustomerId,
                    CompanyId = CompanyId
                };
                //todo add private
                var companyMessageId = _db.CompanyMessage_Insert(messageDto, messageTypeId: MessageTypeEnum.Customer);

                if (companyMessageId.HasValue)
                {
                    messageDto.Id = companyMessageId.GetValueOrDefault();
                    await _hubContext.Clients.All.SendAsync($"visitor_{messageDto.CompanyId}_{messageDto.VisitorId}_new_message", messageDto);
                    await _hubContext.Clients.All.SendAsync($"company_{messageDto.CompanyId}_new_message", messageDto);
                    //todo generic proc response, created int
                    return ApiJson(resultModel.Success(messageDto), HttpContext);
                }
            }

            resultModel.IsValid = false;
            return ApiJson(resultModel.Error(), HttpContext);
        }

        [HttpPost("Typing")]
        public async Task<IActionResult> Typing([FromBody] TypingModel model)
        {
            await _hubContext.Clients.All.SendAsync($"company_{CompanyId}_{model.VisitorId}_typing");
            return ApiJson(null, HttpContext);
        }

        #endregion
    }
}
