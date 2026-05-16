using System.Collections.Generic;

namespace API_KURS.Contracts.Movies
{
    public class MovieEditRequest
    {
        public string Title { get; set; } = string.Empty;

        public int Year { get; set; }

        public string Description { get; set; } = string.Empty;

        public int? DirectorId { get; set; }

        public int? CuratorUserId { get; set; }

        public List<int> GenreIds { get; set; } = new List<int>();

        public List<int> ActorIds { get; set; } = new List<int>();
    }
}
