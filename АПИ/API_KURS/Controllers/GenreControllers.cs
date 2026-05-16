using API_KURS.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using MovieLibrary.Models;
using System.Linq;
using Пример_1._1.Contexts;

namespace API_KURS.Controllers
{
    [Route("api/genres")]
    public class GenreController : ApiControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string? search = null)
        {
            using var context = new DatabaseContext();
            if (RequireUser(context, out var failure) == null)
            {
                return failure!;
            }

            var query = context.Genres.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(genre => genre.name.ToLower().Contains(term));
            }

            var result = query
                .OrderBy(genre => genre.name)
                .Select(genre => new LookupItemDto
                {
                    Id = genre.genreId,
                    Name = genre.name
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
                return Error(400, "Название жанра не заполнено");
            }

            var genre = new Genre { name = request.Name.Trim() };
            context.Genres.Add(genre);
            context.SaveChanges();

            return Ok(new LookupItemDto { Id = genre.genreId, Name = genre.name });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] LookupItemDto request)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var genre = context.Genres.FirstOrDefault(item => item.genreId == id);
            if (genre == null)
            {
                return Error(404, "Жанр не найден");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Error(400, "Название жанра не заполнено");
            }

            genre.name = request.Name.Trim();
            context.SaveChanges();

            return Ok(new LookupItemDto { Id = genre.genreId, Name = genre.name });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var genre = context.Genres.FirstOrDefault(item => item.genreId == id);
            if (genre == null)
            {
                return Error(404, "Жанр не найден");
            }

            var links = context.MovieGenres.Where(item => item.GenreId == id).ToList();
            if (links.Count > 0)
            {
                context.MovieGenres.RemoveRange(links);
            }

            context.Genres.Remove(genre);
            context.SaveChanges();
            return Ok();
        }
    }
}
