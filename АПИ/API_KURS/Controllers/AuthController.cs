using API_KURS.Contracts.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Пример_1._1.Contexts;

namespace API_KURS.Controllers
{
    [Route("api/auth")]
    public class AuthController : ApiControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) || string.IsNullOrWhiteSpace(request.Password))
            {
                return Error(400, "Логин и пароль обязательны");
            }

            using var context = new DatabaseContext();
            var user = context.UserAccounts.FirstOrDefault(item =>
                item.Login == request.Login.Trim() &&
                item.Password == request.Password &&
                item.IsActive);

            if (user == null)
            {
                return Error(401, "Неверный логин или пароль");
            }

            return Ok(new LoginResponse
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Login = user.Login,
                Role = user.Role
            });
        }
    }
}
