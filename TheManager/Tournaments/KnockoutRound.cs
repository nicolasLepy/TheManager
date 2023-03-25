using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using TheManager.Comparators;
using TheManager.Comparators;
using TheManager.Tournaments;

namespace TheManager
{


    [DataContract(IsReference =true)]
    public class KnockoutRound : Round
    {

        [DataMember]
        private RandomDrawingMethod _randomDrawingMethod;

        /// <summary>
        /// Teams are dispatched in regards with the precedant round (e.g. the final phase of WC)
        /// </summary>
        [DataMember]
        private bool _noRandomDrawing;

        public RandomDrawingMethod randomDrawingMethod => _randomDrawingMethod;


        public KnockoutRound(string name, Hour hour, List<GameDay> dates, List<TvOffset> offsets, bool twoLegs, int phases, GameDay initialisation, GameDay end, RandomDrawingMethod method, bool noRandomDrawing) : base(name, hour, dates, offsets, initialisation,end, twoLegs, phases, 0, -1)
        {
            _randomDrawingMethod = method;
            _noRandomDrawing = noRandomDrawing;
        }

        public override Round Copy()
        {
            Round t = new KnockoutRound(name, this.programmation.defaultHour, new List<GameDay>(programmation.gamesDays), new List<TvOffset>(programmation.tvScheduling), twoLegs, phases, programmation.initialisation, programmation.end, _randomDrawingMethod, _noRandomDrawing);
            
            foreach (Club c in this.clubs)
            {
                t.clubs.Add(c);
            }
            foreach (Match m in this.matches)
            {
                t.matches.Add(m);
            }
            foreach(Qualification q in this.qualifications)
            {
                t.qualifications.Add(q);
            }

            return t;
        }

        public override List<Match> NextMatchesDay()
        {
            List<Match> res = new List<Match>(this.matches);

            if (twoLegs)
            {
                bool firstLegMatchesAreAllPlayed = true;
                for(int i = 0;i<res.Count/2; i++)
                {
                    if (!matches[i].Played)
                    {
                        firstLegMatchesAreAllPlayed = false;
                    }
                }
                int deb = 0;
                if (firstLegMatchesAreAllPlayed)
                {
                    deb = res.Count / 2;
                }
                res = new List<Match>(res.GetRange(deb, res.Count/2));
            }

            try
            {
                res.Sort(new MatchDateComparator());
            }
            catch
            {
                Utils.Debug("TourElimination : match date comparator failed");
            }

            return res;
        }

        public override void DistributeGrants()
        {
            List<Match> matches;
            if(twoLegs)
            {
                matches = new List<Match>(_matches);
            }
            else
            {
                matches = new List<Match>();
                int nbMatches = _matches.Count;
                for(int i = 0; i<nbMatches; i++)
                {
                    matches.Add(_matches[i]);
                }
            }
            foreach(Prize d in _prizes)
            {
                if(d.Ranking == 1)
                {
                    foreach(Match m in matches)
                    {
                        CityClub cv = m.Winner as CityClub;
                        if(cv != null)
                        {
                            cv.ModifyBudget(d.Amount, BudgetModificationReason.TournamentGrant);
                        }
                    }
                }
                if(d.Ranking == 2)
                {
                    foreach (Match m in matches)
                    {
                        CityClub cv = m.Looser as CityClub;
                        if (cv != null)
                        {
                            cv.ModifyBudget(d.Amount, BudgetModificationReason.TournamentGrant);
                        }
                    }
                }
            }
        }

        public override void Initialise()
        {
            if (!Tournament.IsInternational())
            {
                AddTeamsToRecover();
            }
            //If it's an international tournament (national teams or continental cup eg), we add all teams to recover for all rounds now because ranking can fluctuate after and the same team could be selected for 2 differents rounds
            else if (Tournament.rounds[0] == this)
            {
                foreach (Round r in Tournament.rounds)
                {
                    r.AddTeamsToRecover();
                }
            }

            if(_noRandomDrawing)
            {
                Round previousRound = Tournament.rounds[Tournament.rounds.IndexOf(this) - 1];
                if(previousRound as KnockoutRound != null)
                {
                    KnockoutRound previousRoundKO = previousRound as KnockoutRound;
                    _matches = Calendar.DrawNoRandomDrawing(this, previousRoundKO);
                }
                else if (previousRound as GroupsRound != null)
                {
                    GroupsRound previousRoundG = previousRound as GroupsRound;
                    _matches = Calendar.DrawNoRandomDrawing(this, previousRoundG);
                }
            }
            else
            {
                _matches = Calendar.Draw(this);
            }

        }

        public override void QualifyClubs()
        {
            List<Match> matches = new List<Match>();
            if (!twoLegs)
            {
                matches = new List<Match>(_matches);
            }
            else
            {
                for (int i = 0; i < _matches.Count / 2; i++)
                {
                    matches.Add(_matches[_matches.Count / 2 + i]);
                }
            }

            foreach (Qualification q in _qualifications)
            {
                foreach (Match m in matches)
                {
                    Club c = null;
                    //Winners
                    if (q.ranking == 1)
                    {
                        c = m.Winner;
                        if (!q.isNextYear)
                        {
                            q.tournament.rounds[q.roundId].clubs.Add(c);
                        }
                        else
                        {
                            q.tournament.AddClubForNextYear(c, q.roundId);
                        }
                    }
                    //Losers
                    else if (q.ranking == 2)
                    {
                        c = m.Looser;
                        if (!q.isNextYear)
                        {
                            q.tournament.rounds[q.roundId].clubs.Add(c);
                        }
                        else
                        {
                            q.tournament.AddClubForNextYear(c, q.roundId);
                        }
                    }
                    if(c != null)
                    {
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
        }

        public override Club Winner()
        {
            Club res = null;
            if(_clubs.Count > 0)
            {
                //It's a final
                if(_clubs.Count == 2 && _matches.Count > 0)
                {
                    res = _matches[_matches.Count - 1].Winner;
                }
            }
            return res;
        }

        public override int MatchesDayNumber()
        {
            return _twoLegs ? 2 : 1;
        }

        public override List<Match> GamesDay(int journey)
        {
            List<Match> res = new List<Match>();
            int gamesByDay = _twoLegs ? _matches.Count / 2 : _matches.Count;
            for(int i = (journey-1)* gamesByDay; i < journey*gamesByDay; i++)
            {
                res.Add(_matches[i]);
            }
            return res;

        }

    }
}