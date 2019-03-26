using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;

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
            ChargerGeographie();
            ChargerClubs();
            ChargerCompetitions();
        }

        public void ChargerGeographie()
        {
            XDocument doc = XDocument.Load("Donnees/continent.xml");
            foreach (XElement e in doc.Descendants("Monde"))
            {
                foreach (XElement e2 in e.Descendants("Continent"))
                {
                    string nom_continent = e2.Attribute("nom").Value;
                    Continent c = new Continent(nom_continent);
                    foreach(XElement e3 in e2.Descendants("Pays"))
                    {
                        string nom_pays = e3.Attribute("nom").Value;
                        Pays p = new Pays(nom_pays);
                        foreach(XElement e4 in e3.Descendants("Ville"))
                        {
                            string nom_ville = e4.Attribute("nom").Value;
                            int population = int.Parse(e4.Attribute("population").Value);
                            Ville v = new Ville(nom_ville, population);
                            p.Villes.Add(v);
                        }
                        c.Pays.Add(p);
                    }
                    _gestionnaire.Continents.Add(c);
                }
            }
        }

        public void ChargerClubs()
        {
            XDocument doc = XDocument.Load("Donnees/club.xml");
            foreach(XElement e in doc.Descendants("Stades"))
            {
                foreach(XElement e2 in e.Descendants("Stade"))
                {
                    string nom = e2.Attribute("nom").Value;
                    int capacite = int.Parse(e2.Attribute("capacite").Value);
                    string nom_ville = e2.Attribute("ville").Value;
                    Ville v = _gestionnaire.String2Ville(nom_ville);
                    Stade s = new Stade(nom, capacite, v);
                    v.Pays(_gestionnaire).Stades.Add(s);
                }
            }
            foreach(XElement e in doc.Descendants("Clubs"))
            {
                foreach (XElement e2 in e.Descendants("Club"))
                {
                    string nom = e2.Attribute("nom").Value;
                    string nomCourt = e2.Attribute("nomCourt").Value;
                    int reputation = int.Parse(e2.Attribute("reputation").Value);
                    int budget = int.Parse(e2.Attribute("budget").Value);
                    int supporters = int.Parse(e2.Attribute("supporters").Value);
                    string nom_stade = e2.Attribute("stade").Value;
                    Stade stade = _gestionnaire.String2Stade(nom_stade);
                    string nom_ville = e2.Attribute("ville").Value;
                    Ville ville = _gestionnaire.String2Ville(nom_ville);
                    int centreFormation = int.Parse(e2.Attribute("centreFormation").Value);
                    string logo = e2.Attribute("logo").Value;
                    Club c = new Club_Ville(nom, nomCourt, reputation, budget, supporters, centreFormation, ville, 0, logo, stade);
                    _gestionnaire.Clubs.Add(c);
                }
                foreach (XElement e2 in e.Descendants("Selection"))
                {
                    string nom = e2.Attribute("nom").Value;
                    string nomCourt = e2.Attribute("nomCourt").Value;
                    int reputation = int.Parse(e2.Attribute("reputation").Value);
                    int supporters = int.Parse(e2.Attribute("supporters").Value);
                    string nom_stade = e2.Attribute("stade").Value;
                    Stade stade = _gestionnaire.String2Stade(nom_stade);
                    int centreFormation = int.Parse(e2.Attribute("centreFormation").Value);
                    string logo = e2.Attribute("logo").Value;
                    float coefficient = float.Parse(e2.Attribute("coefficient").Value);
                    Club c = new SelectionNationale(nom, nomCourt, reputation, supporters, centreFormation, logo, stade, coefficient);
                    _gestionnaire.Clubs.Add(c);
                }
            }
        }

        public void ChargerCompetitions()
        {
            XDocument doc = XDocument.Load("Donnees/competitions.xml");
            foreach (XElement e in doc.Descendants("Competitions"))
            {
                foreach(XElement e2 in e.Descendants("Competition"))
                {
                    string nom = e2.Attribute("nom").Value;
                    string nomCourt = e2.Attribute("nomCourt").Value;
                    string logo = e2.Attribute("logo").Value;
                    string debutSaison = e2.Attribute("debut_saison").Value;
                    DateTime debut = String2Date(debutSaison);
                    Competition c = new Competition(nom, logo, debut, logo);
                    foreach(XElement e3 in e2.Descendants("Tour"))
                    {
                        string type = e2.Attribute("type").Value;
                        if(type == "championnat")
                        {
                            string nomTour = e3.Attribute("nom").Value;
                            bool allerRetour = e3.Attribute("allerRetour").Value == "oui" ? true : false;
                            string heureParDefaut = e3.Attribute("heureParDefaut").Value;
                            List<DateTime> dates = new List<DateTime>();
                            bool onPeut = true;
                            int i = 1;
                            while(onPeut)
                            {
                                if(e3.Attribute("j" + i) != null)
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
                            TourChampionnat tc = new TourChampionnat(nom, String2Heure(heureParDefaut), dates, allerRetour, 0, 0);


                        }
                    }
                }
            }
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
