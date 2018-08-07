using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using RedditSharp;
using RedditSharp.Things;
using FrederickLin.RedditBot;
using System.Collections.Generic;

namespace FrederickLin.RedditBot
{
    public static class TriggerOnNewMessage
    {
        public static readonly string UserName = Configuration.GetValue("RedditUserName");
        public static readonly string Password = Configuration.GetValue("RedditPassword");
        public static readonly string ClientId = Configuration.GetValue("RedditClientId");
        public static readonly string ClientSecret = Configuration.GetValue("RedditClientSecret");
        public static readonly string RedirectUrl = Configuration.GetValue("RedditRedirectUrl");
        public static readonly string PrivateMessageBodyKeyword = Configuration.GetValue("PrivateMessageBodyKeyword");
        public static readonly string PrivateMessageBody = Configuration.GetValue("PrivateMessageBody");


        [FunctionName("TriggerOnNewMessage")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            bool newMessagesFound = false;

            // connect to Reddit
            WebAgent agent = new BotWebAgent(UserName, Password, ClientId, ClientSecret, RedirectUrl);
            Reddit reddit = new Reddit(agent, true);

            Listing<PrivateMessage> messages = reddit.User.PrivateMessages;
            
            foreach(PrivateMessage message in messages)
            {
                if(message.Unread)
                {
                    // flag unread messages
                    newMessagesFound = true;

                    if (message.Body.Contains(PrivateMessageBodyKeyword))
                    {
                        message.SetAsRead();
                        await message.ReplyAsync(PrivateMessageBody);
                        log.Info(String.Format("Referral request detected from {0}!", message.Author));
                        log.Info(String.Format("Title: {0}", message.Subject));
                        log.Info(String.Format("Body: {0}", message.Body));
                    } else
                    {
                        log.Info("Private message found but does not match search text. Message ignored.");
                    }

                }
            }

            if(!newMessagesFound)
            {
                log.Info("No new private messages found.");
            }
            
            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
