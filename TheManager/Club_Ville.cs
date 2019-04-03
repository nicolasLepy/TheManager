using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TheManager
{
    public class Club_Ville : Club
    {

        private float _budget;
        private Ville _ville;
        private float _sponsor;
        private List<Contrat> _joueurs;

        public float Budget { get => _budget; }
        public Ville Ville { get => _ville; }
        public float Sponsor { get => _sponsor; }
        public List<Contrat> Contrats { get => _joueurs; }

        public Club_Ville(string nom, string nomCourt, int reputation, float budget, float supporters, int centreFormation, Ville ville, string logo, Stade stade) : base(nom,nomCourt,reputation,supporters,centreFormation,logo,stade)
        {
            _budget = budget;
            _ville = ville;
            _sponsor = 0;
            _joueurs = new List<Contrat>();
        }

        public void AjouterJoueur(Contrat c)
        {
            _joueurs.Add(c);
        }

        public override List<Joueur> Joueurs()
        {
            List<Joueur> joueurs = new List<Joueur>();
            foreach (Contrat c in _joueurs)
                joueurs.Add(c.Joueur);
            return joueurs;
        }

        public override float Niveau()
        {
            float niveau = 0;
            foreach(Contrat c in _joueurs)
            {
                niveau += c.Joueur.Niveau;
            }
            return niveau / (_joueurs.Count+0.0f);
        }

        public void GenererJoueur(Poste p)
        {
            Joueur j = new Joueur(Session.Instance.Partie.Gestionnaire.Langues[0].ObtenirPrenom(), Session.Instance.Partie.Gestionnaire.Langues[0].ObtenirNom(), new DateTime(1990, 1, 1), CentreFormation, CentreFormation + 2, this.Ville.Pays(), p);
            int annee = Session.Instance.Random(Session.Instance.Partie.Date.Year + 1, Session.Instance.Partie.Date.Year + 5);
            Contrats.Add(new Contrat(j, j.EstimerSalaire(), new DateTime(annee, 7, 1)));
        }

        public void GenererJoueur()
        {
            Poste p = Poste.GARDIEN;
            int random = Session.Instance.Random(1, 12);
            if(random >= 2 && random <= 5) p = Poste.DEFENSEUR;
            if (random >= 6 && random <= 9) p = Poste.MILIEU;
            if (random >= 10) p = Poste.ATTAQUANT;
            GenererJoueur(p);
        }

        public void ModifierBudget(float somme)
        {
            _budget += somme;
        }
        
        public void PayerSalaires()
        {
            foreach(Contrat ct in Contrats)
            {
                ModifierBudget(-ct.Salaire);
            }
        }

        public void SubvensionSponsor()
        {
            ModifierBudget(Sponsor / 12);
        }

        public void ObtenirSponsor()
        {
            int sponsor = 0;
            float niveau = Niveau();
            if (niveau < 1000) sponsor = Session.Instance.Random(5000, 14000);
            else if (niveau < 3000) sponsor = Session.Instance.Random(85000, 323000);
            else if (niveau < 4000) sponsor = Session.Instance.Random(200000, 500000);
            else if (niveau < 5000) sponsor = Session.Instance.Random(500000, 800000);
            else if (niveau < 6000) sponsor = Session.Instance.Random(800000, 2500000);
            else if (niveau < 7000) sponsor = Session.Instance.Random(2500000, 6500000);
            else if (niveau < 8000) sponsor = Session.Instance.Random(6000000, 14000000);
            else if (niveau < 9000) sponsor = Session.Instance.Random(14000000, 23000000);
            else sponsor = Session.Instance.Random(23000000, 40000000);
            _sponsor = sponsor;
        }

        /// <summary>
        /// Fin d'année : mise à jour du centre de formation par le club
        /// Baisse de niveau randomly
        /// Si le club à de l'argent il peut le renflouer
        /// </summary>
        public void MiseAJourCentreFormation()
        {
            _centreFormation -= Session.Instance.Random(1, 3);
            if (_centreFormation < 1)
                _centreFormation = 1;
            for(int i = 0; i<5; i++)
            {
                if(_budget > 800*(CentreFormation*CentreFormation) && CentreFormation<99)
                {
                    ModifierBudget(-800 * CentreFormation * CentreFormation);
                    _centreFormation++;
                }
            }
        }

        /// <summary>
        /// Tente de prolonger un contrat
        /// </summary>
        /// <param name="ct">Le contrat à prolonger</param>
        /// <returns>Vrai si le contrat a été prolongé, faux sinon</returns>
        public bool Prolonger(Contrat ct)
        {
            bool res = false;
            int salaire = ct.Joueur.EstimerSalaire();
            if (salaire < ct.Salaire) salaire = ct.Salaire;
            salaire = (int)(salaire * (Session.Instance.Random(10, 14) / (10.0f)));

            if(_budget > 12*salaire)
            {
                res = true;
                int annee = Session.Instance.Random(Session.Instance.Partie.Date.Year + 1, Session.Instance.Partie.Date.Year + 5);
                ct.MettreAJour(salaire, new DateTime(annee, 7, 1));
            }

            return res;
        }
    }
}