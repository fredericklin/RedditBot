using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using RedditSharp;
using RedditSharp.Things;


namespace FrederickLin.RedditBot
{
    public static class TriggerOnNewPost
    {
        public static readonly string UserName = GetSetting("RedditUserName");
        public static readonly string Password = GetSetting("RedditPassword");
        public static readonly string ClientId = GetSetting("RedditClientId");
        public static readonly string ClientSecret = GetSetting("RedditClientSecret");
        public static readonly string RedirectUrl = GetSetting("RedditRedirectUrl");
        public static readonly string SubredditName = GetSetting("SubredditName");
        public static readonly string SearchKeyword = GetSetting("PostKeyword");

        public static DateTimeOffset lastProcessed = DateTime.MinValue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myTimer"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        [FunctionName("TriggerOnNewPost")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            // connect to Reddit
            WebAgent agent = new BotWebAgent(UserName, Password, ClientId, ClientSecret, RedirectUrl);
            Reddit reddit = new Reddit(agent, true);

            // retrive subreddit
            Subreddit sub = await reddit.GetSubredditAsync(SubredditName);

            // get first 5 postings
            IEnumerable<Post> postings = sub.New.GetListing(5)
                .OrderBy(p => p.Created);

            // Do not process if this is the first run.  Just cache the
            // last processed date and time.  If the process is killed
            // and restarted, this method may miss new postings.  If an issue,
            // implement persistent storage to store this value.
            if (lastProcessed != DateTime.MinValue)
            {
                bool hasNewPost = false;

                foreach (Post post in postings)
                {
                    // this is a new posting!
                    if (post.Created > lastProcessed)
                    {
                        hasNewPost = true;
                        lastProcessed = post.Created;

                        log.Info(String.Format("NEW POST DETECTED. ID: {0}, Title: {1}", post.Id, post.Title));

                        // process only if the title contains the keyword
                        if(post.Title.ToLower().Contains(SearchKeyword))
                        {
                            log.Info("Keyword detected");

                            // post if you are the first
                            if(post.CommentCount == 0)
                            {
                                Comment c = post.Comment("First! I'm not really a bot.");
                                RedditUser pAuthor = post.Author;

                                reddit.ComposePrivateMessage("this is a test", "this is a test", post.Author.Name);
                            }
                        } else
                        {
                            log.Info("No keyword detected");
                        }
                    }
                    {
                        log.Info(String.Format("Skipping post: {0}", post.Title));
                    }
                }

                if(hasNewPost == false)
                {
                    log.Info("No posts detected");
                }
            }
            else
            {
                // store the last timestamp and skip
                // this iteration
                lastProcessed = postings.Last().Created;

            }


            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
            log.Info(agent.AccessToken);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetSetting(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }


    }
}
