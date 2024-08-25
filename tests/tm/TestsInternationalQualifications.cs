using Microsoft.VisualStudio.TestTools.UnitTesting;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using tm;
using tm.Tournaments;

namespace tests.tm
{
    [TestClass]
    public class TestsInternationalQualifications : TheManagerTest
    {

        [TestMethod]
        public void TestInternationalQualifications()
        {
            InitGame("database_france_light", new List<string>());
            bool keepGoin = true;
            Continent europe = Session.Instance.Game.kernel.String2Continent("Europe");
            Country france = Session.Instance.Game.kernel.String2Country("France");
            Association uefa = (from a in Session.Instance.Game.kernel.worldAssociation.associations where a.localisation == europe select a).First();
            Tournament championsLeague = uefa.GetContinentalClubTournament(1);
            Tournament europaLeague = uefa.GetContinentalClubTournament(2);
            Tournament europaConferenceLeague = uefa.GetContinentalClubTournament(3);

            //SWITCH FROM ASSOCIATION TO CONTINENT HOLDING :
            // => uefa.getcont... -> europe.getcont
            // => from q in uefa.continentalQualifications -> from q in europe.continentalQualifications
            // => datasetloader.cs => if(false && localisation as Continent != null)
            // => game.cs => foreach(Association a in kernel.GetAllAssociations())

            while (keepGoin)
            {
                Session.Instance.Game.NextDay();
                Session.Instance.Game.UpdateTournaments();

                foreach (Country country in europe.countries)
                {
                    foreach(Tournament cup in country.Cups())
                    {
                        if (Utils.CompareDates(Session.Instance.Game.date, cup.rounds.Last().DateEndRound()))
                        {
                            ForceCupWinner(country, cup);
                            Console.WriteLine("[" + Session.Instance.Game.date.ToShortDateString() + "][force] " + cup.name);
                        }
                    }
                    if (country.League(1) != null && Utils.CompareDates(Session.Instance.Game.date, country.League(1).rounds.Last().DateEndRound()))
                    {
                        ForceLeague(country, country.League(1));
                        Console.WriteLine("[" + Session.Instance.Game.date.ToShortDateString() + "][force] " + country.League(1).name);
                    }
                }
                int weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(Session.Instance.Game.date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                if (Utils.CompareDates(championsLeague.rounds.Last().DateEndRound().AddDays(-1), Session.Instance.Game.date))
                {
                    Round clFinal = championsLeague.rounds[championsLeague.rounds.Count - 1];
                    Club clubWinner = france.League(1).rounds[0].clubs[10];
                    clFinal.clubs[0] = clubWinner;
                    clFinal.matches[0].home = clubWinner;
                    clFinal.matches[0].Force(1, 0);
                }
                if (Utils.CompareDates(europaLeague.rounds.Last().DateEndRound().AddDays(-1), Session.Instance.Game.date))
                {
                    Round elFinal = europaLeague.rounds[europaLeague.rounds.Count - 1];
                    Club clubWinner = france.League(1).rounds[0].clubs[11];
                    elFinal.clubs[0] = clubWinner;
                    elFinal.matches[0].home = clubWinner;
                    elFinal.matches[0].Force(1, 0);
                }
                if (Utils.CompareDates(europaConferenceLeague.rounds.Last().DateEndRound().AddDays(-1), Session.Instance.Game.date))
                {
                    Round elcFinal = europaConferenceLeague.rounds[europaConferenceLeague.rounds.Count - 1];
                    Club clubWinner = france.League(1).rounds[0].clubs[12];
                    elcFinal.clubs[0] = clubWinner;
                    elcFinal.matches[0].home = clubWinner;
                    elcFinal.matches[0].Force(1, 0);
                }
                if (uefa.resetWeek == weekNumber && Session.Instance.Game.date.DayOfWeek == DayOfWeek.Wednesday)
                {
                    keepGoin = false;
                }
            }
            int rank = 1;
            List<Country> associations = (from a in uefa.associationRanking select a.localisation as Country).ToList();
            int checksCount = 0;
            foreach (Country association in associations)
            {
                List<Qualification> associationQualifications = (from q in uefa.continentalQualifications where q.ranking == rank select q).ToList();
                Console.WriteLine(associationQualifications.Count);
                List<Club> cupWinners = (from c in association.Cups() select c.Winner()).ToList();
                List<Club> bestTeams = association.League(1).rounds[0].clubs.GetRange(0, 8);
                foreach(Qualification q in associationQualifications)
                {
                    for(int i = 0; i < q.qualifies; i++)
                    {
                        if(q.isNextYear && cupWinners.Count > 0)
                        {
                            Club club = cupWinners[0];
                            Assert.IsTrue(q.tournament.nextYearQualified[q.roundId].Contains(club));
                            cupWinners.RemoveAt(0);
                            checksCount++;
                        }
                        else
                        {
                            Club club = bestTeams[0];
                            Assert.IsTrue(q.tournament.nextYearQualified[q.roundId].Contains(club));
                            bestTeams.RemoveAt(0);
                            checksCount++;
                        }
                    }
                }
                rank++;
            }
            Assert.AreEqual(checksCount, 124);
        }

        private void ForceLeague(Country country, Tournament league)
        {
            if(league != null)
            {
                Round firstRound = league.rounds[0];
                GroupInactiveRound lastChampionshipRound = league.GetLastChampionshipRound() as GroupInactiveRound;
                List<Club> topClubs = firstRound.clubs.GetRange(0, 8);
                topClubs.Reverse();
                List<Club> allClubs = new List<Club>(firstRound.clubs);
                foreach(Club club in topClubs)
                {
                    allClubs.Remove(club);
                    allClubs.Insert(0, club);
                }
                lastChampionshipRound.ForceRanking(allClubs);
            }
        }

        private void ForceCupWinner(Country country, Tournament cup)
        {
            Tournament firstLeague = country.League(1);
            if(firstLeague != null)
            {
                Round lastRound = cup.rounds[cup.rounds.Count - 1];
                Club clubWinner = firstLeague.Clubs()[firstLeague.Clubs().Count - cup.level];
                lastRound.clubs[0] = clubWinner;
                lastRound.matches[0].home = clubWinner;
                lastRound.matches[0].Force(1, 0);
            }
        }

    }
}
