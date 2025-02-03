using InsightApp.Models;

namespace InsightApp.Services
{
    public static class TweetUtilities
    {
        public static readonly int MAX_TWEET_TEXT = 240;
        public static readonly int MAX_FINAL_TWEETTEXT_LENGTH = 230;

        public static string ConstructWishlistTweetText(List<WishlistViewModel> gamesInWishlist, string currentUrl)
        {
            if (gamesInWishlist.Count == 0)
            {
                return System.Net.WebUtility.UrlEncode("There are no games on my wishlist");
            }
            string tweetText = "Check out my game wishlist on the CVGS Game Store:" + Environment.NewLine;
            int maxTweetLengthIncludingUrl = MAX_TWEET_TEXT - currentUrl.Length;
            int indexOfLastAddedGame = 0;
            for (int i=0; i<gamesInWishlist.Count; i++)
            {
                if (tweetText.Length >= maxTweetLengthIncludingUrl)
                {
                    break;
                }
                else
                {
                    tweetText += $"{gamesInWishlist[i].GameName}" + Environment.NewLine;
                    indexOfLastAddedGame = i;
                }
            }
            int gamesNotAddedToTweet = (gamesInWishlist.Count - 1) - indexOfLastAddedGame;

            string finalTweetText = tweetText;
            if (gamesNotAddedToTweet != 0)
            {
                finalTweetText += $"And {gamesNotAddedToTweet} more at..." + Environment.NewLine;
            }
            return System.Net.WebUtility.UrlEncode(finalTweetText);
        }
    }
}
