using MathNet.Numerics.RootFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm.Comparators;
using tm.Tournaments;

namespace tm.Algorithms
{

    public struct CupStructureResult
    {
        public List<List<RecoverTeams>> structure { get; set; }
        public List<int> teamsByRound { get; set; }
        public int roundsCount { get; set; }
        public HashSet<Tournament> leaguesRepresented { get; set; }
    }

    public class CupStructure
    {
        /// <summary>
        /// Reserves can enter the tournament
        /// </summary>
        public bool allowReserves { get; set; }
        /// <summary>
        /// Teams for child associations can enter the tournament
        /// </summary>
        public bool includeChildAssociations { get; set; }

        /// <summary>
        /// Structure constraints
        /// </summary>
        public List<List<RecoverTeams>> constraints { get; set; }
        /// <summary>
        /// All teams entering the tournament at the first round
        /// </summary>
        public List<RecoverTeams> teams { get; set; }

        public CupStructure(bool allowReserves, bool includeChildAssociations, List<List<RecoverTeams>> constraints, List<RecoverTeams> teams)
        {
            this.allowReserves = allowReserves;
            this.includeChildAssociations = includeChildAssociations;
            this.constraints = constraints;
            this.teams = teams;
        }
    }

    public class CupCreator
    {

        private readonly Kernel _kernel;

