using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using System.Linq;
using System.Text;

namespace TheManager_GUI.VueClassement
{
    public class ViewRankingChampionship : ViewRanking
    {

        private readonly ChampionshipRound _round;
        private readonly double _sizeMultiplier;
        private readonly bool _focusOnTeam;
        private readonly Club _team;
        private readonly bool _reduced;
        private readonly RankingType _rankingType;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="round"></param>
        /// <param name="sizeMultiplier">Width and font size multiplier</param>
        /// <param name="focusOnTeam">If true, only show 5 rows, focus the ranking around the team</param>
        /// <param name="team">The team to focus ranking on</param>
        public ViewRankingChampionship(ChampionshipRound round, double sizeMultiplier, bool focusOnTeam = false, Club team = null, bool reduced = false, RankingType rankingType = RankingType.General) : base(round.Tournament)
        {
            _round = round;
            _sizeMultiplier = sizeMultiplier;
            _focusOnTeam = focusOnTeam;
            _team = team;
            _reduced = reduced;
            _rankingType = rankingType;
        }

        private string QualificationColor(Qualification q)
        {
            string color = "backgroundColor";
            if (q.tournament.level == 1 && q.tournament.rounds[q.roundId] as GroupsRound != null)
            {
                color = "cl1Color";
            }
            else if (q.tournament.level == 1 && q.tournament.rounds[q.roundId] as GroupsRound == null)
            {
                color = "cl2Color";
            }
            else if (q.tournament.level == 2 && q.tournament.rounds[q.roundId] as GroupsRound != null)
            {
                color = "el1Color";
            }
            else if (q.tournament.level == 2 && q.tournament.rounds[q.roundId] as GroupsRound == null)
            {
                color = "el2Color";
            }
            else if (q.tournament.level == 3)
            {
                color = "ecl1Color";
            }
            return color;
        }

