﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Utils
    {

        public static bool ComparerDates(DateTime a, DateTime b)
        {
            bool res = false;

            if (a.Year == b.Year && a.Month == b.Month && a.Day == b.Day) res = true;
            return res;
        }

    }
}