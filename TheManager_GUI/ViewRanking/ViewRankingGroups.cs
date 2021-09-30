using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TheManager;
using TheManager.Comparators;

namespace TheManager_GUI.VueClassement
{
    public class ViewRankingGroups : View
    {

        private readonly GroupsRound _round;
        private readonly bool _focusOnTeam;
        private readonly double _sizeMultiplier;
        private readonly Club _team;

        public ViewRankingGroups(GroupsRound round, double sizeMultiplier, bool focusOnTeam, Club team)
        {
            _round = round;
            _focusOnTeam = focusOnTeam;
            _sizeMultiplier = sizeMultiplier;
            _team = team;
        }

        private StackPanel CreateRanking(int index, Club c)
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            int fontBase = (int)(14 * _sizeMultiplier);
            sp.Children.Add(ViewUtils.CreateLabel(index.ToString(), "StyleLabel2", fontBase, 30));
            if (_round.Tournament.IsInternational() && (c as CityClub) != null)
            {
                sp.Children.Add(ViewUtils.CreateFlag((c as CityClub).city.Country() , 30 * _sizeMultiplier, 30 * _sizeMultiplier));
            }
            else if (_round.Tournament.IsInternational() && (c as ReserveClub) != null)
            {
                sp.Children.Add(ViewUtils.CreateFlag((c as ReserveClub).FannionClub.city.Country(), 30 * _sizeMultiplier, 30 * _sizeMultiplier));
            }
            else
            {
                sp.Children.Add(ViewUtils.CreateLogo(c, 30 * _sizeMultiplier, 30 * _sizeMultiplier));
            }


            Label l2 = ViewUtils.CreateLabel(c.shortName, "StyleLabel2", fontBase, 150 * _sizeMultiplier);
            l2.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            { clubNameButtonClick(c); };

            sp.Children.Add(l2);
            sp.Children.Add(ViewUtils.CreateLabel(_round.Points(c).ToString(), "StyleLabel2", fontBase, 25));
            sp.Children.Add(ViewUtils.CreateLabel(_round.Played(c).ToString(), "StyleLabel2", fontBase, 25));
            sp.Children.Add(ViewUtils.CreateLabel(_round.Wins(c).ToString(), "StyleLabel2", fontBase, 25));
            sp.Children.Add(ViewUtils.CreateLabel(_round.Draws(c).ToString(), "StyleLabel2", fontBase, 25));
            sp.Children.Add(ViewUtils.CreateLabel(_round.Loses(c).ToString(), "StyleLabel2", fontBase, 25));
            sp.Children.Add(ViewUtils.CreateLabel(_round.GoalsFor(c).ToString(), "StyleLabel2", fontBase, 25));
            sp.Children.Add(ViewUtils.CreateLabel(_round.GoalsAgainst(c).ToString(), "StyleLabel2", fontBase, 25));
            sp.Children.Add(ViewUtils.CreateLabel(_round.Difference(c).ToString(), "StyleLabel2", fontBase, 25));

            return sp;

        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            //If focusing on a team, only show five teams around the current team
            if (_focusOnTeam)
            {
                List<Club> ranking = null;
                for(int group = 0; group < _round.groupsCount; group++)
                {
                    if (_round.groups[group].Contains(_team))
                    {
                        ranking = _round.Ranking(group);
                    }
                }
                //Never know if not null
                if(ranking != null)
                {
                    int beginningIndex = ranking.IndexOf(_team) - 2;
                    if(beginningIndex < 0)
                    {
                        beginningIndex = 0;
                    }
                    else if (beginningIndex > ranking.Count - 5)
                    {
                        beginningIndex = ranking.Count - 5;
                    }
                    for(int i = beginningIndex; i < beginningIndex + 5; i++)
                    {
                        spRanking.Children.Add(CreateRanking(i, ranking[i]));
                    }
                }
            }
            else
            {
                for (int poule = 0; poule < _round.groupsCount; poule++)
                {
                    Label labelPoule = new Label();
                    labelPoule.Content = _round.GroupName(poule);
                    labelPoule.Style = Application.Current.FindResource("StyleLabel1") as Style;
                    labelPoule.FontSize *= _sizeMultiplier;
                    spRanking.Children.Add(labelPoule);
                    int i = 0;
                    foreach (Club c in _round.Ranking(poule))
                    {
                        i++;
                        spRanking.Children.Add(CreateRanking(i, c));
                    }


                    int roundLevel = _round.Tournament.level;
                    foreach (Qualification q in _round.qualifications)
                    {
                        string color = "backgroundColor";
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
                            color = "el1Color";
                        }
                        int index = q.ranking;
                        if (color != "backgroundColor")
                        {
                            SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                            (spRanking.Children[  spRanking.Children.Count-_round.Ranking(poule).Count + index-1] as StackPanel).Background = lineColor;
                        }

                    }



                }

            }


