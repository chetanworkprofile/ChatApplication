using System.ComponentModel.DataAnnotations;

namespace ChatApplication.Models
{
    public class OutputChatMappings
    {
        public Guid ChatId { get; set; }
        public string FirstEmail { get; set; } = string.Empty;
        public string SecondEmail { get; set; } = string.Empty;
        public DateTime DateTime { get; set; } = DateTime.Now;
        //public bool IsDeleted { get; set; }
    }
}
