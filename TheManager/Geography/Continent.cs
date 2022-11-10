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
        /**
         * Represent qualification in continental clubs competitions in function of the country place in the coefficient ranking
         */
        [DataMember]
        private List<Qualification> _continentalQualifications;
        [DataMember]
        private List<Country> _associationRanking;

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
        

        public Continent(string name)
        {
            _name = name;
            _countries = new List<Country>();
            _tournaments = new List<Tournament>();
            _continentalQualifications = new List<Qualification>();
            _associationRanking = new List<Country>();
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


        public void QualifiesClubForContinentalCompetitionNextYear()
        {
            List<Country> countriesRanking = new List<Country>(associationRanking);
            
            for(int i = 0; i< countriesRanking.Count; i++)
            {
                List<Club> registeredClubs = new List<Club>();
                List<Club> clubs = new List<Club>();
                Tournament firstDivisionChampionship = countriesRanking[i].FirstDivisionChampionship();
                Round championshipRound = firstDivisionChampionship.rounds[0];
                Club cupWinner = countriesRanking[i].Cup(1)?.Winner();

                if(championshipRound as ChampionshipRound != null)
                {
                    clubs = (championshipRound as ChampionshipRound).Ranking();
                }
                if (championshipRound as InactiveRound != null)
                {
                    clubs = (championshipRound as InactiveRound).Ranking();
                }
                if (championshipRound as GroupsRound != null)
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
                foreach(Qualification q in _continentalQualifications)
                {
                    if(q.ranking == rank)
                    {
                        //isNextYear is used as "cup winner" here instead of league qualification
                        if (!q.isNextYear || registeredClubs.Contains(cupWinner))
                        {
                            for (int j = 0; j < q.qualifies; j++)
                            {
                                Club qualifiedClub = clubs[currentLevel];
                                //If we get the cup winner and is already qualified, then we move to the next candidate team to avoid the cup winner entering two times in continental tournament
                                //TODO: Can't manage multiple cup winners (replace by a list of cup winners ?)
                                if(qualifiedClub == cupWinner && registeredClubs.Contains(cupWinner))
                                {
                                    currentLevel++;
                                    qualifiedClub = clubs[currentLevel];
                                }
                                currentLevel++;
                                q.tournament.AddClubForNextYear(qualifiedClub, q.roundId);
                                registeredClubs.Add(qualifiedClub);
                            }

                        }
                        else
                        {
                            q.tournament.AddClubForNextYear(cupWinner, q.roundId);
                            registeredClubs.Add(cupWinner);
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