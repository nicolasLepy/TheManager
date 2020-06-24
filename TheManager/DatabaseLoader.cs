using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;

namespace TheManager
{
    /// <summary>
    /// Manage loading of all stored game data
    /// </summary>
    public class DatabaseLoader
    {

        private Dictionary<int, Club> _clubsId;

        private Gestionnaire _kernel;
        public DatabaseLoader(Gestionnaire kernel)
        {
            _kernel = kernel;
            _clubsId = new Dictionary<int, Club>();
        }


        private int GetClubId(Club club)
        {
            int res = -1;
            foreach (KeyValuePair<int, Club> kvp in _clubsId)
            {
                if (kvp.Value == club) res = kvp.Key;
            }
            return res;
        }

        private int NextClubId()
        {
            int res = -1;

            foreach(KeyValuePair<int,Club> kvp in _clubsId)
            {
                if (kvp.Key > res) res = kvp.Key;
            }

            res++;
            return res;
        }

        public void FIFACSV2Joueurs()
        {
            XDocument d = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement root = new XElement("Joueurs");
            d.Add(root);

            string[] lines = File.ReadAllLines("Donnees/Joueurs_FIFA.csv",Encoding.UTF8);
            foreach(string line in lines)
            {
                string[] joueur = line.Split(';');
                string nom = joueur[0];
                int age = int.Parse(joueur[1]);
                DateTime naissance = new DateTime(2019 - age, 1, 1);
                string paysNom = joueur[2];
                Pays pays = _kernel.String2Pays(paysNom);
                if (pays == null) pays = _kernel.String2Pays("France");
                int niveau = int.Parse(joueur[3]) - 2;
                int potentiel = int.Parse(joueur[4]) - 2;
                int idclub = 0;
                bool res = int.TryParse(joueur[5],out idclub);
                string postestr = joueur[6];
                Position p = Position.Midfielder;
                switch (postestr)
                {
                    case "GK": p = Position.Goalkeeper;
                        break;
                    case "CB": case "LB": case "RB": case "LCB": case "RCB": case "RDM": p = Position.Defender; 
                        break;
                    case "CDM": case "CM": case "LM": case "LW": case "LWB":  case "RM": case "RCM": case "LDM":  case "RW": case "RWB": p = Position.Midfielder; 
                        break;
                    case "CAM": case "CF": case "ST": case "LAM": case "RF": case "LCM": case "RAM": case "LF": case "LS": case "RS": p = Position.Striker;
                        break;   
                    default : p = Position.Defender;
                        break;
                }
                if(idclub != 0)
                {
                    Console.WriteLine(nom);
                    XElement e = new XElement("Joueur");
                    e.Add(new XAttribute("prenom", ""));
                    e.Add(new XAttribute("nom", nom));
                    e.Add(new XAttribute("niveau", niveau));
                    e.Add(new XAttribute("potentiel", potentiel));
                    e.Add(new XAttribute("poste", p.ToString()));
                    e.Add(new XAttribute("club", idclub));
                    root.Add(e);
                }
            }
            d.Save("Donnees/joueursFIFA.xml");
        }

        public void AddIdToClubs()
        {
            int id = 0;

            XDocument doc = XDocument.Load("Donnees/clubs.xml");

            foreach(XElement x in doc.Descendants("Clubs"))
            {
                foreach(XElement x2 in x.Descendants("Club"))
                {
                    XAttribute attr_id = new XAttribute("id", id);
                    x2.Add(attr_id);
                    id++;
                }

                foreach (XElement x2 in x.Descendants("Selection"))
                {
                    XAttribute attr_id = new XAttribute("id", id);
                    x2.Add(attr_id);
                    id++;
                }


            }

            doc.Save("Donnees/clubs_id.xml");

        }

        private void ReplaceCompetitionId()
        {
            XDocument doc = XDocument.Load("Donnees/competitions.xml");

            foreach(XElement x in doc.Descendants("Club"))
            {
                string nom = x.Attribute("nom").Value;
                Club club = _kernel.String2Club(nom);
                int id_club = GetClubId(club);
                x.RemoveAttributes();
                x.Add(new XAttribute("id", id_club));
            }

            doc.Save("Donnees/competitions_id.xml");
        }

        public void Load()
        {
            LoadLanguages();
            LoadGeography();
            LoadCities();
            LoadStadiums();
            LoadClubs();
            LoadTournaments();
            LoadPlayers();
            LoadManagers();
            InitTeams();
            InitPlayers();
            LoadMedias();
            LoadGamesComments();
            //FIFACSV2Joueurs();
        }

