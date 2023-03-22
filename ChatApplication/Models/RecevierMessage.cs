namespace ChatApplication.Models
{
    public class RecevierMessage
    {
        //public Guid MessageId { get; set; }
        //public string SenderEmail { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderPicPath { get; set; } = string.Empty;
        public string ReceiverEmail { get; set; } = string.Empty;
        public string ReceiverPicPath { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Type { get; set; } = 1;

        public DateTime DateTime { get; set; } = DateTime.MinValue;
        //public bool IsDeleted { get; set; }
        public string PathToFileAttachement { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}


//output model 
// used while sending messages 