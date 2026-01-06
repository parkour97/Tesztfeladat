using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Model.Entity
{
    public class EntityBase
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("created")]
        public DateTime Created { get; set; }


        [Column("modified")]
        public DateTime? Modified { get; set; }

    }

    public class EntityBaseExp
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("original")]
        public int Original { get; set; }

        [Column("expired")]
        public DateTime Expired { get; set; }


        [Column("deleted")]
        public bool Deleted { get; set; }

    }
}
