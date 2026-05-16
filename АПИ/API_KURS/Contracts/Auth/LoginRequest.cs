namespace API_KURS.Contracts.Auth
{
    public class LoginRequest
    {
        public string Login { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;
    }
}
