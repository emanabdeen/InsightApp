using InsightApp.Attributes;
using InsightApp.Entities;

namespace InsightApp.Components
{
    public class MemberPreferencesViewModel
    {
        public int MemberId { get; set; }
        public List<GameCategory> AllCategories { get; set; } = new List<GameCategory>();
        public List<LanguageTable> AllLanguages { get; set; } = new List<LanguageTable>();
        public List<GamePlatform> AllPlatforms { get; set; } = new List<GamePlatform>();

        public List<int>? SelectedCategoryIds { get; set; } = new List<int>();
        public List<int>? SelectedLanguageIds { get; set; } = new List<int>();
        public List<int>? SelectedPlatformIds { get; set; }= new List<int>();
    }
}
