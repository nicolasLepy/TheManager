using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{

    public struct EntreeHistorique
    {
        public DateTime Date { get; set; }
        public int Budget { get; set; }
        public int CentreFormation { get; set; }

        public EntreeHistorique(DateTime date, int budget, int centreFormation)
        {
            Date = date;
            Budget = budget;
            CentreFormation = centreFormation;
        }
    }

    public class HistoriqueClub
    {
        private List<EntreeHistorique> _elements;

        public List<EntreeHistorique> Elements { get => _elements; }

        public HistoriqueClub()
        {
            _elements = new List<EntreeHistorique>();
        }
    }
}