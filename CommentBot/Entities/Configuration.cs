using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Newtonsoft.Json.Linq;

namespace FrederickLin.RedditBot
{
    /// <summary>
    /// Singleton pattern for a configuration class
    /// http://csharpindepth.com/Articles/General/Singleton.aspx
    /// </summary>
    public sealed class Configuration
    {
        private string redirectUrl;
        private SubredditPropertyBag subreddit;
        private TriggerRules triggerRules;
        private List<AccountPropertyBag> accounts;

        private AzureServiceTokenProvider s2sAuth;
        private string vaultAddress;


        private static readonly Lazy<Configuration> instance = new Lazy<Configuration>(() => new Configuration());

        private Configuration()
        {
            // initialize token provider using Managed Services Identity
            s2sAuth = new AzureServiceTokenProvider();
            vaultAddress = ConfigurationManager.AppSettings["KeyVaultAddress"];

            // load the "Accounts" JSON from RedditAccounts
            JObject accountConfig = JObject.Parse(ConfigurationManager.AppSettings["RedditAccounts"]);
            accounts = new List<AccountPropertyBag>();

            foreach (var a in accountConfig["Accounts"])
            {
                AccountPropertyBag account = new AccountPropertyBag();

                // account properties
                account.UserName = Convert.ToString(a["UserName"]);
                account.ClientId = Convert.ToString(a["ClientId"]);
                account.TriggerOnNewPost = Convert.ToString(a["TriggerOnNewPost"]);
                account.TriggerOnNewMessage = Convert.ToString(a["TriggerOnNewMessage"]);

                // account secrets
                account.Password = GetSecret(account.UserName.ToLower() + "-redditpassword");
                account.ClientSecret = GetSecret(account.UserName.ToLower() + "-redditclientsecret");

                // comments
                foreach(string c in (JArray)a["Comments"])
                {
                    account.Comments.Add(c);
                }

                // private messages
                foreach(var m in (JArray)a["Messages"])
                {
                    MessagePropertyBag message = new MessagePropertyBag(
                        Convert.ToString(m["Title"]),
                        Convert.ToString(m["Body"]));

                    account.Messages.Add(message);
                }

                accounts.Add(account);
            }

            redirectUrl = ConfigurationManager.AppSettings["RedditRedirectUrl"];

            subreddit = new SubredditPropertyBag();
            subreddit.Name = ConfigurationManager.AppSettings["SubredditName"];
            subreddit.UserIgnorePage = ConfigurationManager.AppSettings["SubredditUserIgnorePage"];
            subreddit.TriggerPostReplyAsComment = Convert.ToBoolean(ConfigurationManager.AppSettings["SubredditPostReplyAsComment"]);
            subreddit.TriggerPostReplyAsMessage = Convert.ToBoolean(ConfigurationManager.AppSettings["SubredditPostReplyAsMessage"]);

            triggerRules = new TriggerRules();
            triggerRules.PostTitleKeyword = ConfigurationManager.AppSettings["PostTitleKeyword"];
            triggerRules.MessageBodyKeyword = ConfigurationManager.AppSettings["PrivateMessageBodyKeyword"];
        }

        private string GetSecret(string secretName)
        {
            string secretValue = String.Empty;
            try
            {
                KeyVaultClient kv = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(s2sAuth.KeyVaultTokenCallback));
                secretValue = kv.GetSecretAsync(vaultAddress, secretName).Result.Value;

            } catch { }

            return secretValue;

        }

        #region Accessors
        public static Configuration Instance { get { return instance.Value; } }
        public string RedirectUrl { get { return redirectUrl; } }
        public SubredditPropertyBag Subreddit { get { return subreddit; } }
        public TriggerRules TriggerRules { get { return triggerRules; } }
        public List<AccountPropertyBag> Accounts { get { return accounts; } }
        #endregion


    }
}