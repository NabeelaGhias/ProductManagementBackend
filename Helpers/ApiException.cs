namespace ProductManagementSystem.Helpers
{
    public class ApiException : Exception
    {
        public int StatusCode { get; }
        public string Details { get; }

        public ApiException(int statusCode, string message, string details = "") : base(message)
        {
            StatusCode = statusCode;
            Details = details;
        }
    }
}
