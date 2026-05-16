using API_KURS.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using MovieLibrary.Models;
using System.Linq;
using Пример_1._1.Contexts;

namespace API_KURS.Controllers
{
    [Route("api/directors")]
    public class DirectorsController : ApiControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string? search = null)
        {
            using var context = new DatabaseContext();
            if (RequireUser(context, out var failure) == null)
            {
                return failure!;
            }

            var query = context.Directors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(director => director.Name.ToLower().Contains(term));
            }

            var result = query
                .OrderBy(director => director.Name)
                .Select(director => new LookupItemDto
                {
                    Id = director.DirectorId,
                    Name = director.Name
                })
                .ToList();

            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] LookupItemDto request)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Error(400, "Имя режиссера не заполнено");
            }

            var director = new Director { Name = request.Name.Trim() };
            context.Directors.Add(director);
            context.SaveChanges();

            return Ok(new LookupItemDto { Id = director.DirectorId, Name = director.Name });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] LookupItemDto request)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var director = context.Directors.FirstOrDefault(item => item.DirectorId == id);
            if (director == null)
            {
                return Error(404, "Режиссер не найден");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Error(400, "Имя режиссера не заполнено");
            }

            director.Name = request.Name.Trim();
            context.SaveChanges();

            return Ok(new LookupItemDto { Id = director.DirectorId, Name = director.Name });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var director = context.Directors.FirstOrDefault(item => item.DirectorId == id);
            if (director == null)
            {
                return Error(404, "Режиссер не найден");
            }

            var movies = context.Movies.Where(movie => movie.directorId == id).ToList();
            foreach (var movie in movies)
            {
                movie.directorId = null;
            }

            context.Directors.Remove(director);
            context.SaveChanges();
            return Ok();
        }
    }
}
