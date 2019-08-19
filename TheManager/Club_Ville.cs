using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{

    [DataContract]
    public struct OffreContrat
    {
        public int Salaire { get; set; }
        public int DureeContrat { get; set; }
        public Joueur Joueur { get; set; }

        public OffreContrat(Joueur joueur, int salaire, int dureeContrat)
        {
            Joueur = joueur;
            Salaire = salaire;
            DureeContrat = dureeContrat;
        }
    }

    [DataContract(IsReference =true)]
    public class GestionTransfertsClub
    {
        [DataMember]
        private List<Joueur> _joueursCibles;
        [DataMember]
        private List<OffreContrat> _offres;

        public List<Joueur> JoueursCibles { get => _joueursCibles; }
        public List<OffreContrat> Offres { get => _offres; }

        public GestionTransfertsClub()
        {
            _joueursCibles = new List<Joueur>();
            _offres = new List<OffreContrat>();
        }
    }


    [DataContract(IsReference =true)]
    public class Club_Ville : Club
    {

        [DataMember]
        private int _budget;
        [DataMember]
        private Ville _ville;
        [DataMember]
        private float _sponsor;
        [DataMember]
        private List<Contrat> _joueurs;
        [DataMember]
        private HistoriqueClub _historique;
        [DataMember]
        private GestionTransfertsClub _gestionTransfertsClub;
        [DataMember]
        private List<Club_Reserve> _reserves;
        [DataMember]
        private bool _equipePremiere;

        public int Budget { get => _budget; }
        public Ville Ville { get => _ville; }
        public float Sponsor { get => _sponsor; }
        public List<Contrat> Contrats { get => _joueurs; }
        public HistoriqueClub Historique { get => _historique; }
        public GestionTransfertsClub GestionTransfertsClub { get => _gestionTransfertsClub; }
        public List<Club_Reserve> Reserves { get => _reserves; }
        private bool EquipePremiere { get => _equipePremiere; }

        public float MasseSalariale
        {
            get
            {
                float res = 0;

                foreach (Contrat c in _joueurs) res += c.Salaire;

                return res;
            }
        }

        public Club_Ville(string nom, Entraineur entraineur, string nomCourt, int reputation, int budget, int supporters, int centreFormation, Ville ville, string logo, Stade stade, string musiqueBut, bool equipePremiere) : base(nom,entraineur,nomCourt,reputation,supporters,centreFormation,logo,stade,musiqueBut)
        {
            _budget = budget;
            _ville = ville;
            _sponsor = 0;
            _joueurs = new List<Contrat>();
            _historique = new HistoriqueClub();
            _gestionTransfertsClub = new GestionTransfertsClub();
            _equipePremiere = equipePremiere;
            _reserves = new List<Club_Reserve>();
        }

        public void AjouterJoueur(Contrat c)
        {
            _joueurs.Add(c);
        }

        public void RetirerJoueur(Joueur j)
        {
            Contrat aRetirer = null;

            foreach (Contrat ct in _joueurs) if (ct.Joueur == j) aRetirer = ct;

            if (aRetirer != null)
                _joueurs.Remove(aRetirer);
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

            
            List<Joueur> joueurs = new List<Joueur>();
            foreach (Contrat c in _joueurs) joueurs.Add(c.Joueur);
            joueurs.Sort(new Joueur_Niveau_Comparator());

            int total = 0;
            for(int i = 0;i<16;i++)
            {
                if (joueurs.Count > i)
                {
                    niveau += joueurs[i].Niveau;
                    total++;
                }
            }
            return niveau / (total+0.0f);
        }

        public void GenererJoueur(Poste p, int ageMin, int ageMax, int decalagePotentiel = 0)
        {
            string prenom = _ville.Pays().Langue.ObtenirPrenom();
            string nom = _ville.Pays().Langue.ObtenirNom();
            int anneeNaissance = Session.Instance.Random(Session.Instance.Partie.Date.Year - ageMax, Session.Instance.Partie.Date.Year - ageMin+1);

            //Potentiel
            int potentiel = Session.Instance.Random(CentreFormation - 18, CentreFormation + 18) + decalagePotentiel;
            if (potentiel < 1) potentiel = 1;
            if (potentiel > 99) potentiel = 99;

            //Niveau
            int age = Session.Instance.Partie.Date.Year - anneeNaissance;
            int diff = 24 - age;
            int niveau = potentiel;
            if (diff > 0) niveau -= 3 * diff;
            
            Joueur j = new Joueur(prenom, nom, new DateTime(anneeNaissance, Session.Instance.Random(1,13), Session.Instance.Random(1,29)), niveau, potentiel, this.Ville.Pays(), p);
            int annee = Session.Instance.Random(Session.Instance.Partie.Date.Year + 1, Session.Instance.Partie.Date.Year + 5);
            Contrats.Add(new Contrat(j, j.EstimerSalaire(), new DateTime(annee, 7, 1), new DateTime(Session.Instance.Partie.Date.Year, Session.Instance.Partie.Date.Month, Session.Instance.Partie.Date.Day)));
        }

        public void GenererJoueur(int ageMin, int ageMax)
        {
            Poste p = Poste.GARDIEN;
            int random = Session.Instance.Random(1, 13);
            if(random >= 2 && random <= 5) p = Poste.DEFENSEUR;
            if (random >= 6 && random <= 9) p = Poste.MILIEU;
            if (random >= 10) p = Poste.ATTAQUANT;
            GenererJoueur(p,ageMin,ageMax);
        }

        public void ModifierBudget(int somme)
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
            ModifierBudget((int)(Sponsor / 12));
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
            for (int i = 0; i<5; i++)
            {
                int prix = (int)(967.50471* Math.Pow(1.12867,CentreFormation));
                //Console.WriteLine(Nom + " doit payer " + prix + " euros pour ameliorer son centre.");
                if (_budget/3 > prix && CentreFormation<99)
                {
                    ModifierBudget(-prix);
                    _centreFormation++;
                }
            }
        }

        /// <summary>
        /// Génère quelques juniors chaque début d'année pour le club
        /// </summary>
        public void GenererJeunes()
        {

            int nb = Session.Instance.Random(1, 3);

            int nbJoueursClub = Contrats.Count;
            if(nbJoueursClub < 13)
            {
                nb = 13 - nbJoueursClub;
            }
            for(int i = 0; i<nb; i++)
            {
                GenererJoueur(17, 19);
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

            bool ageValide = true;
            if (ct.Joueur.Age > 32)
                if (Session.Instance.Random(1, 3) == 1) ageValide = false;

            bool assezBon = true;
            if (ct.Joueur.Age < 25 && ct.Joueur.Potentiel < Niveau() - 12)
                assezBon = false;
            else if (ct.Joueur.Age >= 25 && ct.Joueur.Niveau < Niveau() - 12)
                assezBon = false;

            if(_budget > 12*salaire && ageValide && assezBon)
            {
                res = true;
                int annee = Session.Instance.Random(Session.Instance.Partie.Date.Year + 1, Session.Instance.Partie.Date.Year + 5);
                ct.MettreAJour(salaire, new DateTime(annee, 7, 1));
            }

            return res;
        }

        public void MettreAJourListeTransferts()
        {
            float niveauClub = Niveau();
            foreach(Contrat ct in _joueurs)
            {
                //Si le joueur est trop mauvais
                if (ct.Joueur.Potentiel / niveauClub < 0.80) ct.Transferable = true;
                else ct.Transferable = false;
            }
        }

        public void RecevoirOffre(Contrat contrat, Club_Ville interessee, int somme, int salaire, int dureeContrat)
        {
            if(contrat.Transferable)
            {
                if(somme > contrat.Joueur.EstimerValeurTransfert())
                {
                    interessee.GestionTransfertsClub.Offres.Add(new OffreContrat(contrat.Joueur, salaire, dureeContrat));
                    //contrat.Joueur.Offres.Add(new OffreContrat(interessee, salaire, dureeContrat));
                }
            }
            else
            {
                if(somme > contrat.Joueur.EstimerValeurTransfert()*1.2f)
                {
                    interessee.GestionTransfertsClub.Offres.Add(new OffreContrat(contrat.Joueur, salaire, dureeContrat));
                    //contrat.Joueur.Offres.Add(new OffreContrat(interessee, salaire, dureeContrat));
                }
            }
        }

        /// <summary>
        /// Génère le calendrier des matchs amicaux du club avant le début de la compétition
        /// </summary>
        public void GenererCalendrierMatchsAmicaux()
        {
            Competition championnat = Championnat;
            List<Club> adversairesPossibles = new List<Club>();
            
            if(championnat != null && championnat.Tours[0] as TourChampionnat != null)
            {
                foreach (Club c in Session.Instance.Partie.Gestionnaire.Clubs)
                {
                    Club_Ville cv = c as Club_Ville;
                    if (cv != null && Utils.Distance(cv.Ville, Ville) < 300)
                    {
                        adversairesPossibles.Add(cv);
                    }

                }
                int nbMatchs = Session.Instance.Random(3, 6);
                if (nbMatchs > adversairesPossibles.Count) nbMatchs = adversairesPossibles.Count;
                for(int i = 0; i < nbMatchs; i++)
                {
                    Club adv = adversairesPossibles[Session.Instance.Random(0, adversairesPossibles.Count)];
                    adversairesPossibles.Remove(adv);
                    DateTime debut = new DateTime(championnat.Tours[0].Matchs[0].Jour.Year, championnat.Tours[0].Matchs[0].Jour.Month, championnat.Tours[0].Matchs[0].Jour.Day);
                    debut = debut.AddDays(Session.Instance.Random(-30, -10));
                    debut = debut.AddHours(Session.Instance.Random(14, 22));
                    Match match = new Match(this, adv, debut, false);
                    match.Reprogrammer(0);
                    Session.Instance.Partie.Gestionnaire.AjouterMatchAmical(match);
                }
            }
        }

        public void ConsiderationOffres()
        {
            foreach(OffreContrat oc in GestionTransfertsClub.Offres)
            {
                oc.Joueur.ConsidererOffre(oc, this);
            }

            GestionTransfertsClub.Offres.Clear();
        }

        public void FaireOffreJoueurs()
        {
            if(GestionTransfertsClub.JoueursCibles.Count > 0)
            {
                Joueur cible = GestionTransfertsClub.JoueursCibles[0];
                int salaire = (int)(cible.EstimerSalaire() * (Session.Instance.Random(80, 120) / 100.0f));
                GestionTransfertsClub.Offres.Add(new OffreContrat(cible, salaire, Session.Instance.Random(1, 5)));
            }
        }

        public void RechercherJoueursLibres()
        {
            GestionTransfertsClub.JoueursCibles.Clear();

            float niveau = Niveau();
            Pays paysClub = Ville.Pays();

            int chance = 100 - (int)niveau;

            int joueursARechercher = 20 - Contrats.Count;
            if (joueursARechercher < 0) joueursARechercher = 0;

            int i = 0;
            bool poursuivre = true;
            int joueursTrouves = 0;
            while(poursuivre)
            {
                Joueur j = Session.Instance.Partie.Gestionnaire.JoueursLibres[i];
                //Susceptible d'interesser le club
                if ((Session.Instance.Random(1,chance) == 1) && j.Niveau / niveau > 0.90f)
                {
                    bool peut = true;
                    //Si le joueur n'a pas un niveau pro, il faut qu'il soit du même pays
                    if (j.Niveau < 60 && j.Nationalite != paysClub) peut = false;
                    if(peut)
                    {
                        GestionTransfertsClub.JoueursCibles.Add(j);
                        joueursTrouves++;
                    }
                }
                i++;
                if (i == Session.Instance.Partie.Gestionnaire.JoueursLibres.Count || joueursTrouves == joueursARechercher)
                    poursuivre = false;
            }
            GestionTransfertsClub.JoueursCibles.Sort(new Joueur_Niveau_Comparator());
        }

        /// <summary>
        /// Complète une équipe à la fin de la période de transfert si le club n'a pas assez de joueurs pour faire la saison
        /// Bien entendu, les joueurs générés sont plus faibles
        /// </summary>
        public void CompleterEquipe()
        {
            List<Joueur> joueurs = ListerJoueurPoste(Poste.GARDIEN);
            if(joueurs.Count < 2)
            {
                for (int i = 0; i < 2 - joueurs.Count; i++) GenererJoueur(Poste.GARDIEN, 18, 22, -(int)(CentreFormation-(CentreFormation * 0.75f)));
            }
            joueurs = ListerJoueurPoste(Poste.DEFENSEUR);
            if (joueurs.Count < 5)
            {
                for (int i = 0; i < 5 - joueurs.Count; i++) GenererJoueur(Poste.DEFENSEUR, 18, 22, -(int)(CentreFormation - (CentreFormation * 0.75f)));
            }
            
            joueurs = ListerJoueurPoste(Poste.MILIEU);
            if (joueurs.Count < 5)
            {
                for (int i = 0; i < 5 - joueurs.Count; i++) GenererJoueur(Poste.MILIEU, 18, 22, -(int)(CentreFormation - (CentreFormation * 0.75f)));
            }
            joueurs = ListerJoueurPoste(Poste.ATTAQUANT);
            if (joueurs.Count < 5)
            {
                for (int i = 0; i < 5 - joueurs.Count; i++) GenererJoueur(Poste.ATTAQUANT, 18, 22, -(int)(CentreFormation - (CentreFormation * 0.75f)));
            }
        }

        /// <summary>
        /// Dispatche les joueurs de l'équipe parmi l'équipe première et les réserves
        /// </summary>
        public void RemplirEquipesReserves()
        {
            if(_reserves.Count > 0)
            {

                List<Joueur>[] joueurs = new List<Joueur>[1+_reserves.Count];
                for (int i = 0; i < joueurs.Length; i++) joueurs[i] = new List<Joueur>();

                List<Contrat>[] contrats = new List<Contrat>[1 + _reserves.Count];
                for (int i = 0; i < contrats.Length; i++) contrats[i] = new List<Contrat>();

                List<Contrat> equipeComplete = new List<Contrat>(_joueurs);
                foreach (Club_Reserve cr in _reserves) foreach (Contrat ct in cr.Contrats) equipeComplete.Add(ct);

                List<Joueur> joueursComplets = new List<Joueur>(Joueurs());
                foreach (Club_Reserve cr in _reserves) foreach (Joueur j in cr.Joueurs()) joueursComplets.Add(j);

                int[] equipePremiereQuotas = new int[4] { 3, 6, 6, 4 };
                int[] equipesReservesQuotas = new int[4] { 2, 5, 5, 4 };
                Poste[] postes = new Poste[4] { Poste.GARDIEN, Poste.DEFENSEUR, Poste.MILIEU, Poste.ATTAQUANT };

                //Pour tous les postes
                for(int numposte = 0; numposte < 4; numposte++)
                {
                    Poste poste = postes[numposte];
                    int quotaEquipePremiere = equipePremiereQuotas[numposte];
                    int quotaEquipeReserve = equipesReservesQuotas[numposte];

                    List<Joueur> joueursPoste = Utils.JoueursPoste(joueursComplets, poste);
                    joueursPoste.Sort(new Joueur_Niveau_Comparator());
                    //Equipe première
                    for (int i = 0; i < quotaEquipePremiere; i++)
                        if (joueursPoste.Count > 0)
                        {
                            joueurs[0].Add(joueursPoste[0]);
                            joueursPoste.RemoveAt(0);
                        }
                    //Pour les équipes réserves : 2 gardiens
                    for (int i = 1; i < _reserves.Count + 1; i++)
                    {
                        for (int j = 0; j < quotaEquipeReserve; j++)
                            if (joueursPoste.Count > 0)
                            {
                                joueurs[i].Add(joueursPoste[0]);
                                joueursPoste.RemoveAt(0);
                            }
                    }

                    //Les joueurs qui restent
                    while(joueursPoste.Count>0)
                    {
                        joueurs[joueurs.Length - 1].Add(joueursPoste[0]);
                        joueursPoste.RemoveAt(0);
                    }

                }

                //Récupérer les contrats associés aux joueurs
                for(int i = 0; i<joueurs.Length; i++)
                {
                    foreach(Joueur j in joueurs[i])
                    {
                        Contrat ct = null;
                        foreach (Contrat c in equipeComplete) if (c.Joueur == j) ct = c;

                        contrats[i].Add(ct);
                    }
                }

                //Répartir les joueurs dans les différentes équipes
                _joueurs.Clear();
                foreach(Contrat ct in contrats[0]) _joueurs.Add(ct);
                
                for (int i = 1; i<_reserves.Count+1;i++)
                {
                    _reserves[i - 1].Contrats.Clear();
                    foreach (Contrat ct in contrats[i]) _reserves[i - 1].Contrats.Add(ct);
                }

            }

        }

    }
}