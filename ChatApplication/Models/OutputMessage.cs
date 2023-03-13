namespace ChatApplication.Models
{
    public class OutputMessage
    {
        public Guid MessageId { get; set; }
        public string SenderEmail { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.MinValue;
        //public bool IsDeleted { get; set; }
        //public string PathToFileAttachement { get; set; }
    }
}
