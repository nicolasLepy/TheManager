using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class TourPoules : Tour
    {
        private int _nombrePoules;
        private List<Club>[] _poules;
        private int _qualifiesParPoule;

        public int NombrePoules { get => _nombrePoules; }
        public int QualifiesParPoule { get => _qualifiesParPoule; }

        public TourPoules(string nom, Heure heure, List<DateTime> dates, List<DecalagesTV> decalages, int nombrePoules, bool allerRetour, int qualifiesParPoule) : base(nom, heure, dates, decalages, allerRetour)
        {
            _nombrePoules = nombrePoules;
            _poules = new List<Club>[_nombrePoules];
            for (int i = 0; i < _nombrePoules; i++)
            {
                _poules[i] = new List<Club>();
            }
            _qualifiesParPoule = qualifiesParPoule;
        }
    }
}