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
    public class ViewRankingGroups : IViewRanking
    {

        private readonly DataGrid _grille;
        private readonly GroupsRound _tour;
        private readonly bool _focusOnTeam;
        private readonly double _sizeMultiplier;
        private readonly Club _team;

        public ViewRankingGroups(DataGrid grille, GroupsRound tour, double sizeMultiplier, bool focusOnTeam, Club team)
        {
            _grille = grille;
            _tour = tour;
            _focusOnTeam = focusOnTeam;
            _sizeMultiplier = sizeMultiplier;
            _team = team;
        }

        private StackPanel CreateRanking(int index, Club c)
        {
            StackPanel sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;

            int fontBase = (int)(14 * _sizeMultiplier);
            Label l1 = ViewUtils.CreateLabel(index.ToString(), "StyleLabel2", fontBase, 30);
            sp.Children.Add(l1);

            Image image = new Image();
            image.Source = new BitmapImage(new Uri(Utils.Logo(c)));
            image.Width = 30 * _sizeMultiplier;
            sp.Children.Add(image);

            Label l2 = ViewUtils.CreateLabel(c.shortName, "StyleLabel2", fontBase, 150 * _sizeMultiplier);
            l2.MouseLeftButtonUp += (object sender, System.Windows.Input.MouseButtonEventArgs e) =>
            { clubNameButtonClick(c); };
            Label l3 = ViewUtils.CreateLabel(_tour.Points(c).ToString(), "StyleLabel2", fontBase, 25);
            Label l4 = ViewUtils.CreateLabel(_tour.Played(c).ToString(), "StyleLabel2", fontBase, 25);
            Label l5 = ViewUtils.CreateLabel(_tour.Wins(c).ToString(), "StyleLabel2", fontBase, 25);
            Label l6 = ViewUtils.CreateLabel(_tour.Draws(c).ToString(), "StyleLabel2", fontBase, 25);
            Label l7 = ViewUtils.CreateLabel(_tour.Loses(c).ToString(), "StyleLabel2", fontBase, 25);
            Label l8 = ViewUtils.CreateLabel(_tour.GoalsFor(c).ToString(), "StyleLabel2", fontBase, 25);
            Label l9 = ViewUtils.CreateLabel(_tour.GoalsAgainst(c).ToString(), "StyleLabel2", fontBase, 25);
            Label l10 = ViewUtils.CreateLabel(_tour.Difference(c).ToString(), "StyleLabel2", fontBase, 25);


            sp.Children.Add(l2);
            sp.Children.Add(l3);
            sp.Children.Add(l4);
            sp.Children.Add(l5);
            sp.Children.Add(l6);
            sp.Children.Add(l7);
            sp.Children.Add(l8);
            sp.Children.Add(l9);
            sp.Children.Add(l10);

            return sp;

        }

        public void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            //If focusing on a team, only show five teams around the current team
            if (_focusOnTeam)
            {
                List<Club> ranking = null;
                for(int group = 0; group < _tour.groupsCount; group++)
                {
                    if (_tour.groups[group].Contains(_team))
                    {
                        ranking = _tour.Ranking(group);
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
                for (int poule = 0; poule < _tour.groupsCount; poule++)
                {
                    Label labelPoule = new Label();
                    labelPoule.Content = _tour.GroupName(poule);
                    labelPoule.Style = Application.Current.FindResource("StyleLabel1") as Style;
                    labelPoule.FontSize *= _sizeMultiplier;
                    spRanking.Children.Add(labelPoule);
                    int i = 0;
                    foreach (Club c in _tour.Ranking(poule))
                    {
                        i++;
                        spRanking.Children.Add(CreateRanking(i, c));
                    }
                }

            }


            //Only show qualification if teams were dispatched in groups (if not useless to show qualifications color) and if we are not focusing on a team
            if (_tour.groups[0].Count > 0 && !_focusOnTeam)
            {

                //Split qualifications in several list because according to group qualifications can be differents (if reserves are not promoted for instance)

                List<Qualification>[] qualifications = new List<Qualification>[_tour.groupsCount];

                List<Club>[] groups = new List<Club>[_tour.groupsCount];
                for (int i = 0; i < _tour.groupsCount; i++)
                {
                    groups[i] = new List<Club>(_tour.Ranking(i));
                }

                for (int i = 0; i< _tour.groupsCount; i++)
                {
                    qualifications[i] = new List<Qualification>(_tour.qualifications);
                    qualifications[i].Sort(new QualificationComparator());

                    //If reserves can't be promoted
                    if (_tour.rules.Contains(Rule.ReservesAreNotPromoted))
                    {
                        qualifications[i] = Utils.AdjustQualificationsToNotPromoteReserves(qualifications[i], groups[i], _tour.Tournament);
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



                for (int j = 0; j<_tour.groupsCount; j++)
                {
                    foreach (Qualification q in qualifications[j])
                    {
                        if (q.tournament.isChampionship)
                        {
                            int niveau = _tour.Tournament.level;
                            string couleur = "backgroundColor";
                            if (q.tournament.level < niveau)
                            {
                                couleur = "promotionColor";
                            }
                            else if (q.tournament.level > niveau)
                            {
                                couleur = "relegationColor";
                            }
                            else if (q.tournament.level == niveau && q.roundId > _tour.Tournament.rounds.IndexOf(_tour))
                            {
                                couleur = "barrageColor";
                            }

                            int index = q.ranking - 1;

                            SolidColorBrush color = Application.Current.TryFindResource(couleur) as SolidColorBrush;
                            int nbChildrenParPoule = (_tour.clubs.Count / _tour.groupsCount) + 1;
                            index++;

                            StackPanel sp = (spRanking.Children[j * nbChildrenParPoule + index] as StackPanel);
                            sp.Background = color;
                        }

                    }
                }
                
            }

        }

        public void Show()
        {
            _grille.Items.Clear();
            for (int poules = 0; poules < _tour.groupsCount; poules++)
            {
                List<Club> poule = new List<Club>(_tour.groups[poules]);
                poule.Sort(new ClubRankingComparator(_tour.matches));
                int i = 0;
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                _grille.Items.Add(new ClassementElement { Classement = i, Nom = "Poule " + (int)(poules + 1) });
                _grille.Items.Add(new ClassementElement { Classement = 0, Nom = "" });
                foreach (Club c in poule)
                {
                    i++;
                    _grille.Items.Add(new ClassementElement { Logo = Utils.Logo(c), Classement = i, Nom = c.shortName, Pts = _tour.Points(c), J = _tour.Played(c), G = _tour.Wins(c), N = _tour.Draws(c), P = _tour.Loses(c), bp = _tour.GoalsFor(c), bc = _tour.GoalsAgainst(c), Diff = _tour.Difference(c) });
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
