using System.ComponentModel.DataAnnotations.Schema;

namespace MovieLibrary.Models
{
    [Table("movie_actors")]
    public class MovieActor
    {
        [Column("movie_id")]
        public int MovieId { get; set; }

        [Column("actor_id")]
        public int ActorId { get; set; }

        [ForeignKey("MovieId")]
        public virtual Movie Movie { get; set; } = null!;

        [ForeignKey("ActorId")]
        public virtual Actor Actor { get; set; } = null!;
    }
}
