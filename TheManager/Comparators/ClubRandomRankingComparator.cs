﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class ClubRandomRankingComparator : IComparer<Club>
    {
        public int Compare(Club x, Club y)
        {
            int nivX = (int)((100*x.Level()) * Session.Instance.Random(5,15)/10.0f );
            int nivY = (int)((100*y.Level()) * Session.Instance.Random(5, 15) / 10.0f);

            return nivY-nivX;
        }
    }
}