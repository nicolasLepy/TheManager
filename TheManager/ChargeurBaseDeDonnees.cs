using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace TheManager
{
    public class ChargeurBaseDeDonnees
    {
        private Gestionnaire _gestionnaire;
        public ChargeurBaseDeDonnees(Gestionnaire gestionnaire)
        {
            _gestionnaire = gestionnaire;
        }

        public void Charger()
        {
            ChargerLangues();
            ChargerGeographie();
            ChargerVilles();
            ChargerStades();
            ChargerClubs();
            ChargerCompetitions();
            ChargerJoueurs();
            InitialiserEquipes();
            InitialiserJoueurs();
        }

        public void ChargerJoueurs()
        {
            XDocument doc = XDocument.Load("Donnees/joueurs.xml");
            foreach (XElement e in doc.Descendants("Joueurs"))
            {
                foreach (XElement e2 in e.Descendants("Joueur"))
                {
                    string nom = e2.Attribute("nom").Value;
                    string prenom = e2.Attribute("prenom").Value;
                    int niveau = int.Parse(e2.Attribute("niveau").Value);
                    string nomClub = e2.Attribute("club").Value;
                    Club_Ville club = _gestionnaire.String2Club(nomClub) as Club_Ville;
                    Poste poste = Poste.GARDIEN;
                    string nomPoste = e2.Attribute("poste").Value;
                    switch(nomPoste)
                    {
                        case "DEFENSEUR": poste = Poste.DEFENSEUR; break;
                        case "MILIEU": poste = Poste.MILIEU; break;
                        case "ATTAQUANT": poste = Poste.ATTAQUANT; break;
                    }
                    Joueur j = new Joueur(prenom, nom, new DateTime(1990, 1, 1), niveau, niveau + 5, _gestionnaire.String2Pays("France"), poste);
                    club.AjouterJoueur(new Contrat(j, j.EstimerSalaire(), new DateTime(Session.Instance.Random(2019,2024), 7, 1)));
                }
            }
        }

            public void ChargerVilles()
        {
            XDocument doc = XDocument.Load("Donnees/villes.xml");
            foreach (XElement e in doc.Descendants("Villes"))
            {
                foreach (XElement e2 in e.Descendants("Ville"))
                {
                    string nom = e2.Element("Nom").Value;
                    int population = int.Parse(e2.Element("Population").Value);
                    float lat = float.Parse(e2.Element("Latitute").Value, CultureInfo.InvariantCulture);
                    float lon = float.Parse(e2.Element("Longitude").Value, CultureInfo.InvariantCulture);
                    _gestionnaire.String2Pays("France").Villes.Add(new Ville(nom, population, lat, lon));

                }
            }
        }

        public void ChargerGeographie()
        {
            XDocument doc = XDocument.Load("Donnees/continents.xml");
            foreach (XElement e in doc.Descendants("Monde"))
            {
                foreach (XElement e2 in e.Descendants("Continent"))
                {
                    string nom_continent = e2.Attribute("nom").Value;
                    Continent c = new Continent(nom_continent);
                    foreach(XElement e3 in e2.Descendants("Pays"))
                    {
                        string nom_pays = e3.Attribute("nom").Value;
                        string langue = e3.Attribute("langue").Value;
                        Langue l = _gestionnaire.String2Langue(langue);
                        Pays p = new Pays(nom_pays,l);
                        foreach(XElement e4 in e3.Descendants("Ville"))
                        {
                            string nom_ville = e4.Attribute("nom").Value;
                            int population = int.Parse(e4.Attribute("population").Value);
                            float lat = float.Parse(e4.Attribute("Latitute").Value);
                            float lon = float.Parse(e4.Attribute("Longitude").Value);

                            Ville v = new Ville(nom_ville, population, lat, lon);
                            p.Villes.Add(v);
                        }
                        c.Pays.Add(p);
                    }
                    _gestionnaire.Continents.Add(c);
                }
            }
        }

        public void ChargerStades()
        {
            XDocument doc = XDocument.Load("Donnees/stades.xml");
            foreach (XElement e in doc.Descendants("Stades"))
            {
                foreach (XElement e2 in e.Descendants("Stade"))
                {
                    string nom = e2.Attribute("nom").Value;
                    int capacite = int.Parse(e2.Attribute("capacite").Value);
                    string nom_ville = e2.Attribute("ville").Value;
                    Ville v = _gestionnaire.String2Ville(nom_ville);
                    Stade s = new Stade(nom, capacite, v);
                    v.Pays().Stades.Add(s);
                }
            }
        }

        public void ChargerClubs()
        {
            XDocument doc = XDocument.Load("Donnees/clubs.xml");
            
            foreach(XElement e in doc.Descendants("Clubs"))
            {
                foreach (XElement e2 in e.Descendants("Club"))
                {
                    string nom = e2.Attribute("nom").Value;
                    string nomCourt = e2.Attribute("nomCourt").Value;
                    int reputation = int.Parse(e2.Attribute("reputation").Value);
                    int budget = int.Parse(e2.Attribute("budget").Value);
                    int supporters = int.Parse(e2.Attribute("supporters").Value);

                    string nom_ville = e2.Attribute("ville").Value;
                    Ville ville = _gestionnaire.String2Ville(nom_ville);

                    Stade stade = null;
                    if (e2.Attribute("stade") != null)
                    {
                        string nom_stade = e2.Attribute("stade").Value;
                        stade = _gestionnaire.String2Stade(nom_stade);
                        if(stade == null)
                        {
                            int capacite = 1000;
                            if (ville != null) capacite = ville.Population / 10;
                            stade = new Stade(nom_stade, capacite, ville);
                            if (ville != null)
                            {
                                ville.Pays().Stades.Add(stade);
                            }
                            else
                            {
                                Console.WriteLine("La ville " + nom_ville + " n'existe pas.");
                            }
                        }
                    }
                    if (stade == null)
                        stade = new Stade("Stade de " + nomCourt, ville.Population / 10, ville);

                    int centreFormation = int.Parse(e2.Attribute("centreFormation").Value);
                    string logo = e2.Attribute("logo").Value;
                    Club c = new Club_Ville(nom, nomCourt, reputation, budget, supporters, centreFormation, ville, logo, stade);
                    _gestionnaire.Clubs.Add(c);
                }
                foreach (XElement e2 in e.Descendants("Selection"))
                {
                    string nom = e2.Attribute("nom").Value;
                    string nomCourt = nom;
                    int reputation = int.Parse(e2.Attribute("reputation").Value);
                    int supporters = int.Parse(e2.Attribute("supporters").Value);
                    Pays pays = _gestionnaire.String2Pays(e2.Attribute("pays").Value);

                    Stade stade = null;
                    if (e2.Attribute("stade") != null)
                    {
                        string nom_stade = e2.Attribute("stade").Value;
                        stade = _gestionnaire.String2Stade(nom_stade);
                    }
                    if (stade == null)
                        stade = new Stade("Stade de " + nomCourt, 15000, null);
                    int centreFormation = int.Parse(e2.Attribute("centreFormation").Value);
                    string logo = e2.Attribute("logo").Value;
                    float coefficient = float.Parse(e2.Attribute("coefficient").Value);
                    Club c = new SelectionNationale(nom, nomCourt, reputation, supporters, centreFormation, logo, stade, coefficient,pays);
                    _gestionnaire.Clubs.Add(c);
                }
            }
        }

        public void ChargerCompetitions()
        {
            XDocument doc = XDocument.Load("Donnees/competitions.xml");

            //Chargement préliminaire de toutes les compétitons pour les référancer
            foreach (XElement e in doc.Descendants("Competitions"))
            {
                foreach (XElement e2 in e.Descendants("Competition"))
                {
                    string nom = e2.Attribute("nom").Value;
                    string nomCourt = e2.Attribute("nomCourt").Value;
                    string logo = e2.Attribute("logo").Value;
                    string debutSaison = e2.Attribute("debut_saison").Value;
                    bool championnat = e2.Attribute("championnat").Value == "oui" ? true : false;
                    int niveau = int.Parse(e2.Attribute("niveau").Value);
                    DateTime debut = String2Date(debutSaison);
                    Competition c = new Competition(nom, logo, debut, nomCourt, championnat, niveau);
                    _gestionnaire.Competitions.Add(c);
                }
            }

            //Chargement détaillé de toutes les compétitions
            foreach (XElement e in doc.Descendants("Competitions"))
            {
                foreach(XElement e2 in e.Descendants("Competition"))
                {
                    string nom = e2.Attribute("nom").Value;
                    Competition c = _gestionnaire.String2Competition(nom);
                    foreach(XElement e3 in e2.Descendants("Tour"))
                    {
                        Tour tour = null;
                        string type = e3.Attribute("type").Value;
                        string nomTour = e3.Attribute("nom").Value;
                        bool allerRetour = e3.Attribute("allerRetour").Value == "oui" ? true : false;
                        string heureParDefaut = e3.Attribute("heureParDefaut").Value;
                        DateTime date_initialisation = String2Date(e3.Attribute("initialisation").Value);
                        DateTime date_fin = String2Date(e3.Attribute("fin").Value);
                        List<DateTime> dates = new List<DateTime>();
                        bool onPeut = true;
                        int i = 1;
                        while (onPeut)
                        {
                            if (e3.Attribute("j" + i) != null)
                            {
                                string dateJ = e3.Attribute("j" + i).Value;
                                DateTime dt = String2Date(dateJ);
                                i++;
                                dates.Add(dt);
                            }
                            else
                            {
                                onPeut = false;
                            }
                        }

                        if (type == "championnat")
                        {
                            tour = new TourChampionnat(nomTour, String2Heure(heureParDefaut), dates, allerRetour,new List<DecalagesTV>(), date_initialisation, date_fin);
                        }
                        else if(type=="elimination")
                        {
                            tour = new TourElimination(nomTour, String2Heure(heureParDefaut), dates, new List<DecalagesTV>(), allerRetour, date_initialisation, date_fin);
                        }
                        else if(type =="poules")
                        {
                            int nbpoules = int.Parse(e3.Attribute("nombrePoules").Value);
                            tour = new TourPoules(nomTour, String2Heure(heureParDefaut), dates, new List<DecalagesTV>(), nbpoules, allerRetour, date_initialisation, date_fin);
                        }
                        else if (type == "inactif")
                        {
                            tour = new TourInactif(nomTour, String2Heure(heureParDefaut), date_initialisation, date_fin);
                        }
                        c.Tours.Add(tour);
                        foreach (XElement e4 in e3.Descendants("Club"))
                        {
                            string nomClub = e4.Attribute("nom").Value;
                            tour.Clubs.Add(_gestionnaire.String2Club(nomClub));
                        }
                        foreach(XElement e4 in e3.Descendants("Participants"))
                        {
                            int nombre = int.Parse(e4.Attribute("nombre").Value);
                            IEquipesRecuperables source = null;
                            XAttribute continent = e4.Attribute("continent");
                            if(continent != null)
                            {
                                source = _gestionnaire.String2Continent(continent.Value);
                            }
                            else
                            {
                                string nomCompetition = e4.Attribute("competition").Value;
                                int indexTour = int.Parse(e4.Attribute("idTour").Value);
                                Competition comp = _gestionnaire.String2Competition(nomCompetition);
                                Tour t = comp.Tours[indexTour];
                                source = t;
                            }
                            MethodeRecuperation methode = MethodeRecuperation.ALEATOIRE;
                            switch(e4.Attribute("methode").Value)
                            {
                                case "meilleurs": methode = MethodeRecuperation.MEILLEURS; break;
                                case "pires": methode = MethodeRecuperation.PIRES; break;
                                case "aleatoire": methode = MethodeRecuperation.ALEATOIRE; break;
                            }
                            tour.RecuperationEquipes.Add(new RecuperationEquipes(source, nombre,methode));
                        }
                        foreach (XElement e4 in e3.Descendants("Decalage"))
                        {
                            int jour = int.Parse(e4.Attribute("jour").Value);
                            Heure heure = String2Heure(e4.Attribute("heure").Value);
                            DecalagesTV dtv = new DecalagesTV(jour, heure);
                            tour.Programmation.DecalagesTV.Add(dtv);
                        }
                        foreach (XElement e4 in e3.Descendants("Regle"))
                        {
                            Regle regle = Regle.RECOIT_SI_DEUX_DIVISION_ECART;
                            switch(e4.Attribute("nom").Value)
                            {
                                case "RECOIT_SI_DEUX_DIVISION_ECART": regle = Regle.RECOIT_SI_DEUX_DIVISION_ECART; break;
                            }
                            tour.Regles.Add(regle);
                        }
                        foreach (XElement e4 in e3.Descendants("Dotation"))
                        {
                            int classement = int.Parse(e4.Attribute("classement").Value);
                            int somme = int.Parse(e4.Attribute("somme").Value);
                            tour.Dotations.Add(new Dotation(classement, somme));
                        }
                        foreach (XElement e4 in e3.Descendants("Qualification"))
                        {
                            int id_tour = int.Parse(e4.Attribute("id_tour").Value);
                            bool anneeSuivante = false;
                            if (e4.Attribute("anneeSuivante") != null) anneeSuivante = e4.Attribute("anneeSuivante").Value == "oui" ? true : false;
                            Competition competitionCible = null;
                            if (e4.Attribute("competition") != null) competitionCible = _gestionnaire.String2Competition(e4.Attribute("competition").Value);
                            else competitionCible = c;

                            //Deux cas
                            //1- On a un attribut "classement", avec un classement précis
                            //2- On a deux attributs "de", "a", qui concerne une plage de classement
                            if (e4.Attribute("classement") != null)
                            {
                                int classement = int.Parse(e4.Attribute("classement").Value);

                                Qualification qu = new Qualification(classement, id_tour, competitionCible, anneeSuivante);
                                tour.Qualifications.Add(qu);
                            }
                            else
                            {
                                int de = int.Parse(e4.Attribute("de").Value);
                                int a = int.Parse(e4.Attribute("a").Value);
                                for(int j = de; j<= a; j++)
                                {
                                    Qualification qu = new Qualification(j, id_tour, competitionCible, anneeSuivante);
                                    tour.Qualifications.Add(qu);
                                }
                            }
                        }
                    }
                }
            }

            foreach(Competition c in _gestionnaire.Competitions)
            {
                c.InitialiserQualificationsAnneesSuivantes();
            }
        }

        public void ChargerLangues()
        {
            ChargerLangue("Francais", "fr");
            ChargerLangue("Anglais", "en");
        }

        private void ChargerLangue(string nomLangue, string nomFichier)
        {
            Langue langue = new Langue(nomLangue);
            string[] text = System.IO.File.ReadAllLines("Donnees/" + nomFichier + "_p.txt");
            foreach(string line in text)
            {
                langue.AjouterPrenom(line);
            }
            text = System.IO.File.ReadAllLines("Donnees/" + nomFichier + "_n.txt");
            foreach (string line in text)
            {
                langue.AjouterNom(line);
            }
            _gestionnaire.Langues.Add(langue);
        }

        public void InitialiserEquipes()
        {
            foreach(Club c in _gestionnaire.Clubs)
            {
                Club_Ville cv = c as Club_Ville;
                if(cv != null)
                {
                    int nbContratsManquants = 19 - cv.Contrats.Count;

                    for (int i = 0; i < nbContratsManquants; i++)
                    {
                        cv.GenererJoueur();
                    }
                }
            }
        }

        public void InitialiserJoueurs()
        {
            foreach(Club c in _gestionnaire.Clubs)
            {
                SelectionNationale sn = c as SelectionNationale;
                if(sn != null)
                {
                    int ecart = 30 - _gestionnaire.NombreJoueursPays(sn.Pays); 
                    if(ecart > 0)
                    {
                        for(int i =0; i<ecart; i++)
                        {
                            string prenom = sn.Pays.Langue.ObtenirPrenom();
                            string nom = sn.Pays.Langue.ObtenirNom();
                            DateTime naissance = new DateTime(1990, 1, 1);
                            Poste p = Poste.GARDIEN;
                            switch(Session.Instance.Random(1,10))
                            {
                                case 1: case 2: case 3: p = Poste.DEFENSEUR;  break;
                                case 4: case 5: case 6: p = Poste.MILIEU;  break;
                                case 7: case 8: p = Poste.ATTAQUANT;  break;
                            }
                            Joueur j = new Joueur(prenom, nom, naissance, sn.CentreFormation, sn.CentreFormation + 2, sn.Pays, p);
                            _gestionnaire.JoueursLibres.Add(j);
                        }
                    }
                }
            }
            _gestionnaire.AppelsSelection();
        }

        private DateTime String2Date(string date)
        {
            string[] splitted = date.Split('/');
            DateTime d = new DateTime(2000, int.Parse(splitted[1]), int.Parse(splitted[0]));
            return d;
        }

        private Heure String2Heure(string heure)
        {
            string[] splitted = heure.Split(':');
            Heure h = new Heure();
            h.Heures = int.Parse(splitted[0]);
            h.Minutes = int.Parse(splitted[1]);
            return h;
        }
    }
}