        /// <summary>
        /// Kernel is required to generate rounds ids
        /// </summary>
        /// <param name="kernel"></param>
        public CupCreator(Kernel kernel)
        {
            _kernel = kernel;
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
                    if (association == null || association.ContainsAssociation(c.Association()))
                    {
                        total++;
                    }
                }
            }
            return total;
        }

        private bool IsPowerOf2(int x)
        {
            return (x != 0) && (x & (x - 1)) == 0;
        }

        private string NameOfRound(List<int> teamsByRound, int indexRound)
        {
            int teams = teamsByRound[indexRound];
            int nextTeams = (indexRound + 1) < teamsByRound.Count ? teamsByRound[indexRound + 1] : -1;
            bool finalPhase = IsPowerOf2(teams) && (nextTeams == teams / 2);
            string res = finalPhase ? String.Format("Round of {0}", teams) : String.Format("Round {0}", indexRound+1);
            if (indexRound == teamsByRound.Count - 1 && teams == 2)
            {
                res = "Final";
            }
            else if (finalPhase && teams == 4)
            {
                res = "Semifinals";
            }
            else if (finalPhase && teams == 8)
            {
                res = "Quarterfinals";
            }
            return res;
        }

        public string NameOfCup(Association association)
        {
            string tournamentName = association.name;
            string acr = "de ";
            if (new char[] { 'E', 'A', 'I', 'O', 'U' }.Contains(tournamentName[0]))
            {
                acr = "d'";
            }
            string cupName = "Coupe " + acr + tournamentName;
            return cupName;
        }

        public CupStructureResult CreateStructure(Association association, CupStructure constraints)
        {
            Console.WriteLine("[Create Structure] {0}", association.name);
            List<List<RecoverTeams>> structure = new List<List<RecoverTeams>>();
            List<int> teamsByRound = new List<int>();
            Dictionary<int, int> teamsByLevel = new Dictionary<int, int>();
            List<KeyValuePair<Tournament, int>> teamsByTournaments = new List<KeyValuePair<Tournament, int>>();
            int totalTeams = 0;

            List<Tournament> tournamentsIncluded = new List<Tournament>();

            foreach(RecoverTeams source in constraints.teams)
            {
                Tournament tSource = (source.Source as Round).Tournament;
                int tournamentLevel = association.TournamentLevel(tSource);
                if (!teamsByLevel.ContainsKey(tournamentLevel))
                {
                    teamsByLevel.Add(tournamentLevel, 0);
                }
                int teamsCount = source.Available(!constraints.allowReserves, association);
                teamsByLevel[tournamentLevel] += teamsCount;
                teamsByTournaments.Add(new KeyValuePair<Tournament, int>(tSource, teamsCount));
                totalTeams += teamsCount;
                tournamentsIncluded.Add(tSource);
            }

            teamsByTournaments.Sort((x, y) => association.TournamentLevel(x.Key) - association.TournamentLevel(y.Key));

            int roundCount = 0;
            int j = 1;
            while ((j * 2) <= totalTeams)
            {
                j *= 2;
                roundCount++;
            }
            int preliRoundTeams = (totalTeams - j) * 2;
            if (j != totalTeams)
            {
                roundCount++;
            }

            int indexRound = 0;
            //Prelimiary round
            if (j != totalTeams)
            {
                structure.Add(new List<RecoverTeams>());
                teamsByRound.Add(preliRoundTeams);
                int currentAddedTeams = 0;
                while (currentAddedTeams < preliRoundTeams)
                {
                    KeyValuePair<Tournament, int> lowerTournament = teamsByTournaments[teamsByTournaments.Count - 1];
                    int teamsToAdd = (currentAddedTeams + lowerTournament.Value) < preliRoundTeams ? lowerTournament.Value : preliRoundTeams - currentAddedTeams;
                    RecuperationMethod recuperationMethod = (teamsToAdd == lowerTournament.Value ? RecuperationMethod.Best : RecuperationMethod.Worst) | RecuperationMethod.AllTeams;
                    RecoverTeams rt = new RecoverTeams(lowerTournament.Key.rounds[0], teamsToAdd, recuperationMethod);
                    structure[indexRound].Add(rt);
                    currentAddedTeams += teamsToAdd;
                    if (currentAddedTeams == preliRoundTeams && teamsToAdd < lowerTournament.Value)
                    {
                        teamsByTournaments[teamsByTournaments.Count - 1] = new KeyValuePair<Tournament, int>(lowerTournament.Key, lowerTournament.Value - teamsToAdd);
                    }
                    else
                    {
                        teamsByTournaments.RemoveAt(teamsByTournaments.Count - 1);
                    }
                }
                indexRound++;
            }
            while (j != 1)
            {
                //First final round : add not added teams
                structure.Add(new List<RecoverTeams>());
                teamsByRound.Add(j);
                foreach (KeyValuePair<Tournament, int> kvp in teamsByTournaments)
                {
                    RecoverTeams rt = new RecoverTeams(kvp.Key.rounds[0], kvp.Value, RecuperationMethod.Best | RecuperationMethod.AllTeams);
                    structure[indexRound].Add(rt);
                }
                teamsByTournaments.Clear();
                indexRound++;
                j /= 2;
            }
            return new CupStructureResult()
            {
                structure = structure,
                roundsCount = structure.Count,
                teamsByRound = teamsByRound,
                leaguesRepresented = new HashSet<Tournament>(tournamentsIncluded)
            };
        }



        public Tournament CreateEmptyTournament(int cupId, string cupName, int cupLevel, Association association, int roundsCount, List<int> teamsByRound, List<GameDay> availableDates, int winnerPrize, CupStructure structure)
        {
            if (roundsCount > availableDates.Count)
            {
                throw new Exception("Too few dates available");
            }
            Tournament emptyCup = new Tournament(cupId, cupName, "", new GameDay(association.resetWeek, false, 0, 0), cupName, false, cupLevel, 1, 1, new Color(200, 0, 0), ClubStatus.Professional, null, structure);
            for (int i = 0; i < roundsCount; i++)
            {
                Hour hour = new Hour() { Hours = 20, Minutes = 0 };
                int weekIndex = (availableDates.Count / roundsCount) * i;
                string name = NameOfRound(teamsByRound, i);
                GameDay gameDate = new GameDay(availableDates[weekIndex].WeekNumber, true, 0, 0);
                GameDay beginDate = new GameDay((availableDates[weekIndex].WeekNumber - 1) % 52, true, 0, 0);
                GameDay endDate = new GameDay((availableDates[weekIndex].WeekNumber + 2) % 52, false, 0, 0);
                Round round = new KnockoutRound(Session.Instance.Game.kernel.NextIdRound(), name, emptyCup, hour, new List<GameDay> { gameDate }, new List<TvOffset>(), 1, beginDate, endDate, RandomDrawingMethod.Random, false, 2);

                round.rules.Add(Rule.AtHomeIfTwoLevelDifference);
                if (!structure.allowReserves)
                {
                    round.rules.Add(Rule.OnlyFirstTeams);
                }
                if (roundsCount - i > 1)
                {
                    round.qualifications.Add(new Qualification(1, i + 1, new QualificationTournament(emptyCup), false, 1));
                }

                emptyCup.rounds.Add(round);
            }

            int maxPrize = winnerPrize;
            for (int i = roundsCount - 1; i >= 0; i--)
            {
                emptyCup.rounds[i].prizes.Add(new Prize(1, maxPrize));
                maxPrize /= 2;
            }
            emptyCup.InitializeQualificationsNextYearsLists();

            return emptyCup;
        }

    }
}
