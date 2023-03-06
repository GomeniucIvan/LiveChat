using Smartstore.Core.Companies.Domain;

namespace Smartstore.Core.Companies.Dtos
{
    public class VisitorResponseDto
    {
        public int NewMessagesCount { get; set; }
        public IList<CompanyMessageDto> Messages { get; set; } = new List<CompanyMessageDto>();
    }
    public class CompanyMessageDto
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string MessageShort => Message.TruncateHtml(maxLength: 45);
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
        public string VisitorUniqueId { get; set; }
        public DateTime? ReadOnUtc { get; set; }


        public string CustomerInitials
        {
            get
            {
                var array = FullName.Split(' ');
                if (array.Length >= 2 && !array[0].IsEmpty() && !array[1].IsEmpty())
                {
                    return $"{array[0].Substring(0,1)}{array[1].Substring(0,1)}";
                }

                return $"{array[0].Substring(0,1)}";
            }
        }
        public string FullName
        {
            get
            {
                var fullName = VisitorFullName;
                if (fullName.IsEmpty())
                {
                    fullName = $"Visitor {VisitorUniqueId}";
                }

                return fullName;
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
