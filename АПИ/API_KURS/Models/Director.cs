using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieLibrary.Models
{
    [Table("directors")]
    public class Director
    {
        [Key]
        [Column("director_id")]
        public int DirectorId { get; set; }

        [Column("name")]
        [MaxLength(100)]
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
