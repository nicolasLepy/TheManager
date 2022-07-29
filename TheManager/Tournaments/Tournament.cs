using MathNet.Numerics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using System.Text;
using TheManager.Exportation;
using System.Windows.Media;
using TheManager.Tournaments;
using TheManager.Comparators;

namespace TheManager
{

    [DataContract]
    public struct TournamentStatistics : IEquatable<TournamentStatistics>
    {
        [DataMember]
        public Match BiggerScore { get; set; }
        [DataMember]
        public Match LargerScore { get; set; }
        [DataMember]
        public KeyValuePair<int, Player> TopGoalscorerOnOneSeason { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> BiggestAttack { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> WeakestAttack { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> BiggestDefense { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> WeakestDefense { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> MostPoints { get; set; }
        [DataMember]
        public KeyValuePair<int, Club> LowestPoints { get; set; }



        public TournamentStatistics(int i)
        {
            BiggerScore = null;
            LargerScore = null;
            TopGoalscorerOnOneSeason = new KeyValuePair<int, Player>(0, null);
            BiggestAttack = new KeyValuePair<int, Club>(0, null);
            WeakestAttack = new KeyValuePair<int, Club>(0, null);
            BiggestDefense = new KeyValuePair<int, Club>(0, null);
            WeakestDefense = new KeyValuePair<int, Club>(0, null);
            LowestPoints = new KeyValuePair<int, Club>(0, null);
            MostPoints = new KeyValuePair<int, Club>(0, null);
        }

        public bool Equals(TournamentStatistics other)
        {
            throw new NotImplementedException();
        }
    }

    [DataContract(IsReference = true)]
    public class Tournament
    {

        [DataMember]
        private string _name;
        [DataMember]
        private List<Round> _rounds;
        [DataMember]
        private string _logo;
        [DataMember]
        private GameDay _seasonBeginning;
        [DataMember]
        private string _shortName;
        [DataMember]
        private List<Club>[] _nextYearQualified;
        [DataMember]
        private bool _isChampionship;
        [DataMember]
        private int _level;
        [DataMember]
        private TournamentStatistics _statistics;
        [DataMember]
        private Dictionary<int, Tournament> _previousEditions;
        [DataMember]
        private int _periodicity;
        [DataMember]
        private int _remainingYears;
        [DataMember]
        private Color _color;
        [DataMember]
        private List<Stadium> _hostStadiums;


        public string name { get => _name; }
        public Color color => _color;
        public List<Round> rounds { get => _rounds; }
        public string logo { get => _logo; }
        [DataMember]
        public int currentRound { get; set; }

        public List<Stadium> hostStadiums => _hostStadiums;
        public bool isHostedByOneCountry
        {
            get
            {
                bool isHostedByOneCountry = false;
                foreach (Round r in _rounds)
                {
                    if (r.rules.Contains(Rule.HostedByOneCountry))
                    {
                        isHostedByOneCountry = true;
                    }
                }
                return isHostedByOneCountry;
            }
        }

        public GameDay seasonBeginning => _seasonBeginning;
        public string shortName => _shortName;
        public Dictionary<int, Tournament> previousEditions => _previousEditions;
        public TournamentStatistics statistics { get => _statistics; set => _statistics = value; }

        public int remainingYears => _remainingYears;
        public int periodicity => _periodicity;

        /// <summary>
        /// Is a championship (L1, L2)
        /// Fixed : if it's a championship, the main round is the round at index 0
        /// </summary>
        public bool isChampionship => _isChampionship;

        /// <summary>
        /// Level in the hierarchy (L1 = 1, L2 = 2 ...)
        /// </summary>
        public int level => _level;

        public Tournament(string name, string logo, GameDay seasonBeginning, string shortName, bool isChampionship, int level, int periodicity, int remainingYears, Color color)
        {
            _rounds = new List<Round>();
            _name = name;
            _logo = logo;
            _seasonBeginning = seasonBeginning;
            currentRound = -1;
            _shortName = shortName;
            _isChampionship = isChampionship;
            _level = level;
            _statistics = new TournamentStatistics(0);
            _previousEditions = new Dictionary<int, Tournament>();
            _periodicity = periodicity;
            _remainingYears = remainingYears;
            _color = color;
            _hostStadiums = new List<Stadium>();
        }

        public void InitializeQualificationsNextYearsLists(int count = -1)
        {
            if (count == -1)
            {
                count = rounds.Count;
            }
            _nextYearQualified = new List<Club>[count];
            for (int i = 0; i < count; i++)
            {
                _nextYearQualified[i] = new List<Club>();
            }
        }

        /// <summary>
        /// Initialize retained country and stadiums for the tournament
        /// </summary>
        public void InitializeHost()
        {
            List<Country> candidates = new List<Country>();
            //Find country
            foreach (Round r in _rounds)
            {
                foreach (Club c in r.clubs)
                {
                    Country candidate = c.Country();
                    if (candidate.stadiums.Count > 7)
                    {
                        candidates.Add(c.Country());
                    }
                }
            }

            Country host = candidates.Count > 0 ? candidates[Session.Instance.Random(0, candidates.Count)] : null;
            //Find stadiums
            if (host != null)
            {
                Console.WriteLine(host.Name() + " chosen to host " + _name);
                List<Stadium> stadiums = new List<Stadium>(host.stadiums);
                stadiums.Sort(new StadiumComparator());
                for (int i = 0; i < 8; i++)
                {
                    _hostStadiums.Add(stadiums[i]);
                    Console.WriteLine(stadiums[i].name + " - " + stadiums[i].capacity);
                }
                Console.WriteLine("=================================");
            }

        }

        private int CountAdditionnalTeamsRound(int round, List<LeagueCupApparition> apparitions)
        {
            int res = 0;
            foreach(LeagueCupApparition lca in apparitions)
            {
                if(lca.apparitionRound == round)
                {
                    res += lca.teams;
                }
            }
            return res;
        }

        public RecoverTeams GetOtherRecoverTeamsOfRound(RecoverTeams rt)
        {
            RecoverTeams res = new RecoverTeams(null, 0, RecuperationMethod.Best);
            foreach (Round round in rounds)
            {
                foreach (RecoverTeams recover in round.recuperedTeams)
                {
                    if (recover.Method != rt.Method && recover.Source == rt.Source)
                    {
                        res = recover;
                    }
                }
            }
            return res;
        }


        private class LeagueCupApparition
        {

            public bool isBestTeams { get => _teams < 0; }

            private int _teams;
            public int teams
            {
                get
                {
                    return Math.Abs(_teams);
                }
                set
                {
                    _teams = value;
                }
            }
            public int apparitionRound { get; set; }
            public Tournament tournament { get; set; }

            public LeagueCupApparition(int teams, int apparitionRound, Tournament tournament)
            {
                _teams = teams;
                this.apparitionRound = apparitionRound;
                this.tournament = tournament;
            }
        }

        public RecoverTeams GetRecoverTeams(Tournament league, bool getBestTeamsRecover)
        {
            RecoverTeams res = new RecoverTeams(null, -1, RecuperationMethod.Best);
            foreach (Round r in _rounds)
            {
                for (int k = 0; k < r.recuperedTeams.Count; k++)
                {
                    RecoverTeams rt = r.recuperedTeams[k];
                    bool isBestTeams = r.recuperedTeams[k].Method == RecuperationMethod.Best;
                    if ((rt.Source as Round).Tournament == league && (getBestTeamsRecover == isBestTeams || GetOtherRecoverTeamsOfRound(rt).Source == null))
                    {
                        res = rt;
                    }
                }
            }
            return res;
        }

        public void UpdateRecuperedTeams(int newTeamsCount, Tournament league, bool updateBestTeamsRecover, bool addUp)
        {
            foreach (Round r in _rounds)
            {
                for (int k = 0; k < r.recuperedTeams.Count; k++)
                {
                    RecoverTeams rt = r.recuperedTeams[k];
                    bool isBestTeams = rt.Method == RecuperationMethod.Best;
                    if ((rt.Source as Round).Tournament == league && (updateBestTeamsRecover == isBestTeams || GetOtherRecoverTeamsOfRound(rt).Source == null))
                    {
                        rt.Number = addUp ? rt.Number + newTeamsCount : newTeamsCount;
                        r.recuperedTeams[k] = rt;
                        Console.WriteLine("[" + r.name + "] Récupère " + rt.Number + " équipes du tour " + (rt.Source as Round).Tournament.name + " (" + rt.Method + ")");
                    }
                }
            }
        }
 
        /// <summary>
        /// Update national cup qualifications due to annual league structure modifications
        /// </summary>
        public void UpdateCupQualifications()
        {

            //Sauvegarde en mémoire les qualifications en coupe par défaut, elles pourraient être amenées à changer en cas de modification de la structure de la ligue
            bool alreadyStoredRecuperedTeams = false;
            foreach(Round r in _rounds)
            {
                if(r.baseRecuperedTeams.Count > 0)
                {
                    alreadyStoredRecuperedTeams = true;
                }
            }
            if(!alreadyStoredRecuperedTeams)
            {
                foreach(Round r in _rounds)
                {
                    r.baseRecuperedTeams.AddRange(new List<RecoverTeams>(r.recuperedTeams));
                }
            }

            // Les qualifications de chaque ligue à chaque tour sont remises par défaut
            foreach(Round r in _rounds)
            {
                r.recuperedTeams.Clear();
                r.recuperedTeams.AddRange(new List<RecoverTeams>(r.baseRecuperedTeams));
            }

            List<LeagueCupApparition> leagueCupApparitions = new List<LeagueCupApparition>();

            // Obtient le nombre d'équipes une fois toutes les ligues entrées dans la compétition
            int teamsKnockout = 0;
            int j = 0;
            for(int i = _rounds.Count-1; i >= 0 && teamsKnockout == 0; i--)
            {
                j++;
                Round r = _rounds[i];
                if(r.recuperedTeams.Count > 0)
                {
                    teamsKnockout = (int)Math.Pow(2, j);
                }
            }

            
            //Crée la liste qui résume à quel moment chaque ligue entre dans la compétition et combien d'équipes entrent par ligue
            foreach(Round r in _rounds)
            {
                List<RecoverTeams> recovers = new List<RecoverTeams>(r.recuperedTeams);
                foreach(RecoverTeams rt in recovers)
                {
                    Round rtRound = rt.Source as Round;
                    if(rtRound != null)
                    {
                        int clubsCount = rtRound.CountWithoutReserves();
                        RecoverTeams otherRecoverTeams = GetOtherRecoverTeamsOfRound(rt);
                        if(otherRecoverTeams.Source != null)
                        {
                            clubsCount = otherRecoverTeams.Method == RecuperationMethod.Worst ? -rt.Number : clubsCount - otherRecoverTeams.Number;
                        }
                        leagueCupApparitions.Add(new LeagueCupApparition(clubsCount, rounds.IndexOf(r), rtRound.Tournament));
                    }
                }
            }

            //Trie la liste en fonction du niveau de la compétition (et si jamais une ligue entre dans la compétition en deux fois avec meilleurs/plus mauvaises équipes)
            leagueCupApparitions.Sort((a, b) => (a.tournament.level != b.tournament.level ? a.tournament.level - b.tournament.level : (a.teams > 0 ? -1 : 0)));

            int roundStart = 0;
            foreach(LeagueCupApparition lca in leagueCupApparitions)
            {
                roundStart = lca.apparitionRound > roundStart ? lca.apparitionRound : roundStart;                
            }
            int currentTeamsCount = teamsKnockout;
            int additionalTeams = 0;
            //Remonte la compétition pour récupérer le nombre d'équipes à ajouter au premier tour
            for(int i = roundStart-1; i > -1; i--)
            {
                //We remove all teams that will be added to the next round
                additionalTeams = CountAdditionnalTeamsRound(i + 1, leagueCupApparitions);
                currentTeamsCount = (currentTeamsCount - additionalTeams) * 2;
                Console.WriteLine("[Round " + i + "] Will add " + additionalTeams + " teams. " + currentTeamsCount + " teams (" + currentTeamsCount / 2 + " matchs)");
            }

            additionalTeams = CountAdditionnalTeamsRound(0, leagueCupApparitions);
            Console.WriteLine(additionalTeams + " >= " + currentTeamsCount + " ?");
            //Petit fix pour corriger les approximation du ratio
            if(additionalTeams - currentTeamsCount == 1)
            {
                leagueCupApparitions[leagueCupApparitions.Count - 1].teams--;
            }
            //On a suffisament d'équipes dans les divisions qui entrent au premier tour, on sélectionne alors n équipes parmis ces ligues
            if(additionalTeams >= currentTeamsCount)
            {
                Console.WriteLine("Suffisament d'équipes disponibles pour les exigences du tour");
                //Ratio d'équipes à prendre dans chaque ligue pour obtenir le nombre d'équipes ciblé
                double ratio = additionalTeams / (currentTeamsCount + 0.0);
                int currentTeamsAdded = 0;
                Console.WriteLine("On prend");
                for(int i = 0; i<leagueCupApparitions.Count; i++)
                {
                    int teamsToSelectFromLeague = leagueCupApparitions[i].apparitionRound == 0 ? (int)Math.Round(leagueCupApparitions[i].teams / ratio) : leagueCupApparitions[i].teams;
                    currentTeamsAdded += leagueCupApparitions[i].apparitionRound == 0 ?  teamsToSelectFromLeague : 0;
                    UpdateRecuperedTeams(teamsToSelectFromLeague, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, false);

                }
                //Dans le cas où le calcul à partir du ratio n'a pas fonctionné (ça ne tombe pas rond), alors on fixe en ajoutant ou retirant manuellement un ou plusieurs équipes pour obtenir le bon compte
                if(currentTeamsAdded != currentTeamsCount)
                {
                    int margin = currentTeamsCount - currentTeamsAdded;
                    Console.WriteLine("Marge : " + margin + " avec " + currentTeamsCount + " equipes a atteindres et " + currentTeamsAdded + " equipes ajoutés");
                    for(int i = 0; i <leagueCupApparitions.Count && margin != 0; i++)
                    {
                        int currentlyAddedTeamFromLeague = 0;
                        RecoverTeams rt = GetRecoverTeams(leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams);
                        if (rt.Source != null && leagueCupApparitions[i].apparitionRound == 0)
                        {
                            currentlyAddedTeamFromLeague = rt.Number;
                        }
                        if(leagueCupApparitions[i].apparitionRound == 0 && currentlyAddedTeamFromLeague + margin <= leagueCupApparitions[i].teams)
                        {
                            UpdateRecuperedTeams(margin, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, true);
                            margin = 0;
                        }
                    }
                }
            }
            //On a pas suffisament d'équipes dans les ligues inférieures qui entrent au premier tour, on supprime ce tour pour passer directement au suivant (voir plus loin si on a toujours pas suffisament d'équipes) en simulant n qualifiés parmis les équipes des ligues inférieures
            else
            {
                Console.WriteLine("Trop peu d'équipes disponibles pour les exigences du tour, obliger de supprimer des tours");
                int delRoundId = 0;
                bool ok = false;
                while(!ok)
                {
                    Console.WriteLine("Suppression du tour ID " + delRoundId);
                    //On supprime un tour en plus, on calcule le nombre d'équipe censée s'être qualifié à ce tour (à partir du tour précédéant) et le nombre total d'équipe à ce tour (en ajoutant celles qui arrivent à ce tour)
                    delRoundId++;
                    int expectedQualified = currentTeamsCount / 2;
                    int expectedRoundTeams = expectedQualified + CountAdditionnalTeamsRound(delRoundId, leagueCupApparitions);
                    Console.WriteLine("[Tour " + delRoundId + "] On attends " + expectedQualified + " qualifiés + " + CountAdditionnalTeamsRound(delRoundId, leagueCupApparitions) + " nouvelles équipes = " + expectedRoundTeams + " équipes à ce tour");
                    int totalDispoTeams = 0;
                    for(int i = 0; i<delRoundId+1;i++)
                    {
                        totalDispoTeams += CountAdditionnalTeamsRound(i, leagueCupApparitions);
                    }
                    Console.WriteLine(totalDispoTeams + " équipes disponibles pour le tour " + delRoundId);
                    //Si le nombre d'équipes disponibles (c'est à dire l'intégralité des équipes des tours en lice à ce moment de la compétition) est supérieur au nombre nécessaire, alors c'est bon on débute la compétition à ce tour
                    if(totalDispoTeams > expectedRoundTeams)
                    {
                        ok = true;
                        int totalApp = 0;
                        for(int i = 0; i<delRoundId; i++)
                        {
                            totalApp += CountAdditionnalTeamsRound(i, leagueCupApparitions);
                        }
                        double ratio = expectedQualified / (totalApp + 0.0); //Ratio d'équipes à prendre dans chaque ligue
                        int currentTeamsAdded = 0;
                        //Créé les qualifications des ligues dans la coupe
                        List<RecoverTeams> newRecoverTeams = new List<RecoverTeams>();
                        for (int i = 0; i<delRoundId;i++)
                        {
                            for(int k = 0; k<leagueCupApparitions.Count; k++)
                            {
                                int teamsToAddFromLeague = (int)Math.Round(leagueCupApparitions[k].teams * ratio); //Le nombre d'équipes à prendre dans la ligue est calculé en fonction du nombre d'équipes nécessaire au tour
                                if (leagueCupApparitions[k].apparitionRound == i)
                                {
                                    RecoverTeams newRt = new RecoverTeams(leagueCupApparitions[k].tournament.rounds[0], teamsToAddFromLeague, leagueCupApparitions[k].isBestTeams ? RecuperationMethod.Best : RecuperationMethod.Worst);
                                    _rounds[delRoundId].recuperedTeams.Add(newRt);
                                    currentTeamsAdded += teamsToAddFromLeague;
                                    newRecoverTeams.Add(newRt);
                                    Console.WriteLine("Tour " + i + ", " + teamsToAddFromLeague + " équipes de D" + (k + 1) + " directement qualifiés au tour " + delRoundId);
                                }
                            }
                            _rounds[i].recuperedTeams.Clear();
                        }
                        //On oublie pas de mettre à jour les qualifications des autres ligues aux tours plus loin car d'une année à l'autre le nombre d'équipes première dans une ligue a changé
                        for(int i = 0; i<leagueCupApparitions.Count; i++)
                        {
                            RecoverTeams rt = GetRecoverTeams(leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams);
                            if(leagueCupApparitions[i].apparitionRound >= delRoundId && !newRecoverTeams.Contains(rt))
                            {
                                UpdateRecuperedTeams(leagueCupApparitions[i].teams, leagueCupApparitions[i].tournament, leagueCupApparitions[i].isBestTeams, false);
                            }
                        }
                        Console.WriteLine(currentTeamsAdded + " équipes ajoutées sur un total de " + expectedQualified);
                        //Même fixe que l'autre cas si le calcul du ratio n'est pas tombé juste
                        if(currentTeamsAdded != expectedQualified)
                        {
                            int margin = expectedQualified - currentTeamsAdded;
                            Console.WriteLine("Marge : " + margin + " avec "+ expectedQualified + " equipes a atteindres et " + currentTeamsAdded + " equipes ajoutés");
                            for (int i = 0; i < newRecoverTeams.Count && margin != 0; i++)
                            {
                                int currentlyAddedTeamFromLeague = 0;
                                RecoverTeams rt = newRecoverTeams[i];
                                LeagueCupApparition lc = null;
                                foreach (LeagueCupApparition lca in leagueCupApparitions)
                                {
                                    if(lca.tournament == (rt.Source as Round).Tournament && lca.isBestTeams == (rt.Method == RecuperationMethod.Best))
                                    {
                                        lc = lca;
                                    }
                                }
                                if (rt.Source != null)
                                {
                                    currentlyAddedTeamFromLeague = rt.Number;
                                }
                                if (currentlyAddedTeamFromLeague + margin <= lc.teams)
                                {
                                    UpdateRecuperedTeams(margin, lc.tournament, lc.isBestTeams, true);
                                    margin = 0;
                                }
                            }
                        }
                    }
                    //Pas assez d'équipe, on passe au tour suivant en mettant à jour le nombre actuel d'équipes dans la compétition
                    currentTeamsCount = expectedRoundTeams;
                }
            }
        }

        /// <summary>
        /// End of the season, all rounds are reset and qualified teams for next years are dispatched
        /// </summary>
        public void Reset()
        {
            if(!isChampionship && (Session.Instance.Game.kernel.LocalisationTournament(this) as Country) == Session.Instance.Game.kernel.String2Country("Azerbaïdjan"))
            {
                Country c = Session.Instance.Game.kernel.String2Country("Azerbaïdjan");
                Console.WriteLine("Initialise la coupe de l'azerbaidjan");
                foreach(Round r in this.rounds)
                {
                    Console.WriteLine("[CDLA] " + r.name);
                    foreach(RecoverTeams rt in r.recuperedTeams)
                    {
                        Console.WriteLine("[CDLA][" + r.name + "]. Ajoute " + rt.Number + " depuis " + (rt.Source as Round).Tournament.name);
                    }
                }
                foreach(Tournament t in c.Tournaments())
                {
                    if(t.isChampionship)
                    {
                        Console.WriteLine("[CDLA][" + t.name + "]. Clubs : " + t.rounds[0].clubs.Count + " (" + t.rounds[0].CountWithoutReserves() + " équipes premières");
                    }
                }
            }
            _remainingYears--;
            if (_remainingYears == 0)
            {
                _remainingYears = _periodicity;
            
                UpdateRecords();
                Tournament copyForArchives = new Tournament(_name, _logo, _seasonBeginning, _shortName, _isChampionship, _level, _periodicity, _remainingYears, _color);

                int gamesCount = 0;
                foreach (Round r in rounds)
                {
                    copyForArchives.rounds.Add(r.Copy());
                    gamesCount += r.matches.Count;
                }
                copyForArchives.statistics = statistics;
                if(_periodicity == 1 || (_periodicity > 1 && gamesCount > 0))
                {
                    _previousEditions.Add(_periodicity == 1 ? Session.Instance.Game.date.Year : Session.Instance.Game.date.Year - periodicity, copyForArchives);
                }
                for (int i = 0; i<rounds.Count; i++)
                {

                    //Delete compo if we chose to reduce size of the savegame
                    if (Session.Instance.Game.options.reduceSaveSize)
                    {
                        foreach(Match m in rounds[i].matches)
                        {
                            m.compo1.Clear();
                            m.compo2.Clear();
                        }
                    }

                    rounds[i].Reset();
                    List<Club> clubs = new List<Club>(_nextYearQualified[i]);
                    foreach (Club c in clubs)
                    {
                        rounds[i].clubs.Add(c);
                    }
                }
                InitializeQualificationsNextYearsLists();
                currentRound = -1;
                if(isHostedByOneCountry)
                {
                    InitializeHost();
                }
            }
            if(!isChampionship && !IsInternational() && (Session.Instance.Game.kernel.LocalisationTournament(this) as Country).LeagueSystemWithReserves())
            {
                UpdateCupQualifications();
            }
        }

        private void UpdateRecords()
        {
            foreach(Round r in _rounds)
            {
                foreach(Match m in r.matches)
                {
                    if (_statistics.LargerScore == null || Math.Abs(m.score1 - m.score2) >
                        Math.Abs(_statistics.LargerScore.score1 - _statistics.LargerScore.score2))
                    {
                        _statistics.LargerScore = m;
                    }

                    if (_statistics.BiggerScore == null || m.score1 + m.score2 >
                        _statistics.BiggerScore.score1 + _statistics.BiggerScore.score2)
                    {
                        _statistics.BiggerScore = m;
                    }
                }
            }
            if (_isChampionship)
            {
                Round championship = _rounds[0];
                foreach(Club c in championship.clubs)
                {
                    int goalsFor = championship.GoalsFor(c);
                    int goalsAgainst = championship.GoalsAgainst(c);
                    int points = championship.Points(c);
                    if(_statistics.MostPoints.Value == null || _statistics.MostPoints.Key < points)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(points, c);
                        _statistics.MostPoints = newRecord;
                    }
                    if (_statistics.LowestPoints.Value == null || _statistics.LowestPoints.Key > points)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(points, c);
                        _statistics.LowestPoints = newRecord;
                    }
                    if (_statistics.BiggestAttack.Value == null || _statistics.BiggestAttack.Key < goalsFor)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsFor, c);
                        _statistics.BiggestAttack = newRecord;
                    }
                    if (_statistics.WeakestAttack.Value == null || _statistics.WeakestAttack.Key > goalsFor)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsFor, c);
                        _statistics.WeakestAttack = newRecord;
                    }
                    if (_statistics.BiggestDefense.Value == null || _statistics.BiggestDefense.Key > goalsAgainst)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsAgainst, c);
                        _statistics.BiggestDefense = newRecord;
                    }
                    if (_statistics.WeakestDefense.Value == null || _statistics.WeakestDefense.Key < goalsAgainst)
                    {
                        KeyValuePair<int, Club> newRecord = new KeyValuePair<int, Club>(goalsAgainst, c);
                        _statistics.WeakestDefense = newRecord;
                    }
                }
            }
        }

        public void NextRound()
        {
            if (currentRound > -1 && currentRound < _rounds.Count)
            {
                _rounds[currentRound].DistributeGrants();
            }
            if (_rounds.Count > currentRound + 1)
            {
                currentRound++;
                _rounds[currentRound].Initialise();
                if(_rounds[currentRound].rules.Contains(Rule.HostedByOneCountry))
                {
                    _rounds[currentRound].AffectHostStadiumsToGames();
                }
            }

            //Tour 0, championnat -> génère match amicaux
            if (currentRound == 0)
            {
                if (isChampionship)
                {
                    foreach (Club c in rounds[0].clubs)
                    {
                        CityClub cv = c as CityClub;
                        if (cv != null)
                        {
                            cv.GenerateFriendlyGamesCalendar();
                        }
                    }
                }
            }



        }

        /// <summary>
        /// Qualify a club for a round on the next year edition fo the tournament
        /// </summary>
        /// <param name="c">The club to add</param>
        /// <param name="tourIndex">Index of the round where club is qualified</param>
        public void AddClubForNextYear(Club c, int tourIndex)
        {
            _nextYearQualified[tourIndex].Add(c);
        }

        public override string ToString()
        {
            return _name;
        }

        public int AverageAttendance(Club c)
        {
            int i = 0;
            int attendance = 0;
            foreach(Round t in _rounds)
            {
                foreach(Match m in t.matches)
                {
                    if((m.home == c) && m.Played)
                    {
                        attendance += m.attendance;
                        i++;
                    }
                }
            }
            return i != 0 ? attendance/i : 0;
        }

        public Club Winner()
        {
            if(isChampionship)
            {
                return _rounds[0].Winner();
            }
            else
            {
                return _rounds[_rounds.Count - 1].Winner();
            }
        }

        public List<KeyValuePair<Player, int>> Goalscorers()
        {
            Dictionary<Player, int> goalscorers = new Dictionary<Player, int>();

            foreach(Round t in _rounds)
            {
                foreach(KeyValuePair<Player,int> kvp in t.GoalScorers())
                {
                    if (goalscorers.ContainsKey(kvp.Key))
                    {
                        goalscorers[kvp.Key] += kvp.Value;
                    }
                    else
                    {
                        goalscorers[kvp.Key] = kvp.Value;
                    }
                }
            }

            List<KeyValuePair<Player, int>> list = goalscorers.ToList();

            list.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));

            return list;
        }


        private void UpdateRecoverTeamsRound(Round oldRound, Round newRound)
        {
            foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                foreach(Round r in t.rounds)
                {
                    for(int i = 0; i < r.recuperedTeams.Count; i++)
                    {
                        RecoverTeams rt = r.recuperedTeams[i];
                        Console.WriteLine(rt.Source.ToString());
                        if (rt.Source == oldRound)
                        {
                            Console.WriteLine("Match entre " + rt.Source.ToString() + " et " + oldRound.ToString());
                            rt.Source = newRound;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Make the tournament inactive (when used decide to disable tournament)
        /// </summary>
        public void DisableTournament()
        {
            List<Round> newRounds = new List<Round>();
            int i = 0;
            foreach(Round t in _rounds)
            {
                InactiveRound newRound = new InactiveRound(t.name, t.programmation.defaultHour, t.programmation.initialisation, t.programmation.end);
                newRound.rules.AddRange(t.rules);
                newRounds.Add(newRound);

                GroupsRound tGroup = t as GroupsRound;
                if(tGroup != null)
                {
                    if(tGroup.RandomDrawingMethod != RandomDrawingMethod.Administrative)
                    {
                        for (int j = 0; j < tGroup.groupsCount; j++)
                        {
                            List<Qualification> groupQualifications = tGroup.GetGroupQualifications(j);
                            groupQualifications.Sort(new QualificationComparator());
                            foreach (Qualification q in groupQualifications)
                            {
                                if (q.qualifies == 0 || (q.qualifies < 0 && (-q.qualifies > j - 1)) || (q.qualifies > 0 && (j < q.qualifies)))
                                {
                                    newRound.qualifications.Add(new Qualification((j + 1) + ((q.ranking - 1) * tGroup.groupsCount), q.roundId, q.tournament, q.isNextYear, q.qualifies));
                                }
                            }
                        }

                        //Groups with extra teams request additional qualification to the next year same tournament
                        int extraTeams = tGroup.clubs.Count % tGroup.groupsCount;
                        for (int j = tGroup.clubs.Count - extraTeams; j < tGroup.clubs.Count; j++)
                        {
                            newRound.qualifications.Add(new Qualification(j + 1, 0, tGroup.Tournament, true, 0));
                        }
                    }
                    else
                    {
                        /*
                        Country c = Session.Instance.Game.kernel.LocalisationTournament(this) as Country;
                        GroupsRound lowerNonDeactivatedDivisionGroups = null;
                        for(int level = _level - 1; level > 0 && lowerNonDeactivatedDivisionGroups == null; level--)
                        {
                            lowerNonDeactivatedDivisionGroups = c.League(level).rounds[0] as GroupsRound;
                        }
                        
                        //List administrative divisions of clubs in the round
                        List<AdministrativeDivision> roundAdministrativeDivisions = new List<AdministrativeDivision>();
                        List<AdministrativeDivision> roundAdministrativeDivisionsLowerDivisionLevel = new List<AdministrativeDivision>();
                        foreach (Club tClub in t.clubs)
                        {
                            AdministrativeDivision adClub = tClub.Country().GetAdministrativeDivisionLevel(tClub.AdministrativeDivision(), tGroup.administrativeLevel);
                            AdministrativeDivision adClubCompLevel = lowerNonDeactivatedDivisionGroups != null ? tClub.Country().GetAdministrativeDivisionLevel(tClub.AdministrativeDivision(), lowerNonDeactivatedDivisionGroups.administrativeLevel) : null;
                            if (!roundAdministrativeDivisions.Contains(adClub))
                            {
                                roundAdministrativeDivisions.Add(adClub);
                            }
                            if (adClubCompLevel != null && !roundAdministrativeDivisionsLowerDivisionLevel.Contains(adClubCompLevel))
                            {
                                roundAdministrativeDivisionsLowerDivisionLevel.Add(adClubCompLevel);
                            }
                        }

                        int relegationsCount = 0;
                        int promotionsCount = 0;
                        Tournament upperTournament = null;
                        Tournament lowerTournament = null;
                        int baseRelegations = 0;
                        int basePromotions = 0;
                        foreach (Qualification q in t.qualifications)
                        {
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level > level)
                            {
                                baseRelegations++;
                                lowerTournament = q.tournament;
                            }
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level < level)
                            {
                                basePromotions++;
                                upperTournament = q.tournament;
                            }
                        }

                        if(true || (lowerNonDeactivatedDivisionGroups != null && lowerNonDeactivatedDivisionGroups.RandomDrawingMethod != RandomDrawingMethod.Administrative))
                        {
                            relegationsCount = roundAdministrativeDivisions.Count * baseRelegations;
                            promotionsCount = roundAdministrativeDivisions.Count * basePromotions;

                        }
                        else
                        {
                            //relegationsCount = int((roundAdministrativeDivisionsLowerDivisionLevel.Count / roundAdministrativeDivisionsLowerDivisionLevel.Count)) * baseRelegations;
                            promotionsCount = 0;
                                //si group_level.adm > lowerNonDeactivatedDivision.adm_level:
			                    //    relegation = int ((nb adm group_level / nb adm lowerNonDeactivatedDivision.adm_level) * relegation)
			                    //    promotions = int ((nb adm group_level / nb adm lowerNonDeactivatedDivision.adm_level) * relegation) //Assurer algo cohérent
                        }
                        //Apply relegations and promotions
                        for (int p = 0; p < promotionsCount; p++)
                        {
                            newRound.qualifications.Add(new Qualification(p + 1, 0, upperTournament, true, 0));
                        }
                        for (int r = t.clubs.Count; r > t.clubs.Count - relegationsCount; r--)
                        {
                            newRound.qualifications.Add(new Qualification(r, 0, lowerTournament, true, 0));
                        }
                        */
                    }

                }
                
                List<Qualification> qualificationsToAdd = new List<Qualification>(t.qualifications);
                
                ChampionshipRound cRound = t as ChampionshipRound;
                if (cRound != null)
                {
                    qualificationsToAdd = cRound.GetQualifications();
                }

                InactiveRound iRound = t as InactiveRound;
                //If this round has less promotions than upper inactive round, then
                if(iRound != null && _level > 1)
                {
                    InactiveRound upperIRound = (Session.Instance.Game.kernel.LocalisationTournament(this) as Country).League(_level - 1).rounds[0] as InactiveRound;
                    if(upperIRound != null)
                    {
                        int totalRelegationsUpper = 0;
                        foreach(Qualification q in upperIRound.qualifications)
                        {
                            if(q.isNextYear && q.roundId == 0 && q.tournament.level == _level)
                            {
                                totalRelegationsUpper++;
                            }
                        }
                        int currentPromotions = 0;
                        int currentRelegations = 0;
                        Tournament lowerTournament = null;
                        Tournament upperTournament = null;
                        foreach(Qualification q in iRound.qualifications)
                        {
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level < _level)
                            {
                                currentPromotions++;
                                upperTournament = q.tournament;
                            }
                            if (q.isNextYear && q.roundId == 0 && q.tournament.level > _level)
                            {
                                currentRelegations++;
                                lowerTournament = q.tournament;
                            }
                        }

                        if(totalRelegationsUpper > currentPromotions && currentPromotions > 0)
                        {
                            int ratio = totalRelegationsUpper / currentPromotions;
                            //Adapt prom/rel to ratio
                            int newPromotions = totalRelegationsUpper;
                            int newRelegations = currentRelegations * ratio;
                            for(int p = 0; p < newPromotions; p++)
                            {
                                for (int j = 0; j < iRound.qualifications.Count; j++)
                                {
                                    Qualification q = iRound.qualifications[j];
                                    if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == p+1)
                                    {
                                        iRound.qualifications[j] = new Qualification(q.ranking, q.roundId, upperTournament, q.isNextYear, 0);
                                    }
                                }
                            }
                            for(int r = iRound.clubs.Count; r > iRound.clubs.Count-newRelegations; r++)
                            {
                                for (int j = 0; j < iRound.qualifications.Count; j++)
                                {
                                    Qualification q = iRound.qualifications[j];
                                    if (q.isNextYear && q.tournament.isChampionship && q.roundId == 0 && q.ranking == r)
                                    {
                                        iRound.qualifications[j] = new Qualification(q.ranking, q.roundId, lowerTournament, q.isNextYear, 0);
                                    }
                                }
                            }
                        }
                    }
                }

                foreach (Qualification q in qualificationsToAdd)
                {
                    //TODO: Non optimal architecture
                    /*if(t as GroupsRound != null)
                    {
                        List<Qualification>[] qualifications = new List<Qualification>[tGroup.groupsCount];
                        for(int j = 0; j<tGroup.groupsCount; j++)
                        {
                            qualifications[i] = tGroup.GetGroupQualifications(j);
                        }
                        int clubsCount = tGroup.clubs.Count;
                        Console.WriteLine("Deactivate " + t.name + " from " + this.name);
                        for(int j = tGroup.groupsCount*(q.ranking-1); j< tGroup.groupsCount * q.ranking; j++)
                        {
                            newRound.qualifications.Add(new Qualification(j+1, q.roundId, q.tournament, q.isNextYear, q.qualifies));
                        }
                    }*/
                    if(t as ChampionshipRound != null)
                    {
                        newRound.qualifications.Add(q);
                    }
                    else if(t as KnockoutRound != null)
                    {
                        int clubCount = t.clubs.Count ;
                        foreach(Tournament otherTournaments in Session.Instance.Game.kernel.Competitions)
                        {
                            foreach(Round otherRound in otherTournaments.rounds)
                            {
                                foreach(Qualification otherQualifications in otherRound.qualifications)
                                {
                                    if(otherQualifications.tournament == this && !otherQualifications.isNextYear && otherQualifications.roundId == i)
                                    {
                                        clubCount++;
                                    }
                                }
                            }
                        }
                        int numberOfGames = clubCount / 2;
                        for(int j = numberOfGames*(q.ranking-1); j< numberOfGames * q.ranking; j++)
                        {
                            newRound.qualifications.Add(new Qualification(j+1, q.roundId, q.tournament, q.isNextYear, q.qualifies));
                        }

                    }
                    else if(t as InactiveRound != null)
                    {
                        newRound.qualifications.Add(q);
                    }
                    

                }
                foreach (RecoverTeams re in t.recuperedTeams)
                {
                    newRound.recuperedTeams.Add(re);
                }

                foreach (Club c in t.clubs)
                {
                    newRound.clubs.Add(c);
                }

                foreach (Prize d in t.prizes)
                {
                    newRound.prizes.Add(d);
                }

                foreach (Rule r in t.rules)
                {
                    newRound.rules.Add(r);
                }

                //UpdateRecoverTeamsRound(t, newRound);
                
                i++;
            }

            foreach(Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                if(c != this)
                {
                    foreach (Round t in c.rounds)
                    {
                        for(int j = 0; j<t.recuperedTeams.Count; j++)
                        {
                            RecoverTeams re = t.recuperedTeams[j];
                            if (_rounds.Contains(re.Source))
                            {
                                int index = _rounds.IndexOf(re.Source as Round);
                                re.Source = newRounds[index];
                            }
                            //Affectation to the list because re is a struct (get a copy, not a ref)
                            t.recuperedTeams[j] = re;
                        }
                        
                    }

                }
            }

            _rounds.Clear();
            foreach (Round t in newRounds)
            {
                _rounds.Add(t);
            }

            /*
            Tour premierTour = _tours[0];
            Tour dernierTour = _tours[_tours.Count - 1];
            TourInactif ti = new TourInactif("Tour", new Heure() { Heures = 18, Minutes = 0 }, premierTour.Programmation.Initialisation.AddDays(3), dernierTour.Programmation.Fin.AddDays(-3));

            List<IEquipesRecuperables> tours = new List<IEquipesRecuperables>(_tours);

            //qualifications

            //Pour toutes les autres compétitions, quand il faut récupérer des équipes de cette compétition on les récupère depuis le tour que l'on est en train de créer
            //Pour toutes les qualitifcations des autres compétitions vers ce tour, elle se transforment au tour 0 si c'est le tour 0 qui est visé et année suivante, sinon on abandonnne
            foreach (Competition c in Session.Instance.Partie.Gestionnaire.Competitions)
            {
                if(c != this)
                {
                    foreach(Tour t in _tours)
                    {
                        for(int i = 0; i<t.RecuperationEquipes.Count; i++)
                        {
                            RecuperationEquipes re = t.RecuperationEquipes[i];
                            if (tours.Contains(re.Source)) re.Source = ti;
                        }
                        for(int i = 0; i<t.Qualifications.Count; i++)
                        {
                            Qualification q = t.Qualifications[i];
                            if(!(q.Competition == this && q.IDTour == 0 && q.AnneeSuivante == true))
                            {
                                t.Qualifications.Remove(q);
                                i--;
                            }
                        }

                    }
                }
            }


            _tours.Clear();
            _tours.Add(ti);*/

        }

        public bool IsInternational()
        {
            return Session.Instance.Game.kernel.LocalisationTournament(this) as Continent != null;
        }

        public bool IsInvolved(Club c)
        {
            bool res = false;
            foreach(Round r in rounds)
            {
                if(r.clubs.Contains(c))
                {
                    res = true;
                }
            }
            return res;
        }
    }
}