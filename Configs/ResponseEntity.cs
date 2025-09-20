namespace ProductManagementSystem.Configs
{
    public class ResponseEntity
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public ResponseEntity()
        {
            Success = true;
            StatusCode = 200;
            Message = "Success";
        }

        public ResponseEntity(object data) : this()
        {
            Data = data;
        }

        public ResponseEntity(int statusCode, string message, object? data = null)
        {
            Success = statusCode >= 200 && statusCode < 300;
            StatusCode = statusCode;
            Message = message;
            Data = data;
        }
    }
}
