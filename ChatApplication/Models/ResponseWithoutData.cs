namespace ChatApplication.Models
{
    public class ResponseWithoutData
    {
        public int StatusCode { get; set; } = 200;
        public string Message { get; set; } = "Ok";
        public bool Success { get; set; } = true;
    }
}
