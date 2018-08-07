using System;
using System.Linq;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using RedditSharp;
using RedditSharp.Things;
using FrederickLin.RedditBot;

namespace FrederickLin.RedditBot
{
    public static class TriggerOnNewPost
    {
        public static readonly string UserName = Configuration.GetValue("RedditUserName");
        public static readonly string Password = Configuration.GetValue("RedditPassword");
        public static readonly string ClientId = Configuration.GetValue("RedditClientId");
        public static readonly string ClientSecret = Configuration.GetValue("RedditClientSecret");
        public static readonly string RedirectUrl = Configuration.GetValue("RedditRedirectUrl");
        public static readonly string SubredditName = Configuration.GetValue("SubredditName");
        public static readonly string PostTitleKeyword = Configuration.GetValue("PostTitleKeyword");
        public static readonly string CommentText = Configuration.GetValue("CommentText");
        public static readonly string PrivateMessageTitle = Configuration.GetValue("PrivateMessageTitle");
        public static readonly string PrivateMessageBody = Configuration.GetValue("PrivateMessageBody");


        public static DateTimeOffset lastProcessed = DateTime.MinValue;

        [FunctionName("TriggerOnNewPost")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            // connect to Reddit
            WebAgent agent = new BotWebAgent(UserName, Password, ClientId, ClientSecret, RedirectUrl);
            Reddit reddit = new Reddit(agent, true);

            // retrive subreddit
            Subreddit sub = await reddit.GetSubredditAsync(SubredditName);

            // get first 5 postings
            // and sort in ascending order
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
                        log.Info(post.SelfText.Count().ToString());

                        // process only if the title contains the keyword
                        // and the post is less than 250 characters
                        if (post.Title.ToLower().Contains(PostTitleKeyword) &&
                            post.SelfText.Count() < 250)
                        {
                            log.Info("Keyword detected ...");

                            // post if you are the first
                            if (post.CommentCount == 0)
                            {
                                log.Info("Posting first comment ...");
                                Comment c = post.Comment(CommentText);
                                RedditUser pAuthor = post.Author;

                                log.Info("Sending OP a private message ...");
                                reddit.ComposePrivateMessage(PrivateMessageTitle, PrivateMessageBody, post.Author.Name);
                            }
                        }
                        else
                        {
                            log.Info("No keyword detected");
                        }
                    }
                    {
                        log.Info(String.Format("Skipping post: {0}", post.Title));
                    }
                }

                if (hasNewPost == false)
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

        }
    }
}
