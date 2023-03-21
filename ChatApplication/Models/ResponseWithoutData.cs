namespace ChatApplication.Models
{
    public class ResponseWithoutData
    {
        public int StatusCode { get; set; } = 200;
        public string Message { get; set; } = "Ok";
        public bool Success { get; set; } = true;

        public ResponseWithoutData() { }
        public ResponseWithoutData(int status,string message,bool success)
        {
            this.StatusCode = status;
            this.Message = message;
            this.Success = success;
        }
    }
}
