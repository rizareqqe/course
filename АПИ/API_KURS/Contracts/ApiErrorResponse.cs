namespace API_KURS.Contracts
{
    public class ApiErrorResponse
    {
        public int StatusCode { get; set; }

        public string Message { get; set; } = string.Empty;

        public string? Details { get; set; }
    }
}
