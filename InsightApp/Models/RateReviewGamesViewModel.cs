using InsightApp.Entities;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace InsightApp.Models
{
    public class RateReviewGamesViewModel
    {
        public Review GameReview { get; set; }

        [ValidateNever]
        public GameRating? GameRating { get; set; }

        [ValidateNever]
        public Game? Game { get; set; }
    }
}
