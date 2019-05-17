using System;
using System.Linq;
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
        public static Configuration config = Configuration.Instance;
        public static DateTimeOffset lastProcessed = DateTime.MinValue;
        public static String[] userIgnoreList;
        public static int userIgnoreListHash = 0;

        public static void RefreshIgnoreList(Subreddit sub, TraceWriter log)
        {
            try
            {
                WikiPage page = sub.Wiki.GetPage(config.Subreddit.UserIgnorePage);

                if (userIgnoreListHash != page.GetHashCode())
                {
                    log.Info("Ignore page changed. Caching new results.");

                    string pContent = page.MarkdownContent.Replace(" ", String.Empty).ToLower();
                    userIgnoreList = pContent.Split(new[] { "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    userIgnoreListHash = page.GetHashCode();
                }

            }
            catch
            {
                userIgnoreList = new String[0];
            }
        }

        [FunctionName("TriggerOnNewPost")]
        public static async Task Run([TimerTrigger("0 */3 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            log.Info("TriggerPostReplyAsComment: " + Convert.ToString(config.Subreddit.TriggerPostReplyAsComment));
            log.Info("TriggerPostReplyAsMessage: " + Convert.ToString(config.Subreddit.TriggerPostReplyAsMessage));

            // get all accounts in "Active" mode
            List<AccountPropertyBag> accounts = config.Accounts.Where(x => x.TriggerOnNewPost == "Active").ToList();
            
            // connect to Reddit
            WebAgent agent = new BotWebAgent(accounts.First().UserName,
                accounts.First().Password,
                accounts.First().ClientId,
                accounts.First().ClientSecret,
                config.RedirectUrl);

            Reddit reddit = new Reddit(agent, true);

            // retrive subreddit
            RedditSharp.Things.Subreddit sub = await reddit.GetSubredditAsync(config.Subreddit.Name);

            // get user ignore list from a wiki page
            RefreshIgnoreList(sub, log);

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

                        // process only if the following conditions are true:
                        // 1: The title contains the keyword
                        // 2: The post is less than 200 characters
                        // 3: The post does not contain a link
                        if (post.Title.ToLower().Contains(config.TriggerRules.PostTitleKeyword) &&
                            post.SelfText.Count() < 200 &&
                            !post.SelfText.ToLower().Contains("https://") &&
                            !post.SelfText.ToLower().Contains("http://"))
                        {
                            log.Info("Keyword detected ...");

                            // check to see if user is in the ignore list
                            bool authorInIgnoreList = userIgnoreList.Contains<String>(post.Author.Name.ToLower());

                            // ignore if poster is on ignore list
                            if (authorInIgnoreList)
                            {
                                log.Info(String.Format("{0} is in the ignore list. Ignoring ...", post.Author.Name));

                            }
                            else
                            {
                                // Reply as a comment
                                if(config.Subreddit.TriggerPostReplyAsComment)
                                {
                                    // ignore if there is a comment
                                    if (post.CommentCount > 0)
                                    {
                                        log.Info(String.Format("The post has already been commented on. Ignoring ...", post.Author.Name));
                                    }
                                    else
                                    {
                                        log.Info("Posting first comment  ...");
                                        Comment c = post.Comment(accounts.First().Comments.First());
                                        RedditUser pAuthor = post.Author;
                                    }
                                }

                                // Reply as a message
                                if(config.Subreddit.TriggerPostReplyAsMessage)
                                {
                                    log.Info("Sending OP a private message ...");
                                    reddit.ComposePrivateMessage(accounts.First().Messages.First().Title,
                                        accounts.First().Messages.First().Body,
                                        post.Author.Name);
                                }

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
