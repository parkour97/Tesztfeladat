using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Model.Entity
{
    [Table("deviceparam")]
    public class DeviceParam : EntityBase
    {
        [Required, MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("deviceid")]
        public int DeviceId { get; set; }

        [Required]
        [Column("value")]
        public int Value { get; set; }

        [MaxLength(200)]
        [Column("modifier")]
        public string? Modifier { get; set; }
    }

    [Table("deviceparamexp")]
    public class DeviceParamExp : EntityBaseExp
    {
        [Required, MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = null!;

        [Required]
        [Column("deviceid")]
        public int DeviceId { get; set; }

        [Required]
        [Column("value")]
        public int Value { get; set; }

        [MaxLength(200)]
        [Column("modifier")]
        public string? Modifier { get; set; }
    }
}
