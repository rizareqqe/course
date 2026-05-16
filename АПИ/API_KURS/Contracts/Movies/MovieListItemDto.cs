using System.Collections.Generic;

namespace API_KURS.Contracts.Movies
{
    public class MovieListItemDto
    {
        public int MovieId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int Year { get; set; }

        public string Description { get; set; } = string.Empty;

        public string DirectorName { get; set; } = "Не назначен";

        public string CuratorName { get; set; } = "Не назначен";

        public List<string> Genres { get; set; } = new List<string>();

        public List<string> Actors { get; set; } = new List<string>();
    }
}
