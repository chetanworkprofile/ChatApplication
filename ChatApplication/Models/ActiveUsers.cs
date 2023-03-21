namespace ChatApplication.Models
{
    public class ActiveUsers
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        //public DateTime DateTime { get; set; } = DateTime.MinValue;
    }
}

//output model
// to return list of active/ online users