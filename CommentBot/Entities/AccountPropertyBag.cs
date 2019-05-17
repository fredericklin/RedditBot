using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FrederickLin.RedditBot
{
    public class AccountPropertyBag
    {
        private List<string> comments;
        private List<MessagePropertyBag> messages;

        public AccountPropertyBag() {
            comments = new List<string>();
            messages = new List<MessagePropertyBag>();
        }

        public AccountPropertyBag(JObject j)
        {
            comments = new List<string>();
            messages = new List<MessagePropertyBag>();
        }

        public override string ToString() =>
            $"{UserName}";

        public string UserName { get; set; }
        public string ClientId { get; set; }
        public string Password { get; set; }
        public string ClientSecret { get; set; }
        public string TriggerOnNewPost { get; set; }
        public string TriggerOnNewMessage { get; set; }
        public List<string> Comments { get { return comments; } set { comments = value; } }
        public List<MessagePropertyBag> Messages { get { return messages; } set { messages = value; } }



    }
}
