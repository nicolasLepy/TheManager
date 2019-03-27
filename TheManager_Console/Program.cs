using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager;
using TheManager.Comparators;

namespace TheManager_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Partie partie = new Partie();
            Session.Instance.Partie = partie;
            Gestionnaire g = partie.Gestionnaire;
            ChargeurBaseDeDonnees cbdd = new ChargeurBaseDeDonnees(g);
            cbdd.Charger();

            /*
            Match m = new Match(null, null, new DateTime(2018, 8, 3));
            m.Jouer();
            Console.WriteLine(m.Score1 + " - " + m.Score2);
            
            
            List<EvenementMatch> evenements = new List<EvenementMatch>(m.Evenements);
            evenements.Sort(new EvenementMatch_Temps_Comparator());
            foreach(EvenementMatch em in evenements)
            {
                int minutes = 45 * (em.MiTemps - 1) + em.Minute;
                Console.WriteLine("(" + em.MiTemps + ") " + minutes + " min, " + em.Type + ", pour " + em.Club.Nom + " de " + em.Joueur.Nom);
            }
            */
            Console.ReadLine();

        }
    }
}
