using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace FrederickLin.RedditBot
{
    public static class Configuration
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetValue(string name)
        {
            return ConfigurationManager.AppSettings[name];
        }
    }
}
