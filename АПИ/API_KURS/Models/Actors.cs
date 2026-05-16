using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieLibrary.Models
{
    [Table("actors")]
    public class Actor
    {
        [Key]
        [Column("actor_id")]
        public int ActorId { get; set; }

        [Column("name")]
        [MaxLength(100)]
        [Required]
        public string name { get; set; } = string.Empty;

        public virtual ICollection<MovieActor> MovieActors { get; set; } = new List<MovieActor>();
    }
}
