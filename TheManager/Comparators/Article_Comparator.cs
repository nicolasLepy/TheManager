using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class Article_Comparator : IComparer<Article>
    {
        public int Compare(Article x, Article y)
        {
            return DateTime.Compare(x.Publication, y.Publication);
        }
    }
}