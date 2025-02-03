using InsightApp.Entities;

namespace InsightApp.Models
{
    public class GamesListModel
    {
        public List<GamesCategoriesViewModel>? GamesList { get; set; }


        public string SearchText { get; set; }
    }
}
