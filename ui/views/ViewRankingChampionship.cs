using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tm;
using System.Linq;
using System.Text;
using TheManager_GUI.Styles;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;

namespace TheManager_GUI.Views
{
    public class ViewRankingChampionship : ViewRanking
    {

        private readonly ChampionshipRound _round;
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="round"></param>
        /// <param name="sizeMultiplier">Width and font size multiplier</param>
        /// <param name="focusOnTeam">If true, only show 5 rows, focus the ranking around the team</param>
        /// <param name="team">The team to focus ranking on</param>
        public ViewRankingChampionship(ChampionshipRound round, double sizeMultiplier, bool focusOnTeam = false, Club team = null, bool reduced = false, RankingType rankingType = RankingType.General) : base(round, round.Tournament, reduced, sizeMultiplier, rankingType, focusOnTeam, team)
        {
            _round = round;
        }

        public override Round Round()
        {
            return _round;
        }

        public override void Full(StackPanel spRanking)
        {
            spRanking.Children.Clear();

            int i = 0;

            List<Club> clubs = _round.Ranking(_rankingType);
            List<Qualification> qualificationsRound = new List<Qualification>(_round.qualifications);
            List<Border> borders = new List<Border>();
            Grid gridTable = new Grid();
            InitColumns(gridTable);
            FillRanking(gridTable, 0, clubs, qualificationsRound, false);

            /*// Get international qualifications
            // Search if the round is an archived round to get qualified teams on the right year
            // Else get qualification for the current season
            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(_tournament);
            Country country = localisation as Country;

            Dictionary<Club, Qualification> continentalClubs = new Dictionary<Club, Qualification>();
            if(country != null)
            {
                int weekStartContinental = country.Continent.GetContinentalClubTournaments().First().rounds.First().programmation.initialisation.WeekNumber;
                int weekEndContinental = country.Continent.GetContinentalClubTournaments().First().rounds.Last().programmation.end.WeekNumber;
                int continentalYear = _year; //International tournament edition where clubs are qualified
                //If the domestic league calendar is not the same than continental association calendar (civil year vs rolling year), clubs are qualified for international tournaments playing one year after
                if (_round.Tournament.rounds.Last().programmation.end.WeekNumber > weekStartContinental)
                {
                    continentalYear += 1;
                }
                //If the international tournament is set on a civil year
                if(weekEndContinental < weekStartContinental)
                {
                    continentalYear += 1;
                }

                if(_round == _tournament.GetLastChampionshipRound())
                {
                    continentalClubs = _year > -1 ? country.Continent.GetClubsQualifiedForInternationalCompetitions(country, continentalYear) : country.Continent.GetClubsQualifiedForInternationalCompetitions(country, true);
                }
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

            double fontSize = (double)Application.Current.FindResource(StyleDefinition.fontSizeRegular);
            double logoSize = fontSize * 5 / 3;
            
            int rows = 1 + clubs.Count; //Rows numbers
            for(int row = 0; row < rows; row++)
            {
                gridTable.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(fontSize * 1.8, GridUnitType.Pixel) });
            }
            int cols = _reduced ? 6 : 11;
            float[] colsWidths = _reduced ? new float[] {8, 10, 70, 10, 10, 10} : new float[] {8, 10, 70, 10, 10, 10, 10, 10, 10, 10, 10};
            for(int col = 0; col < cols; col++)
            {
                gridTable.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(colsWidths[col], GridUnitType.Star) });
            }
            AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("Equipe", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 0, 3);
            AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("Pts", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 3);
            AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("J", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 4);
            if(!_reduced)
            {
                AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("G", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 5);
                AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("N", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 6);
                AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("P", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 7);
                AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("p.", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 8);
                AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("c.", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, 9);
            }
            AddElementToGrid(gridTable, ViewUtils.CreateTextBlock("Diff", StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), 0, _reduced ? 5 : 10);

            foreach (Club c in clubs)
            {
                Console.WriteLine("Create " + c.name);
                i++;

                Border borderRanking = CreateBorder();
                TextBlock tbRanking = ViewUtils.CreateTextBlock(i.ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                borderRanking.Child = tbRanking;
                AddElementToGrid(gridTable, borderRanking, i, 0);
                borders.Add(borderRanking);

                Image imageRanking = null;
                if (_tournament.IsInternational() && (c as CityClub) != null)
                {
                    imageRanking = ViewUtils.CreateFlag((c as CityClub).city.Country(), logoSize, logoSize);

                }
                else if (_tournament.IsInternational() && (c as ReserveClub) != null)
                {
                    imageRanking = ViewUtils.CreateFlag((c as ReserveClub).FannionClub.city.Country(), logoSize, logoSize);
                }
                else
                {
                    imageRanking = ViewUtils.CreateLogo(c, logoSize, logoSize);
                }
                AddElementToGrid(gridTable, imageRanking, i, 1);

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
                TextBlock tbClub = ViewUtils.CreateTextBlockOpenWindow<Club>(c, OpenClub, (_tournament.isChampionship ? c.extendedName(_tournament, _absoluteYear-1) : c.shortName) + cupWinnerStr.ToString(), StyleDefinition.styleTextPlain, fontSize * _sizeMultiplier, -1);
                AddElementToGrid(gridTable, tbClub, i, 2);
                TextBlock tbPoints = ViewUtils.CreateTextBlock(_round.Points(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier, -1, null, null, true);
                AddElementToGrid(gridTable, tbPoints, i, 3);
                TextBlock tbPlayed = ViewUtils.CreateTextBlock(_round.Played(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                AddElementToGrid(gridTable, tbPlayed, i, 4);
                if (!_reduced)
                {
                    TextBlock tbWins = ViewUtils.CreateTextBlock(_round.Wins(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(gridTable, tbWins, i, 5);
                    TextBlock tbDraws = ViewUtils.CreateTextBlock(_round.Draws(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(gridTable, tbDraws, i, 6);
                    TextBlock tbLoses = ViewUtils.CreateTextBlock(_round.Loses(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(gridTable, tbLoses, i, 7);
                    TextBlock tbGoalsFor = ViewUtils.CreateTextBlock(_round.GoalsFor(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(gridTable, tbGoalsFor, i, 8);
                    TextBlock tbGoalsAgainst = ViewUtils.CreateTextBlock(_round.GoalsAgainst(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(gridTable, tbGoalsAgainst, i, 9);
                }
                TextBlock tbGoalsDifference = ViewUtils.CreateTextBlock(_round.Difference(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                AddElementToGrid(gridTable, tbGoalsDifference, i, _reduced ? 5 : 10);
            }

            List<Club>[] retrogradations = country != null ? country.GetAdministrativeRetrogradations() : new List<Club>[0];
            int level = _tournament.level;

            //Only show colors when the ranking is not focused on a team
            if (!_focusOnTeam)
            {
                int roundLevel = _tournament.level;

                bool nationalTeamTournament = _round.clubs.Count > 0 && ((_round.clubs[0] as NationalTeam) != null);
                int qualificationsToTournamentNextRounds = 0;
                List<Qualification> qualifications = _round.GetAdjustedQualifications();
                foreach (Qualification q in qualifications)
                {
                    int index = q.ranking > 0 ? q.ranking - 1: clubs.Count + q.ranking;
                    int clubNextLevel = Array.FindIndex(retrogradations, w => w.Contains(clubs[index])) + 1;

                    string color = "backgroundColor";

                    qualificationsToTournamentNextRounds = !q.isNextYear && q.tournament == _tournament && !QualificationCanLeadToRelegation(q.tournament.rounds[q.roundId], _tournament) ? qualificationsToTournamentNextRounds + 1 : qualificationsToTournamentNextRounds;
                    if (q.tournament.isChampionship)
                    {
                        if (q.tournament.level < roundLevel || (clubNextLevel > 0 && clubNextLevel < roundLevel))
                        {
                            color = "promotionColor";
                        }
                        else if ((clubNextLevel - roundLevel) > 1)
                        {
                            color = "retrogradationColor";
                        }
                        else if (q.tournament.level > roundLevel)
                        {
                            color = clubNextLevel == roundLevel ? "backgroundColor" : "relegationColor"; //Don't forget case were club is rescued
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

                    if (color != "backgroundColor" && clubs.Count > 0)
                    {
                        SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                        borders[index].Background = lineColor;
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
                        borders[indexClub].Background = lineColor;
                    }
                }
            }
            else
            {
                SolidColorBrush color = new SolidColorBrush((System.Windows.Media.Color)Application.Current.TryFindResource("ColorDate"));
                color.Opacity = 0.6;
                borders[indexTeam].Background = color;
            }*/
            spRanking.Children.Add(gridTable);
            if(!_focusOnTeam)
            {
                PrintSanctions(spRanking, _round, _sizeMultiplier);
            }
        }
    }
}