﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference = true)]
    public class Continent : IRecoverableTeams, ILocalisation
    {
        [DataMember]
        private List<Country> _countries;
        [DataMember]
        private List<Tournament> _tournaments;
        [DataMember]
        private string _name;
        [DataMember]
        private string _logo;

        /**
         * Represent qualification in continental clubs competitions in function of the country place in the coefficient ranking
         */
        [DataMember]
        private List<Qualification> _continentalQualifications;
        [DataMember]
        private List<Country> _associationRanking;
        [DataMember]
        private List<List<Country>> _archivalAssociationRanking;

        /// <summary>
        /// As association ranking can be long to be computed (and change only at the end of the season), ranking is stored here to be reused without computing all ranking
        /// </summary>
        public List<Country> associationRanking
        {
            get
            {
                if(_associationRanking == null)
                {
                    UpdateStoredAssociationRanking();
                }
                return _associationRanking;
            }
        }

        public List<List<Country>> archivalAssociationRanking => _archivalAssociationRanking;

        public List<Country> countries => _countries;
        public List<Qualification> continentalQualifications => _continentalQualifications;
        public List<Tournament> Tournaments()
        {
            return _tournaments;
        }

        public string Name()
        {
            return _name;
        }

        public string Logo()
        {
            return _logo;
        }
        

        public Continent(string name, string logo)
        {
            _name = name;
            _logo = logo;
            _countries = new List<Country>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Country>();
            _archivalAssociationRanking = new List<List<Country>>();
        }

        /// <summary>
        /// The parameter "onlyFirstTeams" is ignored because national teams are always first teams
        /// </summary>
        /// <param name="nombre"></param>
        /// <param name="methode"></param>
        /// <param name="onlyFirstTeams"></param>
        /// <returns></returns>
        public List<Club> RetrieveTeams(int number, RecuperationMethod method, bool onlyFirstTeams)
        {
            List<NationalTeam> nationalsTeams = new List<NationalTeam>();
            foreach(Club c in Session.Instance.Game.kernel.Clubs)
            {
                NationalTeam sn = c as NationalTeam;
                if(sn != null)
                {
                    if (_countries.Contains(sn.country))
                    {
                        nationalsTeams.Add(sn);
                    }
                }
            }
            List<Club> res = new List<Club>();
            if (method == RecuperationMethod.Best)
            {
                nationalsTeams.Sort(new NationsFifaRankingComparator());
            }
            else if (method == RecuperationMethod.Worst)
            {
                nationalsTeams.Sort(new NationsFifaRankingComparator(true));
            }
            else if (method == RecuperationMethod.Randomly)
            {
                nationalsTeams = Utils.ShuffleList<NationalTeam>(nationalsTeams);
            }

            for (int i = 0; i < number; i++)
            {
                res.Add(nationalsTeams[i]);
            }
            return res;
        }

        public int CountWithoutReserves()
        {
            return _countries.Count;
        }

        public int ContinentalTournamentsCount
        {
            get
            {
                int res = 0;
                foreach(Tournament t in Tournaments())
                {
                    //TODO: Move friendly tournaments in a "World" category
                    if(t.periodicity == 1 && t.name != Utils.friendlyTournamentName)
                    {
                        res++;
                    }
                }
                return res;
            }
        }

        public Tournament GetContinentalClubTournament(int level)
        {
            Tournament res = null;
            foreach (Tournament t in Tournaments())
            {
                if(t.periodicity == 1 && t.level == level && t.name != Utils.friendlyTournamentName)
                {
                    res = t;
                }
            }
            return res;
        }

        public void UpdateStoredAssociationRanking()
        {
            if(_associationRanking != null && _associationRanking.Count > 0)
            {
                _archivalAssociationRanking.Add(new List<Country>(_associationRanking));
            }
            _associationRanking = new List<Country>();
            foreach (Country c in _countries)
            {
                if (c.Tournaments().Count > 0)
                {
                    _associationRanking.Add(c);
                }
            }

            _associationRanking.Sort(new CountryComparator(CountryAttribute.CONTINENTAL_COEFFICIENT));

        }

        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Country c, int year)
        {
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            int level = 1;
            Tournament tournament = GetContinentalClubTournament(level);
            while(tournament != null)
            {
                Tournament archive = year == Session.Instance.Game.CurrentSeason ? tournament : tournament.previousEditions[year];
                foreach(Round r in archive.rounds)
                {
                    Console.WriteLine(r.clubs.Count);
                    foreach(Club club in r.clubs)
                    {
                        if(club.Country() == c && (!res.ContainsKey(club) || Utils.IsBefore(r.DateInitialisationRound(), res[club].tournament.rounds[res[club].roundId].DateInitialisationRound())))
                        {
                            res.Add(club, new Qualification(1, archive.rounds.IndexOf(r), tournament, true, 0));
                        }
                    }
                }
                level++;
                tournament = GetContinentalClubTournament(level);
            }

            return res;
        }


        public Dictionary<Club, Qualification> GetClubsQualifiedForInternationalCompetitions(Country c)
        {
            Dictionary<Club, Qualification> res = new Dictionary<Club, Qualification>();

            List<Country> countriesRanking = new List<Country>(associationRanking);
            int index = countriesRanking.IndexOf(c);

            List<Club> registeredClubs = new List<Club>();
            List<Club> leagueClubs = new List<Club>();
            Tournament firstDivisionChampionship = c.FirstDivisionChampionship();
            Round championshipRound = firstDivisionChampionship.GetLastChampionshipRound();
            List<Tournament> cups = c.Cups();
            List<Club> cupWinners = new List<Club>();
            foreach(Tournament cup in cups)
            {
                if (cup.currentRound == (cup.rounds.Count - 1) && cup.rounds.Last().matches[0].Played)
                {
                    cupWinners.Add(cup.Winner());
                }
                else
                {
                    cupWinners.Add(null);
                }
            }

            if (championshipRound as ChampionshipRound != null)
            {
                leagueClubs = (championshipRound as ChampionshipRound).Ranking();
            }
            if (championshipRound as InactiveRound != null)
            {
                leagueClubs = (championshipRound as InactiveRound).Ranking();
            }
            if (championshipRound as GroupsRound != null) //TODO: No sense to do this, no tournament finish on a group round (maybe if one day Top and Bottom championships are merged in a group round phase)
            {
                leagueClubs = new List<Club>(championshipRound.clubs);
                leagueClubs.Sort(new ClubRankingComparator(championshipRound.matches));
            }

            List<Club> finalPhasesClubs = firstDivisionChampionship.GetFinalPhasesClubs();
            if (finalPhasesClubs.Count > 0)
            {
                for (int j = finalPhasesClubs.Count - 1; j >= 0; j--)
                {
                    leagueClubs.Remove(finalPhasesClubs[j]);
                    leagueClubs.Insert(0, finalPhasesClubs[j]);
                }
            }
            int rank = index + 1;
            int currentLevel = 0;
            int cupRank = 0;
            foreach (Qualification q in _continentalQualifications)
            {
                if (q.ranking == rank)
                {
                    Club currentCupWinner = cupRank < cupWinners.Count ? cupWinners[cupRank] : null;
                    bool cupWinnerNotDefinedYet = cupRank < cupWinners.Count && cupWinners[cupRank] == null;
                    //isNextYear is used as "cup winner" here instead of league qualification
                    if ((!q.isNextYear || registeredClubs.Contains(currentCupWinner)) || (currentCupWinner == null && !cupWinnerNotDefinedYet))
                    {
                        for (int j = 0; j < q.qualifies; j++)
                        {
                            Club qualifiedClub = leagueClubs[currentLevel];
                            //If we get the cup winner and is already qualified, then we move to the next candidate team to avoid the cup winner entering two times in continental tournament
                            while (registeredClubs.Contains(qualifiedClub))
                            {
                                currentLevel++;
                                qualifiedClub = leagueClubs[currentLevel];
                            }
                            currentLevel++;
                            res.Add(qualifiedClub, q);
                            registeredClubs.Add(qualifiedClub);
                        }
                    }
                    else if(!cupWinnerNotDefinedYet)
                    {
                        res.Add(currentCupWinner, q);
                        registeredClubs.Add(currentCupWinner);
                    }
                    if (q.isNextYear)
                    {
                        cupRank++;
                    }
                }
            }

            return res;
        }

        public void QualifiesClubForContinentalCompetitionNextYear()
        {
            List<Country> countriesRanking = new List<Country>(associationRanking);
            
            for(int i = 0; i< countriesRanking.Count; i++)
            {
                List<Club> registeredClubs = new List<Club>();
                List<Club> clubs = new List<Club>();
                Tournament firstDivisionChampionship = countriesRanking[i].FirstDivisionChampionship();
                Round championshipRound = firstDivisionChampionship.GetLastChampionshipRound(); //firstDivisionChampionship.rounds[0];
                List<Tournament> cups = countriesRanking[i].Cups();
                List<Club> cupWinners = new List<Club>();
                cups.ForEach(t => cupWinners.Add(t.Winner()));

                if(championshipRound as ChampionshipRound != null)
                {
                    clubs = (championshipRound as ChampionshipRound).Ranking();
                }
                if (championshipRound as InactiveRound != null)
                {
                    clubs = (championshipRound as InactiveRound).Ranking();
                }
                if (championshipRound as GroupsRound != null) //TODO: No sense to do this, no tournament finish on a group round (maybe if one day Top and Bottom championships are merged in a group round phase)
                {
                    clubs = new List<Club>(championshipRound.clubs);
                    clubs.Sort(new ClubRankingComparator(championshipRound.matches));
                }

                List<Club> finalPhasesClubs = firstDivisionChampionship.GetFinalPhasesClubs();
                if (finalPhasesClubs.Count > 0)
                {
                    for(int j = finalPhasesClubs.Count-1; j >= 0; j--)
                    {
                        clubs.Remove(finalPhasesClubs[j]);
                        clubs.Insert(0, finalPhasesClubs[j]);
                    }
                }
                int rank = i + 1;
                int currentLevel = 0;
                int cupRank = 0;
                foreach(Qualification q in _continentalQualifications)
                {
                    if(q.ranking == rank)
                    {
                        Club currentCupWinner = cupRank < cupWinners.Count ? cupWinners[cupRank] : null;
                        //isNextYear is used as "cup winner" here instead of league qualification
                        if ((!q.isNextYear || registeredClubs.Contains(currentCupWinner)) || currentCupWinner == null)
                        {
                            for (int j = 0; j < q.qualifies; j++)
                            {
                                Club qualifiedClub = clubs[currentLevel];
                                //If we get the cup winner and is already qualified, then we move to the next candidate team to avoid the cup winner entering two times in continental tournament
                                while(registeredClubs.Contains(qualifiedClub))
                                {
                                    currentLevel++;
                                    qualifiedClub = clubs[currentLevel];
                                }
                                currentLevel++;
                                q.tournament.AddClubForNextYear(qualifiedClub, q.roundId);
                                registeredClubs.Add(qualifiedClub);
                                Console.WriteLine("Qualifie " + qualifiedClub.name + " pour la " + q.tournament.name + " (tour " + q.roundId + ")");
                            }

                        }
                        else
                        {
                            q.tournament.AddClubForNextYear(currentCupWinner, q.roundId);
                            registeredClubs.Add(currentCupWinner);
                            Console.WriteLine("Qualifie " + currentCupWinner.name + " pour la " + q.tournament.name + " (tour " + q.roundId + ")");
                        }
                        if(q.isNextYear)
                        {
                            cupRank++;
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return _name;
        }
    }
}