            //Only show qualification if teams were dispatched in groups (if not useless to show qualifications color) and if we are not focusing on a team
            if (_round.groups[0].Count > 0 && !_focusOnTeam)
            {

                //Split qualifications in several list because according to group qualifications can be differents (if reserves are not promoted for instance)

                List<Qualification>[] qualifications = new List<Qualification>[_round.groupsCount];

                List<Club>[] groups = new List<Club>[_round.groupsCount];
                for (int i = 0; i < _round.groupsCount; i++)
                {
                    groups[i] = new List<Club>(_round.Ranking(i));
                }

                for (int i = 0; i< _round.groupsCount; i++)
                {
                    qualifications[i] = new List<Qualification>(_round.qualifications);
                    qualifications[i].Sort(new QualificationComparator());

                    //If reserves can't be promoted
                    if (_round.rules.Contains(Rule.ReservesAreNotPromoted))
                    {
                        qualifications[i] = Utils.AdjustQualificationsToNotPromoteReserves(qualifications[i], groups[i], _round.Tournament);
                        /*
                        for (int j = 0; j < qualifications[i].Count; j++)
                        {
                            Qualification q = qualifications[i][j];
                            //If the two tournaments involved are championship and the level of the destination is higher in league structure than the current league
                            if (_tour.Tournament.isChampionship && q.tournament.isChampionship && q.tournament.level < _tour.Tournament.level)
                            {
                                Utils.Debug("check " + q.ranking);
                                int offset = 0;
                                bool pursue = true;
                                while (pursue && j + offset < qualifications[i].Count)
                                {
                                    Utils.Debug("check " + groups[i][q.ranking - 1 + offset].name);
                                    //This is a reserve club so it must not be promoted
                                    if (groups[i][q.ranking - 1 + offset] as ReserveClub != null)
                                    {
                                        offset++;
                                    }
                                    else
                                    {
                                        pursue = false;
                                        //If there is an offset, make a swap
                                        if (offset > 0)
                                        {
                                            Utils.Debug("swap " + j + " and " + (j + offset));
                                            Qualification first = qualifications[i][j];
                                            Qualification second = qualifications[i][j + offset];
                                            int tempRanking = second.ranking;
                                            second.ranking = first.ranking;
                                            first.ranking = tempRanking;
                                            qualifications[i][j] = second;
                                            qualifications[i][j + offset] = first;
                                        }
                                    }
                                }
                            }
                        }*/
                    }
                }



                for (int j = 0; j< _round.groupsCount; j++)
                {
                    foreach (Qualification q in qualifications[j])
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

                            int index = q.ranking - 1;

                            SolidColorBrush color = Application.Current.TryFindResource(couleur) as SolidColorBrush;
                            int nbChildrenParPoule = (_round.clubs.Count / _round.groupsCount) + 1;
                            index++;

                            StackPanel sp = (spRanking.Children[j * nbChildrenParPoule + index] as StackPanel);
                            sp.Background = color;
                        }

                    }
                }
                
            }

        }

        private void clubNameButtonClick(Club c)
        {
            if (c != null && c as CityClub != null)
            {
                Windows_Club wc = new Windows_Club(c as CityClub);
                wc.Show();
            }
        }
    }
}
