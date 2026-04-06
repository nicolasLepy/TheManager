using FluentNHibernate;
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

        private int FindEquivalentSource(List<RecoverTeams> pool, RecoverTeams item)
        {
            RetrieveFlags[] specialFlags = new RetrieveFlags[] { RetrieveFlags.QualifiedForInternationalCompetition, RetrieveFlags.QualifiedForInternationalCompetition, RetrieveFlags.StatusPro };
            int idx = -1;
            for(int i = 0; i < pool.Count; i++)
            {
                RecoverTeams rt = pool[i];
                bool match = false;
                if (rt.Source == item.Source)
                {
                    match = true;
                    foreach(RetrieveFlags flag in specialFlags)
                    {
                        match = match && rt.Flags.HasFlag(flag) == item.Flags.HasFlag(flag);
                    }
                }
                if (match)
                {
                    idx = i;
                }
            }
            return idx;
        }

        private void RemoveTeamsFromPool(List<RecoverTeams> pool, RecoverTeams source, int number)
        {
            int match = FindEquivalentSource(pool, source);
            if(match == -1)
            {
                throw new Exception(String.Format("{0} not found in the pool", source.ToString()));
            }
            if (pool[match].Number - number < 0)
            {
                throw new Exception(String.Format("Can't remove {0} teams of {1} from the pool (only {2} teams remaining)", number, source.ToString(), pool[match].Number));
            }
            pool[match] = new RecoverTeams(pool[match].Source, pool[match].Number - number, pool[match].Flags);
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
            if(sampleSize < 1)
            {
                throw new Exception("sampleSize must be at least equal to 1");
            }
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
                    if(rt.Number > 0)
                    {
                        if (!rt.Flags.HasFlag(RetrieveFlags.AllTeams))
                        {
                            optionals.Add(rt);
                        }
                        else
                        {
                            teamsToSample -= rt.Number;
                            sampledPool.Add(rt);
                        }
                    }
                }
                if (teamsToSample < 0)
                {
                    throw new Exception("Too many undeletable teams but can't create an extra-round");
                }
                //Convert RecoverTeams to LeagueCupApparition
                List<LeagueCupApparition> lca = new List<LeagueCupApparition>();
                optionals.Sort((x, y) => CompareRecoverTeams(x, y, association));
                foreach (RecoverTeams rt in optionals)
                {
                    lca.Add(new LeagueCupApparition(rt.Flags.HasFlag(RetrieveFlags.AllTeams) ? rt.Available(!allowReserves, association) : rt.Number, -1, (rt.Source as Round).Tournament));
                }
                //Sampling teams
                lca = UtilsTournaments.SampleTeams(lca, teamsToSample);
                //Convert LeagueCupApparition to RecoverTeams
                for(int i = 0; i < lca.Count; i++)
                {
                    sampledPool.Add(new RecoverTeams(optionals[i].Source, lca[i].teams, optionals[i].Flags));
                }
            }
            return sampledPool;
        }

        public RetrieveFlags FlagsOfSource(RecoverTeams source)
        {
            RetrieveFlags ret = new RetrieveFlags();
            if (source.Flags.HasFlag(RetrieveFlags.StatusPro))
            {
                ret |= RetrieveFlags.StatusPro;
            }
            if (source.Flags.HasFlag(RetrieveFlags.QualifiedForInternationalCompetition))
            {
                ret |= RetrieveFlags.QualifiedForInternationalCompetition;
            }
            if (source.Flags.HasFlag(RetrieveFlags.NotQualifiedForInternationalCompetition))
            {
                ret |= RetrieveFlags.NotQualifiedForInternationalCompetition;
            }
            return ret;
        }


        private int CompareRecoverTeams(RecoverTeams x, RecoverTeams y, Association association)
        {
            int res = 0;
            if (x.Source.IsDummy() && !y.Source.IsDummy())
            {
                res = -1;
            }
            else if (!x.Source.IsDummy() && y.Source.IsDummy())
            {
                res = 1;
            }
            else if (x.Source.IsDummy() && y.Source.IsDummy())
            {
                res = 0;
            }
            else
            {
                int levelX = association.TournamentLevel((x.Source as Round).Tournament);
                int levelY = association.TournamentLevel((y.Source as Round).Tournament);
                res = levelX - levelY;
                if (res == 0)
                {
                    if (x.Flags.HasFlag(RetrieveFlags.QualifiedForInternationalCompetition) && !y.Flags.HasFlag(RetrieveFlags.QualifiedForInternationalCompetition))
                    {
                        res = -1;
                    }
                    if (y.Flags.HasFlag(RetrieveFlags.QualifiedForInternationalCompetition) && !x.Flags.HasFlag(RetrieveFlags.QualifiedForInternationalCompetition))
                    {
                        res = 1;
                    }
                }
            }

            return res;
        }

        /// <summary>
        /// Create a simple tournament structure from a list of teams.
        /// </summary>
        /// <param name="pool">List of teams</param>
        /// <param name="expectedWinners">Expected games in the last round</param>
        /// <returns>Bracket structure</returns>
        public CupStructureResult StructureFromPool(List<RecoverTeams> basePool, int expectedWinners, Association association)
        {
            if (expectedWinners < 1)
            {
                throw new Exception("expectedWinners must be at least equal to 1.");
            }

            List<RecoverTeams> pool = new List<RecoverTeams>();
            foreach(RecoverTeams bprt in basePool)
            {
                if(bprt.Number > 0)
                {
                    pool.Add(bprt);
                }
            }
            List<List<RecoverTeams>> structure = new List<List<RecoverTeams>>();
            List<KeyValuePair<RecoverTeams, int>> teamsBySources = new List<KeyValuePair<RecoverTeams, int>>();
            int maxTeams = 0;
            List<int> teamsByRound = new List<int>();
            foreach (RecoverTeams source in pool)
            {
                int teamsCount = source.Number;
                maxTeams += teamsCount;
                teamsBySources.Add(new KeyValuePair<RecoverTeams, int>(source, teamsCount));
            }

            if(maxTeams < expectedWinners * 2)
            {
                throw new Exception("Too few teams available to create the cup structure");
            }

            teamsBySources.Sort((x, y) => CompareRecoverTeams(x.Key, y.Key, association));

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
                    KeyValuePair<RecoverTeams, int> lowerTournament = teamsBySources[teamsBySources.Count - 1];
                    int teamsToAdd = (currentAddedTeams + lowerTournament.Value) < preliRoundTeams ? lowerTournament.Value : preliRoundTeams - currentAddedTeams;
                    RetrieveFlags flags = (teamsToAdd == lowerTournament.Value ? RetrieveFlags.Best : RetrieveFlags.Worst) | RetrieveFlags.AllTeams;
                    flags |= FlagsOfSource(lowerTournament.Key);
                    if (!lowerTournament.Key.Source.IsDummy())
                    {
                        RecoverTeams rt = new RecoverTeams(lowerTournament.Key.Source, teamsToAdd, flags);
                        structure[indexRound].Add(rt);
                    }
                    currentAddedTeams += teamsToAdd;
                    if (currentAddedTeams == preliRoundTeams && teamsToAdd < lowerTournament.Value)
                    {
                        teamsBySources[teamsBySources.Count - 1] = new KeyValuePair<RecoverTeams, int>(lowerTournament.Key, lowerTournament.Value - teamsToAdd);
                    }
                    else
                    {
                        teamsBySources.RemoveAt(teamsBySources.Count - 1);
                    }
                }
                indexRound++;
            }


            // Calculating rounds

            while (j != expectedWinners)
            {
                //First final round : add not added teams
                structure.Add(new List<RecoverTeams>());
                teamsByRound.Add(j);
                foreach (KeyValuePair<RecoverTeams, int> kvp in teamsBySources)
                {
                    if (!kvp.Key.Source.IsDummy())
                    {
                        RetrieveFlags method = RetrieveFlags.Best | FlagsOfSource(kvp.Key);
                        RecoverTeams rt = new RecoverTeams(kvp.Key.Source, kvp.Value, method);
                        structure[indexRound].Add(rt);
                    }
                }
                teamsBySources.Clear();
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

        private int CountTeams(List<RecoverTeams> pool)
        {
            int res = 0;
            foreach(RecoverTeams rt in pool)
            {
                res += rt.Number;
            }
            return res;
        }

        private List<List<RecoverTeams>> Copy(List<List<RecoverTeams>> list)
        {
            List<List<RecoverTeams>> copy = new List<List<RecoverTeams>>();
            foreach (List<RecoverTeams> rt in list)
            {
                copy.Add(new List<RecoverTeams>(rt));
            }
            return copy;
        }

        /// <summary>
        /// Go backward to a tournament structure and return false if there aren't enough teams available. Otherwise, return true
        /// </summary>
        /// <returns></returns>
        private bool Backward(int maxTeamsAvailable, int n, List<List<RecoverTeams>> constraints, int lastRoundWithConstraint, int numberOfRounds, bool allowReserves, Association association)
        {
            constraints = Copy(constraints);
            List<List<RecoverTeams>> structure = new List<List<RecoverTeams>>();

            for (int i = 0; i <= lastRoundWithConstraint; i++)
            {
                structure.Add(new List<RecoverTeams>());
                int newTeams = 0;
                List<RecoverTeams> structureRound = new List<RecoverTeams>();
                for (int j = 0; j < constraints[i].Count; j++)
                {
                    RecoverTeams sourceRec = constraints[i][j];
                    int number = sourceRec.Flags.HasFlag(RetrieveFlags.AllTeams) ? sourceRec.Available(!allowReserves, association) : sourceRec.Number;
                    newTeams += number;
                    if (!sourceRec.Source.IsDummy())
                    {
                        RecoverTeams rtn = new RecoverTeams(sourceRec.Source, number, sourceRec.Flags);
                        structureRound.Add(rtn);
                    }
                }
                //These constraints are impossible (too many teams). They are pushed to the previous round (or to the pool)
                if (((n * 2) - newTeams) < 1)
                {
                    newTeams = 0;
                    if (i < lastRoundWithConstraint)
                    {
                        constraints[i + 1].AddRange(constraints[i]);
                    }
                }
                else //Constraints ok: they are incorporated into the structure
                {
                    structure[i] = structureRound;
                    foreach (RecoverTeams sourceRec in structureRound)
                    {
                        maxTeamsAvailable -= sourceRec.Number;
                    }
                }
                n = (n * 2) - newTeams;
            }

            int remainingRounds = numberOfRounds - structure.Count;
            int teamsToSample = (int)(n * (Math.Pow(2, remainingRounds)));

            return teamsToSample <= maxTeamsAvailable;

        }

        private CupStructure Copy(CupStructure structure)
        {
            List<List<RecoverTeams>> constraints = Copy(structure.constraints);
            List<RecoverTeams> teams = new List<RecoverTeams>(structure.teams);
            return new CupStructure(structure.allowReserves, structure.includeChildAssociations, constraints, teams, structure.winners, structure.noExtraRound, structure.numberOfRounds);
        }

        private int GetLastRoundWithConstraint(List<List<RecoverTeams>> constraints)
        {
            int lastRoundWithConstraint = -1;
            for (int i = 0; i < constraints.Count; i++)
            {
                lastRoundWithConstraint = constraints[i].Count > 0 ? i : lastRoundWithConstraint;
            }
            return lastRoundWithConstraint;

        }

        public CupStructureResult CreateStructure(Association association, CupStructure constraints)
        {
            constraints = Copy(constraints);
            List<List<RecoverTeams>> structure = new List<List<RecoverTeams>>();
            List<int> teamsByRound = new List<int>();

            //Precomputing totals

            List<RecoverTeams> pool = new List<RecoverTeams>(constraints.teams);
            for (int i = 0; i < pool.Count; i++)
            {
                RecoverTeams source = pool[i];
                int teamsAvailable = source.Available(!constraints.allowReserves, association);
                if (source.Flags.HasFlag(RetrieveFlags.AllTeams))
                {
                    pool[i] = new RecoverTeams(source.Source, teamsAvailable, source.Flags);
                }
            }

            int maxTeams = 0;
            List<Tournament> tournamentsIncluded = new List<Tournament>();
            for(int i = 0; i < constraints.teams.Count; i++)
            {
                RecoverTeams source = constraints.teams[i];
                Round roundSource = source.Source as Round;
                int teamsAvailable = source.Flags.HasFlag(RetrieveFlags.AllTeams) ? source.Available(!constraints.allowReserves, association) : source.Number;
                if(roundSource != null)
                {
                    Tournament tSource = roundSource.Tournament;
                    tournamentsIncluded.Add(tSource);
                }
                maxTeams += teamsAvailable;
            }

            int n = constraints.winners;

            //Calculating rounds
            List<List<RecoverTeams>> roundsConstraints = new List<List<RecoverTeams>>(constraints.constraints);
            roundsConstraints.Reverse();

            int lastRoundWithConstraint = GetLastRoundWithConstraint(roundsConstraints);

            if (constraints.noExtraRound)
            {
                while (!Backward(maxTeams, n, roundsConstraints, lastRoundWithConstraint, constraints.numberOfRounds.Value, constraints.allowReserves, association))
                {
                    if(constraints.numberOfRounds == 1)
                    {
                        throw new Exception("Unable to remove another round");
                    }
                    //TODO: To avoid removing a round, why not push some teams from pool as a constraint to the first round with constraints ?

                    //Remove a round
                    constraints.numberOfRounds -= 1;
                    //Remove the first round (roundsConstraints is reversed)
                    roundsConstraints.RemoveAt(roundsConstraints.Count - 1);
                    //Deleted roundsConstraints (at place 0) will remain in the pool. But these teams could be dropped out if we must sample teams. Mark them to be kept ?
                    roundsConstraints[roundsConstraints.Count-1].Clear();
                    lastRoundWithConstraint = GetLastRoundWithConstraint(roundsConstraints);
                }
            }

            //Backward phase. Same implementation as this.Backward() but commit to the structure. Possible factorize.
            for (int i = 0; i <= lastRoundWithConstraint; i++)
            {
                structure.Add(new List<RecoverTeams>());
                int newTeams = 0;
                List<RecoverTeams> structureRound = new List<RecoverTeams>();
                for(int j = 0; j < roundsConstraints[i].Count; j++)
                {
                    RecoverTeams sourceRec = roundsConstraints[i][j];
                    int number = sourceRec.Flags.HasFlag(RetrieveFlags.AllTeams) ? sourceRec.Available(!constraints.allowReserves, association) : sourceRec.Number;
                    if (!sourceRec.Source.IsDummy())
                    {
                        RecoverTeams rtn = new RecoverTeams(sourceRec.Source, number, sourceRec.Flags);
                        structureRound.Add(rtn);
                    }
                    newTeams += number;
                }
                //These constraints are impossible (too many teams). They are pushed to the previous round (or to the pool)
                if (((n * 2) - newTeams) < 1)
                {
                    newTeams = 0;
                    if(i < lastRoundWithConstraint)
                    {
                        roundsConstraints[i + 1].AddRange(roundsConstraints[i]);
                    }
                }
                else //Constraints ok: they are incorporated into the structure
                {
                    structure[i] = structureRound;
                    foreach(RecoverTeams sourceRec in structureRound)
                    {
                        RemoveTeamsFromPool(pool, sourceRec, sourceRec.Number);
                        maxTeams -= sourceRec.Number;
                    }
                }
                n = (n * 2) - newTeams;
                teamsByRound.Add(n);
            }

            structure.Reverse();
            teamsByRound.Reverse();
            CupStructureResult baseStructure = new CupStructureResult()
            {
                structure = structure,
                roundsCount = structure.Count,
                teamsByRound = teamsByRound,
                leaguesRepresented = new HashSet<Tournament>(tournamentsIncluded)
            };

            List<RecoverTeams> sampledPool;
            if (constraints.noExtraRound)
            {
                int remainingRounds = constraints.numberOfRounds.Value - structure.Count;
                int teamsToSample = (int)(n * (Math.Pow(2, remainingRounds)));
                //TODO: Sometimes, some teams must be sampled. Mark them (how? /remove them before calling SamplePool?)
                sampledPool = SamplePool(pool, teamsToSample, association, constraints.allowReserves);
            }
            else
            {
                sampledPool = pool;
            }
            CupStructureResult headerStructure = StructureFromPool(sampledPool, n, association);
            return Merge(baseStructure, headerStructure);
        }

        private int CountDummyTeams(List<RecoverTeams> pool)
        {
            int res = 0;
            foreach(RecoverTeams rt in pool)
            {
                if (rt.Source.IsDummy())
                {
                    res += rt.Available(false, null);
                }
            }
            return res;
        }

        private List<RecoverTeams> ExtractDummyTeams(List<RecoverTeams> pool)
        {
            List<RecoverTeams> result = new List<RecoverTeams>();
            foreach(RecoverTeams rt in pool)
            {
                if (rt.Source.IsDummy())
                {
                    result.Add(rt);
                }
            }
            return result;
        }

        private List<RecoverTeams> RemoveDummyTeams(List<RecoverTeams> pool)
        {
            List<RecoverTeams> res = new List<RecoverTeams>();
            foreach(RecoverTeams rt in pool)
            {
                if (!rt.Source.IsDummy())
                {
                    res.Add(rt);
                }
            }
            return res;
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
            int teamsCount = headerStructure.roundsCount == 0 ? 0 : (headerStructure.teamsByRound.Last() / 2);
            for(int i = 0; i < baseStructure.roundsCount; i++)
            {
                foreach(RecoverTeams rt in baseStructure.structure[i])
                {
                    teamsCount += rt.Number;
                }
                teamsByRound.Add(teamsCount);
                teamsCount /= 2;
            }

            return new CupStructureResult()
            {
                structure = structure,
                roundsCount = structure.Count,
                teamsByRound = teamsByRound,
                leaguesRepresented = baseStructure.leaguesRepresented
            };
        }
    }
}
