using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Model.Entity
{
    [Table("logs")]
    public class Log
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        [Column("source")]
        public string Source { get; set; } = null!;

        [Required, MaxLength(20)]
        [Column("logtype")]
        public string LogType { get; set; } = null!;

        [Required]
        [Column("message")]
        public string Message { get; set; } = null!;


        [Column("timestamp")]
        public DateTime Timestamp { get; set; }


        [Column("created")]
        public DateTime Created { get; set; }
    }
}
