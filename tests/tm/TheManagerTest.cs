using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using tm;
using tm.Tournaments;
using Path = System.IO.Path;

namespace tests.tm
{
    public class TheManagerTest
    {

        protected void InitGame(string dataset, List<string> activeLeagues)
        {
            string dir = Path.Join((Directory.GetParent(Directory.GetCurrentDirectory())).Parent.Parent.Parent.ToString(), "ui", "bin", "Debug", "net6.0-windows");
            Directory.SetCurrentDirectory(dir);
            Console.WriteLine("[current directory] " + Directory.GetCurrentDirectory());
            Game partie = new Game();
            Session.Instance.Game = partie;
            Kernel g = partie.kernel;
            Utils.dataFolderName = "data\\" + dataset;
            DatabaseLoader cbdd = new DatabaseLoader(g);

            cbdd.LoadLanguages();
            cbdd.LoadWorld();
            cbdd.LoadAudios();
            cbdd.LoadCalendars();
            cbdd.LoadCities();
            cbdd.LoadStadiums();
            cbdd.LoadClubs();
            cbdd.LoadTournaments();
            cbdd.LoadInternationalDates();
            cbdd.LoadPlayers();
            cbdd.LoadManagers();
            cbdd.InitTeams();
            cbdd.InitPlayers();
            cbdd.InitTournaments();
            cbdd.LoadMedias();
            cbdd.LoadGamesComments();
            cbdd.LoadRules();
            cbdd.GenerateNationalCup();
            cbdd.CreateRegionalPathForCups();
            cbdd.LoadArchives();
            Country fr = Session.Instance.Game.kernel.String2Country("France");

            if(activeLeagues != null)
            {
                List<Association> activeAssociations = new List<Association>();
                foreach(string str in activeLeagues)
                {
                    activeAssociations.Add(Session.Instance.Game.kernel.String2Association(str));
                }
                foreach(Association a in new List<Association>(activeAssociations))
                {
                    activeAssociations.AddRange(a.GetAllChilds());
                }
                foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
                {
                    if (c.isChampionship && !activeAssociations.Contains(Session.Instance.Game.kernel.LocalisationTournament(c)))
                    {
                        c.DisableTournament();
                        Console.WriteLine(String.Format("Disable {0}", c.name));
                    }
                }
            }

            //Club club = Session.Instance.Game.kernel.Clubs[70];
            Club club = Session.Instance.Game.kernel.Clubs[445];
            Session.Instance.Game.club = club as CityClub;
            Session.Instance.Game.SetBeginDate(Session.Instance.Game.GetBeginDate(club.Association()));
            Manager manager = new Manager(Session.Instance.Game.kernel.NextIdPerson(), "Name", "Name", 70, new DateTime(1980, 1, 1), fr);
            Session.Instance.Game.club.ChangeManager(manager);
            Session.Instance.Game.options.simulateGames = true;
        }

        private void BackupRound(KnockoutRound round, string name)
        {

            Console.WriteLine(Path.GetDirectoryName(name));
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(name));
            using (StreamWriter outputFile = new StreamWriter(name))
            {
                foreach(Match m in round.matches)
                {
                    outputFile.WriteLine(m.id + "," + m.day.ToString("yyyy-MM-dd HH:mm") + "," + m.home.id + "," + m.away.id + "," + m.score1 + "," + m.score2 + ", " + m.home.name + "," + m.away.name);
                }
            }
        }

        private void BackupRound(GroupInactiveRound round, string name)
        {
            System.IO.Directory.CreateDirectory(Path.GetDirectoryName(name));
            using (StreamWriter outputFile = new StreamWriter(name))
            {
                for (int g = 0; g < round.groupsCount; g++)
                {
                    int i = 0;
                    foreach (Club club in round.Ranking(i))
                    {
                        i++;
                        int points = round.clubs.Count - round.FullRanking().IndexOf(club);
                        outputFile.WriteLine("[" + i + "]," + points + "," + club.id + ", " + club.name);
                    }
                }
            }
        }

        public void BackupLeagues(Country country, string name)
        {
            foreach(Tournament league in country.Leagues())
            {
                foreach(Round round in league.rounds)
                {
                    string fileName = Path.Join(System.IO.Directory.GetCurrentDirectory(), "tests", name, league.Id.ToString(), round.Id.ToString() + ".txt");
                    GroupInactiveRound gr = round as GroupInactiveRound;
                    if(gr != null)
                    {
                        BackupRound(gr, fileName);
                    }
                    KnockoutRound kr = round as KnockoutRound;
                    if(kr != null)
                    {
                        BackupRound(kr, fileName);
                    }
                }
            }
        }

