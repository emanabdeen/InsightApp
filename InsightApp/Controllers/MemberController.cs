using Humanizer;
using InsightApp.Components;
using InsightApp.Entities;
using InsightApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Shared;
using System.Configuration;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace InsightApp.Controllers
{
    [Authorize(Roles = "Member")]
    public class MemberController : Controller
    {
        private InsightUpdateCvgs2Context _SVGSDbContext;
        private readonly UserManager<Entities.Account> _userManager;
        private readonly SignInManager<Entities.Account> _signInManager;

        public MemberController(InsightUpdateCvgs2Context sVGSDbContext, UserManager<Entities.Account> userManager, SignInManager<Entities.Account> signInManager)
        {
            _SVGSDbContext = sVGSDbContext;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        //-----Portal home page ------------
        [HttpGet("Portal")]
        public IActionResult MemberPortal()
        {
            ViewBag.Page = "MemberPortal";
            ViewBag.Account = "Member";
            return View("MemberPortal");
        }


        //-----Portal My profile ------------
        [HttpGet("Portal/profile/{id}")]
        public async Task<ActionResult> MemberProfile(int id , string? activeTab = "profileTab")
        {
            ProfileViewModel profileViewModel = new ProfileViewModel();
            profileViewModel.ActiveMember = new Member();
            profileViewModel.ActiveMember.MemberId= id;
            ViewBag.Page = "MemberPortal";
            ViewBag.Account = "Member";
            
            return View("Profile", profileViewModel);
        }

        [HttpPost("Portal/add-edit-address-requests")]
        public async Task<IActionResult> AddAddressesById(MemberAddressesViewModel memberAddressesViewModel)
        {

            if (ModelState.IsValid)
            {
                //----Member Address------
                AddressTable memberAdr = memberAddressesViewModel.MemberAddress;
                memberAdr.MemberId = memberAddressesViewModel.MemberId;
            
                //-----Shipping Address-----
                AddressTable shippingAdr = memberAddressesViewModel.ShippingAddress;
                shippingAdr.MemberId = memberAddressesViewModel.MemberId;


                //if same address checked copy the main properies will be the same
                if (memberAddressesViewModel.IsAdressesSame == true)
                {
                    shippingAdr.Unit=memberAdr.Unit;
                    shippingAdr.StreetNumber=memberAdr.StreetNumber;
                    shippingAdr.StreetName=memberAdr.StreetName;
                    shippingAdr.City=memberAdr.City;
                    shippingAdr.PostalCode=memberAdr.PostalCode;
                    shippingAdr.Province=memberAdr.Province;
                    shippingAdr.Country=memberAdr.Country;
                }


                //if new addresses, create new address records ( MemberAddress => isShipping=false + ShippingAddress=> isShipping=true)
                if (memberAdr.AddressId == 0 && shippingAdr.AddressId == 0)
                {
                    // it's valid so we want to add the new address to the DB ((Member Address)):
                    await _SVGSDbContext.AddressTables.AddAsync(memberAdr);
                    await _SVGSDbContext.SaveChangesAsync();

                    // it's valid so we want to add the new address to the DB ((Shipping Address)):
                    await _SVGSDbContext.AddressTables.AddAsync(shippingAdr);
                    await _SVGSDbContext.SaveChangesAsync();

                    TempData["LastActionMessage"] = $"The Address is successfully Added";
                }
                else if (memberAdr.AddressId == 0 && shippingAdr.AddressId != 0) //the user has a shipping address but does not have a personal address, save the new personal address and update the shipping address
                {
					//create new personal address records
				    await _SVGSDbContext.AddressTables.AddAsync(memberAdr);
					await _SVGSDbContext.SaveChangesAsync();

                    //update the shipping address
					_SVGSDbContext.AddressTables.Update(shippingAdr);
					await _SVGSDbContext.SaveChangesAsync();

					TempData["LastActionMessage"] = $"The Address is successfully updated";

				}
                else
                {
                    // it's valid so we want to add the new address to the DB ((Member Address)):
                    _SVGSDbContext.AddressTables.Update(memberAdr);
                    await _SVGSDbContext.SaveChangesAsync();

                    // it's valid so we want to add the new address to the DB ((Shipping Address)):
                    _SVGSDbContext.AddressTables.Update(shippingAdr);
                    await _SVGSDbContext.SaveChangesAsync();

                    TempData["LastActionMessage"] = $"The Address is successfully updated";

                }

                return RedirectToAction("MemberProfile", "Member", new { id = memberAddressesViewModel.MemberId, activeTab = "addressTab" });
            }
            else
            {
                ProfileViewModel profileViewModel = new ProfileViewModel();
                profileViewModel.ActiveMember = new Member();
                profileViewModel.ActiveMember.MemberId = memberAddressesViewModel.MemberId;
                return RedirectToAction("MemberProfile", "Member", new { id = memberAddressesViewModel.MemberId, activeTab = "addressTab" });
            }
            
        }

        [HttpPost("Portal/edit-profile-requests")]
        public async Task<IActionResult> EditMemberProfileId( MemberProfileViewModel memberProfileViewModel)
        {
            memberProfileViewModel.ActiveMember.Account = _SVGSDbContext.Accounts.FirstOrDefault(a => a.Id == memberProfileViewModel.ActiveMember.AccountId);
            if (ModelState.IsValid)
            {

                // it's valid so we want to update the existing Members in the DB:
                _SVGSDbContext.Members.Update(memberProfileViewModel.ActiveMember);
                await _SVGSDbContext.SaveChangesAsync();

                TempData["LastActionMessage"] = $"The Profile has been updated.";

                return RedirectToAction("MemberProfile", "Member", new { id = memberProfileViewModel.ActiveMember.MemberId, activeTab = "profileTab" });
            }
            else
            {
                // it's invalid so we simply return the profileViewModel object
                // to the Edit view again:
                ProfileViewModel profileViewModel = new ProfileViewModel();
                profileViewModel.ActiveMember = new Member();
                profileViewModel.ActiveMember.MemberId = memberProfileViewModel.ActiveMember.MemberId;

                return View("Profile", profileViewModel);
            }

            
        }

        [HttpPost("Portal/edit-password-requests")]
        public async Task<IActionResult> EditAccountPassword(MemberPasswordViewModel memberPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                ProfileViewModel profileViewModel = new ProfileViewModel();
                profileViewModel.ActiveMember = new Member();
                profileViewModel.ActiveMember.MemberId = _SVGSDbContext.Members.Where(m => m.AccountId == memberPasswordViewModel.AccountId).FirstOrDefaultAsync().Id;
                return View("Profile", profileViewModel);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            var changePasswordResult = await _userManager.ChangePasswordAsync(user, memberPasswordViewModel.OldPassword, memberPasswordViewModel.NewPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                // it's invalid so we simply return the profileViewModel object
                // to the Edit view again:
                ProfileViewModel profileViewModel = new ProfileViewModel();
                profileViewModel.ActiveMember =  _SVGSDbContext.Members.Where(m => m.AccountId == memberPasswordViewModel.AccountId).FirstOrDefault();

                TempData["PasswordModelErrors"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                return RedirectToAction("MemberProfile", "Member", new { id = memberPasswordViewModel.MemberId, activeTab = "passwordTab" });
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["LastActionMessage"] = $"The Profile has been updated.";
            return RedirectToAction("MemberProfile", "Member", new { id = memberPasswordViewModel.MemberId, activeTab = "passwordTab" });

        }

        [HttpPost("Portal/edit-preferences-request")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPreferences(MemberPreferencesViewModel memberPreferencesViewModel)
        {
            if (!ModelState.IsValid)
            {
                ProfileViewModel profileViewModel = new ProfileViewModel();
                profileViewModel.ActiveMember = new Member();
                profileViewModel.ActiveMember.MemberId = memberPreferencesViewModel.MemberId;
                return View("Profile", profileViewModel);
            }
            else
            {
                // it's valid so we want to update the existing Members in the DB:
                var member = await _SVGSDbContext.Members
                    .Include(m => m.MemberGameCategoryPrefs)
                    .Include(m => m.MemberPlatformPrefs)
                    .Include(m => m.MemberLanguagePrefs)
                    .FirstOrDefaultAsync(m => m.MemberId == memberPreferencesViewModel.MemberId);

                if (member.MemberPlatformPrefs.Count() > 0 || member.MemberGameCategoryPrefs.Count() > 0 || member.MemberLanguagePrefs.Count() > 0)
                {
                    await UpdateMemberPreferences(member, memberPreferencesViewModel, true);
                }
                else
                {
                    await UpdateMemberPreferences(member, memberPreferencesViewModel, false);
                }

                TempData["LastActionMessage"] = $"The preferences have been updated.";

                return RedirectToAction("MemberProfile", "Member", new { id = memberPreferencesViewModel.MemberId, activeTab = "preferencesTab" });
            }
        }

        private async Task UpdateMemberPreferences(Member member, MemberPreferencesViewModel viewModel, bool prefsExist)
        {
            if (prefsExist)
            {
                member.MemberGameCategoryPrefs.Clear();
                member.MemberPlatformPrefs.Clear();
                member.MemberLanguagePrefs.Clear();
            }

            foreach(int categoryId in viewModel.SelectedCategoryIds)
            {
                member.MemberGameCategoryPrefs.Add(new MemberGameCategoryPref
                {
                    MemberId = member.MemberId,
                    CategoryId = categoryId,
                });
            }
            foreach(int platformId in viewModel.SelectedPlatformIds)
            {
                member.MemberPlatformPrefs.Add(new MemberPlatformPref
                {
                    MemberId = member.MemberId,
                    PlatformId = platformId,
                });
            }
            foreach (int languageId in viewModel.SelectedLanguageIds)
            {
                member.MemberLanguagePrefs.Add(new MemberLanguagePref
                {
                    MemberId = member.MemberId,
                    LanguageId = languageId,
                });
            }
            await _SVGSDbContext.SaveChangesAsync();
        }


        //-----Portal wishlist ------------
        [HttpGet("Portal/wish-list")]
        public async Task<IActionResult> wishlist()
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            var memberWishlist = await _SVGSDbContext.WishLists
                .Include(c => c.Game) // Include related Game entities
                .Where(c => c.MemberId == memberId).ToListAsync();

            List<WishlistViewModel> items = new List<WishlistViewModel>();

            foreach (var item in memberWishlist)
            {
                WishlistViewModel wishlistViewModel = new WishlistViewModel()
                {
                    WishlistItemId = item.Id,
                    GameId = item.Game.GameId,
                    GameName = item.Game.GameName,
                    GameImageLink = item.Game.GameImageLink,
                    Price = item.Game.Price
                };
                items.Add(wishlistViewModel);
            }
            return View("wishlist", items);
        }

        [HttpGet("Portal/wish-list/delete-requests/{id?}")]
        public async Task<IActionResult> DeleteWhislistItem(int id)
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            // Find/retrieve/Item to delete:
            var wishlistItem = _SVGSDbContext.WishLists.Find(id);

            if (wishlistItem != null)
            {
                _SVGSDbContext.WishLists.Remove(wishlistItem);
                await _SVGSDbContext.SaveChangesAsync();
            }

            return RedirectToAction("wishlist", "Member");

        }

        [HttpPost("Portal/wish-list/add-requests/{id?}")]
        public async Task<IActionResult> AddWhislistItem(int id)
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            //retrieve game details from game Table & include the Category, Language, and Platform tables to retrieve the names
            bool itemExists = await _SVGSDbContext.WishLists
                .AnyAsync(w => w.GameId == id && w.MemberId == memberId);
            if (itemExists == false)
            {
                WishList wishListItem = new WishList()
                {
                    MemberId = memberId,
                    GameId = id
                };
                await _SVGSDbContext.WishLists.AddAsync(wishListItem);
                await _SVGSDbContext.SaveChangesAsync();
                TempData["LastActionMessage"] = $"Item is added to the wishlist.";
                return RedirectToAction("details", "Product", new { id = id }); // keep in the same page
            }
            else
            {
                TempData["LastActionMessage"] = $"Item is already in the wishlist";
                return RedirectToAction("details", "Product", new { id = id }); // keep in the same page
            }
        }

        //----------------Friend wish list---------------------

        [HttpGet("Portal/friend-wish-list")]
        public async Task<IActionResult> FriendWishList(Member friend)
        {
            var memberId = friend.MemberId;
            var memberWishlist = await _SVGSDbContext.WishLists
                .Include(c => c.Game) // Include related Game entities
                .Where(c => c.MemberId == memberId).ToListAsync();

            List<WishlistViewModel> items = new List<WishlistViewModel>();

            foreach (var item in memberWishlist)
            {
                WishlistViewModel wishlistViewModel = new WishlistViewModel()
                {
                    WishlistItemId = item.Id,
                    GameId = item.Game.GameId,
                    GameName = item.Game.GameName,
                    GameImageLink = item.Game.GameImageLink,
                    Price = item.Game.Price
                };
                items.Add(wishlistViewModel);
            }
            return View("wishlist", items);
        }

        //---------------------Orders--------------------------

        [HttpGet("Portal/Orders")]
        public async Task<IActionResult> Orders()
        {

            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            List<OrderTable> memberOrders = await _SVGSDbContext.OrderTables
                .Where(c => c.MemberId == memberId).ToListAsync();

            return View("Orders", memberOrders);
        }

        [HttpGet("Portal/Orders/{id?}")]
        public async Task<IActionResult> OrderItems(int id)
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            var orderItems = await _SVGSDbContext.OrderItems
                .Where(o => o.OrderId == id && o.MemberId==memberId)
                .Include(o => o.Game)
                .ToListAsync();
            ViewBag.OrderNumber = id.ToString();

            return View("OrderItems", orderItems);
        }


        [HttpGet("Portal/myGames")]
        public async Task<IActionResult> OwnedGames()
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            var myGames = await _SVGSDbContext.OrderItems
                .Where(o => o.MemberId == memberId)
                .Include(o => o.Game)
                .ToListAsync();

            return View("OwnedGames", myGames);
        }

        [HttpGet("Portal/DownloadGame/{gameId}")]
        public async Task<IActionResult> DownloadGame(int gameId)
        {
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            // Retrieve the game details from the database
            var game = await _SVGSDbContext.Games
                .Where(g => g.GameId == gameId)
                .Include(g => g.GameDetailsCategories).ThenInclude(c => c.Category)
                .Include(g => g.GameDetailsLanguages).ThenInclude(l => l.Language)
                .Include(g => g.GameDetailsPlatforms).ThenInclude(p => p.Platform)
                .FirstOrDefaultAsync();
            if (game == null)
            {
                return NotFound("Game not found.");
            }

            //Check if user owns game or if it is free
            var ownedGames = await _SVGSDbContext.OrderItems
                .Where(o => o.MemberId == memberId)
                .Include(o => o.Game)
                .ToListAsync();
            if (!ownedGames.Any(oi => oi.GameId == gameId) && game.Price != 0)
            {
                TempData["LastActionMessage"] = $"You don't own this game";
                return RedirectToAction("details", "Product", new { id = gameId }); // redirect to game page
            }

            // Prepare the content of the .txt file
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine($"Game Title: {game.GameName}\n");
            contentBuilder.AppendLine($"Price: ${game.Price}\n");
            contentBuilder.AppendLine($"Details: {game.Details}\n");
            contentBuilder.AppendLine("Categories:");
            foreach (var category in game.GameDetailsCategories)
            {
                contentBuilder.AppendLine($"  - {category.Category.CategoryName}");
            }
            contentBuilder.AppendLine("\nLanguages:");
            foreach (var language in game.GameDetailsLanguages)
            {
                contentBuilder.AppendLine($"  - {language.Language.LanguageName}");
            }
            contentBuilder.AppendLine("\nPlatforms:");
            foreach (var platform in game.GameDetailsPlatforms)
            {
                contentBuilder.AppendLine($"  - {platform.Platform.PlatformName}");
            }

            // Convert the content to a byte array
            var fileContents = Encoding.UTF8.GetBytes(contentBuilder.ToString());

            // Return the file as a downloadable response
            return File(fileContents, "text/plain", $"{game.GameName}_Details.txt");
        }


        //--------------------------------Review games-----------------

        [HttpGet("Portal/SubmitReview/id")]
        public async Task<IActionResult> ReviewGame(int id)
        {
            // Retrieve the account ID of the signed-in user
            string? accountId = _userManager.GetUserId(User);


            // Retrieve the MemberId from the Members table using the AccountId
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();


            // Check if a review exists for the current game and member
            var currentReview = await _SVGSDbContext.Reviews
                .FirstOrDefaultAsync(r => r.MemberId == memberId && r.GameId == id);

            // Check if a rating exist for the current game and member
            var currentRating = await _SVGSDbContext.GameRatings
                .FirstOrDefaultAsync(r => r.MemberId == memberId && r.GameId == id);

            var game = await _SVGSDbContext.Games.FirstOrDefaultAsync(g => g.GameId == id);

            Review newReview = new Review()
            {
                GameId = id,
                MemberId = memberId,
                ReviewId = 0, // Use 0 if no review exists
                StatusId = 2, // Default status
            };


            // Prepare the ViewModel
            var ratingAndReview = new RateReviewGamesViewModel
            {
                GameReview = newReview, // Assuming ReviewBody contains the review text
                GameRating = currentRating,
                Game = game
            };



            // Pass the ViewModel to the view
            return View("SubmitReview", ratingAndReview);
        }

        [HttpPost("Portal/SubmitReview/id")]
        public async Task<IActionResult> SubmitReview(RateReviewGamesViewModel viewModel)
        {
            // Retrieve the account ID of the signed-in user
            string? accountId = _userManager.GetUserId(User);


            // Retrieve the MemberId from the Members table using the AccountId
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId)
                .Select(m => m.MemberId)
                .FirstOrDefaultAsync();
            

            if (string.IsNullOrWhiteSpace(viewModel.GameReview.ReviewBody))
            {
                // Remove the default error for ReviewBody
                if (ModelState.ContainsKey("GameReview.ReviewBody"))
                {
                    ModelState["GameReview.ReviewBody"].Errors.Clear();
                }
                ModelState.AddModelError("GameReview.ReviewBody", "Review comments cannot be empty. Please write your review before clicking submit.");
            }

            

            if (viewModel.GameReview.StatusId == 2)
            {
                //not wanting to mess with review table, so just removing validation this way
                ModelState.Remove("GameReview.Status");
            }
            
            // Check if the model state is valid
            if (!ModelState.IsValid)
            {
                // Reload game details and rating for redisplaying the form
                viewModel.Game = await _SVGSDbContext.Games.FirstOrDefaultAsync(g => g.GameId == viewModel.GameReview.GameId);
                viewModel.GameRating = await _SVGSDbContext.GameRatings
                    .FirstOrDefaultAsync(r => r.MemberId == memberId && r.GameId == viewModel.GameReview.GameId);

                return View("SubmitReview", viewModel);
            }
            else
            {
                // Add success message to TempData
                viewModel.GameReview.ReviewBody = viewModel.GameReview.ReviewBody.Trim();
                TempData["SuccessMessage"] = "Your review has been received and is pending review by our moderation team.";
                viewModel.GameReview.MemberId = memberId;
                _SVGSDbContext.Reviews.Add(viewModel.GameReview);
                await _SVGSDbContext.SaveChangesAsync();    
            }
            return RedirectToAction("OwnedGames");
        }

        [HttpGet("/Member/Reviews/{id}")]
        public async Task<IActionResult> Reviews(int id)
        {
            try
            {
                List<ReviewViewModel> pendingReviews = await _SVGSDbContext.Reviews
            .Where(r => r.StatusId == 1 && r.GameId == id) // Filter by game ID
            .Include(r => r.Game)
            .Include(r => r.Member)
            .Include(r => r.Status)
            .GroupJoin(
                _SVGSDbContext.GameRatings, // Join with GameRatings table
                review => new { review.GameId, review.MemberId },
                rating => new { rating.GameId, rating.MemberId },
                (review, ratings) => new { review, ratings }
            )
            .SelectMany(
                joined => joined.ratings.DefaultIfEmpty(),
                (joined, rating) => new ReviewViewModel
                {
                    ReviewId = joined.review.ReviewId,
                    GameName = joined.review.Game != null ? joined.review.Game.GameName : "Unknown Game",
                    ReviewedBy = joined.review.Member != null ? joined.review.Member.DisplayName : "Unknown Reviewer",
                    ReviewBody = joined.review.ReviewBody,
                    StatusName = joined.review.Status != null ? joined.review.Status.Statusname : "Unknown Status",
                    GameImageLink = joined.review.Game.GameImageLink,
                    UserRating = rating != null ? rating.RateValue : null // Extract user rating if exists
                }
            )
            .ToListAsync();

                var currentGame = await _SVGSDbContext.Games.Where(g => g.GameId == id).FirstOrDefaultAsync();

                ViewBag.GameId = currentGame.GameId;
                ViewBag.GameName = currentGame.GameName;
                return View("ReviewsForSelectedGame", pendingReviews);
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }


        //----------events----------------

        [HttpGet("Portal/my-events")]
        public async Task<IActionResult> GetMyEvents()
        {
            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            //will return only the events that (isDeleted=false) && start with SearchText
            var myRegistrations = await _SVGSDbContext.MemberEventRegists
                .Where(e => e.MemberId == memberId ).ToListAsync();

            List<GameEvent> events = new List<GameEvent>();

            foreach (var ev in myRegistrations)
            {
                var myEvent = await _SVGSDbContext.GameEvents
                    .Where(e => e.EventId== ev.EventId)
                    .Include(e => e.EvType)
                    .Include(e => e.Address)
                    .Where(e => e.IsDeleted == false)
                    .OrderBy(e => e.StartDate)
                    .FirstOrDefaultAsync();

                //to add only the events that start in the current day or in the future 
                if (myEvent.StartDate>=DateOnly.FromDateTime(DateTime.Today))
                {
                    events.Add(myEvent);
                }
                
            }
            ViewBag.MyEvent = "true";

            return View("MyEventsList", events);
        }

        
        [HttpGet("Portal/friends")]
        public async Task<IActionResult> GetFriends(){

            //retrieve the memberId from members Table according to the signing-in user (AccountId)
            string? accountId = _userManager.GetUserId(User);
            
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            var friendList = await  _SVGSDbContext.Friends.Include(f=>f.FriendNavigation).ThenInclude(fn=>fn.Account).
                                    Where(f => f.MemberId == memberId).ToListAsync();


            List<FriendViewModel> friendListVM = friendList.Select(f => new FriendViewModel{
                FriendId = f.FriendId.Value,
                UserName = f.FriendNavigation.Account?.UserName,
                IsAlreadyFriend = true
            }).ToList().OrderBy(f=>f.UserName).ToList();
       
            return View("FriendList",friendListVM);

        
        }


         [HttpGet("Portal/add-friend")]
        public async Task<IActionResult> SearchFriend(string? friendSearch){
            
            string? accountId = _userManager.GetUserId(User);
            
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

                      
            FriendSearchViewModel friendList = new FriendSearchViewModel{
                 SearchedMembers = new List<FriendViewModel>(),
                  SuggestedFriends = new List<FriendViewModel>()
            };

            List<Friend> alreadyExistingFriends = await _SVGSDbContext.Friends.Where(f=>f.MemberId == memberId).ToListAsync();

            if(!string.IsNullOrEmpty(friendSearch)){

                ViewBag.SearchTerm = friendSearch;

                List<Member> memberSearch = await _SVGSDbContext.Members.Include(m => m.Account).
                                        Where(m=>(m.Account.UserName.ToLower().Contains(friendSearch.ToLower()) || 
                                        m.Account.Email.ToLower().Contains(friendSearch.ToLower())|| 
                                        string.Concat(m.FirstName,m.LastName).ToLower().Contains(friendSearch.ToLower()))
                                        && m.MemberId != memberId)
                                .ToListAsync();

                       
                List<FriendViewModel> searchedFriendsVM = memberSearch.Select(m => new FriendViewModel{
                    
                    FriendId = m.MemberId,
                    UserName = m.Account.UserName,
                    IsAlreadyFriend = false
                    
                }).ToList();

                if(searchedFriendsVM.Any()){
                    friendList.SearchedMembers = searchedFriendsVM;
                }else{
                    var suggestedFriends = await GetSuggestedFriends();
                
                    friendList.SuggestedFriends = suggestedFriends.OrderBy(f=>f.UserName).ToList();
                }

              

                //compare friendList with members and flag already existing friends
                friendList.SearchedMembers.Where(f1 => alreadyExistingFriends.Any(f2=> f2.FriendId == f1.FriendId))
                .ToList()
                .ForEach(f1 => f1.IsAlreadyFriend = true);

            
            }else{
                //no search made (loading screen)
                ViewBag.SearchTerm = String.Empty;

                 var suggestedFriends = await GetSuggestedFriends();
                
                friendList.SuggestedFriends = suggestedFriends.OrderBy(f=>f.UserName).ToList();

            }

            return View("AddFriend",friendList);

        }

        [HttpPost]
         public async Task<IActionResult> AddFriend(int friendId){

            string? accountId = _userManager.GetUserId(User);
            
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            if(friendId != null && friendId>0){


                _SVGSDbContext.Friends.Add( new Friend{
                     MemberId = memberId,
                      FriendId = friendId
                });
                await _SVGSDbContext.SaveChangesAsync();
            }else{

            }
            return RedirectToAction("GetFriends");
         }

         [HttpPost]
         public async Task<IActionResult> RemoveFriend(int friendId){
                string? accountId = _userManager.GetUserId(User);
            
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            if(friendId != null && friendId>0){


                Friend removeFriend = _SVGSDbContext.Friends.Where(f=>f.MemberId == memberId && f.FriendId == friendId).FirstOrDefault();

                if(removeFriend != null){
                _SVGSDbContext.Friends.Remove(removeFriend);
                await _SVGSDbContext.SaveChangesAsync();
                }   
            }
            return RedirectToAction("GetFriends");
         }

        [HttpGet("Portal/friends/wish-list")]
         public async Task<IActionResult>GetFriendWishList(int friendId){

            string? accountId = _userManager.GetUserId(User);
            
            var memberId = await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();

            Friend friend = await _SVGSDbContext.Friends.Where(f => f.MemberId == memberId && f.FriendId == friendId).FirstOrDefaultAsync();

            List<WishlistViewModel> wishListVM = null;

            if (friend != null)
            {
                string friendUserName = await _SVGSDbContext.Members.Include(m => m.Account)
                            .Where(i => i.MemberId == friendId).Select(i => i.Account.UserName).FirstAsync();

                ViewBag.Friend = friendUserName;

                var friendWishList = await _SVGSDbContext.WishLists
                    .Include(c => c.Game) // Include related Game entities
                    .Where(c => c.MemberId == friendId).ToListAsync();


                wishListVM = new List<WishlistViewModel>();

                foreach (var item in friendWishList)
                {
                    WishlistViewModel wishListItem = new WishlistViewModel()
                    {
                        WishlistItemId = item.Id,
                        GameId = item.Game.GameId,
                        GameName = item.Game.GameName,
                        GameImageLink = item.Game.GameImageLink,
                        Price = item.Game.Price
                    };
                    wishListVM.Add(wishListItem);
                }
            }
            else
            {
                ViewBag.Friend = "Non-Friend";
            }
        return View("friendWishList",wishListVM);
        }

        private async Task<List<FriendViewModel>> GetSuggestedFriends(){
            string? accountId = _userManager.GetUserId(User);
            
            var memberId =  await _SVGSDbContext.Members
                .Where(m => m.AccountId.ToString() == accountId).Select(m => m.MemberId).FirstOrDefaultAsync();


            List<Friend> alreadyExistingFriends = await _SVGSDbContext.Friends
                                                .Where(f=>f.MemberId == memberId).ToListAsync();


            List<int> friendsId = alreadyExistingFriends.Select(f => f.FriendId.Value).ToList();

                //including user's own id to not be returned
                friendsId.Add(memberId);

            var membersNotFriends =  await _SVGSDbContext.Members
                                    .Include(m=>m.Account)
                                    .Where(m => !friendsId.Contains(m.MemberId))
                                    .ToListAsync()
                                    ;

            List<FriendViewModel> suggestedList = new List<FriendViewModel>();

            if(membersNotFriends.Count>0){

                suggestedList =  membersNotFriends.Select(m => new FriendViewModel {
                    FriendId = m.MemberId,
                    UserName = m.Account.UserName,
                    IsAlreadyFriend = false
                }).Take(15).ToList().OrderBy(f=>f.UserName).ToList();

                    }else{
                        suggestedList = alreadyExistingFriends.Select(f => new FriendViewModel{
                            FriendId = f.FriendId.Value,
                            UserName = f.FriendNavigation.Account.UserName,
                            IsAlreadyFriend = true
                        }).Take(15).ToList().OrderBy(f=>f.UserName).ToList();
                    }


            return suggestedList;

        }

    }

}
