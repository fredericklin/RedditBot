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
    public static class TriggerOnNewMessage
    {
        public static Configuration config = Configuration.Instance;

        [FunctionName("TriggerOnNewMessage")]
        public static async Task Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, TraceWriter log)
        {
            bool newMessagesFound = false;

            // get all accounts in "Active" mode
            List<AccountPropertyBag> accounts = config.Accounts.Where(x => x.TriggerOnNewMessage == "Active").ToList();

            // iterate through each account
            foreach (AccountPropertyBag account in accounts)
            {
                log.Info("Checking " + account.UserName + "...");

                try
                {
                    // connect to Reddit
                    WebAgent agent = new BotWebAgent(account.UserName,
                        account.Password,
                        account.ClientId,
                        account.ClientSecret,
                        config.RedirectUrl);

                    Reddit reddit = new Reddit(agent, true);

                    Listing<PrivateMessage> messages = reddit.User.PrivateMessages;

                    foreach (PrivateMessage message in messages)
                    {
                        if (message.Unread)
                        {
                            // flag unread messages
                            newMessagesFound = true;

                            if (message.Body.Contains(config.TriggerRules.MessageBodyKeyword))
                            {
                                message.SetAsRead();
                                await message.ReplyAsync(account.Messages.First().Body);

                                log.Info(String.Format("Referral request detected from {0}!", message.Author));
                                log.Info(String.Format("Title: {0}", message.Subject));
                                log.Info(String.Format("Body: {0}", message.Body));
                            }
                            else
                            {
                                log.Info("Private message found but does not match search text. Message ignored.");
                            }

                        }
                    }

                    if (!newMessagesFound)
                    {
                        log.Info("No new private messages found.");
                    }

                }
                catch (Exception ex)
                {
                    log.Info("Unable to connect to account");
                }

            }

            log.Info($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
