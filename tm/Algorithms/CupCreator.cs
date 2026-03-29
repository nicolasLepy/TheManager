using FluentNHibernate.Testing.Values;
using MathNet.Numerics.RootFinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tm.Comparators;
using tm.Tournaments;
using static tm.Tournament;

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

        /// <summary>
        /// If enabled, prevent an extra round from being played. Some teams from the leagues not being marked with the AllTeams flag will not enter the tournament.
        /// </summary>
        public bool noExtraRound { get; set; }

        /// <summary>
        /// Number of teams expected to win the competition
        /// Ex: Regional path of national cups can have more than one winner
        /// </summary>
        public int winners { get; set; }

        /// <summary>
        /// Force the tournament to have this number of rounds
        /// null if noExtraRound is False
        /// must be non null if noExtraRound is True
        /// </summary>
        public int? numberOfRounds { get; set; }

        public CupStructure(bool allowReserves, bool includeChildAssociations, List<List<RecoverTeams>> constraints, List<RecoverTeams> teams, int winners, bool noExtraRound, int? numberOfRounds)
        {
            this.allowReserves = allowReserves;
            this.includeChildAssociations = includeChildAssociations;
            this.constraints = constraints;
            this.teams = teams;
            this.winners = winners;
            this.noExtraRound = noExtraRound;
            this.numberOfRounds = numberOfRounds;
            if (noExtraRound && this.numberOfRounds == null)
            {
                throw new Exception("numberOfRounds must be specified if noExtraRound is not allowed");
            }
            if(!noExtraRound && this.numberOfRounds != null)
            {
                throw new Exception("numberOfRounds is specified but noExtraRound is False");
            }
        }
    }

    public class CupCreator
    {

        private bool debug = false;

        public CupCreator(bool debug = false)
        {
            this.debug = debug;
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


        /// <summary>
        /// Subsample the teams pool to obtain a number of teams that is equal to sampleSize
        /// </summary>
        /// <param name="pool"></param>
        /// <param name="association"></param>
        /// <param name="noExtraRound"></param>
        /// <param name="allowReserves"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private List<RecoverTeams> SamplePool(List<RecoverTeams> pool, int sampleSize, Association association, bool allowReserves)
        {
            pool = new List<RecoverTeams>(pool);

            int maxTeams = 0;
            foreach (RecoverTeams source in pool)
            {
                maxTeams += source.Number; //TODO: Split constrained teams (with AllTeams?) and optional teams
            }

            List<RecoverTeams> sampledPool = new List<RecoverTeams>();

            if (sampleSize != maxTeams)
            {
                int teamsToSample = sampleSize;
                List<RecoverTeams> optionals = new List<RecoverTeams>();
                foreach (RecoverTeams rt in pool)
                {
                    if (!rt.Method.HasFlag(RecuperationMethod.AllTeams))
                    {
                        optionals.Add(rt);
                    }
                    else
                    {
                        teamsToSample -= rt.Number;
                        sampledPool.Add(rt);
                    }
                }
                if (teamsToSample < 0)
                {
                    throw new Exception("Too many undeletable teams but can't create an extra-round");
                }
                //Convert RecoverTeams to LeagueCupApparition
                List<LeagueCupApparition> lca = new List<LeagueCupApparition>();
                optionals.Sort((x, y) => association.TournamentLevel((x.Source as Round).Tournament) - association.TournamentLevel((y.Source as Round).Tournament));
                foreach (RecoverTeams rt in optionals)
                {
                    lca.Add(new LeagueCupApparition(rt.Available(!allowReserves, association), -1, (rt.Source as Round).Tournament));
                }
                //Sampling teams
                lca = UtilsTournaments.SampleTeams(lca, teamsToSample);
                //Convert LeagueCupApparition to RecoverTeams
                for(int i = 0; i < lca.Count; i++)
                {
                    sampledPool.Add(new RecoverTeams(optionals[i].Source, lca[i].teams, optionals[i].Method));
                }
            }
            return sampledPool;
        }

        /// <summary>
        /// Create a simple tournament structure from a list of teams.
        /// </summary>
        /// <param name="pool">List of teams</param>
        /// <param name="expectedWinners">Expected games in the last round</param>
        /// <returns>Bracket structure</returns>
        public CupStructureResult StructureFromPool(List<RecoverTeams> pool, int expectedWinners, Association association)
        {
            pool = new List<RecoverTeams>(pool);
            List<List<RecoverTeams>> structure = new List<List<RecoverTeams>>();
            Dictionary<int, int> teamsByLevel = new Dictionary<int, int>();
            List<KeyValuePair<Tournament, int>> teamsByTournaments = new List<KeyValuePair<Tournament, int>>();
            int maxTeams = 0;
            List<int> teamsByRound = new List<int>();
            foreach (RecoverTeams source in pool)
            {
                Tournament tSource = (source.Source as Round).Tournament;
                int tournamentLevel = association.TournamentLevel(tSource);
                if (!teamsByLevel.ContainsKey(tournamentLevel))
                {
                    teamsByLevel.Add(tournamentLevel, 0);
                }
                int teamsCount = source.Number;
                teamsByLevel[tournamentLevel] += teamsCount;
                teamsByTournaments.Add(new KeyValuePair<Tournament, int>(tSource, teamsCount));
                maxTeams += teamsCount;
            }

            teamsByTournaments.Sort((x, y) => association.TournamentLevel(x.Key) - association.TournamentLevel(y.Key));


            int roundCount = 0;
            int j = expectedWinners;
            while ((j * 2) <= maxTeams)
            {
                j *= 2;
                roundCount++;
            }
            int indexRound = 0;

            int preliRoundTeams = (maxTeams - j) * 2;
            if (j != maxTeams)
            {
                roundCount++;
            }
            //Prelimiary round
            if (j != maxTeams)
            {
                structure.Add(new List<RecoverTeams>());
                int currentAddedTeams = 0;
                teamsByRound.Add(preliRoundTeams);
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

            // Calculating rounds

            while (j != 1)
            {
                //First final round : add not added teams
                structure.Add(new List<RecoverTeams>());
                teamsByRound.Add(j);
                foreach (KeyValuePair<Tournament, int> kvp in teamsByTournaments)
                {
                    RecuperationMethod method = RecuperationMethod.Best;
                    RecoverTeams rt = new RecoverTeams(kvp.Key.rounds[0], kvp.Value, method);
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
                leaguesRepresented = null
            };
        }

        public CupStructureResult CreateStructure(Association association, CupStructure constraints)
        {
            Console.WriteLine("[Create Structure] {0}", association.name);
            List<List<RecoverTeams>> structure = new List<List<RecoverTeams>>();
            List<int> teamsByRound = new List<int>();

            //Precomputing totals

            List<RecoverTeams> allSources = new List<RecoverTeams>(constraints.teams);
            foreach (List<RecoverTeams> rt in constraints.constraints)
            {
                allSources.AddRange(rt);
            }

            // Dict Tournament => each team of the league must be included in the tournament
            Dictionary<Tournament, bool> flagAllTeamsMustBeIncluded = new Dictionary<Tournament, bool>();
            foreach(RecoverTeams source in allSources)
            {
                Tournament sourceT = (source.Source as Round).Tournament;
                bool flag = flagAllTeamsMustBeIncluded.ContainsKey(sourceT) ? flagAllTeamsMustBeIncluded[sourceT] : false;
                flag = flag || source.Method.HasFlag(RecuperationMethod.AllTeams);
                flagAllTeamsMustBeIncluded[sourceT] = flag;
            }
            /*// Dict Tournament => teams from this league enters at differents stage of the tournament
            Dictionary<Tournament, bool> flagMultipleSources = new Dictionary<Tournament, bool>();
            foreach(RecoverTeams source in ...)
            {
                Tournament sourceT = (source.Source as Round).Tournament;
                bool flag = flagMultipleSources.ContainsKey(sourceT) ? true : false;
                flagMultipleSources[sourceT] = flag;
            }
            Dictionary<Tournament, bool> flagBestWorst = new Dictionary<Tournament, bool>();*/

            //TODO: Not sure this will give accurates results (probably more teams than existing)
            Dictionary<int, int> teamsByLevel = new Dictionary<int, int>();
            List<KeyValuePair<Tournament, int>> teamsByTournaments = new List<KeyValuePair<Tournament, int>>();
            int maxTeams = 0;
            List<Tournament> tournamentsIncluded = new List<Tournament>();
            foreach(RecoverTeams source in allSources)
            {
                int teamsAvailable = source.Available(!constraints.allowReserves, association);
                Tournament tSource = (source.Source as Round).Tournament;
                int tournamentLevel = association.TournamentLevel(tSource);
                if (!teamsByLevel.ContainsKey(tournamentLevel))
                {
                    teamsByLevel.Add(tournamentLevel, 0);
                }
                teamsByLevel[tournamentLevel] += teamsAvailable;
                teamsByTournaments.Add(new KeyValuePair<Tournament, int>(tSource, teamsAvailable));
                maxTeams += teamsAvailable;
                tournamentsIncluded.Add(tSource);
            }
            teamsByTournaments.Sort((x, y) => association.TournamentLevel(x.Key) - association.TournamentLevel(y.Key));
            Dictionary<Tournament, int> flagRemainingTeams = new Dictionary<Tournament, int>();
            foreach(KeyValuePair<Tournament, int> teamsKvp in teamsByTournaments)
            {
                if (flagAllTeamsMustBeIncluded[teamsKvp.Key])
                {
                    flagRemainingTeams[teamsKvp.Key] = teamsKvp.Value;
                }
            }

            int n = constraints.winners;

            //Calculating rounds
            List <List<RecoverTeams>> roundsConstraints = new List<List<RecoverTeams>>(constraints.constraints);
            roundsConstraints.Reverse();

            int lastRoundWithConstraint = -1;
            for(int i = 0; i < roundsConstraints.Count; i++)
            {
                lastRoundWithConstraint = roundsConstraints[i].Count > 0 ? i : lastRoundWithConstraint;
            }

            for (int i = 0; i < lastRoundWithConstraint; i++)
            {
                structure.Add(new List<RecoverTeams>());
                int newTeams = 0;
                foreach(RecoverTeams sourceRec in roundsConstraints[i])
                {
                    Tournament tSource = (sourceRec.Source as Round).Tournament;
                    int number = sourceRec.Available(!constraints.allowReserves, association);
                    RecoverTeams rtn = new RecoverTeams(sourceRec.Source, number, sourceRec.Method);
                    structure[i].Add(rtn);
                    newTeams += number;
                    if (flagRemainingTeams.ContainsKey(tSource))
                    {
                        flagRemainingTeams[tSource] -= number;
                    }
                    //teamsByLevel[(sourceRec.Source as Round).Tournament.level] -= number;
                }
                n = n - newTeams;
                n = n * 2;
            }

            CupStructureResult baseStructure = new CupStructureResult()
            {
                structure = structure,
                roundsCount = structure.Count,
                teamsByRound = teamsByRound,
                leaguesRepresented = new HashSet<Tournament>(tournamentsIncluded)
            };

            //Preprocess pool
            List<RecoverTeams> pool = new List<RecoverTeams>();
            foreach(RecoverTeams rt in constraints.teams)
            {
                Tournament t = (rt.Source as Round).Tournament;
                int teamsAvailable = flagRemainingTeams.ContainsKey(t) ? flagRemainingTeams[t] : rt.Number; 
                pool.Add(new RecoverTeams(rt.Source, rt.Number, rt.Method));
            }

            List<RecoverTeams> sampledPool;
            if (constraints.noExtraRound)
            {
                int remainingRounds = constraints.numberOfRounds.Value - structure.Count;
                int teamsToSample = (int)(n * (Math.Pow(2, remainingRounds)));
                sampledPool = SamplePool(constraints.teams, teamsToSample, association, constraints.allowReserves);
            }
            else
            {
                sampledPool = constraints.teams;
            }
            CupStructureResult headerStructure = StructureFromPool(sampledPool, n, association);
            return Merge(baseStructure, headerStructure);
        }

        /// <summary>
        /// Add headerStructure (first rounds) to baseStructure
        /// </summary>
        /// <param name="baseStructure"></param>
        /// <param name="header"></param>
        /// <returns></returns>
        private CupStructureResult Merge(CupStructureResult baseStructure, CupStructureResult headerStructure)
        {
            List<List<RecoverTeams>> structure = new List<List<RecoverTeams>>(headerStructure.structure);
            structure.AddRange(baseStructure.structure);
            List<int> teamsByRound = new List<int>(headerStructure.teamsByRound);
            teamsByRound.AddRange(baseStructure.teamsByRound);

            return new CupStructureResult()
            {
                structure = structure,
                roundsCount = structure.Count,
                teamsByRound = teamsByRound,
                leaguesRepresented = baseStructure.leaguesRepresented
            };
        }

        //TODO: Probably need to replace this by a CreateCupStructure function after (create empty tournament from CupResultStructure) ?
        //Because CupCreator is not responsible to create the Tournament structure, rounds ids, names...
        public Tournament CreateEmptyTournament(int cupId, string cupName, int cupLevel, Association association, int roundsCount, List<int> teamsByRound, List<GameDay> availableDates, int winnerPrize, CupStructure structure)
        {
            if (roundsCount > availableDates.Count)
            {
                throw new Exception("Too few dates available");
            }
            Tournament emptyCup = new Tournament(cupId, cupName, "", association, new GameDay(association.resetWeek, false, 0, 0), cupName, false, cupLevel, 1, 1, new Color(200, 0, 0), ClubStatus.Professional, null, structure);
            for (int i = 0; i < roundsCount; i++)
            {
                Hour hour = new Hour() { Hours = 20, Minutes = 0 };
                int weekIndex = (availableDates.Count / roundsCount) * i;
                string name = NameOfRound(teamsByRound, i);
                GameDay gameDate = new GameDay(availableDates[weekIndex].WeekNumber, true, 0, 0);
                GameDay beginDate = new GameDay((availableDates[weekIndex].WeekNumber - 1) % 52, true, 0, 0);
                GameDay endDate = new GameDay((availableDates[weekIndex].WeekNumber + 2) % 52, false, 0, 0);
                Round round = new KnockoutRound(-1, name, emptyCup, hour, new List<GameDay> { gameDate }, new List<TvOffset>(), 1, beginDate, endDate, RandomDrawingMethod.Random, false, 2);

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