        public void LoadGamesComments()
        {
            XDocument doc = XDocument.Load("Donnees/actions.xml");
            foreach (XElement e in doc.Descendants("Actions"))
            {
                foreach (XElement e2 in e.Descendants("Action"))
                {
                    string type = e2.Attribute("type").Value;
                    string content = e2.Value;
                    GameEvent gameEvent = GameEvent.Goal;
                    switch(type)
                    {
                        case "tir": 
                            gameEvent = GameEvent.Shot; 
                            break;
                        case "but": 
                            gameEvent = GameEvent.Goal; 
                            break;
                        case "but_pen": 
                            gameEvent = GameEvent.PenaltyGoal; 
                            break;
                        case "carton_jaune": 
                            gameEvent = GameEvent.YellowCard; 
                            break;
                        case "carton_rouge": 
                            gameEvent = GameEvent.RedCard; 
                            break;
                        default : 
                            gameEvent = GameEvent.Goal;
                            break;
                    }
                    _kernel.AjouterCommmentaireMatch(gameEvent, content);
                }
            }
        }

        public void LoadMedias()
        {
            XDocument doc = XDocument.Load("Donnees/medias.xml");
            foreach (XElement e in doc.Descendants("Medias"))
            {
                foreach (XElement e2 in e.Descendants("Media"))
                {
                    string name = e2.Attribute("nom").Value;
                    Pays country = _kernel.String2Pays(e2.Attribute("pays").Value);
                    Media m = new Media(name, country);
                    _kernel.Medias.Add(m);

                    foreach (XElement e3 in e2.Descendants("Journaliste"))
                    {
                        string firstName = e3.Attribute("prenom").Value;
                        string lastName = e3.Attribute("nom").Value;
                        int age = int.Parse(e3.Attribute("age").Value);
                        Ville city = _kernel.String2Ville(e3.Attribute("ville").Value);
                        int offset = 0;
                        if(e3.Attribute("retrait") != null)
                            offset = int.Parse(e3.Attribute("retrait").Value);
                        if (city == null)
                            Console.WriteLine(e3.Attribute("ville").Value + " n'est pas une ville.");
                        Journaliste j = new Journaliste(firstName, lastName, age, city, offset);
                        m.Journalistes.Add(j);
                    }

                    foreach (XElement e3 in e2.Descendants("Couvre"))
                    {
                        int index = int.Parse(e3.Attribute("aPartir").Value);
                        Competition tournament = _kernel.String2Competition(e3.Attribute("competition").Value);
                        int averageGames = -1;
                        int multiplexMinGames = -1;
                        if (e3.Attribute("matchParMultiplex") != null) averageGames = int.Parse(e3.Attribute("matchParMultiplex").Value);
                        if (e3.Attribute("multiplex") != null) multiplexMinGames = int.Parse(e3.Attribute("multiplex").Value);
                        m.Couvertures.Add(new CouvertureCompetition(tournament, index, multiplexMinGames, averageGames));
                    }
                }
            }
        }

        public void LoadPlayers()
        {
            XDocument doc = XDocument.Load("Donnees/joueurs.xml");
            foreach (XElement e in doc.Descendants("Joueurs"))
            {
                foreach (XElement e2 in e.Descendants("Joueur"))
                {
                    string lastName = e2.Attribute("nom").Value;
                    string firstName = e2.Attribute("prenom").Value;
                    int level = int.Parse(e2.Attribute("niveau").Value);
                    int potential = int.Parse(e2.Attribute("potentiel").Value);
                    int clubId = int.Parse(e2.Attribute("club").Value);
                    Club_Ville club = _clubsId[clubId] as Club_Ville;
                    Position position = Position.Goalkeeper;
                    string positionName = e2.Attribute("poste").Value;
                    switch(positionName)
                    {
                        case "DEFENSEUR": 
                            position = Position.Defender; 
                            break;
                        case "MILIEU": 
                            position = Position.Midfielder; 
                            break;
                        case "ATTAQUANT": 
                            position = Position.Striker; 
                            break;
                        default :
                            position = Position.Defender;
                            break;
                    }
                    Joueur j = new Joueur(firstName, lastName, new DateTime(1995, 1, 1), level, potential, _kernel.String2Pays("France"), position);
                    club.AjouterJoueur(new Contrat(j, j.EstimerSalaire(), new DateTime(Session.Instance.Random(2019,2024), 7, 1), new DateTime(Session.Instance.Partie.Date.Year, Session.Instance.Partie.Date.Month, Session.Instance.Partie.Date.Day)));
                }
            }
        }

