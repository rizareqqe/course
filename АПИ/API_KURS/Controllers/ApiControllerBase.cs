using API_KURS.Contracts;
using Microsoft.AspNetCore.Mvc;
using MovieLibrary.Models;
using System.Linq;
using Пример_1._1.Contexts;

namespace API_KURS.Controllers
{
    [ApiController]
    public abstract class ApiControllerBase : ControllerBase
    {
        protected IActionResult Error(int statusCode, string message, string? details = null)
        {
            return StatusCode(statusCode, new ApiErrorResponse
            {
                StatusCode = statusCode,
                Message = message,
                Details = details
            });
        }

        protected UserAccount? GetAuthorizedUser(DatabaseContext context)
        {
            if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader))
            {
                return null;
            }

            if (!int.TryParse(userIdHeader.FirstOrDefault(), out var userId))
            {
                return null;
            }

            var role = Request.Headers["X-User-Role"].FirstOrDefault();
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            return context.UserAccounts.FirstOrDefault(user =>
                user.UserId == userId &&
                user.IsActive &&
                user.Role == role);
        }

        protected UserAccount? RequireUser(DatabaseContext context, out IActionResult? failure)
        {
            var user = GetAuthorizedUser(context);
            if (user == null)
            {
                failure = Error(401, "Требуется авторизация", "Выполните вход в систему и повторите запрос.");
                return null;
            }

            failure = null;
            return user;
        }

        protected UserAccount? RequireAdministrator(DatabaseContext context, out IActionResult? failure)
        {
            var user = RequireUser(context, out failure);
            if (failure != null)
            {
                return null;
            }

            if (user!.Role != "Administrator")
            {
                failure = Error(403, "Недостаточно прав", "Операция доступна только администратору.");
                return null;
            }

            return user;
        }
    }
}
