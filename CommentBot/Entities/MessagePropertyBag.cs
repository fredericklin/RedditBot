using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrederickLin.RedditBot
{
    public class MessagePropertyBag
    {
        private string title = String.Empty;
        private string body = String.Empty;

        public MessagePropertyBag(string title, string body)
        {
            this.title = title;
            this.body = body;
        }

        public MessagePropertyBag(string body)
        {
            this.body = body;
        }

        public string Title { get { return title; } set { title = value; } }
        public string Body { get { return body; } set { body = value; } }
    }
}
