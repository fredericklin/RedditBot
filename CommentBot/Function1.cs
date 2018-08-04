using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using RedditSharp;

namespace FrederickLin.RedditBot
{
    public static class Function1
    {

        [FunctionName("Function1")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            string userName = String.Empty;
            string password = String.Empty;
            string redditClientId = String.Empty;
            string redditClientSecret = String.Empty;
            string redditRedirectUrl = "http://localhost";
            string subredditName = String.Empty;

            WebAgent agent = new RedditSharp.BotWebAgent(userName, password, redditClientId, redditClientSecret, redditRedirectUrl);
            Reddit reddit = new RedditSharp.Reddit(agent);

            var sub = reddit.GetSubredditAsync(subredditName).GetAwaiter().GetResult();
            var contribs = sub.New;

            foreach(RedditSharp.Things.Post post in contribs)
            {
                log.Info(post.Title);
            }

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            log.Info(agent.AccessToken);
        }
    }
}
