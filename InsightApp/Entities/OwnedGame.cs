using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InsightApp.Entities
{
    [Table("OwnedGame")]
    public class OwnedGame
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        public int? MemberId { get; set; }

        public int? GameId { get; set; }

        [ForeignKey("GameId")]
        [InverseProperty("OwnedGames")]
        public virtual Game? Game { get; set; }

        [ForeignKey("MemberId")]
        [InverseProperty("OwnedGames")]
        public virtual Member? Member { get; set; }
    }
}
