using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieLibrary.Models
{
    [Table("genres")]
    public class Genre
    {
        [Key]
        [Column("genre_id")]
        public int genreId { get; set; }

        [Column("name")]
        [MaxLength(50)]
        [Required]
        public string name { get; set; } = string.Empty;

        public virtual ICollection<MovieGenre> MovieGenres { get; set; } = new List<MovieGenre>();
    }
}
