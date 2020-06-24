﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Club_Niveau_Comparator : IComparer<Club>
    {

        private bool _envers;

        public Club_Niveau_Comparator(bool aLEnvers = false)
        {
            _envers = aLEnvers;
        }

        public int Compare(Club x, Club y)
        {
            int res;
            if (Math.Abs(x.Level() - y.Level()) < 0.01) res = 0;
            else res = (int)(y.Level() - x.Level());
            if (_envers)
                res = -res;
            return res;
        }
    }
}
