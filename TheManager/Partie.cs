using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Partie
    {
        private DateTime _date;
        public DateTime Date { get { return _date; } }

        public Partie()
        {
            _date = new DateTime(2018, 07, 01);
        }

        public void Avancer()
        {
            _date.AddDays(1);
        }
    }
}