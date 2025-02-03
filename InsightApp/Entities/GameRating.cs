using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace InsightApp.Entities
{
    public class GameRating
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        public int? MemberId { get; set; }

        public int? GameId { get; set; }

        public double? RateValue { get; set; }

        [ForeignKey("GameId")]
        [InverseProperty("GameRatings")]
        public virtual Game? Game { get; set; }

        [ForeignKey("MemberId")]
        [InverseProperty("GameRatings")]
        public virtual Member? Member { get; set; }
    }
}
