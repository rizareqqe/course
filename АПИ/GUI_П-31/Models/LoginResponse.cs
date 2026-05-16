namespace GuiP31.Models
{
    public class LoginResponse
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