        public void LoadManagers()
        {
            XDocument doc = XDocument.Load("Donnees/entraineurs.xml");
            foreach (XElement e in doc.Descendants("Entraineurs"))
            {
                foreach (XElement e2 in e.Descendants("Entraineur"))
                {
                    string lastName = e2.Attribute("nom").Value;
                    string firstName = e2.Attribute("prenom").Value;
                    int level = int.Parse(e2.Attribute("niveau").Value);
                    string clubName = e2.Attribute("club").Value;
                    Club_Ville club = _kernel.String2Club(clubName) as Club_Ville;
                    string countryName = e2.Attribute("nationalite").Value;
                    Pays country = _kernel.String2Pays(countryName);
                    Entraineur manager = new Entraineur(firstName, lastName, level, new DateTime(1970, 1, 1), country);
                    club.Entraineur = manager;
                }
            }
        }

        public void LoadCities()
        {
            XDocument doc = XDocument.Load("Donnees/villes.xml");
            foreach (XElement e in doc.Descendants("Villes"))
            {
                foreach (XElement e2 in e.Descendants("Ville"))
                {
                    string name = e2.Element("Nom").Value;
                    int population = int.Parse(e2.Element("Population").Value);
                    float lat = float.Parse(e2.Element("Latitute").Value, CultureInfo.InvariantCulture);
                    float lon = float.Parse(e2.Element("Longitude").Value, CultureInfo.InvariantCulture);
                    _kernel.String2Pays("France").Villes.Add(new Ville(name, population, lat, lon));

                }
            }
        }

        public void LoadGeography()
        {
            XDocument doc = XDocument.Load("Donnees/continents.xml");
            foreach (XElement e in doc.Descendants("Monde"))
            {
                foreach (XElement e2 in e.Descendants("Continent"))
                {
                    string continentName = e2.Attribute("nom").Value;
                    Continent c = new Continent(continentName);
                    foreach(XElement e3 in e2.Descendants("Pays"))
                    {
                        string countryName = e3.Attribute("nom").Value;
                        string language = e3.Attribute("langue").Value;
                        Langue l = _kernel.String2Langue(language);
                        Pays p = new Pays(countryName,l);
                        foreach(XElement e4 in e3.Descendants("Ville"))
                        {
                            string cityName = e4.Attribute("nom").Value;
                            int population = int.Parse(e4.Attribute("population").Value);
                            float lat = float.Parse(e4.Attribute("Latitute").Value);
                            float lon = float.Parse(e4.Attribute("Longitude").Value);

                            Ville v = new Ville(cityName, population, lat, lon);
                            p.Villes.Add(v);
                        }
                        c.Pays.Add(p);
                    }
                    _kernel.Continents.Add(c);
                }
            }
        }

        public void LoadStadiums()
        {
            XDocument doc = XDocument.Load("Donnees/stades.xml");
            foreach (XElement e in doc.Descendants("Stades"))
            {
                foreach (XElement e2 in e.Descendants("Stade"))
                {
                    string name = e2.Attribute("nom").Value;
                    int capacity = int.Parse(e2.Attribute("capacite").Value);
                    string cityName = e2.Attribute("ville").Value;
                    Ville v = _kernel.String2Ville(cityName);
                    Stade s = new Stade(name, capacity, v);
                    v.Pays().Stades.Add(s);
                }
            }
        }

