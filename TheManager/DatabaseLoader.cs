using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Xml;
using TheManager.Tournaments;

namespace TheManager
{
    /// <summary>
    /// Manage loading of all stored game data
    /// </summary>
    public class DatabaseLoader
    {

        private readonly Dictionary<int, Club> _clubsId;

        private readonly Kernel _kernel;
        public DatabaseLoader(Kernel kernel)
        {
            _kernel = kernel;
            _clubsId = new Dictionary<int, Club>();
        }


        private int GetClubId(Club club)
        {
            int res = -1;
            foreach (KeyValuePair<int, Club> kvp in _clubsId)
            {
                if (kvp.Value == club)
                {
                    res = kvp.Key;
                }
            }
            return res;
        }

        private int NextClubId()
        {
            int res = -1;

            foreach(KeyValuePair<int,Club> kvp in _clubsId)
            {
                if (kvp.Key > res)
                {
                    res = kvp.Key;
                }
            }

            res++;
            return res;
        }

        /*
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
                Country pays = _kernel.String2Country(paysNom);
                if (pays == null)
                {
                    pays = _kernel.String2Country("France");
                }
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
                    Utils.Debug(nom);
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
        }*/

        public void AddIdToClubs()
        {
            int id = 0;

            XDocument doc = XDocument.Load(Utils.dataFolderName + "/clubs.xml");

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

            doc.Save(Utils.dataFolderName + "/clubs_id.xml");
        }

        private void ReplaceCompetitionId()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/competitions.xml");

            foreach(XElement x in doc.Descendants("Club"))
            {
                string nom = x.Attribute("nom").Value;
                Club club = _kernel.String2Club(nom);
                int id_club = GetClubId(club);
                x.RemoveAttributes();
                x.Add(new XAttribute("id", id_club));
            }

            doc.Save(Utils.dataFolderName + "/competitions_id.xml");
        }

        public void Load()
        {
            /*
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

            */
            //FIFACSV2Joueurs();
        }

