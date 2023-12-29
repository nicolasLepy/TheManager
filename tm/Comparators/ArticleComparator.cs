using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace tm.Comparators
{
    public class ArticleComparator : IComparer<Article>
    {
        public int Compare(Article x, Article y)
        {
            return DateTime.Compare(x.publication, y.publication);
        }
    }
}