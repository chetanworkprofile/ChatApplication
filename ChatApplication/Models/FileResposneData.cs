namespace ChatApplication.Models
{
    public class FileResponseData
    {
        public ResponseUser? User { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string PathToPic { get; set; } = string.Empty;
    }
}


//output model
//to give response after profile pic upload