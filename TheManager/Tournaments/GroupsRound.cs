﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;

namespace TheManager
{
    [DataContract(IsReference =true)]
    public class GroupsRound : Round
    {
        [DataMember]
        private int _groupsNumber;
        [DataMember]
        private List<Club>[] _groups;

        [DataMember]
        private RandomDrawingMethod _randomDrawingMethod;
        [DataMember]
        private List<GeographicPosition> _groupsLocalisation;
        [DataMember]
        private List<string> _groupsNames;
        

        public List<Club>[] groups { get => _groups; }

        public int groupsCount { get { return _groupsNumber; } }

        public List<GeographicPosition> groupsLocalisation { get => _groupsLocalisation; }

        public string GroupName(int groupId)
        {
            string res = "";
            if (_groupsNames.Count > groupId)
            {
                res = _groupsNames[groupId];
            }
            else
            {
                res = "Groupe " + (groupId + 1);
            }
            return res;
        }

        public void AddGroupName(string name)
        {
            _groupsNames.Add(name);
        }

        public GroupsRound(string name, Hour hour, List<DateTime> dates, List<TvOffset> offsets, int groupsCount, bool twoLegs, DateTime initialisation, DateTime end, RandomDrawingMethod randomDrawingMethod) : base(name, hour, dates, offsets, initialisation,end, twoLegs,0)
        {
            _groupsNumber = groupsCount;
            _groups = new List<Club>[_groupsNumber];
            _groupsNames = new List<string>();
            for (int i = 0; i < _groupsNumber; i++)
            {
                _groups[i] = new List<Club>();
            }
            _randomDrawingMethod = randomDrawingMethod;
            _groupsLocalisation = new List<GeographicPosition>();
        }

        public override Round Copy()
        {
            GroupsRound t = new GroupsRound(name, this.programmation.defaultHour, new List<DateTime>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), groupsCount, twoLegs, programmation.initialisation, programmation.end, _randomDrawingMethod);
            foreach (Match m in this.matches)
            {
                t.matches.Add(m);
            }
            foreach (Club c in this.clubs)
            {
                t.clubs.Add(c);
            }
            int i = 0;
            foreach (List<Club> c in _groups)
            {
                t._groups[i] = new List<Club>(c);
                i++;
            }
            return t;
        }

        public override void Initialise()
        {
            _groups = new List<Club>[_groupsNumber];
            for (int i = 0; i < _groupsNumber; i++)
            {
                _groups[i] = new List<Club>();
            }
            AddTeamsToRecover();
            SetGroups();
            for (int i = 0; i < _groupsNumber; i++)
            {
                _matches.AddRange(Calendar.GenerateCalendar(_groups[i], _programmation, twoLegs));
            }

        }

