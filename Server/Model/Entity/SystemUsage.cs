using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Model.Entity
{
    [Table("systemusage")]
    public class SystemUsage
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("deviceid")]
        public int DeviceId { get; set; }

        [Required, MaxLength(50)]
        [Column("measurementname")]
        public string MeasurementName { get; set; } = null!;

        [Required]
        [Column("usage")]
        public float Usage { get; set; }


        [Column("created")]
        public DateTime Created { get; set; }
    }

    [Table("systemusageexp")]
    public class SystemUsageExp : EntityBaseExp
    {
        [Required]
        [Column("deviceid")]
        public int DeviceId { get; set; }

        [Required, MaxLength(50)]
        [Column("measurementname")]
        public string MeasurementName { get; set; } = null!;

        [Required]
        [Column("usage")]
        public float Usage { get; set; }
    }
}
