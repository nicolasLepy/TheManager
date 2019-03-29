using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager.Comparators
{
    public class Joueur_Niveau_Comparator : IComparer<Joueur>
    {
        public int Compare(Joueur x, Joueur y)
        {
            return y.Niveau - x.Niveau;
        }
    }
}