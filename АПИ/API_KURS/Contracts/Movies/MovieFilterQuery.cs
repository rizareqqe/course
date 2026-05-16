namespace API_KURS.Contracts.Movies
{
    public class MovieFilterQuery
    {
        public string? Search { get; set; }

        public int? YearFrom { get; set; }

        public int? YearTo { get; set; }

        public int? DirectorId { get; set; }

        public int? GenreId { get; set; }

        public string? PrimarySort { get; set; }

        public bool PrimaryDescending { get; set; }

        public string? SecondarySort { get; set; }

        public bool SecondaryDescending { get; set; }
    }
}
