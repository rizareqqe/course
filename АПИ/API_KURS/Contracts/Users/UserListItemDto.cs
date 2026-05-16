namespace API_KURS.Contracts.Users
{
    public class UserListItemDto
    {
        public int UserId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Login { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}
