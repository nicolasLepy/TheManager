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
                    string nom;
                    string nomCourt;
                    int reputation;
                    int budget;
                    int supporters;
                    string nom_stade;
                    string nom_ville;
                    int centreFormation;
                    string logo;
                }
            }
        }

        public void ChargerCompetitions()
        {

        }
    }
}
