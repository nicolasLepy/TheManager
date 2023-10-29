using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TheManager;
using TheManager.Comparators;
using System.Windows.Media.Imaging;
using TheManager_GUI.Styles;
using System.Linq;

namespace TheManager_GUI.Views
{
    public class ViewRankingKnockout : View
    {

        private readonly KnockoutRound _round;
        private readonly double _sizeMultiplier;

        public ViewRankingKnockout(KnockoutRound round, double sizeMultiplier)
        {
            _round = round;
            _sizeMultiplier = sizeMultiplier;
        }

        public void FillOpposition(Grid grid, List<Match> pair, bool isInternational, bool nationalCup, int row)
        {
            Image homeLogo = null;
            Image awayLogo = null;
            Match lastMatch = pair.Last();
            bool lastMatchInversed = pair.Count == 2;
            if (isInternational)
            {
                homeLogo = ViewUtils.CreateFlag(pair[0].home.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier);
                awayLogo = ViewUtils.CreateFlag(pair[0].away.Country(), 15 * _sizeMultiplier, 15 * _sizeMultiplier);
            }
            else
            {
                homeLogo = ViewUtils.CreateLogo(pair[0].home, 15 * _sizeMultiplier, 15 * _sizeMultiplier);
                awayLogo = ViewUtils.CreateLogo(pair[0].away, 15 * _sizeMultiplier, 15 * _sizeMultiplier);
            }
            AddElementToGrid(grid, homeLogo, row, 0);
            AddElementToGrid(grid, awayLogo, row+1, 0);

            if (!isInternational && !_round.Tournament.isChampionship && pair[0].home.Championship != null && pair[0].away.Championship != null)
            {
                TextBlock tbChampionshipHome = ViewUtils.CreateTextBlock(pair[0].home.Championship.shortName, StyleDefinition.styleTextPlain, 10 * _sizeMultiplier, 30 * _sizeMultiplier);
                TextBlock tbChampionshipAway = ViewUtils.CreateTextBlock(pair[0].away.Championship.shortName, StyleDefinition.styleTextPlain, 10 * _sizeMultiplier, 30 * _sizeMultiplier);
                AddElementToGrid(grid, tbChampionshipHome, row, 1);
                AddElementToGrid(grid, tbChampionshipAway, row+1, 1);
            }

            TextBlock tbHomeName = ViewUtils.CreateTextBlock(pair[0].home.name, StyleDefinition.styleTextPlain, -1, -1, null, null, lastMatch.Winner == pair[0].home ? true : false);
            TextBlock tbAwayName = ViewUtils.CreateTextBlock(pair[0].away.name, StyleDefinition.styleTextPlain, -1, -1, null, null, lastMatch.Winner == pair[0].away ? true : false);
            AddElementToGrid(grid, tbHomeName, row, 2);
            AddElementToGrid(grid, tbAwayName, row+1, 2);

            string homeScore1 = pair[0].Played ? pair[0].score1.ToString() : "";
            string awayScore1 = pair[0].Played ? pair[0].score2.ToString() : "";
            TextBlock tbHomeScore1 = ViewUtils.CreateTextBlock(homeScore1, StyleDefinition.styleTextPlainCenter);
            TextBlock tbAwayScore1 = ViewUtils.CreateTextBlock(awayScore1, StyleDefinition.styleTextPlainCenter);
            AddElementToGrid(grid, tbHomeScore1, row, 3);
            AddElementToGrid(grid, tbAwayScore1, row + 1, 3);

            if(pair.Count > 1)
            {
                string homeScore2 = pair[1].Played ? pair[1].score1.ToString() : "";
                string awayScore2 = pair[1].Played ? pair[1].score2.ToString() : "";
                TextBlock tbHomeScore2 = ViewUtils.CreateTextBlock(awayScore2, StyleDefinition.styleTextPlainCenter);
                TextBlock tbAwayScore2 = ViewUtils.CreateTextBlock(homeScore2, StyleDefinition.styleTextPlainCenter);
                AddElementToGrid(grid, tbHomeScore2, row, 4);
                AddElementToGrid(grid, tbAwayScore2, row + 1, 4);
            }
            
            if (lastMatch.prolongations)
            {
                TextBlock tbHomeExtra = ViewUtils.CreateTextBlock(lastMatch.Winner == pair[0].home ? "p." : "", StyleDefinition.styleTextPlainCenter);
                TextBlock tbAwayExtra = ViewUtils.CreateTextBlock(lastMatch.Winner != pair[0].home ? "p." : "", StyleDefinition.styleTextPlainCenter);
                AddElementToGrid(grid, tbHomeExtra, row, 5);
                AddElementToGrid(grid, tbAwayExtra, row + 1, 5);
            }

            if (lastMatch.PenaltyShootout)
            {
                TextBlock tbHomePen = ViewUtils.CreateTextBlock(lastMatchInversed ? lastMatch.penaltyShootout2.ToString() : lastMatch.penaltyShootout1.ToString(), StyleDefinition.styleTextPlainCenter);
                TextBlock tbAwayPen = ViewUtils.CreateTextBlock(lastMatchInversed ? lastMatch.penaltyShootout1.ToString() : lastMatch.penaltyShootout2.ToString(), StyleDefinition.styleTextPlainCenter);
                AddElementToGrid(grid, tbHomePen, row, 6);
                AddElementToGrid(grid, tbAwayPen, row + 1, 6);
            }
        }

        public override void Full(StackPanel spRanking)
        {
            bool internationalTournament = _round.Tournament.IsInternational();
            bool nationalCup = !internationalTournament && !_round.Tournament.isChampionship;


            List<Match> matchs = new List<Match>(_round.matches);
            matchs.Sort(new MatchDateComparator());

            Grid grid = new Grid();
            float[] colsWidths = new float[] { 10, nationalCup ? 10 : 0, 100, 10, 10, 10, 10 };
            for(int col = 0; col < colsWidths.Length; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(colsWidths[col], GridUnitType.Star) });
            }

            List<Match>[] pairs;
            if (_round.twoLegs)
            {
                pairs = new List<Match>[matchs.Count / 2];
                for (int i = 0; i < matchs.Count / 2; i++)
                {
                    pairs[i] = new List<Match>();
                }

                foreach(Match m in matchs)
                {
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

                /*foreach(List<Match> pair in pairs)
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

                    if (pair[1].prolongations)
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
                }*/
            }
            else
            {
                pairs = new List<Match>[matchs.Count];
                int i = 0;
                foreach (Match m in matchs)
                {
                    pairs[i] = new List<Match>() { m };
                    i++;

                    /*StackPanel spFirstTeam = new StackPanel();
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
                    spRanking.Children.Add(new Separator());*/

                }
            }

            for (int row = 0; row < pairs.Length * 3; row++)
            {
                int gridSize = (row + 1) % 3 == 0 ? 20 : 30;
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(gridSize, GridUnitType.Pixel) });
            }


            for (int i = 0; i < pairs.Length; i++)
            {
                FillOpposition(grid, pairs[i], internationalTournament, nationalCup, i * 3);
            }

            spRanking.Children.Clear();
            spRanking.Children.Add(grid);
        }
    }
}
