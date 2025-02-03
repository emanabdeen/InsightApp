using InsightApp.Components;
using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace InsightApp.Controllers
{
    [Authorize(Roles = "Member")]
    public class ProductController : Controller
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
        private readonly SignInManager<Account> _signInManager;
        private readonly UserManager<Account> _userManager;
        public ProductController(InsightUpdateCvgs2Context sVGSDbContext, UserManager<Account> userManager)
        {
            _SVGSDbContext = sVGSDbContext;
            _userManager = userManager;
        }

        [HttpGet("Portal/product-details/{id?}")]
        public async Task<IActionResult> details(int id)
        {
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

            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = User.Identity.IsAuthenticated ? _userManager.GetUserId(User) : "";
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            //retrieve the preferences according to the memberId
            var languagePrefIds = await _SVGSDbContext.MemberLanguagePrefs
                .Where(l => l.MemberId == memberId).Select(l => l.LanguageId).ToListAsync();//list of language preferences ids 
            var categoryPrefIds = await _SVGSDbContext.MemberGameCategoryPrefs
                .Where(c => c.MemberId == memberId).Select(c => c.CategoryId).ToListAsync();//list of category preferences ids 
            var platformPrefIds = await _SVGSDbContext.MemberPlatformPrefs
                .Where(p => p.MemberId == memberId).Select(p => p.PlatformId).ToListAsync();//list of platform preferences ids 



            // get a list of games related to this current product  (Recommended games related)      

            var relatedGames = await _SVGSDbContext.Games
            .Where(g => g.GameId != game.GameId && g.IsDeleted == false) // Exclude current game and deleted games
            .Include(g => g.GameDetailsCategories).ThenInclude(gc => gc.Category)
            .Include(g => g.GameDetailsPlatforms).ThenInclude(gp => gp.Platform)
            .Include(g => g.GameDetailsLanguages).ThenInclude(gl => gl.Language)
            .Where(g => g.GameDetailsCategories.Any(gc => game.GameDetailsCategories.Select(c => c.CategoryId).Contains(gc.CategoryId))) //match category
            .ToListAsync();


            var sharedGameCategories = new Dictionary<int, string>();

            Console.WriteLine("Active Game Categories:");
            foreach (var relatedGame in relatedGames)
            {
                // Find the first shared category
                var sharedCategory = relatedGame.GameDetailsCategories
                    .Where(gc => game.GameDetailsCategories.Any(agc => agc.CategoryId == gc.CategoryId))
                    .Select(gc => gc.Category.CategoryName)
                    .FirstOrDefault(); // Get the first shared category or null if none

                // Add to the dictionary if a shared category exists
                if (!string.IsNullOrEmpty(sharedCategory))
                {
                    sharedGameCategories.Add(relatedGame.GameId, sharedCategory);
                }
            }

            //get a list of games related to this member preferences  (Recommended games you may like)
            var gamePreferenceMap = new Dictionary<int, List<string>>();

            var gamesByLanguage = await _SVGSDbContext.Games
            .Where(g => g.IsDeleted != true && g.GameId != game.GameId)
            .Include(g => g.GameDetailsLanguages).ThenInclude(gl => gl.Language)
            .Where(g => g.GameDetailsLanguages.Any(gl => languagePrefIds.Contains(gl.LanguageId)))
            .ToListAsync();

            foreach (var currentGame in gamesByLanguage)
            {
                if (!gamePreferenceMap.ContainsKey(currentGame.GameId))
                {
                    gamePreferenceMap[currentGame.GameId] = new List<string>();
                }
                gamePreferenceMap[currentGame.GameId].Add("Language");
            }

            var gamesByCategory = await _SVGSDbContext.Games
                .Where(g => g.IsDeleted != true && g.GameId != game.GameId)
                .Include(g => g.GameDetailsCategories).ThenInclude(gc => gc.Category)
                .Where(g => g.GameDetailsCategories.Any(gc => categoryPrefIds.Contains(gc.CategoryId)))
                .ToListAsync();

            foreach (var currentGame in gamesByCategory)
            {
                if (!gamePreferenceMap.ContainsKey(currentGame.GameId))
                {
                    gamePreferenceMap[currentGame.GameId] = new List<string>();
                }
                gamePreferenceMap[currentGame.GameId].Add("Category");
            }
            var gamesByPlatform = await _SVGSDbContext.Games
                .Where(g => g.IsDeleted != true && g.GameId != game.GameId)
                .Include(g => g.GameDetailsPlatforms).ThenInclude(gp => gp.Platform)
                .Where(g => g.GameDetailsPlatforms.Any(gp => platformPrefIds.Contains(gp.PlatformId)))
                .ToListAsync();

            foreach (var currentGame in gamesByPlatform)
            {
                if (!gamePreferenceMap.ContainsKey(currentGame.GameId))
                {
                    gamePreferenceMap[currentGame.GameId] = new List<string>();
                }
                gamePreferenceMap[currentGame.GameId].Add("Platform");
            }

            ViewBag.PreferencesMap = gamePreferenceMap;

            var gamesFromPreferences = gamesByLanguage
                .Union(gamesByCategory)
                .Union(gamesByPlatform)
                .Distinct()
                .ToList();

            ProductDetailsViewModel productDetailViewModel = new ProductDetailsViewModel()
            {
                ActiveGame = game,
                AverageRating = averageRating,
                RelatedGames = relatedGames,
                SharedGameCategories = sharedGameCategories,
                GamesFromPreferences = gamesFromPreferences
            };

            ViewBag.CurrentUser = memberId;

            return View("details", productDetailViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> SaveRating(int gameId, float rating)
        {

            // Retrieve the accountId of the logged-in user
            string accountId = _userManager.GetUserId(User);

            // Get the memberId associated with this accountId
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();
            if (rating < 0 || rating > 5) //invalid rating is possible by editing javascript code for page, prevent from getting into database and return to page with toast message
            {
                var currentAverageRating = await _SVGSDbContext.GameAverageRatings.Where(g => g.GameId == gameId).Select(g => g.AverageRating).FirstOrDefaultAsync();
                return RedirectToAction("details", gameId);
            }
            else
            {
                try
                {
                    // Check if this user has already rated this game
                    var existingRating = await _SVGSDbContext.GameRatings.FirstOrDefaultAsync(r => r.GameId == gameId && r.MemberId == memberId);

                    if (existingRating != null)
                    {
                        // Update the existing rating
                        existingRating.RateValue = rating;
                        _SVGSDbContext.GameRatings.Update(existingRating);
                    }
                    else
                    {
                        // Add a new rating if none exists
                        var newRating = new GameRating
                        {
                            GameId = gameId,
                            MemberId = memberId,
                            RateValue = rating,
                        };
                        await _SVGSDbContext.GameRatings.AddAsync(newRating);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error retrieving rating: " + ex.Message);
                    return Json(new { success = false, message = "An error occurred while saving the rating. Please try again." });
                }
                // Save changes to the database
                await _SVGSDbContext.SaveChangesAsync();

                var newAverageRating = await _SVGSDbContext.GameAverageRatings.Where(g => g.GameId == gameId).Select(g => g.AverageRating).FirstOrDefaultAsync();

                // Render the partial view and pass the new average rating
                return PartialView("_GameRatingPartial", newAverageRating);
            }
            
        }

        //this function will get how many stars the user has previously selected
        [HttpGet("/Product/GetUserRating/{gameId}")]
        public async Task<IActionResult> GetUserRating(int gameId)
        {
            // Retrieve the accountId of the logged-in user
            string accountId = _userManager.GetUserId(User);

            // Get the memberId associated with this accountId
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();

            // Retrieve the user's existing rating for this game, if it exists
            var userRating = await _SVGSDbContext.GameRatings.FirstOrDefaultAsync(r => r.GameId == gameId && r.MemberId == memberId);

            // If userRating is null, return a response with rating set to null (no rating found)
            double? ratingValue = userRating != null ? userRating.RateValue : 0;
            return Json(new { rating = ratingValue});
        }

        [HttpGet("Portal/products")]
        [HttpPost("Portal/products")]
        public async Task<IActionResult> products(GamesListModel gamesListModel)
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
                        GameImageLink = g.GameImageLink,
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
                        GameImageLink = g.GameImageLink,
                        Categories = g.GameDetailsCategories
                            .Where(gdc => gdc.Category != null)
                            .Select(gdc => gdc.Category.CategoryName)
                            .ToList()
                    })
                    .OrderBy(g => g.GameName)
                    .ToListAsync();

                gamesListModel.GamesList = allGames;

            }

            return View("products", gamesListModel);
        }

        [HttpGet("Portal/cart")]
        public async Task<IActionResult> Cart()
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            var memberCart = await _SVGSDbContext.Carts
                .Include(c => c.Game) // Include related Game entities
                .Where(c => c.MemberId == memberId).ToListAsync();

            List<CartItemsViewModel> items = new List<CartItemsViewModel>();
            double totalPrice = 0;

            foreach (var item in memberCart)
            {
                CartItemsViewModel cartItemsViewModel = new CartItemsViewModel()
                {
                    CartItemId = item.Id,
                    GameName = item.Game.GameName,
                    GameImageLink = item.Game.GameImageLink,
                    Price = item.Game.Price ?? 0,
                    IsPhysical = item.IsPhysical

                };
                items.Add(cartItemsViewModel);
            }

            ViewBag.TotalItemsPrice = await CalculateTotalPrice(memberId);

            return View("cart", items);
        }

        [HttpGet("Portal/add-to-cart-request")]
        public async Task<IActionResult> AddProductToCart(int id, int isPhysical)
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            //retrieve game details from game Table & include the Category, Language, and Platform tables to retrieve the names
            bool cartItemExists = await _SVGSDbContext.Carts
                .AnyAsync(c => c.GameId == id && c.MemberId == memberId && c.IsPhysical==isPhysical);
            if (cartItemExists==false) 
            {
                Cart cartItem = new Cart() { GameId = id, MemberId = memberId, IsPhysical = isPhysical };
                await _SVGSDbContext.Carts.AddAsync(cartItem);
                await _SVGSDbContext.SaveChangesAsync();
                TempData["LastActionMessage"] = $"Item is added to the cart.";
                return RedirectToAction("products", "Product"); //return to the product list page
            }
            else 
            {
                TempData["LastActionMessage"] = $"Item is already in the cart";
                return RedirectToAction("details", "Product", new { id = id }); // keep in the same page
            }
        }


		[HttpGet("Portal/remove-cart-item-request")]
		public async Task<IActionResult> RemoveCartItem(int id)
        {
			//retrieve the memberId from members Table according to the signing-in user (AccountId)
			string? accountId = _userManager.GetUserId(User);
			var memberId = await _SVGSDbContext.Members
				.Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            // Find/retrieve/Item to delete:
            var cartItem = _SVGSDbContext.Carts.Find(id);

			if (cartItem != null)
			{
				_SVGSDbContext.Carts.Remove(cartItem);
				await _SVGSDbContext.SaveChangesAsync();
			}

			return RedirectToAction("cart", "Product");
		}


		[HttpGet("Portal/cart/checkout-shipping-address")]
        public async Task<IActionResult> CheckoutShippingAddress()
        {
			//retrieve the memberId from members Table according to the signing-in user (AccountId)
			string? accountId = _userManager.GetUserId(User);
			var memberId = await _SVGSDbContext.Members
				.Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            var shippingAddress= await _SVGSDbContext.AddressTables
                .Where(a=> a.MemberId== memberId && a.IsShipping== true).FirstOrDefaultAsync();
			var countries = await _SVGSDbContext.Country.ToListAsync();
			var provinces = await _SVGSDbContext.Province.ToListAsync();

            if (shippingAddress == null)
            {
                ViewBag.ShippingAddress = "not-found";
            }
            else { ViewBag.ShippingAddress = "found"; }
            
            CheckoutViewModel checkoutViewModel = new CheckoutViewModel()
            {
                Countries = countries,
                Provinces = provinces,
                ShippingAddress = shippingAddress,
                TotalPrice = await CalculateTotalPrice(memberId),
                HasPhysicalItems= await HasPhysicalItems (memberId)
			};
			return View("checkoutShippingAddress", checkoutViewModel);
        }

		[HttpPost("Portal/add-edit-shipping-address-requests")]
		public async Task<IActionResult> AddShippingAddress(CheckoutViewModel checkoutViewModel)
        {
			//retrieve the memberId from members Table according to the signing-in user (AccountId)
			string? accountId = _userManager.GetUserId(User);
			var memberId = await _SVGSDbContext.Members
				.Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

			AddressTable shippingAdr = checkoutViewModel.ShippingAddress;
			shippingAdr.MemberId = memberId;

			if (shippingAdr.AddressId == 0)
            {
                // it's valid so we want to add the new address to the DB ((Shipping Address)):
                await _SVGSDbContext.AddressTables.AddAsync(shippingAdr);
                await _SVGSDbContext.SaveChangesAsync();

                TempData["LastActionMessage"] = $"The Shipping Address is successfully Added";
			}
            else
            {
                // it's valid so we want to add the new address to the DB ((Shipping Address)):
                _SVGSDbContext.AddressTables.Update(shippingAdr);
                await _SVGSDbContext.SaveChangesAsync();

                TempData["LastActionMessage"] = $"The Address is successfully updated";
			}
			return RedirectToAction("CheckoutShippingAddress", "Product");


		}

        [HttpGet("Portal/cart/checkout-credit")]
        public async Task<IActionResult> CheckoutCredit()
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            ViewBag.TotalPrice = await CalculateTotalPrice(memberId);
            return View("checkoutCredit");
        }

        [HttpPost("Portal/cart/checkout-post")]
        public async Task<IActionResult> CheckoutPost()
        {
            try
            {
                //retrieve the memberId from members Table according to the signing-in user (AccountId)
                string? accountId = _userManager.GetUserId(User);
                var memberId = await _SVGSDbContext.Members
                    .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

                //calculate the total price 
                double totalPrice = await CalculateTotalPrice(memberId);

                //check if there is a physical item in the cart. if all are digital, OrderFulfilled=true else it will be false. Then the admin will fullfill the order
                bool allDigital = true;
                if (await HasPhysicalItems(memberId) == 1) { allDigital = false; }

                OrderTable memberOrder = new OrderTable()
                {
                    TotalPayment = totalPrice,
                    OrderFulfilled = allDigital,
                    MemberId = memberId,
                };


                //add the new order to the DB Order table---------------------------------------------
                await _SVGSDbContext.OrderTables.AddAsync(memberOrder);
                await _SVGSDbContext.SaveChangesAsync();

                // Retrieve the last OrderId
                int lastOrderId = await _SVGSDbContext.OrderTables
                    .OrderByDescending(o => o.OrderId)
                    .Select(o => o.OrderId)
                    .FirstOrDefaultAsync();


                //Retrieve All the cart items
                var memberCart = await _SVGSDbContext.Carts
                    .Where(c => c.MemberId == memberId).ToListAsync();

                foreach (var item in memberCart) 
                {
                    //Add the item to orderItem table ------------------------------------
                    OrderItem orderItem = new OrderItem()
                    {
                        OrderId= lastOrderId,
                        GameId = item.GameId,
                        MemberId= memberId,
                        IsPhysical = item.IsPhysical
                    };
                    
                    await _SVGSDbContext.OrderItems.AddAsync(orderItem);
                    await _SVGSDbContext.SaveChangesAsync();


                    //Remove the item from cart table ------------------------------------
                    //Find the item in the Carts table
                    var cartItem = await _SVGSDbContext.Carts.FindAsync(item.Id);

                    //Check if the item exists
                    if (cartItem == null)
                    {
                        throw new Exception ();
                    }

                    //Remove the item
                    _SVGSDbContext.Carts.Remove(cartItem);
                    await _SVGSDbContext.SaveChangesAsync();
                }


                return RedirectToAction("CheckoutSuccess", "Product");
            }
            catch (Exception)
            {
                return RedirectToAction("ErrorPage", "Home");
                
            }
        }

        [HttpGet("Portal/CheckoutSuccess")]
        public  IActionResult CheckoutSuccess()
        {
            return View("CheckoutSuccess");
        }


        

        /// <summary>
        /// calculate the total price of the items in the cart
        /// </summary>
        /// <param name="memberId"></param>
        private async Task<double> CalculateTotalPrice(int memberId)
        {
            double totalPrice = 0;
			var memberCart = await _SVGSDbContext.Carts
				.Include(c => c.Game) // Include related Game entities
				.Where(c => c.MemberId == memberId).ToListAsync();

			foreach (var item in memberCart)
			{
				totalPrice += item.Game.Price ?? 0;
			}
			return totalPrice;
        }

        /// <summary>
        /// check the items in the cart and return 1 if there is a physical item otherwise return 0
        /// </summary>
        /// <param name="memberId"></param>
		private async Task<int> HasPhysicalItems(int memberId)
		{
			int hasPhysicalItems = 0;
			var memberCart = await _SVGSDbContext.Carts
				.Include(c => c.Game) // Include related Game entities
				.Where(c => c.MemberId == memberId).ToListAsync();

                foreach (var item in memberCart)
			    {
                    if (item.IsPhysical == 1) 
                    {
						hasPhysicalItems = 1;
						return hasPhysicalItems;
					} 
			    }

			return hasPhysicalItems;
		}
	}
}