        public void ForceRound(KnockoutRound round, string name)
        {
            var lines = File.ReadAllLines(name);

            List<Club> clubs = new List<Club>();
            int i = 0;
            foreach (string line in lines)
            {
                i++;
                ForceGame(round.matches[i], line);
                clubs.Add(round.matches[i].home);
                clubs.Add(round.matches[i].away);
            }
            //round.ForceClubs(clubs); //TODO
        }

        private void ForceGame(Match match, string line)
        {
            string[] tokens = line.Split(',');
            int id = int.Parse(tokens[0]);

            DateTime.ParseExact(tokens[1], "yyyy-MM-dd HH:mm", null);

            int homeId = int.Parse(tokens[3]);
            int awayId = int.Parse(tokens[4]);
            int score1 = int.Parse(tokens[5]);
            int score2 = int.Parse(tokens[6]);
            //TODO
        }

        public void ForceRound(GroupInactiveRound round, string name)
        {

            var lines = File.ReadAllLines(name);

            List<Club> clubs = new List<Club>();
            for(int i = 0; i< lines.Length; i++)
            {
                clubs.Add(null);
            }
            foreach (string line in lines)
            {
                string[] tokens = line.Split(',');
                int points = int.Parse(tokens[1]);
                int id = int.Parse(tokens[2]);
                clubs[points] = Session.Instance.Game.kernel.GetClubById(id);
            }
            round.ForceRanking(clubs);
        }

        public void ForceLeagues(Country country, string name)
        {
            foreach (Tournament league in country.Leagues())
            {
                foreach (Round round in league.rounds)
                {
                    GroupInactiveRound gr = round as GroupInactiveRound;
                    if (gr != null)
                    {
                        string fileName = Path.Join(System.IO.Directory.GetCurrentDirectory(), "tests", name, league.name, round.name);
                        ForceRound(gr, fileName);
                    }
                }
            }
        }

        public Association MakeBasicStructure()
        {
            Association world = new Association(1, "World", "", null, null, 0, false, null, false);
            Association europe = new Association(2, "Europe", "", null, world, 0, false, null, false);
            world.associations.Add(europe);
            Association france = new Association(3, "France", "", null, europe, 0, false, null, true);
            Association spain = new Association(4, "Spain", "", null, europe, 0, false, null, true);
            europe.associations.Add(france);
            europe.associations.Add(spain);
            Association bfc = new Association(5, "BFC", "", null, france, 0, false, null, false);
            Association nord = new Association(6, "Nord", "", null, france, 0, false, null, false);
            france.associations.Add(bfc);
            france.associations.Add(nord);

            Tournament wt1 = new Tournament(1, "WT1", "", null, "", false, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament wc1 = new Tournament(2, "WC1", "", null, "", true, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament et1 = new Tournament(3, "ET1", "", null, "", false, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament ec1 = new Tournament(4, "EC1", "", null, "", true, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament ft1 = new Tournament(5, "FT1", "", null, "", false, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament fc1 = new Tournament(6, "FL1", "", null, "", true, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament fc2 = new Tournament(11, "FL2", "", null, "", true, 2, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament st1 = new Tournament(7, "ST1", "", null, "", false, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament sc1 = new Tournament(8, "S_LIGA1", "", null, "", true, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament sc2 = new Tournament(14, "S_LIGA2", "", null, "", true, 2, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament bt1 = new Tournament(9, "BC1", "", null, "", false, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament bc1 = new Tournament(10, "B_R1", "", null, "", true, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament bc2 = new Tournament(12, "B_R2", "", null, "", true, 2, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament bc3 = new Tournament(13, "B_R3", "", null, "", true, 3, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament nt1 = new Tournament(14, "NC1", "", null, "", false, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament nr1 = new Tournament(15, "N_R1", "", null, "", true, 1, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);
            Tournament nr2 = new Tournament(15, "N_R2", "", null, "", true, 2, 1, 0, new Color(200, 0, 0), ClubStatus.Professional, null);

            world.tournaments.Add(wt1);
            world.tournaments.Add(wc1);
            europe.tournaments.Add(et1);
            europe.tournaments.Add(ec1);
            france.tournaments.Add(ft1);
            france.tournaments.Add(fc1);
            france.tournaments.Add(fc2);
            spain.tournaments.Add(st1);
            spain.tournaments.Add(sc1);
            spain.tournaments.Add(sc2);
            bfc.tournaments.Add(bt1);
            bfc.tournaments.Add(bc1);
            bfc.tournaments.Add(bc2);
            bfc.tournaments.Add(bc3);
            nord.tournaments.Add(nt1);
            nord.tournaments.Add(nr1);
            nord.tournaments.Add(nr2);

            Session.Instance.Game = new Game();
            Session.Instance.Game.kernel.worldAssociation = world;

            return world;
        }

    }
}
