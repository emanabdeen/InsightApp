using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InsightApp.Entities
{
    [Table("Cart")]
    public class Cart
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        public int? MemberId { get; set; }

        public int? GameId { get; set; }

        public int IsPhysical { get; set; }

        [ForeignKey("GameId")]
        [InverseProperty("Carts")]
        public virtual Game? Game { get; set; }

        [ForeignKey("MemberId")]
        [InverseProperty("Carts")]
        public virtual Member? Member { get; set; }
    }
}
