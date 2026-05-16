using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieLibrary.Models
{
    [Table("movies")]
    public class Movie
    {
        [Key]
        [Column("movie_id")]
        public int movieId { get; set; }

        [Column("title")]
        [MaxLength(200)]
        [Required]
        public string title { get; set; } = string.Empty;

        [Column("year")]
        [Required]
        public int year { get; set; }

        [Column("description")]
        public string description { get; set; } = string.Empty;

        [Column("director_id")]
        public int? directorId { get; set; }

        public virtual Director? Director { get; set; }

        [Column("curator_user_id")]
        public int? CuratorUserId { get; set; }

        public virtual UserAccount? CuratorUser { get; set; }
        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