        public Qualification[] SwapQualifications(Qualification[] qualifications)
        {
            Qualification[] res = new Qualification[2];
            if(qualifications.Length > 1)
            {
                res[0] = qualifications[0];
                res[1] = qualifications[1];
                res[0].ranking = qualifications[1].ranking;
                res[1].ranking = qualifications[0].ranking;
            }
            return res;
        }
        public override void QualifyClubs()
        {
           
            List<Club>[] groups = new List<Club>[_groupsNumber];
            for (int i = 0; i < _groupsNumber; i++)
            {
                groups[i] = new List<Club>(Ranking(i));
            }
            for (int i = 0; i < _groupsNumber; i++)
            {

                List<Qualification> qualifications = new List<Qualification>(_qualifications);
                qualifications.Sort(new QualificationComparator());

                if (_rules.Contains(Rule.ReservesAreNotPromoted))
                {
                    qualifications = Utils.AdjustQualificationsToNotPromoteReserves(qualifications, groups[i], Tournament);
                }

                /*
                //If reserves can't be promoted
                if (_rules.Contains(Rule.ReservesAreNotPromoted))
                {
                    for (int j = 0; j<qualifications.Count; j++)
                    {
                        Qualification q = qualifications[j];
                        //If the two tournaments involved are championship and the level of the destination is higher in league structure than the current league
                        if(Tournament.isChampionship && q.tournament.isChampionship && q.tournament.level < Tournament.level)
                        {
                            Utils.Debug("check for the " + q.ranking + " place");
                            int offset = 0;
                            bool pursue = true;
                            while (pursue && j+offset < qualifications.Count)
                            {
                                //This is a reserve club so it must not be promoted
                                if (groups[i][q.ranking - 1 + offset] as ReserveClub != null)
                                {
                                    offset++;
                                }
                                else
                                {
                                    pursue = false;
                                    //If there is an offset, make a swap
                                    if(offset > 0)
                                    {
                                        Utils.Debug("swap " + j + " and " + (j + offset));
                                        Qualification first = qualifications[j];
                                        Qualification second = qualifications[j + offset];
                                        int tempRanking = second.ranking;
                                        second.ranking = first.ranking;
                                        first.ranking = tempRanking;
                                        qualifications[j] = second;
                                        qualifications[j + offset] = first;
                                    }
                                }
                            }
                        }
                    }
                }*/

                foreach (Qualification q in qualifications)
                {
                    Utils.Debug(q.ranking + " - " + q.tournament.name);
                }

                foreach (Qualification q in qualifications)
                {
                    Club c = groups[i][q.ranking - 1];
                    if (!q.isNextYear)
                    {
                        q.tournament.rounds[q.roundId].clubs.Add(c);
                    }
                    else
                    {
                        q.tournament.AddClubForNextYear(c, q.roundId);
                    }
                    if (q.tournament.isChampionship && c.Championship != null)
                    {
                        if (q.tournament.level > c.Championship.level)
                        {
                            c.supporters = (int)(c.supporters / 1.8f);
                        }
                        else if (q.tournament.level < c.Championship.level)
                        {
                            c.supporters = (int)(c.supporters * 1.8f);
                        }
                    }
                }
            }
        }

        public override  List<Match> NextMatchesDay()
        {
            List<Match> res = new List<Match>();
            return NextMatches();

        }

        public List<Match> GamesDay(int journey)
        {
            List<Match> res = new List<Match>();
            int matchesPerGroups = MatchesPerGamesDay() * ((_clubs.Count / _groupsNumber) - 1);
            if (twoLegs)
            {
                matchesPerGroups *= 2;
            }
            for (int i = 0; i < _groupsNumber; i++)
            {
                int baseIndex = (matchesPerGroups * i) + (MatchesPerGamesDay() * (journey - 1));
                for (int j = 0; j < MatchesPerGamesDay(); j++)
                {
                    res.Add(_matches[j + baseIndex]);
                }

            }

            return res;
        }

        public int MatchesPerGamesDay()
        {
            return (_clubs.Count / _groupsNumber) / 2;
        }

        public List<Club> Ranking(int group)
        {
            List<Club> res = new List<Club>(_groups[group]);
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches);
            res.Sort(comparator);
            return res;
        }

        public List<Club> RankingWithoutReserves(int ranking)
        {
            List<Club> res = new List<Club>();
            foreach(Club c in _groups[ranking])
            {
                if ((c as ReserveClub) == null)
                {
                    res.Add(c);
                }
            }
            ClubRankingComparator comparator = new ClubRankingComparator(this.matches);
            res.Sort(comparator);
            return res;
        }

        private void SetGroups()
        {
            IRandomDrawing randomDrawing = null;
            switch (_randomDrawingMethod)
            {
                case RandomDrawingMethod.Level:
                    randomDrawing = new RandomDrawingLevel(this);
                    break;
                case RandomDrawingMethod.Geographic:
                    randomDrawing = new RandomDrawingGeographic(this);
                    break;
            }
            randomDrawing.RandomDrawing();
        }

        public override void DistributeGrants()
        {
            foreach(Prize d in _prizes)
            {
                for(int i = 0;i<groupsCount; i++)
                {
                    CityClub cv = Ranking(i)[d.Ranking - 1] as CityClub;
                    if (cv != null)
                    {
                        cv.ModifyBudget(d.Amount, BudgetModificationReason.TournamentGrant);
                    }
                }

            }
        }

        /// <summary>
        /// No sense, there is no competition finishing with groups round
        /// </summary>
        /// <returns></returns>
        public override Club Winner()
        {
            return null;
        }
    }
}