        private bool QualificationCanLeadToRelegation(Round r, Tournament tournamentReference)
        {
            bool res = false;
            foreach(Qualification q in r.qualifications)
            {
                if(q.isNextYear && q.tournament.level > tournamentReference.level)
                {
                    res = true;
                }
            }
            if(!res)
            {
                foreach(Qualification q in r.qualifications)
                {
                    if (!q.isNextYear && q.tournament == tournamentReference && !q.tournament.IsInternational() && q.tournament.isChampionship)
                    {
                        res = QualificationCanLeadToRelegation(q.tournament.rounds[q.roundId], tournamentReference);
                    }
                }
            }

            return res;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            int i = 0;

            List<Club> clubs = _round.Ranking(_rankingType);

            // Get international qualifications
            // Search if the round is an archived round to get qualified teams on the right year
            // Else get qualification for the current season
            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(_tournament);
            Country country = localisation as Country;

            Dictionary<Club, Qualification> continentalClubs = new Dictionary<Club, Qualification>();
            if(country != null)
            {
                continentalClubs = _year > -1 ? country.Continent.GetClubsQualifiedForInternationalCompetitions(country, _year + 1) : country.Continent.GetClubsQualifiedForInternationalCompetitions(country);
            }

            //If we choose to focus on a team, we center the ranking on the team and +-2 other teams around
            int indexTeam = -1;
            if (_focusOnTeam && _team != null)
            {
                clubs = new List<Club>();
                List<Club> ranking = _round.Ranking();
                int index = ranking.IndexOf(Session.Instance.Game.club);
                index = index - 2;
                if (index < 0)
                {
                    index = 0;
                }
                int rankingBuffer = ranking.Count >= 5 ? 5 : 3;
                if (index > ranking.Count - rankingBuffer)
                {
                    index = ranking.Count - rankingBuffer;
                }
                i = index;
                for (int j = index; j < index + rankingBuffer; j++)
                {
                    Club c = ranking[j];
                    clubs.Add(c);
                    if (c == Session.Instance.Game.club)
                    {
                        indexTeam = j - index;
                    }
                }
            }

            double fontSize = (double)Application.Current.FindResource("TailleMoyenne");
            double regularCellWidth = 36 * _sizeMultiplier;

            StackPanel spTitle = new StackPanel();
            spTitle.Orientation = Orientation.Horizontal;
            spTitle.Children.Add(ViewUtils.CreateLabel("", "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth/1.25 + regularCellWidth / 1.5 + regularCellWidth*3.5));
            spTitle.Children.Add(ViewUtils.CreateLabel("Pts", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth));
            spTitle.Children.Add(ViewUtils.CreateLabel("J", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth));
            if(!_reduced)
            {
                spTitle.Children.Add(ViewUtils.CreateLabel("G", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth));
                spTitle.Children.Add(ViewUtils.CreateLabel("N", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth));
                spTitle.Children.Add(ViewUtils.CreateLabel("P", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth));
                spTitle.Children.Add(ViewUtils.CreateLabel("p.", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth));
                spTitle.Children.Add(ViewUtils.CreateLabel("c.", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth));
            }
            spTitle.Children.Add(ViewUtils.CreateLabel("Diff", "StyleLabel2Center", fontSize * _sizeMultiplier, regularCellWidth * 1.25));
            spRanking.Children.Add(spTitle);

            foreach (Club c in clubs)
            {
                i++;
                StackPanel sp = new StackPanel();
                sp.Orientation = Orientation.Horizontal;

                sp.Children.Add(ViewUtils.CreateLabel(i.ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth/1.25));

                if (_tournament.IsInternational() && (c as CityClub) != null)
                {
                    sp.Children.Add(ViewUtils.CreateFlag((c as CityClub).city.Country(), regularCellWidth / 1.5, regularCellWidth / 1.5));

                }
                else if (_tournament.IsInternational() && (c as ReserveClub) != null)
                {
                    sp.Children.Add(ViewUtils.CreateFlag((c as ReserveClub).FannionClub.city.Country(), regularCellWidth / 1.5, regularCellWidth / 1.5));
                }
                else
                {
                    sp.Children.Add(ViewUtils.CreateLogo(c, regularCellWidth / 1.5, regularCellWidth / 1.5));
                }

                StringBuilder cupWinnerStr = new StringBuilder("");
                foreach(KeyValuePair<Tournament, Club> kvp in _cupsWinners)
                {
                    if(kvp.Value == c)
                    {
                        cupWinnerStr.Append(" (").Append(kvp.Key.shortName).Append(") ");
                    }
                }
                if(_championshipTitleHolder == c)
                {
                    cupWinnerStr.Append(" (TT) ");
                }
                sp.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(c, OpenClub, (_tournament.isChampionship ? c.extendedName(_tournament, _absoluteYear-1) : c.shortName) + cupWinnerStr.ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth * 3.5));
                sp.Children.Add(ViewUtils.CreateLabel(_round.Points(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth, null, null, true));
                sp.Children.Add(ViewUtils.CreateLabel(_round.Played(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                if(!_reduced)
                {
                    sp.Children.Add(ViewUtils.CreateLabel(_round.Wins(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.Draws(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.Loses(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.GoalsFor(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.GoalsAgainst(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                }
                sp.Children.Add(ViewUtils.CreateLabel(_round.Difference(c, _rankingType).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth * 1.25));

                spRanking.Children.Add(sp);

            }

            //Only show colors when the ranking is not focused on a team
            if (!_focusOnTeam)
            {
                int roundLevel = _tournament.level;

                bool nationalTeamTournament = _round.clubs.Count > 0 && ((_round.clubs[0] as NationalTeam) != null);
                int qualificationsToTournamentNextRounds = 0;
                foreach (Qualification q in _round.qualifications)
                {
                    string color = "backgroundColor";

                    qualificationsToTournamentNextRounds = !q.isNextYear && q.tournament == _tournament && !QualificationCanLeadToRelegation(q.tournament.rounds[q.roundId], _tournament) ? qualificationsToTournamentNextRounds + 1 : qualificationsToTournamentNextRounds;
                    if (q.tournament.isChampionship)
                    {
                        if (q.tournament.level < roundLevel)
                        {
                            color = "promotionColor";
                        }
                        else if (q.tournament.level > roundLevel)
                        {
                            color = "relegationColor";
                        }
                        else if (q.tournament.level == roundLevel && q.roundId > _tournament.rounds.IndexOf(_round))
                        {
                            color = "barrageColor";
                        }
                    }
                    else if(nationalTeamTournament)
                    {
                        if (q.tournament.level == roundLevel && q.tournament != _round.Tournament)
                        {
                            color = "cl1Color";
                        }
                        else if (q.tournament.level == roundLevel && q.tournament == _round.Tournament)
                        {
                            color = "cl2Color";
                        }
                        else if (q.tournament.level > roundLevel)
                        {
                            color = q.qualifies >= 0 ? "el1Color" : "barrageRelegationColor";
                        }

                    }

                    int index = q.ranking > 0 ? q.ranking : clubs.Count + q.ranking + 1;
                    if (color != "backgroundColor" && clubs.Count > 0)
                    {
                        SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                        (spRanking.Children[index] as StackPanel).Background = lineColor;
                    }

                }

                int counter = 0;
                foreach (KeyValuePair<Club, Qualification> kvp in continentalClubs)
                {
                    counter++;
                    int indexClub = clubs.IndexOf(kvp.Key);
                    if (indexClub > -1 && counter > qualificationsToTournamentNextRounds)
                    {
                        string color = QualificationColor(kvp.Value);
                        SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                        (spRanking.Children[indexClub + 1] as StackPanel).Background = lineColor;
                    }
                }
            }
            else
            {
                SolidColorBrush color = new SolidColorBrush((System.Windows.Media.Color)Application.Current.TryFindResource("ColorDate"));
                color.Opacity = 0.6;
                (spRanking.Children[indexTeam + 1] as StackPanel).Background = color;
            }
        }
    }
}