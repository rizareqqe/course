using System.Collections.Generic;

namespace GuiP31.Models
{
    public class MovieListItemDto
    {
        public int MovieId { get; set; }

        public string Title { get; set; } = string.Empty;

        public int Year { get; set; }

        public string Description { get; set; } = string.Empty;

        public string DirectorName { get; set; } = string.Empty;

        public string CuratorName { get; set; } = string.Empty;

        public List<string> Genres { get; set; } = new List<string>();

        public List<string> Actors { get; set; } = new List<string>();

        public string GenresDisplay => string.Join(", ", Genres);

        public string ActorsDisplay => string.Join(", ", Actors);
    }
}