        public void LoadClubs()
        {
            XDocument doc = XDocument.Load("Donnees/clubs.xml");
            
            foreach(XElement e in doc.Descendants("Clubs"))
            {
                foreach (XElement e2 in e.Descendants("Club"))
                {
                    int id = int.Parse(e2.Attribute("id").Value);
                    string name = e2.Attribute("nom").Value;
                    string shortName = e2.Attribute("nomCourt").Value;
                    if (shortName == "") shortName = name;
                    int reputation = int.Parse(e2.Attribute("reputation").Value);
                    int budget = int.Parse(e2.Attribute("budget").Value);
                    int supporters = int.Parse(e2.Attribute("supporters").Value);

                    string cityName = e2.Attribute("ville").Value;
                    Ville city = _kernel.String2Ville(cityName);

                    Stade stadium = null;
                    if (e2.Attribute("stade") != null)
                    {
                        string stadiumName = e2.Attribute("stade").Value;
                        stadium = _kernel.String2Stade(stadiumName);
                        if(stadium == null)
                        {
                            int capacite = 1000;
                            if (city != null) capacite = city.Population / 10;
                            stadium = new Stade(stadiumName, capacite, city);
                            if (city != null)
                            {
                                city.Pays().Stades.Add(stadium);
                            }
                            else
                            {
                                Console.WriteLine("La ville " + stadiumName + " n'existe pas.");
                            }
                        }
                    }
                    if (stadium == null)
                        stadium = new Stade("Stade de " + shortName, city.Population / 10, city);


                    int centreFormation = int.Parse(e2.Attribute("centreFormation").Value);
                    string logo = e2.Attribute("logo").Value;
                    if (logo == "" || !File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\Output\\Logos\\" + logo + ".png")) logo = "generic";

                    string musiqueBut = "";
                    if (e2.Attribute("musiqueBut") != null)
                        musiqueBut = e2.Attribute("musiqueBut").Value ;
                    else
                        musiqueBut = "null";

                    //Simplification
                    reputation = centreFormation;

                    Pays pays = city.Pays();
                    Entraineur entraineur = new Entraineur(pays.Langue.ObtenirPrenom(), pays.Langue.ObtenirNom(), centreFormation, new DateTime(1970, 1, 1), pays);

                    bool equipePremiere = true;
                    Club c = new Club_Ville(name,entraineur, shortName, reputation, budget, supporters, centreFormation, city, logo, stadium,musiqueBut, equipePremiere);
                    _clubsId[id] = c;
                    _kernel.Clubs.Add(c);
                }
                foreach (XElement e2 in e.Descendants("Selection"))
                {
                    int id = int.Parse(e2.Attribute("id").Value);
                    string name = e2.Attribute("nom").Value;
                    string shortName = name;
                    int reputation = int.Parse(e2.Attribute("reputation").Value);
                    int supporters = int.Parse(e2.Attribute("supporters").Value);
                    Pays country = _kernel.String2Pays(e2.Attribute("pays").Value);

                    Stade stadium = null;
                    if (e2.Attribute("stade") != null)
                    {
                        string nom_stade = e2.Attribute("stade").Value;
                        stadium = _kernel.String2Stade(nom_stade);
                    }
                    if (stadium == null)
                        stadium = new Stade("Stade de " + shortName, 15000, null);
                    int formationFacilities = int.Parse(e2.Attribute("centreFormation").Value);
                    string logo = e2.Attribute("logo").Value;
                    float coefficient = float.Parse(e2.Attribute("coefficient").Value);

                    string goalMusic = "";
                    if (e2.Attribute("musiqueBut") != null)
                        goalMusic = e2.Attribute("musiqueBut").Value;
                    else
                        goalMusic = "null";

                    Entraineur entraineur = new Entraineur(country.Langue.ObtenirPrenom(), country.Langue.ObtenirNom(), formationFacilities, new DateTime(1970, 1, 1), country);

                    Club c = new SelectionNationale(name,entraineur, shortName, reputation, supporters, formationFacilities, logo, stadium, coefficient,country,goalMusic);
                    _clubsId[id] = c;
                    _kernel.Clubs.Add(c);
                }
            }
        }
        
        public void LoadTournaments()
        {
            XDocument doc = XDocument.Load("Donnees/competitions.xml");

            //Chargement préliminaire de toutes les compétitons pour les référancer
            foreach (XElement e in doc.Descendants("Competitions"))
            {
                foreach (XElement e2 in e.Descendants("Competition"))
                {
                    string name = e2.Attribute("nom").Value;
                    string shortName = e2.Attribute("nomCourt").Value;
                    string logo = e2.Attribute("logo").Value;
                    string seasonBeginning = e2.Attribute("debut_saison").Value;
                    bool isChampionship = e2.Attribute("championnat").Value == "oui" ? true : false;
                    int level = int.Parse(e2.Attribute("niveau").Value);
                    ILocalisation localisation = _kernel.String2Localisation(e2.Attribute("localisation").Value);
                    DateTime debut = String2Date(seasonBeginning);
                    int periodicity = 1;
                    if(e2.Attribute("periodicite") != null)
                        periodicity = int.Parse(e2.Attribute("periodicite").Value);
                    int remainingYears = 1;
                    if (e2.Attribute("anneesRestantes") != null)
                        remainingYears = int.Parse(e2.Attribute("anneesRestantes").Value);
                    Competition c = new Competition(name, logo, debut, shortName, isChampionship, level, periodicity, remainingYears);
                    localisation.Competitions().Add(c);
                    //_gestionnaire.Competitions.Add(c);
                }
            }

            //Chargement détaillé de toutes les compétitions
            foreach (XElement e in doc.Descendants("Competitions"))
            {
                foreach(XElement e2 in e.Descendants("Competition"))
                {
                    string name = e2.Attribute("nom").Value;
                    Competition c = _kernel.String2Competition(name);
                    foreach(XElement e3 in e2.Descendants("Tour"))
                    {
                        Tour round = null;
                        string type = e3.Attribute("type").Value;
                        string nomTour = e3.Attribute("nom").Value;
                        bool twoLegged = e3.Attribute("allerRetour").Value == "oui";
                        string hourByDefault = e3.Attribute("heureParDefaut").Value;
                        DateTime initialisationDate = String2Date(e3.Attribute("initialisation").Value);
                        DateTime endDate = String2Date(e3.Attribute("fin").Value);
                        List<DateTime> dates = new List<DateTime>();
                        bool weCan = true;
                        int i = 1;
                        while (weCan)
                        {
                            if (e3.Attribute("j" + i) != null)
                            {
                                string dayDate = e3.Attribute("j" + i).Value;
                                DateTime dt = String2Date(dayDate);
                                i++;
                                dates.Add(dt);
                            }
                            else
                            {
                                weCan = false;
                            }
                        }

                        if (type == "championnat")
                        {
                            int dernieresJourneesMemeJour = int.Parse(e3.Attribute("dernieresJourneesMemeJour").Value);

                            round = new TourChampionnat(nomTour, String2Hour(hourByDefault), dates, twoLegged,new List<DecalagesTV>(), initialisationDate, endDate, dernieresJourneesMemeJour);
                        }
                        else if(type=="elimination")
                        {
                            round = new TourElimination(nomTour, String2Hour(hourByDefault), dates, new List<DecalagesTV>(), twoLegged, initialisationDate, endDate);
                        }
                        else if(type =="poules")
                        {
                            int groupsNumber = int.Parse(e3.Attribute("nombrePoules").Value);
                            DrawingMethod method = String2DrawingMethod(e3.Attribute("methode").Value);
                            round = new TourPoules(nomTour, String2Hour(hourByDefault), dates, new List<DecalagesTV>(), groupsNumber, twoLegged, initialisationDate, endDate, method);

                            if(method == DrawingMethod.Geographic)
                            {
                                //Lecture position poules
                                for (int groupNum = 1; groupNum <= groupsNumber; groupNum++)
                                {
                                    string[] poulePosition = e3.Attribute("poule" + groupNum).Value.Split(';');
                                    float latitude = float.Parse(poulePosition[0], CultureInfo.InvariantCulture);
                                    float longitude = float.Parse(poulePosition[1], CultureInfo.InvariantCulture);
                                    TourPoules tp = round as TourPoules;
                                    tp.LocalisationGroupes.Add(new GeographicPosition(latitude, longitude));
                                }
                            }

                            foreach(XElement eNoms in e3.Descendants("Nom"))
                            {
                                TourPoules tp = round as TourPoules;
                                tp.AjouterNomGroupe(eNoms.Value);
                            }
                            
                        }
                        else if (type == "inactif")
                        {
                            round = new TourInactif(nomTour, String2Hour(hourByDefault), initialisationDate, endDate);
                        }
                        c.Tours.Add(round);
                        foreach (XElement e4 in e3.Descendants("Club"))
                        {
                            Club club;
                            int clubId = int.Parse(e4.Attribute("id").Value);
                            if(e4.Attribute("reserve") == null)
                            {
                                club = _clubsId[clubId];
                            }
                            else
                            {
                                Club_Ville firstTeam = _clubsId[int.Parse(e4.Attribute("id").Value)] as Club_Ville;
                                string nameAddon = " B";
                                float divider = 1.5f;
                                if (firstTeam.Reserves.Count == 1) { nameAddon = " C"; divider = 2.5f; }
                                if (firstTeam.Reserves.Count == 2) { nameAddon = " D"; divider = 3.5f; }
                                if (firstTeam.Reserves.Count == 3) { nameAddon = " E"; divider = 4.5f; }
                                club = new Club_Reserve(firstTeam, firstTeam.Nom + nameAddon, firstTeam.NomCourt + nameAddon, null);
                                int newId = NextClubId();
                                _clubsId[newId] = club;
                                _kernel.Clubs.Add(club);
                                firstTeam.Reserves.Add(club as Club_Reserve);
                                //A reserve team was generated, let's create same players in base club to populate this reserve team
                                int averagePotential = (int)(firstTeam.CentreFormation - (firstTeam.CentreFormation / divider));
                                //Warning for {2,5,5,3} -> 15 is used in team initialisation to determine player number of first team
                                for (int g = 0; g < 2; g++) firstTeam.GenererJoueur(Position.Goalkeeper, 16, 23, -averagePotential);
                                for (int g = 0; g < 5; g++) firstTeam.GenererJoueur(Position.Defender , 16, 23, -averagePotential);
                                for (int g = 0; g < 5; g++) firstTeam.GenererJoueur(Position.Midfielder, 16, 23, -averagePotential);
                                for (int g = 0; g < 3; g++) firstTeam.GenererJoueur(Position.Striker, 16, 23, -averagePotential);
                            }

                           
                            round.Clubs.Add(club);
                        }
                        foreach(XElement e4 in e3.Descendants("Participants"))
                        {
                            int number = int.Parse(e4.Attribute("nombre").Value);
                            IEquipesRecuperables source = null;
                            XAttribute continent = e4.Attribute("continent");
                            if(continent != null)
                            {
                                source = _kernel.String2Continent(continent.Value);
                            }
                            else
                            {
                                string competitionName = e4.Attribute("competition").Value;
                                int tourIndex = int.Parse(e4.Attribute("idTour").Value);
                                Competition comp = _kernel.String2Competition(competitionName);
                                Tour r = comp.Tours[tourIndex];
                                source = r;
                            }
                            MethodeRecuperation method;
                            switch(e4.Attribute("methode").Value)
                            {
                                case "meilleurs":
                                    method = MethodeRecuperation.MEILLEURS; 
                                    break;
                                case "pires":
                                    method = MethodeRecuperation.PIRES; 
                                    break;
                                case "aleatoire":
                                    method = MethodeRecuperation.ALEATOIRE; 
                                    break;
                                default :
                                    method = MethodeRecuperation.MEILLEURS;
                                    break;
                            }
                            round.RecuperationEquipes.Add(new RecuperationEquipes(source, number,method));
                        }
                        foreach (XElement e4 in e3.Descendants("Decalage"))
                        {
                            int day = int.Parse(e4.Attribute("jour").Value);
                            Hour hour = String2Hour(e4.Attribute("heure").Value);
                            int probability = 1;
                            if (e4.Attribute("probabilite") != null)
                                probability = int.Parse(e4.Attribute("probabilite").Value);
                            int matchDay = 0;
                            if (e4.Attribute("journee") != null)
                                matchDay = int.Parse(e4.Attribute("journee").Value);
                            DecalagesTV dtv = new DecalagesTV(day, hour, probability, matchDay);
                            round.Programmation.DecalagesTV.Add(dtv);
                        }
                        foreach (XElement e4 in e3.Descendants("Regle"))
                        {
                            Rule rule = Rule.AtHomeIfTwoLevelDifference;
                            switch(e4.Attribute("nom").Value)
                            {
                                case "RECOIT_SI_DEUX_DIVISION_ECART": rule = Rule.AtHomeIfTwoLevelDifference; break;
                                case "EQUIPES_PREMIERES_UNIQUEMENT":rule = Rule.OnlyFirstTeams;break;
                                case "RESERVES_NE_MONTENT_PAS": rule = Rule.ReservesAreNotPromoted; break;
                            }
                            round.Regles.Add(rule);
                        }
                        foreach (XElement e4 in e3.Descendants("Dotation"))
                        {
                            int ranking = int.Parse(e4.Attribute("classement").Value);
                            int prize = int.Parse(e4.Attribute("somme").Value);
                            round.Dotations.Add(new Dotation(ranking, prize));
                        }
                        foreach (XElement e4 in e3.Descendants("Qualification"))
                        {
                            int tourId = int.Parse(e4.Attribute("id_tour").Value);
                            bool nextYear = false;
                            if (e4.Attribute("anneeSuivante") != null) nextYear = e4.Attribute("anneeSuivante").Value == "oui" ? true : false;
                            Competition targetedTournament = null;
                            if (e4.Attribute("competition") != null) targetedTournament = _kernel.String2Competition(e4.Attribute("competition").Value);
                            else targetedTournament = c;

                            //Deux cas
                            //1- On a un attribut "classement", avec un classement précis
                            //2- On a deux attributs "de", "a", qui concerne une plage de classement
                            if (e4.Attribute("classement") != null)
                            {
                                int ranking = int.Parse(e4.Attribute("classement").Value);

                                Qualification qu = new Qualification(ranking, tourId, targetedTournament, nextYear);
                                round.Qualifications.Add(qu);
                            }
                            else
                            {
                                int from = int.Parse(e4.Attribute("de").Value);
                                int to = int.Parse(e4.Attribute("a").Value);
                                for(int j = from; j<= to; j++)
                                {
                                    Qualification qu = new Qualification(j, tourId, targetedTournament, nextYear);
                                    round.Qualifications.Add(qu);
                                }
                            }
                        }
                    }
                }
            }

            foreach(Competition c in _kernel.Competitions)
            {
                c.InitialiserQualificationsAnneesSuivantes();
            }
        }

        public void LoadLanguages()
        {
            LoadLanguage("Francais", "fr");
            LoadLanguage("Anglais", "en");
        }

        private void LoadLanguage(string languageName, string filename)
        {
            Langue language = new Langue(languageName);
            string[] text = System.IO.File.ReadAllLines("Donnees/" + filename + "_p.txt");
            foreach(string line in text)
            {
                language.AjouterPrenom(line);
            }
            text = System.IO.File.ReadAllLines("Donnees/" + filename + "_n.txt");
            foreach (string line in text)
            {
                language.AjouterNom(line);
            }
            _kernel.Langues.Add(language);
        }

        public void InitTeams()
        {
            foreach(Club c in _kernel.Clubs)
            {
                Club_Ville cityClub = c as Club_Ville;
                if(cityClub != null)
                {
                    int firstTeamPlayersNumber = cityClub.Contrats.Count - (cityClub.Reserves.Count * 15);
                    int missingContractNumber = 19 - firstTeamPlayersNumber;
                    for (int i = 0; i < missingContractNumber; i++)
                    {
                        cityClub.GenererJoueur(24,33);
                    }
                }
            }
        }

        public void InitPlayers()
        {
            foreach(Club c in _kernel.Clubs)
            {
                SelectionNationale nationalTeam = c as SelectionNationale;
                if(nationalTeam != null)
                {
                    int gap = 30 - _kernel.NombreJoueursPays(nationalTeam.Pays); 
                    if(gap > 0)
                    {
                        for(int i =0; i<gap; i++)
                        {
                            string firstName = nationalTeam.Pays.Langue.ObtenirPrenom();
                            string lastName = nationalTeam.Pays.Langue.ObtenirNom();
                            DateTime birthday = new DateTime(1990, 1, 1);
                            Position p = Position.Goalkeeper;
                            switch(Session.Instance.Random(1,10))
                            {
                                case 1: case 2: case 3: p = Position.Defender;  break;
                                case 4: case 5: case 6: p = Position.Midfielder;  break;
                                case 7: case 8: p = Position.Striker;  break;
                            }
                            Joueur j = new Joueur(firstName, lastName, birthday, nationalTeam.CentreFormation, nationalTeam.CentreFormation + 2, nationalTeam.Pays, p);
                            _kernel.JoueursLibres.Add(j);
                        }
                    }
                }
            }
            _kernel.AppelsSelection();
        }

        private DateTime String2Date(string date)
        {
            string[] split = date.Split('/');
            DateTime d = new DateTime(2000, int.Parse(split[1]), int.Parse(split[0]));
            return d;
        }

        private Hour String2Hour(string hour)
        {
            string[] split = hour.Split(':');
            Hour h = new Hour();
            h.Hours = int.Parse(split[0]);
            h.Minutes = int.Parse(split[1]);
            return h;
        }

        private DrawingMethod String2DrawingMethod(string method)
        {
            DrawingMethod res = DrawingMethod.Level;

            if (method == "Niveau") res = DrawingMethod.Level;
            else if (method == "Geographique") res = DrawingMethod.Geographic;

            return res;
        }
    }
}