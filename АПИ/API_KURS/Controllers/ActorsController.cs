using API_KURS.Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using MovieLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using Пример_1._1.Contexts;

namespace API_KURS.Controllers
{
    [Route("api/actors")]
    public class ActorsController : ApiControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromQuery] string? search = null)
        {
            using var context = new DatabaseContext();
            var user = RequireUser(context, out var failure);
            if (failure != null)
            {
                return failure;
            }

            var query = context.Actors.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(actor => actor.name.ToLower().Contains(term));
            }

            var actors = query
                .OrderBy(actor => actor.name)
                .Select(actor => new LookupItemDto
                {
                    Id = actor.ActorId,
                    Name = actor.name
                })
                .ToList();

            return Ok(actors);
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
                return Error(400, "Имя актера не заполнено");
            }

            var actor = new Actor { name = request.Name.Trim() };
            context.Actors.Add(actor);
            context.SaveChanges();

            return Ok(new LookupItemDto { Id = actor.ActorId, Name = actor.name });
        }

        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] LookupItemDto request)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var actor = context.Actors.FirstOrDefault(item => item.ActorId == id);
            if (actor == null)
            {
                return Error(404, "Актер не найден");
            }

            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return Error(400, "Имя актера не заполнено");
            }

            actor.name = request.Name.Trim();
            context.SaveChanges();

            return Ok(new LookupItemDto { Id = actor.ActorId, Name = actor.name });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            using var context = new DatabaseContext();
            if (RequireAdministrator(context, out var failure) == null)
            {
                return failure!;
            }

            var actor = context.Actors.FirstOrDefault(item => item.ActorId == id);
            if (actor == null)
            {
                return Error(404, "Актер не найден");
            }

            var links = context.MovieActors.Where(item => item.ActorId == id).ToList();
            if (links.Count > 0)
            {
                context.MovieActors.RemoveRange(links);
            }

            context.Actors.Remove(actor);
            context.SaveChanges();
            return Ok();
        }
    }
}
