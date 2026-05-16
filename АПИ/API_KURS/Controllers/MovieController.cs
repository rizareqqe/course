using API_KURS.Contracts.Movies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MovieLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Пример_1._1.Contexts;
using API_KURS.Services;
using API_KURS.Contracts.Common;

namespace API_KURS.Controllers
{
    [Route("api/movies")]
    public class MovieController : ApiControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] MovieFilterQuery query)
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var movies = BuildMovieQuery(context, user!, query).ToList();
            return Ok(movies);
        }

        [HttpGet("filter")]
        public IActionResult Filter([FromQuery] MovieFilterQuery query)
        {
            return Get(query);
        }

        [HttpGet("{id}/edit")]
        public IActionResult GetEditModel(int id)
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var movie = context.Movies
                .Include(item => item.MovieGenres)
                .Include(item => item.MovieActors)
                .FirstOrDefault(item => item.movieId == id);

            if (movie == null)
            {
                return Error(404, "Фильм не найден");
            }

            if (user!.Role != "Administrator" && movie.CuratorUserId != user.UserId)
            {
                return Error(403, "Недостаточно прав", "Можно открывать только фильмы, назначенные текущему пользователю.");
            }

            return Ok(new MovieEditDto
            {
                MovieId = movie.movieId,
                Title = movie.title,
                Year = movie.year,
                Description = movie.description,
                DirectorId = movie.directorId,
                CuratorUserId = movie.CuratorUserId,
                GenreIds = movie.MovieGenres.Select(item => item.GenreId).ToList(),
                ActorIds = movie.MovieActors.Select(item => item.ActorId).ToList()
            });
        }

        [HttpGet("statistics")]
        public IActionResult Statistics([FromQuery] MovieFilterQuery query)
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var filteredMovieIds = BuildMovieEntityQuery(context, user!, query);

            var movies = filteredMovieIds
                .Include(movie => movie.Director)
                .Include(movie => movie.MovieGenres)
                    .ThenInclude(link => link.Genre)
                .ToList();

            var stats = new MovieStatisticsDto
            {
                TotalMovies = movies.Count,
                AverageYear = movies.Count == 0 ? 0 : Math.Round(movies.Average(movie => movie.year), 2),
                ByDirector = movies
                    .GroupBy(movie => movie.Director != null ? movie.Director.Name : "Без режиссера")
                    .Select(group => new StatisticsRowDto { Name = group.Key, Count = group.Count() })
                    .OrderByDescending(item => item.Count)
                    .ThenBy(item => item.Name)
                    .ToList(),
                ByGenre = movies
                    .SelectMany(movie => movie.MovieGenres.Select(link => link.Genre?.name ?? "Без жанра"))
                    .GroupBy(name => name)
                    .Select(group => new StatisticsRowDto { Name = group.Key, Count = group.Count() })
                    .OrderByDescending(item => item.Count)
                    .ThenBy(item => item.Name)
                    .ToList()
            };

            return Ok(stats);
        }

        [HttpPost]
        public IActionResult Post([FromBody] MovieEditRequest request)
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var validation = ValidateMovieRequest(request, context, user!);
            if (validation != null)
            {
                return validation;
            }

            var movie = new Movie();
            ApplyMovieChanges(movie, request, context, user!);

            context.Movies.Add(movie);
            context.SaveChanges();

            SyncMovieLinks(movie.movieId, request, context);
            context.SaveChanges();

            return Ok(new { movieId = movie.movieId });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] MovieEditRequest request)
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var movie = context.Movies.FirstOrDefault(item => item.movieId == id);
            if (movie == null)
            {
                return Error(404, "Фильм не найден");
            }

            if (user!.Role != "Administrator" && movie.CuratorUserId != user.UserId)
            {
                return Error(403, "Недостаточно прав", "Редактор может изменять только свои фильмы.");
            }

            var validation = ValidateMovieRequest(request, context, user);
            if (validation != null)
            {
                return validation;
            }

            ApplyMovieChanges(movie, request, context, user);
            context.SaveChanges();

            SyncMovieLinks(id, request, context);
            context.SaveChanges();

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var movie = context.Movies.FirstOrDefault(item => item.movieId == id);
            if (movie == null)
            {
                return Error(404, "Фильм не найден");
            }

            if (user!.Role != "Administrator" && movie.CuratorUserId != user.UserId)
            {
                return Error(403, "Недостаточно прав", "Редактор может удалять только свои фильмы.");
            }

            context.MovieGenres.RemoveRange(context.MovieGenres.Where(item => item.MovieId == id));
            context.MovieActors.RemoveRange(context.MovieActors.Where(item => item.MovieId == id));
            context.Movies.Remove(movie);
            context.SaveChanges();

            return Ok();
        }

        [HttpGet("lookups")]
        public IActionResult Lookups()
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var users = context.UserAccounts
                .Where(item => item.IsActive && (user!.Role == "Administrator" || item.UserId == user.UserId))
                .OrderBy(item => item.FullName)
                .Select(item => new LookupItemDto { Id = item.UserId, Name = item.FullName })
                .ToList();

            return Ok(new
            {
                Directors = context.Directors.OrderBy(item => item.Name).Select(item => new LookupItemDto { Id = item.DirectorId, Name = item.Name }).ToList(),
                Genres = context.Genres.OrderBy(item => item.name).Select(item => new LookupItemDto { Id = item.genreId, Name = item.name }).ToList(),
                Actors = context.Actors.OrderBy(item => item.name).Select(item => new LookupItemDto { Id = item.ActorId, Name = item.name }).ToList(),
                Curators = users
            });
        }

        private static IQueryable<Movie> BuildMovieEntityQuery(DatabaseContext context, UserAccount user, MovieFilterQuery filter)
        {
            var query = context.Movies
                .Include(movie => movie.Director)
                .Include(movie => movie.CuratorUser)
                .Include(movie => movie.MovieGenres)
                    .ThenInclude(link => link.Genre)
                .Include(movie => movie.MovieActors)
                    .ThenInclude(link => link.Actor)
                .AsQueryable();

            if (user.Role != "Administrator")
            {
                query = query.Where(movie => movie.CuratorUserId == user.UserId);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var term = filter.Search.Trim().ToLower();
                query = query.Where(movie =>
                    movie.title.ToLower().Contains(term) ||
                    movie.description.ToLower().Contains(term) ||
                    movie.year.ToString().Contains(term) ||
                    (movie.Director != null && movie.Director.Name.ToLower().Contains(term)) ||
                    (movie.CuratorUser != null && movie.CuratorUser.FullName.ToLower().Contains(term)) ||
                    movie.MovieGenres.Any(link => link.Genre != null && link.Genre.name.ToLower().Contains(term)) ||
                    movie.MovieActors.Any(link => link.Actor != null && link.Actor.name.ToLower().Contains(term)));
            }

            if (filter.YearFrom.HasValue)
            {
                query = query.Where(movie => movie.year >= filter.YearFrom.Value);
            }

            if (filter.YearTo.HasValue)
            {
                query = query.Where(movie => movie.year <= filter.YearTo.Value);
            }

            if (filter.DirectorId.HasValue)
            {
                query = query.Where(movie => movie.directorId == filter.DirectorId.Value);
            }

            if (filter.GenreId.HasValue)
            {
                query = query.Where(movie => movie.MovieGenres.Any(link => link.GenreId == filter.GenreId.Value));
            }

            return query;
        }

        private static IEnumerable<MovieListItemDto> BuildMovieQuery(DatabaseContext context, UserAccount user, MovieFilterQuery filter)
        {
            var query = BuildMovieEntityQuery(context, user, filter).AsEnumerable();
            query = MovieSortingService.ApplySort(query, filter);

            return query.Select(movie => new MovieListItemDto
            {
                MovieId = movie.movieId,
                Title = movie.title,
                Year = movie.year,
                Description = movie.description,
                DirectorName = movie.Director?.Name ?? "Не назначен",
                CuratorName = movie.CuratorUser?.FullName ?? "Не назначен",
                Genres = movie.MovieGenres.Select(link => link.Genre?.name ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().OrderBy(name => name).ToList(),
                Actors = movie.MovieActors.Select(link => link.Actor?.name ?? string.Empty).Where(name => !string.IsNullOrWhiteSpace(name)).Distinct().OrderBy(name => name).ToList()
            });
        }

        private IActionResult? ValidateMovieRequest(MovieEditRequest request, DatabaseContext context, UserAccount currentUser)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return Error(400, "Название фильма не заполнено");
            }

            if (request.Year < 1900 || request.Year > DateTime.Now.Year + 1)
            {
                return Error(400, "Год фильма должен быть в диапазоне от 1900 до следующего календарного года");
            }

            if (request.DirectorId.HasValue && !context.Directors.Any(item => item.DirectorId == request.DirectorId.Value))
            {
                return Error(400, "Указан несуществующий режиссер");
            }

            if (request.GenreIds.Any() && context.Genres.Count(item => request.GenreIds.Contains(item.genreId)) != request.GenreIds.Distinct().Count())
            {
                return Error(400, "Один или несколько выбранных жанров не существуют");
            }

            if (request.ActorIds.Any() && context.Actors.Count(item => request.ActorIds.Contains(item.ActorId)) != request.ActorIds.Distinct().Count())
            {
                return Error(400, "Один или несколько выбранных актеров не существуют");
            }

            if (currentUser.Role == "Administrator")
            {
                if (request.CuratorUserId.HasValue && !context.UserAccounts.Any(item => item.UserId == request.CuratorUserId.Value && item.IsActive))
                {
                    return Error(400, "Выбран несуществующий куратор");
                }

                return null;
            }

            if (request.CuratorUserId.HasValue && request.CuratorUserId.Value != currentUser.UserId)
            {
                return Error(403, "Редактор не может назначать фильм другому пользователю");
            }

            return null;
        }

        private static void ApplyMovieChanges(Movie movie, MovieEditRequest request, DatabaseContext context, UserAccount currentUser)
        {
            movie.title = request.Title.Trim();
            movie.year = request.Year;
            movie.description = (request.Description ?? string.Empty).Trim();
            movie.directorId = request.DirectorId;
            movie.CuratorUserId = currentUser.Role == "Administrator"
                ? request.CuratorUserId
                : currentUser.UserId;
        }

        private static void SyncMovieLinks(int movieId, MovieEditRequest request, DatabaseContext context)
        {
            var existingGenres = context.MovieGenres.Where(item => item.MovieId == movieId).ToList();
            var existingActors = context.MovieActors.Where(item => item.MovieId == movieId).ToList();

            context.MovieGenres.RemoveRange(existingGenres);
            context.MovieActors.RemoveRange(existingActors);

            var uniqueGenreIds = request.GenreIds.Distinct().ToList();
            var uniqueActorIds = request.ActorIds.Distinct().ToList();

            context.MovieGenres.AddRange(uniqueGenreIds.Select(genreId => new MovieGenre
            {
                MovieId = movieId,
                GenreId = genreId
            }));

            context.MovieActors.AddRange(uniqueActorIds.Select(actorId => new MovieActor
            {
                MovieId = movieId,
                ActorId = actorId
            }));
        }
    }
}
