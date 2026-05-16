using GuiP31.Models;

namespace GuiP31.Services
{
    internal static class SessionContext
    {
        public static LoginResponse? CurrentUser { get; private set; }

        public static bool IsAdministrator => CurrentUser?.Role == "Administrator";

        public static void SetUser(LoginResponse user)
        {
            CurrentUser = user;
        }

        public static void Clear()
        {
            CurrentUser = null;
        }
    }
}
