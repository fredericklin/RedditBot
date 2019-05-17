using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrederickLin.RedditBot
{
    public class UserIgnoreList
    {
        private int sourceHash = 0;
        private String[] ignoreList;

        #region Accessors
        public int SourceHash
        {
            get { return sourceHash; }
            set { sourceHash = value; }
        }
        #endregion
    }
}
