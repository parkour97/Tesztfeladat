using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Model.Entity
{
    [Table("device")]
    public class Device : EntityBase
    {
        [Required, MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        [Column("ipaddress")]
        public string? IPAddress { get; set; }

        [Required]
        [Column("connected")]
        public bool Connected { get; set; }

        [Column("measurementcount")]
        public int? MeasurementCount { get; set; }
    }

    [Table("deviceexp")]
    public class DeviceExp : EntityBaseExp
    {
        [Required, MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [MaxLength(20)]
        [Column("ipaddress")]
        public string? IPAddress { get; set; }

        [Required]
        [Column("connected")]
        public bool Connected { get; set; }

        [Column("measurementcount")]
        public int? MeasurementCount { get; set; }
    }
}
