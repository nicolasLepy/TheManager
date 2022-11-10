using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TheManager;
using TheManager.Comparators;
using System.Windows.Media.Imaging;

namespace TheManager_GUI.VueClassement
{
    public class ViewRankingElimination : View
    {

        private readonly KnockoutRound _round;
        private readonly double _sizeMultiplier;

        public ViewRankingElimination(KnockoutRound round, double sizeMultiplier)
        {
            _round = round;
            _sizeMultiplier = sizeMultiplier;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();
            List<Match> matchs = new List<Match>(_round.matches);
            matchs.Sort(new MatchDateComparator());

            bool internationalTournament = _round.Tournament.IsInternational();

            if(_round.twoLegs)
            {
                List<Match>[] pairs = new List<Match>[matchs.Count/2];
                for (int i = 0; i < matchs.Count / 2; i++) pairs[i] = new List<Match>();

                foreach(Match m in matchs)
                {
                    Utils.Debug(m.home.ToString() + " - " + m.away.ToString());
                    bool foundPair = false;
                    foreach(List<Match> pair in pairs)
                    {

                        if (pair.Count > 0 && pair[0].away == m.home)
                        {
                            pair.Add(m);
                            foundPair = true;
                        }
                    }
                    if(!foundPair)
                    {
                        int i = 0;
                        List<Match> pair = pairs[i];
                        while(pair.Count > 0)
                        {
                            pair = pairs[++i];
                        }
                        pair.Add(m);
                    }
                }

                foreach(List<Match> pair in pairs)
                {

                    StackPanel spFirstTeam = new StackPanel();
                    spFirstTeam.Orientation = Orientation.Horizontal;
                    StackPanel spSecondTeam = new StackPanel();
                    spSecondTeam.Orientation = Orientation.Horizontal;

                    if(internationalTournament)
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateFlag(pair[0].home.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateFlag(pair[0].away.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                    }
                    else
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateLogo(pair[0].home, 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLogo(pair[0].away, 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                    }

                    if (!internationalTournament && !_round.Tournament.isChampionship)
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateLabel(pair[0].home.Championship.shortName, "StyleLabel2", 10 * _sizeMultiplier, 30 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLabel(pair[0].away.Championship.shortName, "StyleLabel2", 10 * _sizeMultiplier, 30 * _sizeMultiplier));
                    }

                    spFirstTeam.Children.Add(ViewUtils.CreateLabel(pair[0].home.name, "StyleLabel2", 10 * _sizeMultiplier, 225 * _sizeMultiplier, null, null, pair[1].Winner == pair[0].home ? true : false));                    
                    spSecondTeam.Children.Add(ViewUtils.CreateLabel(pair[0].away.name, "StyleLabel2", 10 * _sizeMultiplier, 225 * _sizeMultiplier, null, null, pair[1].Winner == pair[0].away ? true : false));

                    spFirstTeam.Children.Add(ViewUtils.CreateLabel(pair[0].score1.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                    spSecondTeam.Children.Add(ViewUtils.CreateLabel(pair[0].score2.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));

                    spFirstTeam.Children.Add(ViewUtils.CreateLabel(pair[1].score2.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                    spSecondTeam.Children.Add(ViewUtils.CreateLabel(pair[1].score1.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));

                    if(pair[1].prolongations)
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateLabel(pair[1].Winner == pair[0].home ? "p." : "", "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLabel(pair[1].Winner != pair[0].home ? "p." : "", "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                    }

                    if (pair[1].PenaltyShootout)
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateLabel(pair[1].penaltyShootout2.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLabel(pair[1].penaltyShootout1.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                    }

                    spRanking.Children.Add(spFirstTeam);
                    spRanking.Children.Add(spSecondTeam);
                    spRanking.Children.Add(new Separator());
                }
            }
            else
            {
                Dictionary<int, int> clubsByLevel = new Dictionary<int, int>();
                for(int i = 1; i<10; i++)
                {
                    clubsByLevel[i] = 0;
                }
                foreach(Match m in matchs)
                {
                    StackPanel spFirstTeam = new StackPanel();
                    spFirstTeam.Orientation = Orientation.Horizontal;
                    StackPanel spSecondTeam = new StackPanel();
                    spSecondTeam.Orientation = Orientation.Horizontal;

                    if (internationalTournament)
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateFlag(m.home.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateFlag(m.away.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                    }
                    else
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateLogo(m.home, 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLogo(m.away, 15 * _sizeMultiplier, 15 * _sizeMultiplier));
                    }

                    if (!internationalTournament && !_round.Tournament.isChampionship)
                    {
                        clubsByLevel[m.home.Championship.level]++;
                        clubsByLevel[m.away.Championship.level]++;
                        spFirstTeam.Children.Add(ViewUtils.CreateLabel(m.home.Championship.shortName, "StyleLabel2", 10 * _sizeMultiplier, 30 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLabel(m.away.Championship.shortName, "StyleLabel2", 10 * _sizeMultiplier, 30 * _sizeMultiplier));
                    }

                    spFirstTeam.Children.Add(ViewUtils.CreateLabel(m.home.name, "StyleLabel2", 10 * _sizeMultiplier, 225 * _sizeMultiplier, null, null, m.Winner == m.home ? true : false));
                    spSecondTeam.Children.Add(ViewUtils.CreateLabel(m.away.name, "StyleLabel2", 10 * _sizeMultiplier, 225 * _sizeMultiplier, null, null, m.Winner == m.away ? true : false));

                    spFirstTeam.Children.Add(ViewUtils.CreateLabel(m.score1.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                    spSecondTeam.Children.Add(ViewUtils.CreateLabel(m.score2.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));

                    if (m.prolongations)
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateLabel(m.Winner == m.home ? "p." : "", "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLabel(!(m.Winner == m.home) ? "p." : "", "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                    }

                    if (m.PenaltyShootout)
                    {
                        spFirstTeam.Children.Add(ViewUtils.CreateLabel(m.penaltyShootout1.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                        spSecondTeam.Children.Add(ViewUtils.CreateLabel(m.penaltyShootout2.ToString(), "StyleLabel2", 10 * _sizeMultiplier, 25 * _sizeMultiplier));
                    }

                    spRanking.Children.Add(spFirstTeam);
                    spRanking.Children.Add(spSecondTeam);
                    spRanking.Children.Add(new Separator());

                }
            }
        }
    }
}
