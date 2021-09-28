using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public class ViewRankingChampionship : View
    {

        private readonly ChampionshipRound _round;
        private readonly double _sizeMultiplier;
        private readonly bool _focusOnTeam;
        private readonly Club _team;
        private readonly bool _reduced;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="round"></param>
        /// <param name="sizeMultiplier">Width and font size multiplier</param>
        /// <param name="focusOnTeam">If true, only show 5 rows, focus the ranking around the team</param>
        /// <param name="team">The team to focus ranking on</param>
        public ViewRankingChampionship(ChampionshipRound round, double sizeMultiplier, bool focusOnTeam = false, Club team = null, bool reduced = false)
        {
            _round = round;
            _sizeMultiplier = sizeMultiplier;
            _focusOnTeam = focusOnTeam;
            _team = team;
            _reduced = reduced;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            int i = 0;

            List<Club> clubs = _round.Ranking();

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

                sp.Children.Add(ViewUtils.CreateLabelOpenWindow<Club>(c, OpenClub, c.shortName,"StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth * 3.5));
                sp.Children.Add(ViewUtils.CreateLabel(_round.Points(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth, null, null, true));
                sp.Children.Add(ViewUtils.CreateLabel(_round.Played(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                if(!_reduced)
                {
                    sp.Children.Add(ViewUtils.CreateLabel(_round.Wins(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.Draws(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.Loses(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.GoalsFor(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                    sp.Children.Add(ViewUtils.CreateLabel(_round.GoalsAgainst(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth));
                }
                sp.Children.Add(ViewUtils.CreateLabel(_round.Difference(c).ToString(), "StyleLabel2", fontSize * _sizeMultiplier, regularCellWidth * 1.25));

                spRanking.Children.Add(sp);

            }

            //Only show colors when the ranking is not focused on a team
            if (!_focusOnTeam)
            {
                foreach (Qualification q in _round.qualifications)
                {
                    if (q.tournament.isChampionship)
                    {
                        int niveau = _round.Tournament.level;
                        string couleur = "backgroundColor";
                        if (q.tournament.level < niveau)
                        {
                            couleur = "promotionColor";
                        }
                        else if (q.tournament.level > niveau)
                        {
                            couleur = "relegationColor";
                        }
                        else if (q.tournament.level == niveau && q.roundId > _round.Tournament.rounds.IndexOf(_round))
                        {
                            couleur = "barrageColor";
                        }

                        int index = q.ranking;

                        if(couleur != "backgroundColor")
                        {
                            SolidColorBrush color = Application.Current.TryFindResource(couleur) as SolidColorBrush;
                            (spRanking.Children[index] as StackPanel).Background = color;
                        }
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

        private void clubNameButtonClick(Club c)
        {
            if(c != null && c as CityClub != null)
            {
                Windows_Club wc = new Windows_Club(c as CityClub);
                wc.Show();
            }
 
        }

    }
}