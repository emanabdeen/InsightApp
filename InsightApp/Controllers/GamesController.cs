using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using InsightApp.Entities;
using InsightApp.Models;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace InsightApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GamesController : Controller
    {
        private readonly InsightUpdateCvgs2Context _SVGSDbContext;
        private const string ROLE_ADMIN = "Admin";
        private const string ROLE_MEMBER = "Member";

        public GamesController(InsightUpdateCvgs2Context context)
        {
            _SVGSDbContext = context;
        }

        // GET: Games
        [HttpGet("AdminPanel/Games")]
        [HttpPost("AdminPanel/Games")]
        public async Task<IActionResult> List(GamesListModel gamesListModel)
        {
            if (gamesListModel.SearchText == null)
            {
                //will return only the events that (isDeleted=false)
                var allGames = await _SVGSDbContext.Games
                                        .Include(g => g.GameDetailsCategories)          // Include GameDetailsCategories
                                            .ThenInclude(gdc => gdc.Category)           // Include the related Category entity
                                        .Where(g => g.IsDeleted == false)               // Only include non-deleted games
                                        .Select(g => new GamesCategoriesViewModel
                                        {
                                            GameId = g.GameId,
                                            GameName = g.GameName,
                                            GamePrice = g.Price,
                                            Categories = g.GameDetailsCategories
                                                .Where(gdc => gdc.Category != null)     // Ensure Category is not null
                                                .Select(gdc => gdc.Category.CategoryName)  // Select the CategoryName from the Category entity
                                                .ToList()
                                        })
                                        .OrderBy(g => g.GameName)
                                        .ToListAsync();

                gamesListModel.GamesList = allGames;

            }
            else
            {
				//will return only the games that (isDeleted=false) && Contains SearchText
				// Filter games based on the search text (search by game name or category)
				var allGames = await _SVGSDbContext.Games
                    .Include(g => g.GameDetailsCategories)
                        .ThenInclude(gdc => gdc.Category)
                    .Where(g => g.IsDeleted == false &&
                                (g.GameName.Contains(gamesListModel.SearchText) || // Search by game name
                                 g.GameDetailsCategories.Any(gdc => gdc.Category.CategoryName.Contains(gamesListModel.SearchText)))) // Search by category name
                    .Select(g => new GamesCategoriesViewModel
                    {
                        GameId = g.GameId,
                        GameName = g.GameName,
                        GamePrice = g.Price,
                        Categories = g.GameDetailsCategories
                            .Where(gdc => gdc.Category != null)
                            .Select(gdc => gdc.Category.CategoryName)
                            .ToList()
                    })
                    .OrderBy(g => g.GameName)
                    .ToListAsync();

                gamesListModel.GamesList = allGames;

            }

            return View("List", gamesListModel);
        }

        // GET: Games/Details/5
        [HttpGet("AdminPanel/Games/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                //return NotFound();----------------------------temporary fix
                return View();
            }
            ViewBag.Title = "Game Details";
            //retrieve game details from game Table & include the Category, Language, and Platform tables to retrieve the names
            var game = await _SVGSDbContext.Games
                .Where(g => g.GameId == id)
                .Include(g => g.GameDetailsCategories).ThenInclude(c => c.Category)
                .Include(g => g.GameDetailsLanguages).ThenInclude(l => l.Language)
                .Include(g => g.GameDetailsPlatforms).ThenInclude(p => p.Platform)
                .Include(g => g.Reviews)
                .FirstOrDefaultAsync();

            //retrieve the game average rating from GameAverageRatings View
            var averageRating = await _SVGSDbContext.GameAverageRatings.Where(g => g.GameId == id).Select(g => g.AverageRating).FirstOrDefaultAsync();




            //TO DO get a list of games related to this current product  (Recommended games related)

            //relatedGames=


            //TO DO get a list of games related to this member preferences  (Recommended games you may like)
            //
            //gamesFromPreferences=


            ProductDetailsViewModel productDetailViewModel = new ProductDetailsViewModel()
            {
                ActiveGame = game,
                AverageRating = averageRating
                //RelatedGames = relatedGames,
                //GamesFromPreferences = gamesFromPreferences
            };

            return View("Details", productDetailViewModel);
        }

        // GET: Games/New
        [HttpGet("AdminPanel/Games/add-request")]
        public async Task<IActionResult> AddNewGame()
        {
            // Call EditGame with null to indicate a new game
            ViewBag.ImageExists = "Add";
            return await EditGame(null);
        }

        [HttpGet("AdminPanel/Games/{id}/edit-request")]
        public async Task<IActionResult> ViewEditGame(int id)
        {
            // Call EditGame with null to indicate a new game
            ViewBag.ImageExists = "Replace";
            return await EditGame(id);
        }

        [HttpGet]
        // Common method to handle both adding new game and editing
        private async Task<IActionResult> EditGame(int? id)
        {
            // Create a new view model to pass to the view
            var viewModel = new EditGameViewModel();

            string returnView = "";

            if (id == null)
            {
                // This means we're adding a new game
                viewModel.Game = new Game { IsDeleted = false }; // Default value for IsDeleted
                viewModel.SelectedCategoryIds = new List<int?>();
                viewModel.SelectedLanguageIds = new List<int?>();
                viewModel.SelectedPlatformIds = new List<int?>();
                ViewBag.Title = "Add New Game";
                returnView = "Edit";
            }
            else
            {
                // Edit existing game: Retrieve the game from the database
                viewModel.Game = await _SVGSDbContext.Games
                    .Include(g => g.GameDetailsCategories)
                    .Include(g => g.GameDetailsLanguages)
                    .Include(g => g.GameDetailsPlatforms)
                    .FirstOrDefaultAsync(m => m.GameId == id);

                if (viewModel.Game == null)
                {
                    return NotFound(); // If the game doesn't exist, show a not found page.
                }

                // Set selected IDs based on existing data
                viewModel.SelectedCategoryIds = viewModel.Game.GameDetailsCategories.Select(gdc => gdc.CategoryId).ToList();
                viewModel.SelectedLanguageIds = viewModel.Game.GameDetailsLanguages.Select(gdl => gdl.LanguageId).ToList();
                viewModel.SelectedPlatformIds = viewModel.Game.GameDetailsPlatforms.Select(gdp => gdp.PlatformId).ToList();
                if(ViewBag.Title != "Game Details")
                {
                    ViewBag.Title = "Save Changes";
                    returnView = "Edit";
                }
                else
                {
                    returnView = "Details";
                }
                
            }

            if (returnView == "Edit")
            {
                // Load the checkbox data for categories, languages, and platforms
                viewModel.Categories = await _SVGSDbContext.GameCategories.ToListAsync();
                viewModel.Languages = await _SVGSDbContext.LanguageTables.ToListAsync();
                viewModel.Platforms = await _SVGSDbContext.GamePlatforms.ToListAsync();
            }
            else
            {
                // Load only the selected categories, languages, and platforms based on selected IDs
                viewModel.Categories = await _SVGSDbContext.GameCategories
                                            .Where(c => viewModel.SelectedCategoryIds.Contains(c.CategoryId))
                                            .ToListAsync();

                viewModel.Languages = await _SVGSDbContext.LanguageTables
                                            .Where(l => viewModel.SelectedLanguageIds.Contains(l.LanguageId))
                                            .ToListAsync();

                viewModel.Platforms = await _SVGSDbContext.GamePlatforms
                                            .Where(p => viewModel.SelectedPlatformIds.Contains(p.PlatformId))
                                            .ToListAsync();
            }
            

            return View(returnView, viewModel); // return view is either returning edit view or details view
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveEditGame(EditGameViewModel viewModel, IFormFile? GameImage)
        {
            viewModel.Categories = await _SVGSDbContext.GameCategories.ToListAsync();
            viewModel.Languages = await _SVGSDbContext.LanguageTables.ToListAsync();
            viewModel.Platforms = await _SVGSDbContext.GamePlatforms.ToListAsync();
            if (viewModel.Game.GameName != null) {
                viewModel.Game.GameName = viewModel.Game.GameName.Trim();
            }
            var allGames = await _SVGSDbContext.Games.OrderBy(g => g.GameName).ToListAsync();



            // Manually validate that at least one item is selected for each
            if (viewModel.SelectedCategoryIds == null || !viewModel.SelectedCategoryIds.Any())
            {
                ModelState.AddModelError("SelectedCategoryIds", "Please select at least one category.");
            }
            if (viewModel.SelectedLanguageIds == null || !viewModel.SelectedLanguageIds.Any())
            {
                ModelState.AddModelError("SelectedLanguageIds", "Please select at least one language.");
            }
            if (viewModel.SelectedPlatformIds == null || !viewModel.SelectedPlatformIds.Any())
            {
                ModelState.AddModelError("SelectedPlatformIds", "Please select at least one platform.");
            }
            // Check if the game name already exists in the database
            if (allGames.Any(g => g.GameName.Equals(viewModel.Game.GameName, StringComparison.OrdinalIgnoreCase) && g.GameId != viewModel.Game.GameId))
            {
                ModelState.AddModelError("Game.GameName", "This game name already exists. Please choose a different name.");
            }
            if (GameImage == null || viewModel.Game.GameImageLink == null)
            {
                ViewBag.ImageError = "Please upload an image (.jpeg or .png).";
            }
            if (GameImage == null && viewModel.Game.GameImageLink == null)
            {
                ViewBag.ImageError = "Please upload an image (.jpeg or .png).";
                ModelState.AddModelError("GameImageLink", "Please upload an image (.jpeg or .png).");
            }
            else
            {
                ViewBag.ImageError = "";
            }

            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                // output errors to console of modelstate
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Property: {state.Key}, Error: {error.ErrorMessage}");
                    }
                }

                ViewBag.Title = viewModel.Game.GameId == 0 ? "Add New Game" : "Save Changes";

                return View("Edit", viewModel);
            }

            // Output all form data to diagnose
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"Form Key: {key}, Value: {Request.Form[key]}");
            }

            // Manually bind the radio button value to the Game.PhysicalAvailable property
            if (Request.Form.ContainsKey("Game.PhysicalAvailable"))
            {
                // Parse the value from the form submission
                string physicalAvailableValue = Request.Form["Game.PhysicalAvailable"];
                viewModel.Game.PhysicalAvailable = physicalAvailableValue == "true";
            }

            // Define the target directory path
            string targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Imgs", "Games");

            // Ensure the directory exists
            if (!Directory.Exists(targetDirectory))
            {
                Directory.CreateDirectory(targetDirectory);
            }

            // Check if a file has been uploaded
            if (GameImage != null && GameImage.Length > 0)
            {
                try
                {
                    // Sanitize the game name to remove spaces and special characters
                    string sanitizedGameName = string.Concat(viewModel.Game.GameName.Split(Path.GetInvalidFileNameChars()));

                    // Create the file name using only the sanitized game name and file extension
                    string uniqueFileName = $"{sanitizedGameName}{Path.GetExtension(GameImage.FileName)}";
                    string filePath = Path.Combine(targetDirectory, uniqueFileName);

                    // Save the file to the target directory
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        GameImage.CopyTo(stream);
                    }


                    // Store the relative path to the image in the model (to save in the database)
                    viewModel.Game.GameImageLink = $"~/Imgs/Games/{uniqueFileName}";
                }
                catch (Exception ex)
                {
                    // Log the exception or display an error message
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                
            }

            // Check if this is a new game (GameId is 0 or not set)
            if (viewModel.Game.GameId == 0)
            {
                // Adding a new game
                _SVGSDbContext.Games.Add(viewModel.Game);
                await _SVGSDbContext.SaveChangesAsync();

                // Now add the associated categories, languages, and platforms
                await UpdateGameDetails(viewModel.Game.GameId, viewModel);
            }
            else
            {
                // Update existing game
                var existingGame = await _SVGSDbContext.Games
                    .Include(g => g.GameDetailsCategories)
                    .Include(g => g.GameDetailsLanguages)
                    .Include(g => g.GameDetailsPlatforms)
                    .FirstOrDefaultAsync(g => g.GameId == viewModel.Game.GameId);

                if (existingGame == null)
                {
                    return NotFound();
                }

                // Update the game properties
                existingGame.GameName = viewModel.Game.GameName;
                existingGame.Details = viewModel.Game.Details;
                existingGame.Price = viewModel.Game.Price;
                existingGame.PhysicalAvailable = viewModel.Game.PhysicalAvailable;
                existingGame.IsDeleted = viewModel.Game.IsDeleted;
                if(viewModel.Game.GameImageLink != "")
                {
                    existingGame.GameImageLink = viewModel.Game.GameImageLink;
                }
                

                // Save changes to the game
                _SVGSDbContext.Games.Update(existingGame);
                await _SVGSDbContext.SaveChangesAsync();

                // Update game details (categories, languages, platforms)
                await UpdateGameDetails(existingGame.GameId, viewModel);
            }

            // Redirect to the list or detail page after saving
            return RedirectToAction(nameof(List));
        }

        private async Task UpdateGameDetails(int gameId, EditGameViewModel viewModel)
        {
            // Remove existing entries in GameDetailsCategory, GameDetailsLanguage, and GameDetailsPlatform for this game
            var existingCategories = _SVGSDbContext.GameDetailsCategories.Where(gdc => gdc.GameId == gameId);
            var existingLanguages = _SVGSDbContext.GameDetailsLanguages.Where(gdl => gdl.GameId == gameId);
            var existingPlatforms = _SVGSDbContext.GameDetailsPlatforms.Where(gdp => gdp.GameId == gameId);

            _SVGSDbContext.GameDetailsCategories.RemoveRange(existingCategories);
            _SVGSDbContext.GameDetailsLanguages.RemoveRange(existingLanguages);
            _SVGSDbContext.GameDetailsPlatforms.RemoveRange(existingPlatforms);
            await _SVGSDbContext.SaveChangesAsync();

            // Add new entries based on the selected IDs in the view model
            if (viewModel.SelectedCategoryIds != null)
            {
                foreach (var categoryId in viewModel.SelectedCategoryIds)
                {
                    var gameCategory = new GameDetailsCategory
                    {
                        GameId = gameId,
                        CategoryId = categoryId
                    };
                    _SVGSDbContext.GameDetailsCategories.Add(gameCategory);
                }
            }

            if (viewModel.SelectedLanguageIds != null)
            {
                foreach (var languageId in viewModel.SelectedLanguageIds)
                {
                    var gameLanguage = new GameDetailsLanguage
                    {
                        GameId = gameId,
                        LanguageId = languageId
                    };
                    _SVGSDbContext.GameDetailsLanguages.Add(gameLanguage);
                }
            }

            if (viewModel.SelectedPlatformIds != null)
            {
                foreach (var platformId in viewModel.SelectedPlatformIds)
                {
                    var gamePlatform = new GameDetailsPlatform
                    {
                        GameId = gameId,
                        PlatformId = platformId
                    };
                    _SVGSDbContext.GameDetailsPlatforms.Add(gamePlatform);
                }
            }

            // Save the changes to the database
            await _SVGSDbContext.SaveChangesAsync();
        }

        // POST: soft deleting game
        [HttpPost("AdminPanel/Games/Delete/{id}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            //-------This is the soft delete version------------------------
            // Find the game entity using its ID
            var game = await _SVGSDbContext.Games
                .Include(g => g.GameDetailsCategories)
                .Include(g => g.GameDetailsLanguages)
                .Include(g => g.GameDetailsPlatforms)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            // Define the target directory path for images file
            string targetDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Imgs", "Games");
            string oldFileName = Path.GetFileName(game.GameImageLink.Replace("~", ""));
            string oldFilePath = Path.Combine(targetDirectory, oldFileName);

            // Instead of deleting, mark the game as soft-deleted (IsDeleted = true & rename the game name +image link in the database and the image file  name)
            game.IsDeleted = true;
            game.GameName = "DELETED_" + game.GameId.ToString() + "_" + game.GameName;
            game.GameImageLink = "~/Imgs/Games/DELETED_" + game.GameId.ToString() + "_" + game.GameImageLink.Substring(13);

            // Update the game entity in the database context
            _SVGSDbContext.Games.Update(game);

            // Save changes to the database
            await _SVGSDbContext.SaveChangesAsync();


            // Compute the old and new file paths
            string newFileName = $"{game.GameImageLink.Substring(13)}";
            string newFilePath = Path.Combine(targetDirectory, newFileName);

            // Physically rename the file if it exists
            if (System.IO.File.Exists(oldFilePath))
            {
                try
                {
                    System.IO.File.Move(oldFilePath, newFilePath);
                    // Update the GameImageLink to the new file name
                    game.GameImageLink = $"~/Imgs/Games/{newFileName}";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error renaming file: {ex.Message}");
                }
            }

            // Redirect to the list of games after soft deletion
            return RedirectToAction(nameof(List));
        }

        [HttpGet("AdminPanel/Games/OrdersToFulfill")]
        public async Task<IActionResult> OrdersToFulfill()
        {
            List<OrderTable> ordersToFulfill = await _SVGSDbContext.OrderTables
                .Where(c => c.OrderFulfilled == false).ToListAsync();

            return View("OrdersToFulfill", ordersToFulfill);
        }

        [HttpGet("AdminPanel/Games/order/{id?}")]
        public async Task<IActionResult> OrderPhysicalItems(int id)
        {
            var orderPhysicalItems= await _SVGSDbContext.OrderItems
                .Where(o=> o.OrderId == id && o.IsPhysical==1)
                .Include(o=>o.Game)
                .ToListAsync();
            ViewBag.OrderNumber = id.ToString();

            return View("OrderPhisycalItems", orderPhysicalItems);
        }


        [HttpGet("AdminPanel/Games/FulfillOrder-request")]
        public async Task<IActionResult> FulfillOrder(int id)
        {
            try
            {
                // Retrieve the order by id
                var fulfilledOrder = await _SVGSDbContext.OrderTables
                    .Where(o => o.OrderId == id)
                    .FirstOrDefaultAsync();

                if (fulfilledOrder != null)
                {
                    //update OrderFulfilled attrbute to true
                    fulfilledOrder.OrderFulfilled = true;

                    //update the order at the DB
                    _SVGSDbContext.OrderTables.Update(fulfilledOrder);
                    await _SVGSDbContext.SaveChangesAsync();
                }
                else
                {
                    return RedirectToAction("ErrorPage", "Home");
                }

                TempData["LastActionMessage"] = $"The order is fulfilled";
                return RedirectToAction("OrdersToFulfill", "Games");
            }
            catch (Exception)
            {

                return RedirectToAction("ErrorPage", "Home");
            }
        }


        [HttpGet("AdminPanel/Games/Reviews")]
        public async Task<IActionResult> Reviews()
        {
            try
            {
                List<ReviewViewModel> pendingReviews = await _SVGSDbContext.Reviews
            .Where(r => r.StatusId == 2)
            .Include(r => r.Game)
            .Include(r => r.Member)
            .Include(r => r.Status)
            .Select(r => new ReviewViewModel
            {
                ReviewId = r.ReviewId,
                GameName = r.Game != null ? r.Game.GameName : "Unknown Game",
                ReviewedBy = r.Member != null ? r.Member.DisplayName : "Unknown Reviewer",
                ReviewBody = r.ReviewBody,
                StatusName = r.Status != null ? r.Status.Statusname : "Unknown Status"
            })
            .ToListAsync();


                return View("Reviews", pendingReviews);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        [HttpGet("AdminPanel/Games/Reviews/{id}")]
        public async Task<IActionResult> ViewReview(int id)
        {
            var review = await _SVGSDbContext.Reviews
                .Include(r => r.Game)    // Include Game for GameName
                .Include(r => r.Member) // Include Member for Reviewer
                .Include(r => r.Status) // Include Status for StatusName
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound("Review not found.");
            }

            // Populate ViewBag values
            ViewBag.GameName = review.Game?.GameName ?? "Unknown Game";
            ViewBag.Reviewer = review.Member?.DisplayName ?? "Unknown Reviewer";

            return View("ViewReview", review);
        }

        [HttpPost("Reviews/Approve/{id}")]
        public IActionResult ApproveReview(int id)
        {
            var review = _SVGSDbContext.Reviews.Find(id);
            if (review == null)
            {
                TempData["ErrorMessage"] = "Review not found.";
                return RedirectToAction("Reviews"); // Redirect to the reviews list page
            }

            review.StatusId = 1; // Set status to "Approved"
            _SVGSDbContext.SaveChanges();

            TempData["SuccessMessage"] = "Review approved successfully.";
            return RedirectToAction("Reviews"); // Redirect back to the reviews list page
        }

        [HttpPost("Reviews/Decline")]
        public IActionResult DeclineReview(int reviewId, string rejectReason)
        {
            if (string.IsNullOrWhiteSpace(rejectReason))
            {
                TempData["ErrorMessage"] = "A reason for declining is required.";
                return RedirectToAction("ViewReview", new { id = reviewId }); // Redirect back to the current review
            }

            var existingReview = _SVGSDbContext.Reviews.Find(reviewId);
            if (existingReview == null)
            {
                TempData["ErrorMessage"] = "Review not found.";
                TempData["ShowRejectForm"] = true;
                return RedirectToAction("Reviews");
            }

            existingReview.StatusId = 3; // Set status to "Declined"
            existingReview.RejectReason = rejectReason; // Save the reason for declining
            _SVGSDbContext.SaveChanges();

            TempData["SuccessMessage"] = "Review declined successfully.";
            return RedirectToAction("Reviews"); // Redirect back to the reviews list page
        }


        private bool GameExists(int id)
        {
            return _SVGSDbContext.Games.Any(e => e.GameId == id);
        }

      
        //---------------------------------------Hard deleting a game from database---------------------------------
        //[HttpGet("AdminPanel/Games/Delete/{id}")]
        //public async Task<IActionResult> HardDeleteConfirmed(int id)
        //{
        //    //----This is the hard delete version
        //    // Find the game entity using its ID
        //    var game = await _SVGSDbContext.Games
        //        .Include(g => g.GameDetailsCategories)
        //        .Include(g => g.GameDetailsLanguages)
        //        .Include(g => g.GameDetailsPlatforms)
        //        .FirstOrDefaultAsync(g => g.GameId == id);

        //    if (game == null)
        //    {
        //        return NotFound();
        //    }

        //    // Remove related GameDetailsCategory entries
        //    if (game.GameDetailsCategories != null && game.GameDetailsCategories.Any())
        //    {
        //        _SVGSDbContext.GameDetailsCategories.RemoveRange(game.GameDetailsCategories);
        //    }

        //    // Remove related GameDetailsLanguage entries
        //    if (game.GameDetailsLanguages != null && game.GameDetailsLanguages.Any())
        //    {
        //        _SVGSDbContext.GameDetailsLanguages.RemoveRange(game.GameDetailsLanguages);
        //    }

        //    // Remove related GameDetailsPlatform entries
        //    if (game.GameDetailsPlatforms != null && game.GameDetailsPlatforms.Any())
        //    {
        //        _SVGSDbContext.GameDetailsPlatforms.RemoveRange(game.GameDetailsPlatforms);
        //    }

        //    // Finally, remove the game entity itself
        //    _SVGSDbContext.Games.Remove(game);

        //    // Save changes to the database
        //    await _SVGSDbContext.SaveChangesAsync();

        //    // Redirect to the list of games after deletion
        //    return RedirectToAction(nameof(List));

        //}
    }
}