        public void LoadGamesComments()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/actions.xml");
            foreach (XElement e in doc.Descendants("Actions"))
            {
                foreach (XElement e2 in e.Descendants("Action"))
                {
                    string type = e2.Attribute("type").Value;
                    string content = e2.Value;
                    GameEvent gameEvent;
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
                    _kernel.AddMatchCommentary(gameEvent, content);
                }
            }
        }

        public GameDay String2GameDay(string value)
        {
            bool midweekGame = false;
            int yearOffset = 0;

            string dayDate = value;
            if (dayDate.Contains(".5"))
            {
                midweekGame = true;
                dayDate = dayDate.Replace(".5", "");
            }
            if (dayDate.Contains("+"))
            {
                string[] daySplit = dayDate.Split('+');
                yearOffset = int.Parse(daySplit[1]);
                dayDate = daySplit[0];
            }
            return new GameDay(int.Parse(dayDate), midweekGame, yearOffset, 0);
        }

        public List<GameDay> CreateGameDaysList(XElement node)
        {
            List<GameDay> dates = new List<GameDay>();
            bool weCan = true;
            int i = 1;
            while (weCan)
            {
                if (node.Attribute("j" + i) != null)
                {
                    dates.Add(String2GameDay(node.Attribute("j" + i).Value));
                }
                else
                {
                    weCan = false;
                }
                i++;
            }
            return dates;
        }

        public void LoadCalendars()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/calendars.xml");
            foreach (XElement e in doc.Descendants("Calendars"))
            {
                foreach (XElement e2 in e.Descendants("Calendar"))
                {
                    string name = e2.Attribute("name").Value;

                    List<GameDay> dates = CreateGameDaysList(e2);

                    _kernel.AddGenericCalendar(new GenericCalendar(name, dates));
                }
            }
        }

        public void LoadMedias()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/medias.xml");
            foreach (XElement e in doc.Descendants("Medias"))
            {
                foreach (XElement e2 in e.Descendants("Media"))
                {
                    string name = e2.Attribute("nom").Value;
                    Country country = _kernel.String2Country(e2.Attribute("pays").Value);
                    Media m = new Media(name, country);
                    _kernel.medias.Add(m);

                    foreach (XElement e3 in e2.Descendants("Journaliste"))
                    {
                        string firstName = e3.Attribute("prenom").Value;
                        string lastName = e3.Attribute("nom").Value;
                        int age = int.Parse(e3.Attribute("age").Value);
                        City city = _kernel.String2City(e3.Attribute("ville").Value);
                        int offset = 0;
                        bool isNational = false;
                        if (e3.Attribute("retrait") != null)
                        {
                            offset = int.Parse(e3.Attribute("retrait").Value);
                        }
                        if (e3.Attribute("national") != null)
                        {
                            if(e3.Attribute("national").Value == "oui")
                            {
                                isNational = true;
                            }
                        }

                        if (city == null)
                        {
                            Utils.Debug(e3.Attribute("ville").Value + " n'est pas une ville.");
                        }
                        Journalist j = new Journalist(firstName, lastName, age, city, offset, isNational);
                        m.journalists.Add(j);
                    }

                    foreach (XElement e3 in e2.Descendants("Couvre"))
                    {
                        int index = int.Parse(e3.Attribute("aPartir").Value);
                        Tournament tournament = _kernel.String2Tournament(e3.Attribute("competition").Value);
                        int averageGames = -1;
                        int multiplexMinGames = -1;
                        int level = -1;
                        if (e3.Attribute("matchParMultiplex") != null)
                        {
                            averageGames = int.Parse(e3.Attribute("matchParMultiplex").Value);
                        }

                        if (e3.Attribute("multiplex") != null)
                        {
                            multiplexMinGames = int.Parse(e3.Attribute("multiplex").Value);
                        }
                        if (e3.Attribute("level") != null)
                        {
                            level = int.Parse(e3.Attribute("level").Value);
                        }

                        m.coverages.Add(new TournamentCoverage(tournament, index, multiplexMinGames, averageGames, level));
                    }
                }
            }
        }

        public void LoadPlayers()
        {
            StreamReader reader = new StreamReader(Utils.dataFolderName + "/players.xml", Encoding.UTF8);
            XDocument doc = XDocument.Load(reader);
            foreach (XElement e in doc.Descendants("Joueurs"))
            {
                foreach (XElement e2 in e.Descendants("Joueur"))
                {
                    string lastName = e2.Attribute("nom").Value;
                    string firstName = e2.Attribute("prenom").Value;
                    int level = int.Parse(e2.Attribute("niveau").Value);
                    int potential = int.Parse(e2.Attribute("potentiel").Value);
                    int clubId = int.Parse(e2.Attribute("club").Value);
                    CityClub club = clubId > 0 ? _clubsId[clubId] as CityClub : null ;
                    Position position;
                    string positionName = e2.Attribute("poste").Value;
                    string playerBirthName = e2.Attribute("naissance").Value;
                    DateTime playerBirth = new DateTime(int.Parse(playerBirthName.Split('-')[2]), int.Parse(playerBirthName.Split('-')[1]), int.Parse(playerBirthName.Split('-')[0]));
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
                            position = Position.Goalkeeper;
                            break;
                    }
                    Country playerCountry = _kernel.String2Country(e2.Attribute("pays").Value);
                    Player j = new Player(firstName, lastName, playerBirth, level, potential, playerCountry == null ? _kernel.String2Country("France") : playerCountry, position);
                    if(club != null)
                    {
                        club.AddPlayer(new Contract(j, j.EstimateWage(), new DateTime(Session.Instance.Random(Utils.beginningYear, Utils.beginningYear + 5), 7, 1), new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, Session.Instance.Game.date.Day)));
                    }
                    else
                    {
                        Session.Instance.Game.kernel.freePlayers.Add(j);
                    }
                }
            }
        }

        public void LoadManagers()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/entraineurs.xml");
            foreach (XElement e in doc.Descendants("Entraineurs"))
            {
                foreach (XElement e2 in e.Descendants("Entraineur"))
                {
                    string lastName = e2.Attribute("nom").Value;
                    string firstName = e2.Attribute("prenom").Value;
                    int level = int.Parse(e2.Attribute("niveau").Value);
                    string clubName = e2.Attribute("club").Value;
                    CityClub club = _kernel.String2Club(clubName) as CityClub;
                    string countryName = e2.Attribute("nationalite").Value;
                    Country country = _kernel.String2Country(countryName);
                    Manager manager = new Manager(firstName, lastName, level, new DateTime(1970, 1, 1), country);
                    club.manager = manager;
                }
            }
        }

        public void LoadCities()
        {
            foreach (string xmlFile in Directory.EnumerateFiles(Utils.dataFolderName + "/cities/"))
            {
                //XDocument doc = XDocument.Load(Utils.dataFolderName + "/cities/cities.xml");
                XDocument doc = XDocument.Load(xmlFile);
                foreach (XElement e in doc.Descendants("Cities"))
                {
                    foreach (XElement e2 in e.Descendants("City"))
                    {
                        string name = e2.Attribute("Name").Value;
                        int population = 0;
                        try
                        {
                            population = int.Parse(e2.Attribute("Population").Value);
                        }
                        catch(Exception exception)
                        {
                            population = 0;
                        }
                        float lat = float.Parse(e2.Attribute("Latitude").Value, CultureInfo.InvariantCulture);
                        float lon = float.Parse(e2.Attribute("Longitude").Value, CultureInfo.InvariantCulture);
                        string country = e2.Attribute("Country").Value;
                        _kernel.DbCountryName2Country(country).cities.Add(new City(name, population, lat, lon));
                    }
                }

            }

        }

        public void ReformateCities()
        {
            foreach (string xmlFile in Directory.EnumerateFiles(Utils.dataFolderName + "/old_rawdb/rawcities/"))
            {
                //XDocument doc = XDocument.Load(Utils.dataFolderName + "/cities/cities.xml");
                XDocument doc = XDocument.Load(xmlFile);
                List<XElement> toDelete = new List<XElement>();
                foreach (XElement e in doc.Descendants("Cities"))
                {
                    foreach (XElement e2 in e.Descendants("City"))
                    {
                        e2.Attribute("A").Remove();
                        e2.Attribute("F").Remove();
                        e2.Attribute("G").Remove();
                        if (e2.Attribute("H") != null)
                        {
                            e2.Attribute("H").Remove();
                        }
                        if (e2.Attribute("K") != null)
                        {
                            e2.Attribute("K").Remove();
                        }
                        if (e2.Attribute("I") != null)
                        {
                            e2.Attribute("I").Remove();
                        }
                        if (e2.Attribute("L") != null)
                        {
                            e2.Attribute("L").Remove();
                        }
                        if (e2.Attribute("M") != null)
                        {
                            e2.Attribute("M").Remove();
                        }
                        if(e2.Attribute("Population") == null)
                        {
                            toDelete.Add(e2);
                        }
                    }
                }

                foreach(XElement toDel in toDelete)
                {
                    toDel.Remove();
                }

                foreach (XElement e in doc.Descendants("Villes"))
                {
                    var elements = e.Elements("Ville").OrderBy(el => el.Attribute("Country").Value).ToArray();
                    Console.WriteLine(elements);
                    e.Elements().Remove();
                    e.Add(elements);
                    
                }

                doc.Save("data/old_rawdb/rawcities/all.xml");

            }

        }

        public void LoadGeography()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/continents.xml");
            int maxAdmId = 0;
            foreach (XElement e in doc.Descendants("World"))
            {
                foreach (XElement e2 in e.Descendants("Continent"))
                {
                    string continentName = e2.Attribute("name").Value;
                    Continent c = new Continent(continentName);
                    foreach(XElement e3 in e2.Descendants("Country"))
                    {
                        string countryName = e3.Attribute("name").Value;
                        string countrydBName = e3.Attribute("db_name").Value;
                        string language = e3.Attribute("langue").Value;
                        int countryShape = int.Parse(e3.Attribute("shape").Value);
                        Language l = _kernel.String2Language(language);
                        Country p = new Country(countrydBName,countryName,l, countryShape);
                        foreach(XElement e4 in e3.Descendants("Ville"))
                        {
                            string cityName = e4.Attribute("nom").Value;
                            int population = int.Parse(e4.Attribute("population").Value);
                            float lat = float.Parse(e4.Attribute("Latitute").Value);
                            float lon = float.Parse(e4.Attribute("Longitude").Value);

                            City v = new City(cityName, population, lat, lon);
                            p.cities.Add(v);
                        }

                        foreach (XElement e4 in e3.Descendants("AdministrativeDivision"))
                        {
                            string administrationName = e4.Attribute("name").Value;
                            int administrationId = int.Parse(e4.Attribute("id").Value);
                            int administrationParent = e4.Attribute("parent") != null ? int.Parse(e4.Attribute("parent").Value) : 0;
                            maxAdmId = administrationId > maxAdmId ? administrationId : maxAdmId;
                            AdministrativeDivision ad = new AdministrativeDivision(administrationId, administrationName);
                            if (administrationParent > 0)
                            {
                                p.GetAdministrativeDivision(administrationParent).divisions.Add(ad);
                            }
                            else
                            {
                                p.administrativeDivisions.Add(ad);
                            }
                        }
                        c.countries.Add(p);
                    }
                    _kernel.continents.Add(c);
                }
            }

            maxAdmId++;
            foreach (Continent c in _kernel.continents)
            {
                foreach (Country cc in c.countries)
                {
                    AdministrativeDivision adCountry = new AdministrativeDivision(maxAdmId++, cc.Name());
                    cc.administrativeDivisions.Add(adCountry);
                }
            }
        }

        public void LoadStadiums()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/stades.xml");
            foreach (XElement e in doc.Descendants("Stades"))
            {
                foreach (XElement e2 in e.Descendants("Stade"))
                {
                    string name = e2.Attribute("nom").Value;
                    int capacity = int.Parse(e2.Attribute("capacite").Value);
                    string cityName = e2.Attribute("ville").Value;
                    City v = _kernel.String2City(cityName);
                    Stadium s = new Stadium(name, capacity, v);
                    v.Country().stadiums.Add(s);
                }
            }
        }

        private string RemoveClubDenomination(string name, string denomination)
        {
            string res = name.Replace(denomination + " ", "");
            res = res.Replace(" " + denomination, "");
            return res;
        }

        private void AddStadium(Stadium stadium)
        {
            if(stadium.city != null && stadium.city.Name != "NoCity")
            {
                stadium.city.Country().stadiums.Add(stadium);
            }
            else
            {
                Console.WriteLine("La ville du stade " + stadium.name + "n'existe pas ou n'est pas vraie");
            }
        }

        public void LoadClubs()
        {

            foreach(string xmlFile in Directory.EnumerateFiles(Utils.dataFolderName + "/clubs/"))
            {
                //XDocument doc = XDocument.Load(Utils.dataFolderName + "/clubs/clubs.xml");
                Utils.Debug(xmlFile);
                XDocument doc = XDocument.Load(xmlFile);

                foreach (XElement e in doc.Descendants("Clubs"))
                {
                    foreach (XElement e2 in e.Descendants("Club"))
                    {
                        int id = int.Parse(e2.Attribute("id").Value);
                        string name = e2.Attribute("nom").Value;
                        string shortName = e2.Attribute("nomCourt") != null ? e2.Attribute("nomCourt").Value : "";
                        if (shortName == "")
                        {
                            shortName = name;
                            shortName = RemoveClubDenomination(shortName, "Olympique");
                            shortName = RemoveClubDenomination(shortName, "FC");
                            shortName = RemoveClubDenomination(shortName, "AS");
                            shortName = RemoveClubDenomination(shortName, "US");
                            shortName = RemoveClubDenomination(shortName, "RC");
                            shortName = RemoveClubDenomination(shortName, "AC");
                            shortName = RemoveClubDenomination(shortName, "ES");
                            shortName = RemoveClubDenomination(shortName, "SO");
                            shortName = RemoveClubDenomination(shortName, "USM");
                        }

                        int reputation = int.Parse(e2.Attribute("reputation").Value);
                        int budget = int.Parse(e2.Attribute("budget").Value);
                        int supporters = int.Parse(e2.Attribute("supporters").Value);

                        string cityName = e2.Attribute("ville").Value;
                        City city = _kernel.String2City(cityName);
                        
                        Stadium stadium = null;
                        string stadiumName = "Stade de " + shortName;

                        if (e2.Attribute("stade") != null)
                        {
                            if(e2.Attribute("stade").Value != "")
                            {
                                stadiumName = e2.Attribute("stade").Value;
                            }
                            stadium = _kernel.String2Stadium(stadiumName);
                        }

                        if (stadium == null)
                        {
                            int capacite = 1000;

                            if (city != null)
                            {
                                capacite = supporters > 0 ? (int)(supporters * 1.5) : city.Population / 10;
                            }
                            stadium = new Stadium(stadiumName, capacite, city);
                            if(city != null)
                            {
                                AddStadium(stadium);
                            }
                        }
                        
                        int idAdministrativeDivision = 0;
                        AdministrativeDivision administrativeDivision = null;
                        if (e2.Attribute("administrativeDivision") != null)
                        {
                            idAdministrativeDivision = int.Parse(e2.Attribute("administrativeDivision").Value);
                            administrativeDivision = _kernel.GetAdministrativeDivision(idAdministrativeDivision); //city?.Country().GetAdministrativeDivision(idAdministrativeDivision);
                        }

                        if (administrativeDivision == null)
                        {
                            administrativeDivision = city?.Country().GetCountryAdministrativeDivision();
                        }

                        int centreFormation = int.Parse(e2.Attribute("centreFormation").Value);
                        string logo = e2.Attribute("logo").Value;
                        if (logo == "" ||
                            !File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\" + Utils.clubLogoFolderName + "\\" + logo + ".png"))
                        {
                            logo = "generic";
                        }

                        string musiqueBut = "";
                        if (e2.Attribute("musiqueBut") != null)
                        {
                            musiqueBut = e2.Attribute("musiqueBut").Value;
                        }
                        else
                        {
                            musiqueBut = "null";
                        }

                        //Simplification
                        reputation = centreFormation;
                        
                        bool equipePremiere = true;
                        Club c = new CityClub(name, null, shortName, reputation, budget, supporters, centreFormation, city, logo, stadium, musiqueBut, equipePremiere, administrativeDivision);
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
                        Country country = _kernel.String2Country(e2.Attribute("pays").Value);

                        Stadium stadium = null;
                        if (e2.Attribute("stade") != null)
                        {
                            string nom_stade = e2.Attribute("stade").Value;
                            stadium = _kernel.String2Stadium(nom_stade);
                        }

                        if (stadium == null)
                        {
                            stadium = new Stadium("Stade de " + shortName, 75000, null);
                        }
                        int formationFacilities = int.Parse(e2.Attribute("centreFormation").Value);
                        string logo = country.Flag;

                        string goalMusic = "";
                        if (e2.Attribute("musiqueBut") != null)
                        {
                            goalMusic = e2.Attribute("musiqueBut").Value;
                        }
                        else
                        {
                            goalMusic = "null";
                        }
                        float points = 0;
                        if (e2.Attribute("points") != null)
                        {
                            points = float.Parse(e2.Attribute("points").Value);
                        }

                        points = formationFacilities;
                        Manager entraineur = new Manager(country.language.GetFirstName(), country.language.GetLastName(), formationFacilities, new DateTime(1970, 1, 1), country);

                        Club c = new NationalTeam(name, entraineur, shortName, reputation, supporters, formationFacilities, logo, stadium, country, goalMusic, points);
                        _clubsId[id] = c;
                        _kernel.Clubs.Add(c);
                    }
                }
            }

        }
        
        public void LoadTournaments()
        {
            foreach (string xmlFile in Directory.EnumerateFiles(Utils.dataFolderName + "/comp/"))
            {
                XDocument doc = XDocument.Load(xmlFile);
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
                        GameDay debut = String2GameDay(seasonBeginning);
                        int periodicity = 1;
                        if (e2.Attribute("periodicite") != null)
                        {
                            periodicity = int.Parse(e2.Attribute("periodicite").Value);
                        }
                        int remainingYears = 1;
                        if (e2.Attribute("anneesRestantes") != null)
                        {
                            remainingYears = int.Parse(e2.Attribute("anneesRestantes").Value);
                        }

                        string[] colorStr = e2.Attribute("color").Value.Split(',');
                        Color color = new Color(byte.Parse(colorStr[0]), byte.Parse(colorStr[1]), byte.Parse(colorStr[2]));

                        Console.WriteLine(name);
                        Tournament tournament = new Tournament(name, logo, debut, shortName, isChampionship, level, periodicity, remainingYears, color);
                        tournament.InitializeQualificationsNextYearsLists(e2.Descendants("Tour").Count());
                        localisation.Tournaments().Add(tournament);
                        //_gestionnaire.Competitions.Add(c);
                    }
                }

            }


            foreach (string xmlFile in Directory.EnumerateFiles(Utils.dataFolderName + "/comp/"))
            {
                XDocument doc = XDocument.Load(xmlFile);
                //Chargement détaillé de toutes les compétitions
                foreach (XElement e in doc.Descendants("Competitions"))
                {
                    foreach (XElement e2 in e.Descendants("Competition"))
                    {
                        string name = e2.Attribute("nom").Value;
                        Tournament c = _kernel.String2Tournament(name);
                        int roundIndex = 0;
                        foreach (XElement e3 in e2.Descendants("Tour"))
                        {
                            Round round = null;
                            string type = e3.Attribute("type").Value;
                            string nomTour = e3.Attribute("nom").Value;
                            bool twoLegged = e3.Attribute("allerRetour") != null ? e3.Attribute("allerRetour").Value == "oui" : false;
                            string hourByDefault = e3.Attribute("heureParDefaut").Value;
                            GameDay initialisationDate = String2GameDay(e3.Attribute("initialisation").Value);
                            GameDay endDate = String2GameDay(e3.Attribute("fin").Value);
                            List<GameDay> dates = CreateGameDaysList(e3);
                            if(dates.Count == 0)
                            {
                                if(e3.Attribute("calendrier") != null)
                                {
                                    GenericCalendar genericCalendar = Session.Instance.Game.kernel.GetGenericCalendar(e3.Attribute("calendrier").Value);
                                    dates = new List<GameDay>(genericCalendar.GameDays);
                                }
                            }
                            if(e3.Attribute("offsetDay") != null)
                            {
                                int offset = int.Parse(e3.Attribute("offsetDay").Value);
                                foreach(GameDay gd in dates)
                                {
                                    gd.DayOffset = offset;
                                }
                            }

                            if (type == "championnat")
                            {
                                int dernieresJourneesMemeJour = int.Parse(e3.Attribute("dernieresJourneesMemeJour").Value);

                                round = new ChampionshipRound(nomTour, String2Hour(hourByDefault), dates, twoLegged, new List<TvOffset>(), initialisationDate, endDate, dernieresJourneesMemeJour);
                            }
                            else if (type == "elimination")
                            {
                                RandomDrawingMethod method = RandomDrawingMethod.Random;
                                if (e3.Attribute("methode") != null)
                                {
                                    method = String2DrawingMethod(e3.Attribute("methode").Value);
                                }
                                bool noRandomDrawing = false;
                                if(e3.Attribute("noRandomDrawing") != null)
                                {
                                    noRandomDrawing = e3.Attribute("noRandomDrawing").Value == "true";
                                }
                                round = new KnockoutRound(nomTour, String2Hour(hourByDefault), dates, new List<TvOffset>(), twoLegged, initialisationDate, endDate, method, noRandomDrawing);
                            }
                            else if (type == "poules")
                            {
                                int groupsNumber = int.Parse(e3.Attribute("nombrePoules").Value);
                                RandomDrawingMethod method = String2DrawingMethod(e3.Attribute("methode").Value);
                                int administrativeLevel = 0;
                                if (method == RandomDrawingMethod.Administrative)
                                {
                                    administrativeLevel = int.Parse(e3.Attribute("administrative_level").Value);
                                }
                                round = new GroupsRound(nomTour, String2Hour(hourByDefault), dates, new List<TvOffset>(), groupsNumber, twoLegged, initialisationDate, endDate, method, administrativeLevel);

                                if (method == RandomDrawingMethod.Geographic)
                                {
                                    //Read groups localisation
                                    for (int groupNum = 1; groupNum <= groupsNumber; groupNum++)
                                    {
                                        if(e3.Attribute("poule" + groupNum) != null)
                                        {
                                            string[] poulePosition = e3.Attribute("poule" + groupNum).Value.Split(';');
                                            float latitude = float.Parse(poulePosition[0], CultureInfo.InvariantCulture);
                                            float longitude = float.Parse(poulePosition[1], CultureInfo.InvariantCulture);
                                            GroupsRound tp = round as GroupsRound;
                                            tp.groupsLocalisation.Add(new GeographicPosition(latitude, longitude));
                                        }
                                    }
                                }

                                foreach (XElement eNoms in e3.Descendants("Nom"))
                                {
                                    GroupsRound tp = round as GroupsRound;
                                    tp.AddGroupName(eNoms.Value);
                                }

                            }
                            else if (type == "inactif")
                            {
                                round = new InactiveRound(nomTour, String2Hour(hourByDefault), initialisationDate, endDate);
                            }
                            c.rounds.Add(round);
                            foreach (XElement e4 in e3.Descendants("Club"))
                            {
                                Club club;
                                int clubId = int.Parse(e4.Attribute("id").Value);
                                if (e4.Attribute("reserve") == null)
                                {
                                    club = _clubsId[clubId];
                                }
                                else
                                {
                                    CityClub firstTeam = _clubsId[int.Parse(e4.Attribute("id").Value)] as CityClub;
                                    string nameAddon = " B";
                                    float divider = 1.75f;
                                    if (firstTeam.reserves.Count == 1)
                                    {
                                        nameAddon = " C"; divider = 2.5f;
                                    }
                                    if (firstTeam.reserves.Count == 2)
                                    {
                                        nameAddon = " D"; divider = 3.5f;
                                    }
                                    if (firstTeam.reserves.Count == 3)
                                    {
                                        nameAddon = " E"; divider = 4.5f;
                                    }
                                    club = new ReserveClub(firstTeam, firstTeam.name + nameAddon, firstTeam.shortName + nameAddon, null);
                                    int newId = NextClubId();
                                    _clubsId[newId] = club;
                                    _kernel.Clubs.Add(club);
                                    firstTeam.reserves.Add(club as ReserveClub);
                                }

                                if(c.periodicity == c.remainingYears)
                                {
                                    round.clubs.Add(club);
                                }
                                //Upcoming competition, so teams are added in the next year case
                                else
                                {
                                    c.AddClubForNextYear(club, roundIndex);
                                }

                            }
                            foreach (XElement e4 in e3.Descendants("Participants"))
                            {
                                int number = int.Parse(e4.Attribute("nombre").Value);
                                IRecoverableTeams source = null;
                                XAttribute continent = e4.Attribute("continent");
                                if (continent != null)
                                {
                                    source = _kernel.String2Continent(continent.Value);
                                }
                                else
                                {
                                    string competitionName = e4.Attribute("competition").Value;
                                    int tourIndex = int.Parse(e4.Attribute("idTour").Value);
                                    Tournament comp = _kernel.String2Tournament(competitionName);
                                    Round r = comp.rounds[tourIndex];
                                    source = r;
                                }
                                RecuperationMethod method;
                                switch (e4.Attribute("methode").Value)
                                {
                                    case "meilleurs":
                                        method = RecuperationMethod.Best;
                                        break;
                                    case "pires":
                                        method = RecuperationMethod.Worst;
                                        break;
                                    case "aleatoire":
                                        method = RecuperationMethod.Randomly;
                                        break;
                                    default:
                                        method = RecuperationMethod.Best;
                                        break;
                                }
                                round.recuperedTeams.Add(new RecoverTeams(source, number, method));
                            }
                            foreach (XElement e4 in e3.Descendants("Decalage"))
                            {
                                int day = int.Parse(e4.Attribute("jour").Value);
                                Hour hour = String2Hour(e4.Attribute("heure").Value);
                                int probability = 1;
                                bool isPrimeTime = false;
                                if (e4.Attribute("probabilite") != null)
                                {
                                    probability = int.Parse(e4.Attribute("probabilite").Value);
                                }
                                int matchDay = 0;
                                if (e4.Attribute("journee") != null)
                                {
                                    matchDay = int.Parse(e4.Attribute("journee").Value);
                                }
                                if (e4.Attribute("prime_time") != null)
                                {
                                    if (e4.Attribute("prime_time").Value == "oui")
                                    {
                                        isPrimeTime = true;
                                    }
                                }
                                TvOffset dtv = new TvOffset(day, hour, probability, matchDay, isPrimeTime);
                                round.programmation.tvScheduling.Add(dtv);
                            }
                            foreach (XElement e4 in e3.Descendants("Regle"))
                            {
                                Rule rule;
                                switch (e4.Attribute("nom").Value)
                                {
                                    case "RECOIT_SI_DEUX_DIVISION_ECART":
                                        rule = Rule.AtHomeIfTwoLevelDifference;
                                        break;
                                    case "EQUIPES_PREMIERES_UNIQUEMENT":
                                        rule = Rule.OnlyFirstTeams;
                                        break;
                                    case "RESERVES_NE_MONTENT_PAS":
                                        rule = Rule.ReservesAreNotPromoted;
                                        break;
                                    case "UN_CLUB_PAR_PAYS_GROUPE":
                                        rule = Rule.OneClubByCountryInGroup;
                                        break;
                                    case "HOSTED_BY_ONE_COUNTRY":
                                        rule = Rule.HostedByOneCountry;
                                        break;
                                    default:
                                        rule = Rule.OnlyFirstTeams;
                                        break;
                                }
                                round.rules.Add(rule);
                            }
                            foreach (XElement e4 in e3.Descendants("Dotation"))
                            {
                                int ranking = int.Parse(e4.Attribute("classement").Value);
                                int prize = int.Parse(e4.Attribute("somme").Value);
                                round.prizes.Add(new Prize(ranking, prize));

                                if(e4.Attribute("rate") != null)
                                {
                                    float rate = float.Parse(e4.Attribute("rate").Value, CultureInfo.InvariantCulture);
                                    int to = int.Parse(e4.Attribute("to").Value);
                                    for (int i = ranking + 1; i <= to; i++)
                                    {
                                        prize = (int)(prize * rate);
                                        round.prizes.Add(new Prize(i, prize));
                                    }
                                }
                            }
                            foreach (XElement e4 in e3.Descendants("Qualification"))
                            {
                                int tourId = int.Parse(e4.Attribute("id_tour").Value);
                                bool nextYear = false;
                                int qualifies = 0;
                                if (e4.Attribute("anneeSuivante") != null)
                                {
                                    nextYear = e4.Attribute("anneeSuivante").Value == "oui";
                                }
                                if (e4.Attribute("qualifies") != null)
                                {
                                    qualifies = int.Parse(e4.Attribute("qualifies").Value);
                                }
                                Tournament targetedTournament = null;
                                if (e4.Attribute("competition") != null)
                                {
                                    targetedTournament = _kernel.String2Tournament(e4.Attribute("competition").Value);
                                }
                                else
                                {
                                    targetedTournament = c;
                                }

                                //Deux cas
                                //1- On a un attribut "classement", avec un classement précis
                                //2- On a deux attributs "de", "a", qui concerne une plage de classement
                                if (e4.Attribute("classement") != null)
                                {
                                    int ranking = int.Parse(e4.Attribute("classement").Value);

                                    Qualification qu = new Qualification(ranking, tourId, targetedTournament, nextYear, qualifies);
                                    round.qualifications.Add(qu);
                                }
                                else
                                {
                                    int from = int.Parse(e4.Attribute("de").Value);
                                    int to = int.Parse(e4.Attribute("a").Value);
                                    for (int j = from; j <= to; j++)
                                    {
                                        Qualification qu = new Qualification(j, tourId, targetedTournament, nextYear, qualifies);
                                        round.qualifications.Add(qu);
                                    }
                                }
                            }
                            roundIndex++;
                        }
                    }
                }

            }

            /*
            foreach (Tournament c in _kernel.Competitions)
            {
                c.InitializeQualificationsNextYearsLists();
            }*/
        }

        public void LoadRules()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/rules.xml");
            foreach (XElement e in doc.Descendants("Rules"))
            {
                foreach (XElement e2 in e.Descendants("Continent"))
                {
                    Continent continent = Session.Instance.Game.kernel.String2Continent(e2.Attribute("name").Value);
                    foreach(XElement e3 in e2.Descendants("Qualifications"))
                    {
                        int rank = int.Parse(e3.Attribute("rank").Value);
                        string tournamentName = e3.Attribute("tournament").Value;
                        Tournament targetTournament = Session.Instance.Game.kernel.String2Tournament(tournamentName);
                        int roundId = int.Parse(e3.Attribute("id_tour").Value);
                        int count = int.Parse(e3.Attribute("count").Value);
                        bool isCupWinner = false;
                        if(e3.Attribute("cup") != null)
                        {
                            isCupWinner = e3.Attribute("cup").Value.ToLower() == "yes";
                        }
                        //Here qualification structure is used to store continental qualifications but meaning of field can differ than "classic" qualification (rank is nation coefficient rank instead of league rank and isNextYear is used for isCupWinner)
                        Qualification q = new Qualification(rank, roundId, targetTournament, isCupWinner, count);
                        continent.continentalQualifications.Add(q);
                    }
                }
            }
        }

        public void LoadLanguages()
        {
            LoadLanguage("Francais", "fr");
            LoadLanguage("Anglais", "en");
        }

        private void LoadLanguage(string languageName, string filename)
        {
            Language language = new Language(languageName);
            string[] text = System.IO.File.ReadAllLines(Utils.dataFolderName + "/" + Utils.namesSubfolderName + "/" + filename + "_p.txt", Encoding.UTF8);
            foreach(string line in text)
            {
                language.AddFirstName(line);
            }
            text = System.IO.File.ReadAllLines(Utils.dataFolderName + "/" + Utils.namesSubfolderName + "/" + filename + "_n.txt", Encoding.UTF8);
            foreach (string line in text)
            {
                language.AddLastName(line);
            }
            _kernel.languages.Add(language);
        }

        public void InitTournaments()
        {
            foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                //TODO: remove this in the reset fonction (reset function to adapt to the first initialization)
                if (t.isHostedByOneCountry)
                {
                    t.InitializeHost();
                }
            }
        }

        public void InitTeams()
        {
            foreach(Club c in _kernel.Clubs)
            {
                ReserveClub reserveClub = c as ReserveClub;
                if (reserveClub != null)
                {
                    //A reserve team was generated, let's create some players in base club to populate this reserve team
                    CityClub firstTeam = reserveClub.FannionClub;

                    string lastChar = reserveClub.name.Split(' ').Last();
                    float divider = 1.75f;
                    if (lastChar.Equals("C"))
                    {
                        divider = 2.5f;
                    }
                    if (lastChar.Equals("D"))
                    {
                        divider = 3.5f;
                    }
                    if (lastChar.Equals("E"))
                    {
                        divider = 4.5f;
                    }
                    int averagePotential = (int)(firstTeam.formationFacilities - (firstTeam.formationFacilities / divider));
                    //Warning for {2,5,5,3} -> 15 is used in team initialisation to determine player number of first team
                    for (int g = 0; g < 2; g++)
                    {
                        firstTeam.GeneratePlayer(Position.Goalkeeper, 16, 23, -averagePotential);
                    }
                    for (int g = 0; g < 5; g++)
                    {
                        firstTeam.GeneratePlayer(Position.Defender, 16, 23, -averagePotential);
                    }
                    for (int g = 0; g < 5; g++)
                    {
                        firstTeam.GeneratePlayer(Position.Midfielder, 16, 23, -averagePotential);
                    }
                    for (int g = 0; g < 3; g++)
                    {
                        firstTeam.GeneratePlayer(Position.Striker, 16, 23, -averagePotential);
                    }
                }
                CityClub cityClub = c as CityClub;
                if(cityClub != null)
                {
                    if (cityClub.city == null)
                    {
                        Country country = cityClub.Championship != null ? Session.Instance.Game.kernel.LocalisationTournament(cityClub.Championship) as Country : _kernel.continents[1].countries[0];
                        if (country.cities.Count == 0)
                        {
                            country.cities.Add(new City(country.Name(), 0, 0, 0));
                        }
                        cityClub.city = country.cities[0];
                        cityClub.stadium.city = country.cities[0];
                        AddStadium(cityClub.stadium);
                    }

                    int firstTeamPlayersNumber = cityClub.contracts.Count - (cityClub.reserves.Count * 15);
                    int missingContractNumber = 19 - firstTeamPlayersNumber;
                    for (int i = 0; i < missingContractNumber; i++)
                    {
                        cityClub.GeneratePlayer(24, 33);
                    }
                    cityClub.DispatchPlayersInReserveTeams();
                    cityClub.GetSponsor();

                    Manager manager = new Manager(cityClub.city.Country().language.GetFirstName(), cityClub.city.Country().language.GetLastName(), cityClub.formationFacilities, new DateTime(1970, 1, 1), cityClub.city.Country());
                    cityClub.ChangeManager(manager);

                }
            }
        }

        public void InitPlayers()
        {
            foreach(Club c in _kernel.Clubs)
            {
                NationalTeam nationalTeam = c as NationalTeam;
                if(nationalTeam != null)
                {
                    int gap = 25 - _kernel.NumberPlayersOfCountry(nationalTeam.country);
                    Console.WriteLine(nationalTeam.name + " - " + _kernel.NumberPlayersOfCountry(nationalTeam.country));
                    if(gap > 0)
                    {
                        for(int i =0; i<gap; i++)
                        {
                            string firstName = nationalTeam.country.language.GetFirstName();
                            string lastName = nationalTeam.country.language.GetLastName();
                            DateTime birthday = new DateTime(Session.Instance.Random(Utils.beginningYear-30, Utils.beginningYear - 18) , Session.Instance.Random(1,13), Session.Instance.Random(1,28));
                            Position p;
                            switch(Session.Instance.Random(1,10))
                            {
                                case 1: case 2: case 3: p = Position.Defender;  break;
                                case 4: case 5: case 6: p = Position.Midfielder;  break;
                                case 7: case 8: p = Position.Striker;  break;
                                default:
                                    p = Position.Goalkeeper;
                                    break;
                            }
                            int level = nationalTeam.formationFacilities + Session.Instance.Random(-5, 5);
                            if(level < 0)
                            {
                                level = 0;
                            }
                            if(level > 99)
                            {
                                level = 99;
                            }
                            Player j = new Player(firstName, lastName, birthday, level, level + 2, nationalTeam.country, p);
                            _kernel.freePlayers.Add(j);
                        }
                    }
                }
            }
            _kernel.NationalTeamsCall();
        }

        public void GenerateNationalCup()
        {
            foreach(Continent ct in Session.Instance.Game.kernel.continents)
            {
                List<int> continentAvailableWeeks = new List<int>();
                for (int i = 30; i < 52 + 15; i++)
                {
                    int week = i % 52;
                    continentAvailableWeeks.Add(week);
                }

                foreach(Tournament t in ct.Tournaments())
                {
                    if(!t.isChampionship && t.periodicity == 1)
                    {
                        foreach(Round r in t.rounds)
                        {
                            foreach(GameDay gd in r.programmation.gamesDays)
                            {
                                if (gd.MidWeekGame)
                                {
                                    continentAvailableWeeks.Remove(gd.WeekNumber);
                                }
                            }
                        }

                    }
                }
                
                continentAvailableWeeks.Remove(51);
                continentAvailableWeeks.Remove(52); //A mid-week game on the last year week lead to the next year
                continentAvailableWeeks.Remove(0); //Bug 2027
                foreach (Country c in ct.countries)
                {
                    List<int> availableWeeks = new List<int>(continentAvailableWeeks);

                    Dictionary<int, int> teamsByLevel = new Dictionary<int, int>();
                    List<KeyValuePair<Tournament, int>> teamsByTournaments = new List<KeyValuePair<Tournament, int>>();
                    bool noCup = true;
                    int totalTeams = 0;
                    foreach (Tournament t in c.Tournaments())
                    {
                        if(!t.isChampionship)
                        {
                            noCup = false;
                        }
                        else
                        {
                            if(!teamsByLevel.ContainsKey(t.level))
                            {
                                teamsByLevel.Add(t.level, 0);
                            }
                            teamsByLevel[t.level] += t.rounds[0].clubs.Count;
                            teamsByTournaments.Add(new KeyValuePair<Tournament, int>(t, t.rounds[0].clubs.Count));

                            totalTeams += t.rounds[0].clubs.Count;

                            foreach(Round r in t.rounds)
                            {
                                foreach(GameDay gd in r.programmation.gamesDays)
                                {
                                    if(gd.MidWeekGame)
                                    {
                                        availableWeeks.Remove(gd.WeekNumber);
                                    }
                                }
                            }
                        }
                    }

                    if (noCup && c.Tournaments().Count > 0 && totalTeams > 1)
                    {

                        string acr = "de ";
                        if(c.Name()[0] == 'E' || c.Name()[0] == 'A' || c.Name()[0] == 'I' || c.Name()[0] == 'O' || c.Name()[0] == 'U')
                        {
                            acr = "d'";
                        }
                        string cupName = "Coupe " + acr + c.Name();
                        Tournament nationalCup = new Tournament(cupName, "",new GameDay(25,false,0,0), cupName, false, 1, 1, 1, new Color(200, 0, 0));

                        int roundCount = 0;
                        int j = 1;
                        while((j*2) <= totalTeams)
                        {
                            j *= 2;
                            roundCount++;
                        }
                        int preliRoundTeams = (totalTeams - j) * 2;
                        if(j != totalTeams)
                        {
                            roundCount++;
                        }

                        int indexRound = 0;
                        //Prelimiary round
                        if (j != totalTeams)
                        {
                            Hour hour = new Hour() { Hours = 20, Minutes = 0 };
                            GameDay gameDate = new GameDay(availableWeeks[(availableWeeks.Count / roundCount) * indexRound], true, 0, 0);
                            GameDay beginDate = new GameDay( (availableWeeks[(availableWeeks.Count / roundCount) * indexRound]-1) % 52, true, 0, 0);
                            GameDay endDate = new GameDay( (availableWeeks[(availableWeeks.Count / roundCount) * indexRound]+1) % 52, false, 0, 0);
                            Round round = new KnockoutRound("Tour préliminaire", hour, new List<GameDay>() { gameDate }, new List<TvOffset>(), false, beginDate, endDate, RandomDrawingMethod.Random, false);
                            round.rules.Add(Rule.AtHomeIfTwoLevelDifference);
                            //round.rules.Add(Rule.OnlyFirstTeams);
                            round.qualifications.Add(new Qualification(1, indexRound + 1, nationalCup, false, 1));
                            int currentAddedTeams = 0;
                            while(currentAddedTeams < preliRoundTeams)
                            {
                                KeyValuePair<Tournament, int> lowerTournament = teamsByTournaments[teamsByTournaments.Count - 1];
                                int teamsToAdd = (currentAddedTeams + lowerTournament.Value) < preliRoundTeams ? lowerTournament.Value : preliRoundTeams-currentAddedTeams;
                                RecoverTeams rt = new RecoverTeams(lowerTournament.Key.rounds[0], teamsToAdd, RecuperationMethod.Worst);
                                round.recuperedTeams.Add(rt);
                                currentAddedTeams += teamsToAdd;
                                if(currentAddedTeams == preliRoundTeams && teamsToAdd < lowerTournament.Value)
                                {
                                    teamsByTournaments[teamsByTournaments.Count - 1] = new KeyValuePair<Tournament, int>(lowerTournament.Key, lowerTournament.Value - teamsToAdd);
                                }
                                else
                                {
                                    teamsByTournaments.RemoveAt(teamsByTournaments.Count - 1);
                                }
                            }
                            nationalCup.rounds.Add(round);
                            indexRound++;
                        }
                        while (j != 1) 
                        {
                            string name = (j/2) + "èmes de finale";
                            if(j == 8)
                            {
                                name = "Quarts de finale";
                            }
                            else if (j == 4)
                            {
                                name = "Demis-finale";
                            }
                            else if (j == 2)
                            {
                                name = "Finale";
                            }

                            Hour hour = new Hour() { Hours = 20, Minutes = 0 };
                            GameDay gameDate = new GameDay(availableWeeks[(availableWeeks.Count / roundCount) * indexRound], true, 0, 0);
                            GameDay beginDate = new GameDay((availableWeeks[(availableWeeks.Count / roundCount) * indexRound] - 1) % 52, true, 0, 0);
                            GameDay endDate = new GameDay((availableWeeks[(availableWeeks.Count / roundCount) * indexRound] + 1) % 52, false, 0, 0);
                            Round round = new KnockoutRound(name, hour, new List<GameDay>() { gameDate }, new List<TvOffset>(), false, beginDate, endDate, j <= 32 ? RandomDrawingMethod.Random : RandomDrawingMethod.Geographic, false);
                            round.rules.Add(Rule.AtHomeIfTwoLevelDifference);
                            //round.rules.Add(Rule.OnlyFirstTeams);
                            if (j > 2)
                            {
                                round.qualifications.Add(new Qualification(1, indexRound + 1, nationalCup, false, 1));
                            }
                            //First final round : add not added teams
                            foreach(KeyValuePair<Tournament, int> kvp in teamsByTournaments)
                            {
                                RecoverTeams rt = new RecoverTeams(kvp.Key.rounds[0], kvp.Value, RecuperationMethod.Best);
                                round.recuperedTeams.Add(rt);
                            }
                            teamsByTournaments.Clear();

                            nationalCup.rounds.Add(round);
                            indexRound++;
                            j /= 2;
                        }

                        int maxPrize = c.FirstDivisionChampionship().rounds[0].prizes[0].Amount / 40;

                        for(int i = roundCount-1; i >= 0; i--)
                        {
                            nationalCup.rounds[i].prizes.Add(new Prize(1, maxPrize));
                            maxPrize /= 2;
                        }

                        nationalCup.InitializeQualificationsNextYearsLists();
                        c.Tournaments().Add(nationalCup);
                    }
                }
            }
        }

        private Hour String2Hour(string hour)
        {
            string[] split = hour.Split(':');
            Hour h = new Hour();
            h.Hours = int.Parse(split[0]);
            h.Minutes = int.Parse(split[1]);
            return h;
        }

        private RandomDrawingMethod String2DrawingMethod(string method)
        {
            RandomDrawingMethod res = RandomDrawingMethod.Level;

            if (method == "Niveau")
            {
                res = RandomDrawingMethod.Level;
            }
            else if (method == "Geographique")
            {
                res = RandomDrawingMethod.Geographic;
            }
            else if (method == "Administrative")
            {
                res = RandomDrawingMethod.Administrative;
            }
            else if (method == "Coefficient")
            {
                res = RandomDrawingMethod.Coefficient;
            }
            else if (method == "Fixed")
            {
                res = RandomDrawingMethod.Fixed;
            }
            else if (method == "Random")
            {
                res = RandomDrawingMethod.Random;
            }
            else if (method == "Ranking")
            {
                res = RandomDrawingMethod.Ranking;
            }

            return res;
        }
    }
}