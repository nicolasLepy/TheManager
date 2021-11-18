using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using System.Linq;

namespace TheManager_GUI.VueClassement
{
    public class ViewRankingChampionship : View
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
        public ViewRankingChampionship(ChampionshipRound round, double sizeMultiplier, bool focusOnTeam = false, Club team = null, bool reduced = false, RankingType rankingType = RankingType.General)
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

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            int i = 0;

            List<Club> clubs = _round.Ranking(_rankingType);

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
                if (index > ranking.Count - 5)
                {
                    index = ranking.Count - 5;
                }
                i = index;
                for (int j = index; j < index + 5; j++)
                {
                    Club c = ranking[j];
                    clubs.Add(c);
                    if(c == Session.Instance.Game.club)
                    {
                        indexTeam = j-index;
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


                if (_round.Tournament.IsInternational() && (c as CityClub) != null)
                {
                    sp.Children.Add(ViewUtils.CreateFlag((c as CityClub).city.Country(), regularCellWidth / 1.5, regularCellWidth / 1.5));

                }
                else if (_round.Tournament.IsInternational() && (c as ReserveClub) != null)
                {
                    sp.Children.Add(ViewUtils.CreateFlag((c as ReserveClub).FannionClub.city.Country(), regularCellWidth / 1.5, regularCellWidth / 1.5));
                }
                else
                {
                    sp.Children.Add(ViewUtils.CreateLogo(c, regularCellWidth / 1.5, regularCellWidth / 1.5));
                }

                sp.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(c, OpenClub, _round.Tournament.isChampionship ? c.extendedName : c.shortName,"StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth * 3.5));
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
                Club cupWinner = (Session.Instance.Game.kernel.LocalisationTournament(_round.Tournament) as Country)?.Cup(1)?.Winner();
                int roundLevel = _round.Tournament.level;
                ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(_round.Tournament);
                Country country = localisation as Country;
                List<Club> registeredClubs = new List<Club>();
                if(country != null && roundLevel == 1)
                {
                    Continent continent = country.Continent;
                    int nationIndex = continent.associationRanking.IndexOf(country) + 1;
                    int currentRanking = 0;
                    int totalQualificationsFromLeague = (from qualification in continent.continentalQualifications where (qualification.ranking == nationIndex && !qualification.isNextYear) select qualification.qualifies).Sum();

                    foreach(Qualification q in continent.continentalQualifications)
                    {
                        //q.isNextYear refeer to cup winner qualification for continental competition
                        if (q.ranking == nationIndex && (!q.isNextYear || registeredClubs.Contains(cupWinner) ))
                        {
                            for (int j = 0; j < q.qualifies; j++)
                            {
                                registeredClubs.Add(clubs[currentRanking]);
                                string color = QualificationColor(q);
                                SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                                (spRanking.Children[currentRanking + 1] as StackPanel).Background = lineColor;
                                currentRanking++;
                            }
                        }
                        else if(q.ranking == nationIndex && q.isNextYear && clubs.Contains(cupWinner))
                        {
                            string color = QualificationColor(q);
                            SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                            (spRanking.Children[clubs.IndexOf(cupWinner) + 1] as StackPanel).Background = lineColor;
                        }
                    }
                }
                foreach (Qualification q in _round.qualifications)
                {
                    string color = "backgroundColor";
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
                        else if (q.tournament.level == roundLevel && q.roundId > _round.Tournament.rounds.IndexOf(_round))
                        {
                            color = "barrageColor";
                        }
                    }
                    else if(q.tournament.IsInternational())
                    {
                        if (q.tournament.level == 1 && q.tournament.rounds[q.roundId] as GroupsRound != null)
                        {
                            color = "cl1Color";
                        }
                        else if (q.tournament.level == 1)
                        {
                            color = "cl2Color";
                        }
                        else if (q.tournament.level == 2 && q.tournament.rounds[q.roundId] as GroupsRound != null)
                        {
                            color = "el1Color";
                        }
                        else if (q.tournament.level == 2)
                        {
                            color = "el2Color";
                        }
                        else if (q.tournament.level == 3)
                        {
                            color = "el3Color";
                        }
                    }
                    int index = q.ranking;
                    if (color != "backgroundColor" && clubs.Count > 0)
                    {
                        SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                        (spRanking.Children[index] as StackPanel).Background = lineColor;
                    }

                }
            }
            else
            {

                SolidColorBrush color = new SolidColorBrush((System.Windows.Media.Color)Application.Current.TryFindResource("ColorDate"));
                color.Opacity = 0.6;
                (spRanking.Children[indexTeam+1] as StackPanel).Background = color;
            }
        }

    }
}