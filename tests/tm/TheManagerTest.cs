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
                List<Country> activeCountries = new List<Country>();
                foreach(string str in activeLeagues)
                {
                    activeCountries.Add(Session.Instance.Game.kernel.String2Country(str));
                }
                foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
                {
                    if (c.isChampionship && !activeCountries.Contains(Session.Instance.Game.kernel.LocalisationTournament(c)))
                    {
                        c.DisableTournament();
                    }
                }
            }

            Club club = Session.Instance.Game.kernel.Clubs[70];
            Session.Instance.Game.club = club as CityClub;
            Session.Instance.Game.SetBeginDate(Session.Instance.Game.GetBeginDate(club.Country()));
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

    }
}
