using InsightApp.Attributes;
using InsightApp.Entities;
using System.ComponentModel.DataAnnotations;

namespace InsightApp.Models
{
    public class EditGameViewModel
    {
        public Game Game { get; set; } // Main game entity

        //computed properties to check whether game is available or not
        public bool IsPhysicalAvailableYesChecked => Game?.PhysicalAvailable == true; 
        public bool IsPhysicalAvailableNoChecked => Game?.PhysicalAvailable == false;

        public List<GameCategory> Categories { get; set; } = new List<GameCategory>();
        public List<LanguageTable> Languages { get; set; } = new List<LanguageTable>();
        public List<GamePlatform> Platforms { get; set; } = new List<GamePlatform>();


        // Lists to hold the selected IDs from checkboxes
        [MustHaveOneItem(ErrorMessage = "Please select at least one category.")]
        public List<int?> SelectedCategoryIds { get; set; }

        [MustHaveOneItem(ErrorMessage = "Please select at least one language.")]
        public List<int?> SelectedLanguageIds { get; set; }

        [MustHaveOneItem(ErrorMessage = "Please select at least one platform.")]
        public List<int?> SelectedPlatformIds { get; set; }


        // Constructor to ensure collections are initialized
        public EditGameViewModel()
        {
            SelectedCategoryIds = new List<int?>();
            SelectedLanguageIds = new List<int?>();
            SelectedPlatformIds = new List<int?>();
        }
    }
}
