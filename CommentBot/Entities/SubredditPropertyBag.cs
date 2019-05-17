using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrederickLin.RedditBot
{
    public class SubredditPropertyBag
    {
        public SubredditPropertyBag()
        {

        }

        /// <summary>
        /// The subreddit name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Wiki page of users to ignore. Carriage return delimited.
        /// </summary>
        public string UserIgnorePage { get; set; }

        /// <summary>
        /// If a reddit post matches trigger rule, do you reply to post as a comment?
        /// </summary>
        public bool TriggerPostReplyAsComment { get; set; }

        /// <summary>
        /// If a reddit post matches trigger rule, do you reply to post as a private message?
        /// </summary>
        public bool TriggerPostReplyAsMessage { get; set; }
        
    }
}
