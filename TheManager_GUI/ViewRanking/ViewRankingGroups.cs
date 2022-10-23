using System;
using System.Collections.Generic;
using System.Linq;
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


            Label l2 = ViewUtils.CreateLabel(_round.Tournament.isChampionship ? c.extendedName : c.shortName, "StyleLabel2", fontBase, 150 * _sizeMultiplier);
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
            var watch = System.Diagnostics.Stopwatch.StartNew();

            spRanking.Children.Clear();

            //Split qualifications in several list because according to group qualifications can be differents (if reserves are not promoted for instance)
            List<Qualification>[] qualifications = new List<Qualification>[_round.groupsCount];
            if(_round.groups.Length > 0 && _round.groups[0].Count > 0)
            {
                for (int i = 0; i < _round.groupsCount; i++)
                {
                    qualifications[i] = _round.GetGroupQualifications(i);// new List<Qualification>(_round.qualifications);
                    qualifications[i].Sort(new QualificationComparator());
                }
            }


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

                    if (_round.groups[poule].Count > 0)
                    {
                        foreach (Qualification q in qualifications[poule])
                        {
                            string color = "backgroundColor";
                            if (q.tournament.level == roundLevel && q.tournament != _round.Tournament)
                            {
                                color = "cl1Color";
                            }
                            if (q.tournament.level == roundLevel && q.tournament != _round.Tournament && q.qualifies != 0)
                            {
                                color = q.qualifies > 0 ? "cl2Color" : "barrageRelegationColor";
                            }
                            else if (q.tournament.level == roundLevel && q.tournament == _round.Tournament)
                            {
                                color = "cl2Color";
                            }
                            else if (q.tournament.level > roundLevel)
                            {
                                color = q.qualifies >= 0 ? "el1Color" : "barrageRelegationColor";
                            }
                            int index = q.ranking;
                            if (color != "backgroundColor")
                            {
                                SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                                List<Club> rankingPoule = _round.Ranking(poule);
                                if (_round.matches.Count > 0 && index <= rankingPoule.Count)
                                {
                                    (spRanking.Children[spRanking.Children.Count - rankingPoule.Count + index - 1] as StackPanel).Background = lineColor;
                                }
                            }
                        }
                    }
                }

                List<int> rankingDone = new List<int>();
                foreach(Qualification q in _round.qualifications)
                {
                    if(q.qualifies != 0 && _round.matches.Count > 0 && !rankingDone.Contains(q.ranking))
                    {
                        rankingDone.Add(q.ranking);
                        int rankingName = q.ranking > 0 ? q.ranking : _round.groups.Last().Count + q.ranking + 1;
                        spRanking.Children.Add(ViewUtils.CreateLabel("Classement des " + rankingName + "èmes", "StyleLabel2Center", (int)(14 * _sizeMultiplier), -1));
                        List<Club> concernedClubs = new List<Club>();
                        for(int i = 0; i<_round.groupsCount; i++)
                        {
                            int correspondingGroupRanking = q.ranking > 0 ? q.ranking : _round.groups[i].Count + q.ranking + 1;
                            concernedClubs.Add(_round.Ranking(i)[correspondingGroupRanking-1]);
                        }
                        concernedClubs.Sort(new ClubRankingComparator(_round.matches));
                        int j = 0;
                        foreach(Club c in concernedClubs)
                        {
                            j++;
                            StackPanel spLine = CreateRanking(j, c);
                            if((j <= Math.Abs(q.qualifies) && q.qualifies > 0) || (j > concernedClubs.Count-Math.Abs(q.qualifies) && q.qualifies < 0) )
                            {
                                spLine.Background = Application.Current.TryFindResource(q.qualifies > 0 ? "cl2Color" : "relegationColor") as SolidColorBrush;
                            }
                            spRanking.Children.Add(spLine);
                        }
                    }
                }
            }



            //Only show qualification if teams were dispatched in groups (if not useless to show qualifications color) and if we are not focusing on a team
            if (_round.groups[0].Count > 0 && !_focusOnTeam)
            {


                List<Club>[] groups = new List<Club>[_round.groupsCount];
                for (int i = 0; i < _round.groupsCount; i++)
                {
                    groups[i] = new List<Club>(_round.Ranking(i));
                }

                int cumulatedChildrenCount = 0;
                for (int j = 0; j< _round.groupsCount; j++)
                {
                    List<Club> ranking = _round.groups[j];
                    foreach (Qualification q in qualifications[j])
                    {
                        if (q.tournament.isChampionship && q.ranking <= ranking.Count)
                        {
                            int niveau = _round.Tournament.level;
                            string couleur = "backgroundColor";
                            if (q.tournament.level < niveau)
                            {
                                couleur = q.qualifies != 0 ? "el1Color" : "promotionColor";
                            }
                            else if (q.tournament.level > niveau)
                            {
                                couleur = q.qualifies != 0 ? "barrageRelegationColor" : "relegationColor";
                            }
                            else if (q.tournament.level == niveau && q.roundId > _round.Tournament.rounds.IndexOf(_round))
                            {
                                couleur = "barrageColor";
                            }

                            int index = q.ranking; //Ranking index + 1 for the group headline

                            SolidColorBrush color = Application.Current.TryFindResource(couleur) as SolidColorBrush;
                            
                            StackPanel sp = (spRanking.Children[cumulatedChildrenCount + index] as StackPanel);
                            sp.Background = color;
                        }
                    }
                    cumulatedChildrenCount += _round.groups[j].Count +1 ;
                }
                
            }

            watch.Stop();
            Console.WriteLine("Affichage classement :" + watch.ElapsedMilliseconds);
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
