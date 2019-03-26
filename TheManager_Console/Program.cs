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
            Continent europe = new Continent("Europe");
            g.Continents.Add(europe);
            Pays p = new Pays("France");
            europe.Pays.Add(p);
            Ville v1 = new Ville("ville 1");
            Stade s1 = new Stade("stade 1", 2400, v1);
            Joueur j1 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.GARDIEN);
            Joueur j2 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.DEFENSEUR);
            Joueur j3 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.DEFENSEUR);
            Joueur j4 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.DEFENSEUR);
            Joueur j5 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.DEFENSEUR);
            Joueur j6 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.MILIEU);
            Joueur j7 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.MILIEU);
            Joueur j8 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.MILIEU);
            Joueur j9 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.MILIEU);
            Joueur j10 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.ATTAQUANT);
            Joueur j11 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.ATTAQUANT);
            Competition c = new Competition("Test", "logo.png", new DateTime(2000, 6, 25));
            Club_Ville c1 = new Club_Ville("FC 1", "FC1", 25, 2500, 140, 74, v1, 183, "logo1.png", s1);
            c1.AjouterJoueur(new Contrat(j1,1000,new DateTime(2021,1,1)));
            c1.AjouterJoueur(new Contrat(j2, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j3, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j4, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j5, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j6, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j7, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j8, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j9, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j10, 1000, new DateTime(2021, 1, 1)));
            c1.AjouterJoueur(new Contrat(j11, 1000, new DateTime(2021, 1, 1)));

            Joueur j21 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.GARDIEN);
            Joueur j22 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.DEFENSEUR);
            Joueur j23 = new Joueur("a", "a", new DateTime(1980, 1, 1), 36, 90, p, Poste.DEFENSEUR);
            Joueur j24 = new Joueur("a", "a", new DateTime(1980, 1, 1), 46, 90, p, Poste.DEFENSEUR);
            Joueur j25 = new Joueur("a", "a", new DateTime(1980, 1, 1), 56, 90, p, Poste.DEFENSEUR);
            Joueur j26 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.MILIEU);
            Joueur j27 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.MILIEU);
            Joueur j28 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.MILIEU);
            Joueur j29 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.MILIEU);
            Joueur j210 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.ATTAQUANT);
            Joueur j211 = new Joueur("a", "a", new DateTime(1980, 1, 1), 96, 90, p, Poste.ATTAQUANT);
            Ville v2 = new Ville("ville 2");
            Stade s2 = new Stade("stade 2", 2400, v2);
            Club_Ville c2 = new Club_Ville("FC 2", "FC2", 25, 2500, 140, 74, v2, 183, "logo2.png", s1);
            c2.AjouterJoueur(new Contrat(j21, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j22, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j23, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j24, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j25, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j26, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j27, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j28, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j29, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j210, 1000, new DateTime(2021, 1, 1)));
            c2.AjouterJoueur(new Contrat(j211, 1000, new DateTime(2021, 1, 1)));

            Match m = new Match(c1, c2, new DateTime(2018, 8, 3));
            m.Jouer();
            Console.WriteLine(m.Score1 + " - " + m.Score2);
            
            
            List<EvenementMatch> evenements = new List<EvenementMatch>(m.Evenements);
            evenements.Sort(new EvenementMatch_Temps_Comparator());
            foreach(EvenementMatch em in evenements)
            {
                int minutes = 45 * (em.MiTemps - 1) + em.Minute;
                Console.WriteLine("(" + em.MiTemps + ") " + minutes + " min, " + em.Type + ", pour " + em.Club.Nom + " de " + em.Joueur.Nom);
            }

            Console.ReadLine();

        }
    }
}
