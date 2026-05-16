using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Пример_1._1.Contexts;

namespace API_KURS.Controllers
{
    [Route("api/movie-genres")]
    public class MovieGenresController : ApiControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] int? movieId = null)
        {
            using var context = new DatabaseContext();
            if (RequireUser(context, out var failure) == null)
            {
                return failure!;
            }

            var query = context.MovieGenres.AsQueryable();
            if (movieId.HasValue)
            {
                query = query.Where(item => item.MovieId == movieId.Value);
            }

            var result = query
                .OrderBy(item => item.MovieId)
                .ThenBy(item => item.GenreId)
                .Select(item => new { item.MovieId, item.GenreId })
                .ToList();

            return Ok(result);
        }
    }
}
