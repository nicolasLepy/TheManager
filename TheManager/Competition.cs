using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Competition
    {


        

        private string _nom;
        private List<Tour> _tours;
        private string _logo;
        private DateTime _debutSaison;

        public string Nom { get => _nom; }
        public List<Tour> Tours { get => _tours; }
        public string Logo { get => _logo; }
        public int TourActuel { get; set; }
        public DateTime DebutSaison { get { return _debutSaison; } }

        public Competition(string nom, string logo, DateTime debutSaison)
        {
            _tours = new List<Tour>();
            _nom = nom;
            _logo = logo;
            _debutSaison = debutSaison;
            TourActuel = 0;
        }

    }
}