namespace ChatApplication.Models
{
    public class Message
    {
        public Guid MessageId { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.MinValue;
        public int Type { get; set; } = 1;
        //type = 1 for simple text msg   2 for image 3 for file sharing
        public bool IsDeleted { get; set; }
        public string PathToFileAttachement { get; set; } = string.Empty;
    }
}
