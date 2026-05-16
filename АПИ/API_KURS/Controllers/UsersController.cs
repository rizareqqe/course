using API_KURS.Contracts.Users;
using Microsoft.AspNetCore.Mvc;
using MovieLibrary.Models;
using System.Linq;
using Пример_1._1.Contexts;

namespace API_KURS.Controllers
{
    [Route("api/users")]
    public class UsersController : ApiControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var users = context.UserAccounts
                .OrderBy(item => item.FullName)
                .Select(item => new UserListItemDto
                {
                    UserId = item.UserId,
                    FullName = item.FullName,
                    Login = item.Login,
                    Role = item.Role,
                    IsActive = item.IsActive
                })
                .ToList();

            return Ok(users);
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserEditRequest request)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var validation = Validate(request, context, null);
            if (validation != null)
            {
                return validation;
            }

            var user = new UserAccount();
            Apply(user, request);
            context.UserAccounts.Add(user);
            context.SaveChanges();

            return Ok();
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] UserEditRequest request)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var user = context.UserAccounts.FirstOrDefault(item => item.UserId == id);
            if (user == null)
            {
                return Error(404, "Пользователь не найден");
            }

            var validation = Validate(request, context, id);
            if (validation != null)
            {
                return validation;
            }

            Apply(user, request);
            context.SaveChanges();
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var user = context.UserAccounts.FirstOrDefault(item => item.UserId == id);
            if (user == null)
            {
                return Error(404, "Пользователь не найден");
            }

            var assignedMovies = context.Movies.Where(item => item.CuratorUserId == id).ToList();
            foreach (var movie in assignedMovies)
            {
                movie.CuratorUserId = null;
            }

            context.UserAccounts.Remove(user);
            context.SaveChanges();
            return Ok();
        }

        private IActionResult? Validate(UserEditRequest request, DatabaseContext context, int? excludeId)
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
            {
                return Error(400, "ФИО пользователя не заполнено");
            }

            if (string.IsNullOrWhiteSpace(request.Login))
            {
                return Error(400, "Логин пользователя не заполнен");
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Error(400, "Пароль пользователя не заполнен");
            }

            if (request.Role != "Administrator" && request.Role != "CatalogEditor")
            {
                return Error(400, "Роль должна быть Administrator или CatalogEditor");
            }

            var login = request.Login.Trim();
            if (context.UserAccounts.Any(item => item.Login == login && (!excludeId.HasValue || item.UserId != excludeId.Value)))
            {
                return Error(400, "Пользователь с таким логином уже существует");
            }

            return null;
        }

        private static void Apply(UserAccount user, UserEditRequest request)
        {
            user.FullName = request.FullName.Trim();
            user.Login = request.Login.Trim();
            user.Password = request.Password;
            user.Role = request.Role;
            user.IsActive = request.IsActive;
        }
    }
}
