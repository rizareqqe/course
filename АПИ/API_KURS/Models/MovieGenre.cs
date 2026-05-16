using System.ComponentModel.DataAnnotations.Schema;

namespace MovieLibrary.Models
{
    [Table("movies_genres")]
    public class MovieGenre
    {
        [Column("movie_id")]
        public int MovieId { get; set; }

        [Column("genre_id")]
        public int GenreId { get; set; }

        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("GenreId")]
        public virtual Genre Genre { get; set; } = null!;
    }
}
