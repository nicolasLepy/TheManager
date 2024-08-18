using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.IO;
using System.Xml;
using tm.Tournaments;
using System.Data;
using static System.Collections.Specialized.BitVector32;
using static System.Net.WebRequestMethods;
using File = System.IO.File;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace tm
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

        private string SlugifyName(string name)
        {
            return name.Replace(" ", "-").ToLower();
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

            foreach (KeyValuePair<int, Club> kvp in _clubsId)
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

            foreach (XElement x in doc.Descendants("Clubs"))
            {
                foreach (XElement x2 in x.Descendants("Club"))
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

            foreach (XElement x in doc.Descendants("Club"))
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
            LoadWorld();
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
                    switch (type)
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
                        default:
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

        public void LoadAudios()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/audio.xml");
            foreach (XElement e in doc.Descendants("Audio"))
            {
                foreach (XElement e2 in e.Descendants("Sound"))
                {
                    string source = e2.Attribute("source").Value;
                    int min = int.Parse(e2.Attribute("capacity_min").Value);
                    int max = int.Parse(e2.Attribute("capacity_max").Value);
                    string type = e2.Attribute("type").Value;

                    AudioSource audioSource = new AudioSource(_kernel.NextIdAudio(), source, min, max, String2AudioType(type));

                    _kernel.AddAudioSource(audioSource);
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
                    Media m = new Media(_kernel.NextIdMedia(), name, country);
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
                            if (e3.Attribute("national").Value == "oui")
                            {
                                isNational = true;
                            }
                        }

                        if (city == null)
                        {
                            Utils.Debug(e3.Attribute("ville").Value + " n'est pas une ville.");
                        }
                        Journalist j = new Journalist(_kernel.NextIdPerson(), firstName, lastName, age, city, offset, isNational);
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
                    CityClub club = clubId > 0 ? _clubsId[clubId] as CityClub : null;
                    Position position;
                    string positionName = e2.Attribute("poste").Value;
                    string playerBirthName = e2.Attribute("naissance").Value;
                    DateTime playerBirth = new DateTime(int.Parse(playerBirthName.Split('-')[2]), int.Parse(playerBirthName.Split('-')[1]), int.Parse(playerBirthName.Split('-')[0]));
                    switch (positionName)
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
                        default:
                            position = Position.Goalkeeper;
                            break;
                    }
                    Country playerCountry = _kernel.String2Country(e2.Attribute("pays").Value);
                    Player j = new Player(_kernel.NextIdPerson(), firstName, lastName, playerBirth, level, potential, playerCountry == null ? _kernel.String2Country("France") : playerCountry, position);
                    if (club != null)
                    {
                        club.AddPlayer(new Contract(_kernel.NextIdContract(), j, j.EstimateWage(), new DateTime(Session.Instance.Random(Utils.beginningYear, Utils.beginningYear + 5), 7, 1), new DateTime(Session.Instance.Game.date.Year, Session.Instance.Game.date.Month, Session.Instance.Game.date.Day)));
                        j.UpdateClub(club);
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
                    Manager manager = new Manager(_kernel.NextIdPerson(), firstName, lastName, level, new DateTime(1970, 1, 1), country);
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
                        catch (Exception exception)
                        {
                            population = 0;
                        }
                        float lat = float.Parse(e2.Attribute("Latitude").Value, CultureInfo.InvariantCulture);
                        float lon = float.Parse(e2.Attribute("Longitude").Value, CultureInfo.InvariantCulture);
                        string country = e2.Attribute("Country").Value;
                        _kernel.DbCountryName2Country(country).cities.Add(new City(_kernel.NextIdCity(), name, population, lat, lon));
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
                        if (e2.Attribute("Population") == null)
                        {
                            toDelete.Add(e2);
                        }
                    }
                }

                foreach (XElement toDel in toDelete)
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

        public void LoadInternationalDates()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/world.xml");
            foreach(XElement e in doc.Descendants("World"))
            {
                foreach (XElement e2 in e.Descendants("InternationalDates"))
                {
                    GameDay start = e2.Attribute("start") != null ? String2GameDay(e2.Attribute("start").Value) : null;
                    GameDay end = e2.Attribute("end") != null ? String2GameDay(e2.Attribute("end").Value) : null;
                    Tournament t = e2.Attribute("tournament") != null ? _kernel.String2Tournament(e2.Attribute("tournament").Value) : null;
                    _kernel.world.internationalDates.Add(new InternationalDates(start, end, t, false));
                }
            }
        }

        public void LoadWorld()
        {
            XDocument doc = XDocument.Load(Utils.dataFolderName + "/world.xml");
            int maxAdmId = 0;
            foreach (XElement e in doc.Descendants("World"))
            {
                string worldName = e.Attribute("name").Value;
                string worldLogo = e.Attribute("logo").Value;
                int worldResetWeek = int.Parse(e.Attribute("reset_week").Value);
                Continent world = new Continent(_kernel.NextIdContinent(), worldName, worldLogo, worldResetWeek);
                _kernel.world = world;
                foreach (XElement e2 in e.Descendants("Continent"))
                {
                    string continentName = e2.Attribute("name").Value;
                    string continentLogo = e2.Attribute("logo").Value;
                    int continentResetWeek = int.Parse(e2.Attribute("reset_week").Value);
                    Continent c = new Continent(_kernel.NextIdContinent(), continentName, continentLogo, continentResetWeek);
                    
                    foreach (XElement e3 in e2.Descendants("Country"))
                    {
                        string countryName = e3.Attribute("name").Value;
                        string countrydBName = e3.Attribute("db_name").Value;
                        string language = e3.Attribute("langue").Value;
                        int countryShape = int.Parse(e3.Attribute("shape").Value);
                        Language l = _kernel.String2Language(language);
                        int countryResetWeek = int.Parse(e3.Attribute("reset_week").Value);

                        List<AdministrativeSanction> sanctions = new List<AdministrativeSanction>();
                        sanctions.Add(new AdministrativeSanction(SanctionType.Forfeit, 1, 1, 0, 0));
                        sanctions.Add(new AdministrativeSanction(SanctionType.IneligiblePlayer, 1, 1, 0, 0));
                        sanctions.Add(new AdministrativeSanction(SanctionType.FinancialIrregularities, 2, 15, 0, 0));
                        sanctions.Add(new AdministrativeSanction(SanctionType.EnteringAdministration, 3, 3, 0, 0));

                        foreach(XElement e4 in e3.Descendants("Sanction"))
                        {
                            SanctionType sanctionType = String2SanctionType(e4.Attribute("type").Value);
                            int minPoints = 0;
                            int maxPoints = 0;
                            int minRetrogradation = 0;
                            int maxRetrogradation = 0;
                            string strPointsDeduction = e4.Attribute("points-deduction").Value;
                            minPoints = strPointsDeduction.Contains('-') ? int.Parse(strPointsDeduction.Split('-')[0]) : int.Parse(strPointsDeduction);
                            maxPoints = strPointsDeduction.Contains('-') ? int.Parse(strPointsDeduction.Split('-')[1]) : int.Parse(strPointsDeduction);
                            if (e4.Attribute("retrogradation") != null)
                            {
                                string strRetrogradation = e4.Attribute("retrogradation").Value;
                                minRetrogradation = strRetrogradation.Contains('-') ? int.Parse(strRetrogradation.Split('-')[0]) : int.Parse(strRetrogradation);
                                maxRetrogradation = strRetrogradation.Contains('-') ? int.Parse(strRetrogradation.Split('-')[1]) : int.Parse(strRetrogradation);
                            }
                            AdministrativeSanction newAs = new AdministrativeSanction(sanctionType, minPoints, maxPoints, minRetrogradation, maxRetrogradation);
                            for(int i = 0; i < sanctions.Count; i++)
                            {
                                if (sanctions[i].type == sanctionType)
                                {
                                    sanctions[i] = newAs;
                                }
                            }
                        }

                        Country ct = new Country(_kernel.NextIdCountry(), countrydBName, countryName, l, countryShape, countryResetWeek, sanctions);
                        foreach (XElement e4 in e3.Descendants("Ville"))
                        {
                            string cityName = e4.Attribute("nom").Value;
                            int population = int.Parse(e4.Attribute("population").Value);
                            float lat = float.Parse(e4.Attribute("Latitute").Value);
                            float lon = float.Parse(e4.Attribute("Longitude").Value);

                            City cty = new City(_kernel.NextIdCity(), cityName, population, lat, lon);
                            ct.cities.Add(cty);
                        }

                        foreach (XElement e4 in e3.Descendants("AdministrativeDivision"))
                        {
                            string administrationName = e4.Attribute("name").Value;
                            int administrationId = int.Parse(e4.Attribute("id").Value);
                            int administrationParent = e4.Attribute("parent") != null ? int.Parse(e4.Attribute("parent").Value) : 0;
                            maxAdmId = administrationId > maxAdmId ? administrationId : maxAdmId;
                            Association ad = new Association(administrationId, administrationName, ct, null, ct.resetWeek, false);
                            if (administrationParent > 0)
                            {
                                ct.GetAssociation(administrationParent).divisions.Add(ad);
                                ad.parent = ct.GetAssociation(administrationParent);
                            }
                            else
                            {
                                ct.associations.Add(ad);
                            }
                        }

                        foreach (bool weekdaysDates in new[] { false, true })
                        {
                            string[] days = weekdaysDates ? new[] { "Monday", "Tuesday", "Wednesday", "Thursday" } : new[] { "Friday", "Saturday", "Sunday", "Monday" };
                            int previousLevel = -1;
                            foreach (XElement e4 in e3.Descendants("GamesTimes"))
                            {
                                if (((e4.Attribute("weekdays") == null || e4.Attribute("weekdays").Value == "false") && !weekdaysDates) || (e4.Attribute("weekdays") != null && e4.Attribute("weekdays").Value == "true" && weekdaysDates))
                                {
                                    int level = int.Parse(e4.Attribute("level").Value);
                                    if (previousLevel != -1)
                                    {
                                        for (int i = previousLevel; i < level; i++)
                                        {
                                            float[] last = weekdaysDates ? ct.gamesTimesWeekdays.Last() : ct.gamesTimesWeekend.Last();
                                            float[] newArray = new float[Utils.gamesTimesHoursCount * Utils.gamesTimesDaysCount];
                                            for (int d1 = 0; d1 < last.Length; d1++)
                                            {
                                                newArray[d1] = last[d1];
                                            }
                                            if (weekdaysDates)
                                            {
                                                ct.gamesTimesWeekdays.Add(newArray);
                                            }
                                            else
                                            {
                                                ct.gamesTimesWeekend.Add(newArray);
                                            }
                                        }
                                    }
                                    previousLevel = level;
                                    float[] gamesTimes = new float[Utils.gamesTimesHoursCount * Utils.gamesTimesDaysCount];
                                    foreach (XElement e5 in e4.Descendants("GamesDay"))
                                    {
                                        int dayIndex = Array.IndexOf(days, e5.Attribute("day").Value);
                                        for (int j = 0; j < Utils.gamesTimesHoursCount; j++)
                                        {
                                            if (e5.Attribute("h" + j) != null)
                                            {
                                                float value = float.Parse(e5.Attribute("h" + j).Value, CultureInfo.InvariantCulture);
                                                gamesTimes[dayIndex * Utils.gamesTimesHoursCount + j] = value;
                                            }
                                        }
                                    }
                                    if (weekdaysDates)
                                    {
                                        ct.gamesTimesWeekdays.Add(gamesTimes);
                                    }
                                    else
                                    {
                                        ct.gamesTimesWeekend.Add(gamesTimes);
                                    }
                                }
                            }
                        }

                        c.countries.Add(ct);
                    }
                    _kernel.world.continents.Add(c);
                }
            }

            maxAdmId++;
            Association fifa = new Association(maxAdmId++, _kernel.world.Name(), _kernel.world, null, _kernel.world.resetWeek, false);
            foreach (Continent c in _kernel.world.continents)
            {
                Association ca = new Association(maxAdmId++, c.Name(), c, fifa, c.resetWeek, true);
                foreach (Country cc in c.countries)
                {
                    Association adCountry = new Association(maxAdmId++, cc.Name(), cc, ca, cc.resetWeek, false);
                    cc.associations.Add(adCountry);
                    ca.divisions.Add(adCountry);
                    foreach (Association a in cc.associations)
                    {
                        if(a.parent == null)
                        {
                            a.parent = adCountry;
                        }
                    }

                }
                fifa.divisions.Add(ca);
            }
            _kernel.worldAssociation = fifa;
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
                    Stadium s = new Stadium(_kernel.NextIdStadium(), name, capacity, v);
                    v.Country().stadiums.Add(s);
                }
            }
        }

        private string RemoveClubDenomination(string name, string[] denominations)
        {
            string res = name;
            foreach(string denomination in denominations)
            {
                res = res.Replace(denomination + " ", "");
                res = res.Replace(" " + denomination, "");
            }
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
            foreach (string xmlFile in Directory.EnumerateFiles(Utils.dataFolderName + "/clubs/"))
            {
                //XDocument doc = XDocument.Load(Utils.dataFolderName + "/clubs/clubs.xml");
                Utils.Debug(xmlFile);
                XDocument doc = XDocument.Load(xmlFile);

                string[] denominations = new [] { "Olympique", "FC", "AS", "US", "RC", "AC", "ES", "SO", "USM", "OC" };

                foreach (XElement e in doc.Descendants("Clubs"))
                {
                    ClubStatus generalStatus = e.Attribute("status") != null ? String2ClubStatus(e.Attribute("status").Value) : ClubStatus.Professional;
                    foreach (XElement e2 in e.Descendants("Club"))
                    {
                        int id = int.Parse(e2.Attribute("id").Value);
                        string name = e2.Attribute("nom").Value;
                        string shortName = e2.Attribute("nomCourt") != null ? e2.Attribute("nomCourt").Value : "";
                        if (shortName == "")
                        {
                            shortName = RemoveClubDenomination(name, denominations);
                        }

                        int reputation = int.Parse(e2.Attribute("reputation").Value);
                        int budget = int.Parse(e2.Attribute("budget").Value);
                        int supporters = int.Parse(e2.Attribute("supporters").Value);
                        ClubStatus status = e2.Attribute("status") != null ? String2ClubStatus(e2.Attribute("status").Value) : generalStatus;

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
                            stadium = new Stadium(_kernel.NextIdStadium(), stadiumName, capacite, city);
                            if(city != null)
                            {
                                AddStadium(stadium);
                            }
                        }
                        
                        int idAssociation = 0;
                        Association association = null;
                        if (e2.Attribute("administrativeDivision") != null)
                        {
                            idAssociation = int.Parse(e2.Attribute("administrativeDivision").Value);
                            association = _kernel.GetAssociation(idAssociation); //city?.Country().GetAssociation(idAdministrativeDivision);
                        }

                        if (association == null)
                        {
                            association = city?.Country().GetCountryAssociation();
                        }

                        int centreFormation = int.Parse(e2.Attribute("centreFormation").Value);
                        string logo = e2.Attribute("logo").Value;
                        if (logo == "" ||
                            !File.Exists(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\" + Utils.clubLogoFolderName + "\\" + logo + ".png"))
                        {
                            logo = "generic";
                        }

                        string goalSong = "";
                        if (e2.Attribute("goalSong") != null)
                        {
                            goalSong = e2.Attribute("goalSong").Value;
                        }
                        else
                        {
                            goalSong = "null";
                        }

                        //Simplification
                        reputation = centreFormation;
                        
                        bool equipePremiere = true;
                        Club c = new CityClub(id, name, null, shortName, reputation, budget, supporters, centreFormation, city, logo, stadium, goalSong, equipePremiere, association, status);
                        if(_clubsId.ContainsKey(id))
                        {
                            Console.WriteLine("[Club conflict]" + id + " : " + _clubsId[id].name + " vs " + c.name);
                        }
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
                            stadium = new Stadium(_kernel.NextIdStadium(), "Stade de " + shortName, 75000, null);
                        }
                        int formationFacilities = int.Parse(e2.Attribute("centreFormation").Value);
                        string logo = country.Flag;

                        string goalSong = "";
                        if (e2.Attribute("goalSong") != null)
                        {
                            goalSong = e2.Attribute("goalSong").Value;
                        }
                        else
                        {
                            goalSong = "null";
                        }
                        float points = 0;
                        if (e2.Attribute("points") != null)
                        {
                            points = float.Parse(e2.Attribute("points").Value);
                        }

                        points = formationFacilities;
                        Manager entraineur = new Manager(_kernel.NextIdPerson(), country.language.GetFirstName(), country.language.GetLastName(), formationFacilities, new DateTime(1970, 1, 1), country);

                        Club c = new NationalTeam(id, name, entraineur, shortName, reputation, supporters, formationFacilities, logo, stadium, country, goalSong, points);
                        if (_clubsId.ContainsKey(id))
                        {
                            Console.WriteLine("[Club conflict]" + id + " : " + _clubsId[id].name + " vs " + c.name);
                        }
                        _clubsId[id] = c;
                        _kernel.Clubs.Add(c);
                    }
                }
            }

        }
        
        public void LoadArchives()
        {
            foreach(string xmlFile in Directory.EnumerateFiles(Utils.dataFolderName + "/arch/"))
            {
                XDocument doc = XDocument.Load(xmlFile);
                foreach(XElement a in doc.Descendants("Archives"))
                {
                    foreach (XElement e in a.Descendants("HistoricEntry"))
                    {
                        int season = int.Parse(e.Attribute("season").Value);
                        string tournamentName = e.Attribute("tournament").Value;
                        Tournament t = _kernel.String2Tournament(tournamentName);
                        Tournament archive = t.CopyForArchive(true);
                        foreach (Round r in archive.rounds)
                        {
                            r.matches.Clear();
                            r.clubs.Clear();
                        }

                        foreach (XElement e2 in e.Descendants("Ranking"))
                        {
                            int roundId = int.Parse(e2.Attribute("round").Value);
                            List<Club> ranking = new List<Club>();
                            foreach (XElement e3 in e2.Descendants("Club"))
                            {
                                int clubId = int.Parse(e3.Attribute("id").Value);
                                Club c = _clubsId[clubId];
                                ranking.Add(c);
                                (archive.rounds[roundId] as GroupInactiveRound).AddClub(c);
                            }
                            (archive.rounds[roundId] as GroupInactiveRound).SetRanking(ranking);
                        }
                        t.previousEditions.Add(season, archive);
                    }
                }
            }
        }

        public void SetStartClubId()
        {
            int maxId = 0;
            foreach(Club club in _kernel.Clubs)
            {
                maxId = club.id > maxId ? club.id : maxId;
            }
            _kernel.SetClubIdIterator(maxId);
        }

        public void LoadTournaments()
        {
            SetStartClubId();
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
                        string tournamentRuleStr = e2.Attribute("rule") != null ? e2.Attribute("rule").Value : null;
                        bool isChampionship = e2.Attribute("championnat").Value == "oui" ? true : false;
                        ClubStatus tournamentStatus = ClubStatus.Professional;
                        if(isChampionship)
                        {
                            tournamentStatus = String2ClubStatus(e2.Attribute("status").Value);
                        }
                        int level = int.Parse(e2.Attribute("niveau").Value);
                        ILocalisation localisation = _kernel.String2Localisation(e2.Attribute("localisation").Value);
                        GameDay debut = String2GameDay(seasonBeginning);
                        int periodicity = 1;
                        if (e2.Attribute("periodicity") != null)
                        {
                            periodicity = int.Parse(e2.Attribute("periodicity").Value);
                        }
                        int remainingYears = 1;
                        if (e2.Attribute("anneesRestantes") != null)
                        {
                            remainingYears = int.Parse(e2.Attribute("anneesRestantes").Value);
                        }

                        string[] colorStr = e2.Attribute("color").Value.Split(',');
                        Color color = new Color(byte.Parse(colorStr[0]), byte.Parse(colorStr[1]), byte.Parse(colorStr[2]));

                        Console.WriteLine(name);
                        Tournament tournament = new Tournament(_kernel.NextIdTournament(), name, logo, debut, shortName, isChampionship, level, periodicity, remainingYears, color, tournamentStatus, new ParentTournament());
                        if (tournamentRuleStr != null)
                        {
                            TournamentRule tRule;
                            switch (tournamentRuleStr)
                            {
                                case "OnWinnerQualifiedAdaptClubsQualifications":
                                    tRule = TournamentRule.OnWinnerQualifiedAdaptClubsQualifications;
                                    break;
                                case "OnWinnerQualifiedAdaptAssociationQualifications":
                                default:
                                    tRule = TournamentRule.OnWinnerQualifiedAdaptAssociationQualifications;
                                    break;
                            }
                            tournament.rules.Add(tRule);
                        }

                        tournament.InitializeQualificationsNextYearsLists(e2.Descendants("Tour").Count());


                        //Continental tournaments are stored by their association
                        if(localisation as Continent != null)
                        {
                            foreach(Association a in _kernel.GetAllAssociations())
                            {
                                if (a.localisation == localisation)
                                {
                                    a.tournaments.Add(tournament);
                                }
                            }
                        }
                        else
                        {
                            localisation.Tournaments().Add(tournament);
                        }
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
                            string roundName = e3.Attribute("nom").Value;
                            int phases = int.Parse(e3.Attribute("phases").Value);
                            string hourByDefault = e3.Attribute("heureParDefaut").Value;
                            int keepRankingFromPreviousRound = e3.Attribute("keep_ranking_from_previous_round") != null ? int.Parse(e3.Attribute("keep_ranking_from_previous_round").Value) : -1;
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
                            int gamesPriority = 0;
                            if(e3.Attribute("gamesPriority") != null)
                            {
                                gamesPriority = int.Parse(e3.Attribute("gamesPriority").Value);
                            }
                            if (type == "championnat")
                            {
                                int lastDaysSameDay = int.Parse(e3.Attribute("dernieresJourneesMemeJour").Value);
                                //round = new ChampionshipRound(_kernel.NextIdRound(), roundName, c, String2Hour(hourByDefault), dates, phases, new List<TvOffset>(), initialisationDate, endDate, keepRankingFromPreviousRound, lastDaysSameDay, gamesPriority);
                                round = new GroupActiveRound(_kernel.NextIdRound(), roundName, c, String2Hour(hourByDefault), dates, new List<TvOffset>(), 1, phases, initialisationDate, endDate, keepRankingFromPreviousRound, RandomDrawingMethod.Level, 0, false, 0, 0, gamesPriority, lastDaysSameDay);
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
                                round = new KnockoutRound(_kernel.NextIdRound(), roundName, c, String2Hour(hourByDefault), dates, new List<TvOffset>(), phases, initialisationDate, endDate, method, noRandomDrawing, gamesPriority);
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
                                int nonConferencesGamesByTeams = e3.Attribute("non_conferences_games_by_teams") != null ? int.Parse(e3.Attribute("non_conferences_games_by_teams").Value) : 0;
                                bool fusionConferenceAndNoConferenceGames = e3.Attribute("fusion_conferences_and_non_conferences_days") != null ? e3.Attribute("fusion_conferences_and_non_conferences_days").Value.ToLower() == "yes" : false;
                                int nonConferencesGamesByGameday = e3.Attribute("non_conferences_games_by_gameday") != null ? int.Parse(e3.Attribute("non_conferences_games_by_gameday").Value) : 0;
                                round = new GroupActiveRound(_kernel.NextIdRound(), roundName, c, String2Hour(hourByDefault), dates, new List<TvOffset>(), groupsNumber, phases, initialisationDate, endDate, keepRankingFromPreviousRound, method, administrativeLevel, fusionConferenceAndNoConferenceGames, nonConferencesGamesByTeams, nonConferencesGamesByGameday, gamesPriority, 0);

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
                                int administrativeLevel = 0;
                                if(e3.Attribute("administrative_level") != null)
                                {
                                    administrativeLevel = int.Parse(e3.Attribute("administrative_level").Value);
                                }
                                int groupsCount = 1;
                                round = new GroupInactiveRound(_kernel.NextIdRound(), roundName, c, String2Hour(hourByDefault), new List<GameDay>(), new List<TvOffset>(), groupsCount, 1, initialisationDate, endDate, -1, administrativeLevel == 0 ? RandomDrawingMethod.Random : RandomDrawingMethod.Administrative, administrativeLevel, false, 0, 0, 0);
                            }
                            foreach(XElement e4 in e3.Descendants("TeamsByAdministrativeDivision"))
                            {
                                int administrativeId = int.Parse(e4.Attribute("id").Value);
                                Association ad = _kernel.GetAssociation(administrativeId);
                                int teamsCount = int.Parse(e4.Attribute("teams").Value);
                                round.teamsByAssociation.Add(ad, teamsCount);
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
                                    if (firstTeam.reserves.Count == 4)
                                    {
                                        nameAddon = " F"; divider = 5f;
                                    }
                                    if (firstTeam.reserves.Count == 5)
                                    {
                                        nameAddon = " G"; divider = 5.5f;
                                    }
                                    club = new ReserveClub(_kernel.NextIdClub(), firstTeam, firstTeam.name + nameAddon, firstTeam.shortName + nameAddon, null);
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
                                    case "international":
                                        method = RecuperationMethod.QualifiedForInternationalCompetition;
                                        break;
                                    case "notinternational":
                                        method = RecuperationMethod.NotQualifiedForInternationalCompetitionBest;
                                        break;
                                    case "pro":
                                        method = RecuperationMethod.StatusPro;
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
                                Rule rule = String2Rule(e4.Attribute("nom").Value);
                                round.rules.Add(rule);
                            }
                            List<Tiebreaker> tiebreakers = new List<Tiebreaker> { Tiebreaker.GoalDifference, Tiebreaker.GoalFor, Tiebreaker.GoalAgainst, Tiebreaker.HeadToHead, Tiebreaker.Discipline };
                            if(e3.Attribute("tiebreakers") != null)
                            {
                                List<Tiebreaker> definedTiebreakers = new List<Tiebreaker>();
                                string tiebreakersStr = e3.Attribute("tiebreakers").Value;
                                string[] tiebreakersArray = tiebreakersStr.Replace(" ", "").Split(',');
                                foreach(string tiebreakerStr in tiebreakersArray)
                                {
                                    Tiebreaker tiebreaker = String2Tiebreaker(tiebreakerStr);
                                    definedTiebreakers.Add(tiebreaker);
                                }
                                foreach(Tiebreaker tb in tiebreakers)
                                {
                                    if(!definedTiebreakers.Contains(tb))
                                    {
                                        definedTiebreakers.Add(tb);
                                    }
                                }
                                tiebreakers = definedTiebreakers;
                            }
                            round.tiebreakers.AddRange(tiebreakers);
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
                            foreach (XElement e4 in e3.Descendants("RelegationRegion"))
                            {
                                int administrativeId = int.Parse(e4.Attribute("administrative_id").Value);
                                int relegations = int.Parse(e4.Attribute("relegations").Value);
                                Association ad = _kernel.GetAssociation(administrativeId);
                                GroupsRound gr = round as GroupsRound;
                                if(gr != null && ad != null)
                                {
                                    gr.relegationsByAssociations.Add(ad, relegations);
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
                    Association association = null;
                    foreach(Association a in _kernel.GetAllAssociations())
                    {
                        if(a.localisation == continent)
                        {
                            association = a;
                        }
                    }
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
                        association.continentalQualifications.Add(q);
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
                        Country country = cityClub.Championship != null ? Session.Instance.Game.kernel.LocalisationTournament(cityClub.Championship) as Country : _kernel.world.continents[1].countries[0];
                        if (country.cities.Count == 0)
                        {
                            country.cities.Add(new City(_kernel.NextIdCity(), country.Name(), 0, 0, 0));
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

                    Manager manager = new Manager(_kernel.NextIdPerson(), cityClub.city.Country().language.GetFirstName(), cityClub.city.Country().language.GetLastName(), cityClub.formationFacilities, new DateTime(1970, 1, 1), cityClub.city.Country());
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
                            Player j = new Player(_kernel.NextIdPerson(), firstName, lastName, birthday, level, level + 2, nationalTeam.country, p);
                            _kernel.freePlayers.Add(j);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Split each cup with regional qualification to multiple regional competitions
        /// </summary>
        public void CreateRegionalPathForCups()
        {
            foreach(Continent ct in Session.Instance.Game.kernel.world.continents)
            {
                foreach(Country c in ct.countries)
                {
                    List<Tournament> ts = new List<Tournament>(c.Tournaments());
                    foreach(Tournament t in ts)
                    {
                        t.CreateRegionalPathForCup();
                    }
                }
            }

        }

        public void GenerateRegionalCup(Country c, Association association, int level, bool reservesAllowed)
        {
            Utils.Debug(string.Format("[{0}] New regional Cup", association.name));
            int admTeams = 0;
            bool regionalLeagueExists = false;
            bool regionalCupExist = false; //TODO
            foreach(Tournament t in c.Leagues())
            {
                GroupsRound groupRound = t.GetLastChampionshipRound() as GroupsRound;
                if (groupRound != null && groupRound.administrativeLevel == level)
                {
                    regionalLeagueExists = true;
                    foreach(Club cl in groupRound.clubs)
                    {
                        admTeams = association.ContainsAssociation(cl.Association()) ? admTeams + 1 : admTeams;
                    }
                }
            }
            if(regionalLeagueExists && admTeams > 1 && !regionalCupExist)
            {
                CreateNationalCup(c, association, false, false, reservesAllowed);
            }
            foreach (Association ad in association.divisions)
            {
                GenerateRegionalCup(c, ad, level + 1, reservesAllowed);
            }
        }

        public void GenerateNationalCup()
        {
            foreach(Continent ct in Session.Instance.Game.kernel.world.continents)
            {
                foreach(Country c in ct.countries)
                {
                    bool noCup = true;
                    int totalTeams = 0;
                    foreach (Tournament t in c.Tournaments())
                    {
                        if (!t.isChampionship)
                        {
                            noCup = false;
                        }
                        else
                        {
                            totalTeams += t.rounds[0].CountWithoutReserves();
                        }
                    }
                    if (noCup && c.Tournaments().Count > 0 && totalTeams > 1)
                    {
                        CreateNationalCup(c, null, false, true, false);
                    }
                    if(c.Tournaments().Count > 0 && totalTeams > 1)
                    {
                        foreach (Association ad in c.associations)
                        {
                            //GenerateRegionalCup(c, ad, 1, false);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Count teams of a round excluding reserves
        /// </summary>
        /// <param name="r">Count teams of this round</param>
        /// <param name="association">Filter with a particular association</param>
        private int CountTeamsWithoutReserves(Round r, Association association)
        {
            int total = 0;
            foreach (Club c in r.clubs)
            {
                if (c as ReserveClub == null)
                {
                    if(association == null || association.ContainsAssociation(c.Association()))
                    {
                        total++;
                    }
                }
            }
            return total;
        }

        /// <summary>
        /// Create a national/regional cup
        /// </summary>
        /// <param name="country">Create cup for this country</param>
        /// <param name="association">Only teams of this association are allowed to enter</param>
        /// <param name="allowTeamsOfSubAdministrativesDivision">Teams playing on a child association can enter</param>
        /// <param name="midweekGames">TODO: Not used yet. Games are played wednesday</param>
        /// <param name="reservesAllowed">Reserves allowed to enter</param>
        public void CreateNationalCup(Country c, Association association, bool allowTeamsOfSubAdministrativesDivision, bool midweekGames, bool reservesAllowed)
        {
            Dictionary<int, int> teamsByLevel = new Dictionary<int, int>();
            List<KeyValuePair<Tournament, int>> teamsByTournaments = new List<KeyValuePair<Tournament, int>>();
            int totalTeams = 0;
            int associationLevel = c.GetLevelOfAssociation(association);
            int currentAdministrativeLevel = 0;
            foreach (Tournament t in c.Tournaments())
            {
                GroupsRound roundChampionship = (t.GetLastChampionshipRound() as GroupsRound);
                if(roundChampionship != null)
                {
                    currentAdministrativeLevel = roundChampionship.administrativeLevel;
                }
                bool associationGoodLevel = association == null || (associationLevel == currentAdministrativeLevel || (allowTeamsOfSubAdministrativesDivision && associationLevel < currentAdministrativeLevel));


                if (t.isChampionship && associationGoodLevel)
                {
                    if(!teamsByLevel.ContainsKey(t.level))
                    {
                        teamsByLevel.Add(t.level, 0);
                    }
                    teamsByLevel[t.level] += CountTeamsWithoutReserves(t.rounds[0], association);
                    teamsByTournaments.Add(new KeyValuePair<Tournament, int>(t, CountTeamsWithoutReserves(t.rounds[0], association)));

                    totalTeams += CountTeamsWithoutReserves(t.rounds[0], association);
                }
            }

            List<GameDay> availableWeeks = c.GetAvailableCalendarDates(association == null, 2, teamsByLevel.Keys.ToList(), true, false);
            for(int week=25; week<(association == null ? 40 : 48); week++) //52
            {
                availableWeeks.RemoveAll(s => s.WeekNumber == week);
            }

            string tournamentName = association == null ? c.Name() : association.name;
            string acr = "de ";
            if(tournamentName[0] == 'E' || tournamentName[0] == 'A' || tournamentName[0] == 'I' || tournamentName[0] == 'O' || tournamentName[0] == 'U')
            {
                acr = "d'";
            }
            string cupName = "Coupe " + acr + tournamentName;
            int cupLevel = association != null ? 3 : 1; //c.Cups().Count + 1;
            Tournament nationalCup = new Tournament(_kernel.NextIdTournament(), cupName, "",new GameDay(c.resetWeek,false,0,0), cupName, false, cupLevel, 1, 1, new Color(200, 0, 0), ClubStatus.Professional, new ParentTournament(association, null));

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
                int weekIndex = (availableWeeks.Count / roundCount) * indexRound;
                GameDay gameDate = new GameDay(availableWeeks[weekIndex].WeekNumber, true, 0, 0);
                GameDay beginDate = new GameDay( (availableWeeks[weekIndex].WeekNumber - 1) % 52, true, 0, 0);
                GameDay endDate = new GameDay( (availableWeeks[weekIndex].WeekNumber + 1) % 52, false, 0, 0);
                Round round = new KnockoutRound(_kernel.NextIdRound(), "Tour préliminaire", nationalCup, hour, new List<GameDay> { gameDate }, new List<TvOffset>(), 1, beginDate, endDate, RandomDrawingMethod.Random, false, 2);
                round.rules.Add(Rule.AtHomeIfTwoLevelDifference);
                if(!reservesAllowed)
                {
                    round.rules.Add(Rule.OnlyFirstTeams);
                }
                round.qualifications.Add(new Qualification(1, indexRound + 1, nationalCup, false, 1));
                int currentAddedTeams = 0;
                while(currentAddedTeams < preliRoundTeams)
                {
                    KeyValuePair<Tournament, int> lowerTournament = teamsByTournaments[teamsByTournaments.Count - 1];
                    int teamsToAdd = (currentAddedTeams + lowerTournament.Value) < preliRoundTeams ? lowerTournament.Value : preliRoundTeams-currentAddedTeams;
                    RecuperationMethod recuperationMethod = teamsToAdd == lowerTournament.Value ? RecuperationMethod.Best : RecuperationMethod.Worst;
                    RecoverTeams rt = new RecoverTeams(lowerTournament.Key.rounds[0], teamsToAdd, recuperationMethod);
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
                int weekIndex = (availableWeeks.Count / roundCount) * indexRound;
                GameDay gameDate = new GameDay(availableWeeks[weekIndex].WeekNumber, true, 0, 0);
                GameDay beginDate = new GameDay((availableWeeks[weekIndex].WeekNumber - 1) % 52, true, 0, 0);
                GameDay endDate = new GameDay((availableWeeks[weekIndex].WeekNumber + 1) % 52, false, 0, 0);
                Round round = new KnockoutRound(_kernel.NextIdRound(), name, nationalCup, hour, new List<GameDay> { gameDate }, new List<TvOffset>(), 1, beginDate, endDate, j <= 32 ? RandomDrawingMethod.Random : RandomDrawingMethod.Geographic, false, 2);
                round.rules.Add(Rule.AtHomeIfTwoLevelDifference);
                if(!reservesAllowed)
                {
                    round.rules.Add(Rule.OnlyFirstTeams);
                }
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

            int maxPrize = c.FirstDivisionChampionship().rounds[0].prizes.Count > 0 ? c.FirstDivisionChampionship().rounds[0].prizes[0].Amount / 40 : 0;

            for(int i = roundCount-1; i >= 0; i--)
            {
                nationalCup.rounds[i].prizes.Add(new Prize(1, maxPrize));
                maxPrize /= 2;
            }

            nationalCup.InitializeQualificationsNextYearsLists();
            c.Tournaments().Add(nationalCup);
        }

        private Hour String2Hour(string hour)
        {
            string[] split = hour.Split(':');
            Hour h = new Hour();
            h.Hours = int.Parse(split[0]);
            h.Minutes = int.Parse(split[1]);
            return h;
        }

        private AudioType String2AudioType(string value)
        {
            AudioType type;
            switch (value)
            {
                case "background":
                    type = AudioType.Background;
                    break;
                case "goal":
                default:
                    type = AudioType.Event;
                    break;
            }
            return type;
        }

        private ClubStatus String2ClubStatus(string status)
        {
            ClubStatus res = ClubStatus.Professional;
            if(status == "pro")
            {
                res = ClubStatus.Professional;
            }
            else if (status == "semi-pro")
            {
                res = ClubStatus.SemiProfessional;
            }
            else if (status == "amateur")
            {
                res = ClubStatus.Amateur;
            }
            return res;
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

        private Tiebreaker String2Tiebreaker(string value)
        {
            Tiebreaker res = Tiebreaker.GoalDifference;
            if(value == "goal-difference")
            {
                res = Tiebreaker.GoalDifference;
            }
            else if (value == "head-to-head")
            {
                res = Tiebreaker.HeadToHead;
            }
            else if (value == "goal-for")
            {
                res = Tiebreaker.GoalFor;
            }
            else if (value == "goal-against")
            {
                res = Tiebreaker.GoalAgainst;
            }
            else if (value == "discipline")
            {
                res = Tiebreaker.Discipline;
            }
            return res;
        }

        private SanctionType String2SanctionType(string value)
        {
            SanctionType type;
            switch(value)
            {
                case "entering-administration":
                    type = SanctionType.EnteringAdministration;
                    break;
                case "forfeit":
                    type = SanctionType.Forfeit;
                    break;
                case "ineligible-player":
                    type = SanctionType.IneligiblePlayer;
                    break;
                case "financial-irregularities":
                default:
                    type = SanctionType.FinancialIrregularities;
                    break;
            }
            return type;
        }
        private Rule String2Rule(string value)
        {
            Rule rule;
            switch (value)
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
                case "ONE_TEAM_BY_CONTINENT_IN_GROUP":
                    rule = Rule.OneTeamByContinentInGroup;
                    break;
                case "HOSTED_BY_ONE_COUNTRY":
                    rule = Rule.HostedByOneCountry;
                    break;
                case "ULTRAMARINE_TEAMS_CAN_PLAY_HOME_OR_AWAY":
                    rule = Rule.UltramarineTeamsPlayHomeOrAway;
                    break;
                case "ULTRAMARINE_TEAMS_PLAY_AWAY":
                    rule = Rule.UltramarineTeamsPlayAway;
                    break;
                case "ULTRAMARINE_TEAMS_CANT_COMPETE_AGAINST":
                    rule = Rule.UltramarineTeamsCantCompeteAgainst;
                    break;
                case "BOTTOM_TEAM_NOT_ELIGIBLE_FOR_REPECHAGE":
                    rule = Rule.BottomTeamNotEligibleForRepechage;
                    break;
                default:
                    rule = Rule.OnlyFirstTeams;
                    break;
            }
            return rule;
        }
    }
}