using Smartstore.Core.Companies.Domain;

namespace Smartstore.Core.Companies.Dtos
{
    public class CompanyMessageDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Data => Message;
        public int? VisitorId { get; set; }
        public int? CompanyCustomerId { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public bool IsVisitorMessage => MessageType == MessageTypeEnum.Visitor;

        public string IconUrl => "https://placeimg.com/30/30/face";
        public string VisitorFullName { get; set; }
        public string VisitorDescription { get; set; }
        public string CustomerFullName { get; set; }
        public MessageTypeEnum MessageType { get; set; }

        public string FullName
        {
            get
            {
                if (MessageType == MessageTypeEnum.Visitor)
                {
                    return VisitorFullName;
                }
                else
                {
                    return CustomerFullName;
                }
            }
        }

        public string Description
        {
            get
            {
                if (MessageType == MessageTypeEnum.Visitor)
                {
                    return VisitorDescription;
                }
                else
                {
                    return string.Empty;
                }
            }
        }
    }
}
