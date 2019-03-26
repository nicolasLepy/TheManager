using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheManager.Comparators
{
    public class Joueur_Composition_Comparator : IComparer<Joueur>
    {
        public int Compare(Joueur x, Joueur y)
        {
            return (int)(y.Niveau * ((y.Energie/200.0f)+0.5f) - x.Niveau * ((x.Energie/200.0f)+0.5f));
        }
    }
}