namespace API_KURS.Contracts.Users
{
    public class UserEditRequest
    {
        public string FullName { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
