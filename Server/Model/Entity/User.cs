using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Model.Entity
{
    [Table("users")]
    public class User : EntityBase
    {
        [Required, MaxLength(50)]
        [Column("username")]
        public string UserName { get; set; } = null!;

        [Required, MaxLength(200)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required, MaxLength(100)]
        [Column("password")]
        public string Password { get; set; } = null!;
    }

    [Table("usersexp")]
    public class UserExp : EntityBaseExp
    {
        [Required, MaxLength(50)]
        [Column("username")]
        public string UserName { get; set; } = null!;

        [Required, MaxLength(200)]
        [EmailAddress]
        [Column("email")]
        public string Email { get; set; } = null!;

        [Required, MaxLength(100)]
        [Column("password")]
        public string Password { get; set; } = null!;
    }
}
