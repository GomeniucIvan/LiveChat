namespace Smartstore.Web.Models.Conversation
{
    public class MessageModel
    {
        public string Message { get; set; }
        public int VisitorId { get; set; }
    }

    public class TypingModel
    {
        public int VisitorId { get; set; }
    }
}
