using System.Collections.Generic;

namespace API_KURS.Contracts.Movies
{
    public class MovieStatisticsDto
    {
        public int TotalMovies { get; set; }

        public double AverageYear { get; set; }

        public List<StatisticsRowDto> ByDirector { get; set; } = new List<StatisticsRowDto>();

        public List<StatisticsRowDto> ByGenre { get; set; } = new List<StatisticsRowDto>();
    }

    public class StatisticsRowDto
    {
        public string Name { get; set; } = string.Empty;

        public int Count { get; set; }
    }
}
