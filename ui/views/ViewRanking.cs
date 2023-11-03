using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;
using TheManager_GUI.Styles;

namespace TheManager_GUI.Views
{
    public abstract class ViewRanking : View
    {

        protected readonly Tournament _tournament;
        protected readonly int _year;
        protected readonly double _sizeMultiplier;
        protected readonly int _absoluteYear;
        protected readonly bool _reduced;
        protected readonly RankingType _rankingType;

        protected readonly bool _focusOnTeam;
        protected readonly Club _team;

        protected readonly List<Club>[] _retrogradations;


        protected readonly Dictionary<Tournament, Club> _cupsWinners = new Dictionary<Tournament, Club>();
        protected readonly Club _championshipTitleHolder;

        protected Dictionary<Club, Qualification> _continentalClubs;

        public abstract Round Round();

        public ViewRanking(Round round, Tournament tournament, bool reduced, double sizeMultiplier, RankingType rankingType, bool focusOnTeam, Club team)
        {
            _reduced = reduced;
            _tournament = tournament;
            _rankingType = rankingType;
            _sizeMultiplier = sizeMultiplier;
            _focusOnTeam = focusOnTeam;
            _team = team;


            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(_tournament);
            Country country = localisation as Country;
            _retrogradations = country != null ? country.GetAdministrativeRetrogradations() : new List<Club>[0];

            _year = -1;
            foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
            {
                foreach (KeyValuePair<int, Tournament> kvp in t.previousEditions)
                {
                    if (kvp.Value == _tournament)
                    {
                        _year = kvp.Key;
                    }
                }
            }

            //Get title holder
            _absoluteYear = _year > -1 ? _year : Session.Instance.Game.CurrentSeason;

            if (tournament.isChampionship)
            {
                //Get cups winner to add an annotation
                foreach (Tournament cup in country.Cups())
                {
                    if (cup.parent.Value == null)
                    {
                        _cupsWinners.Add(cup, _year > -1 ? cup.previousEditions[_year].Winner() : cup.Winner());
                    }
                }
                _championshipTitleHolder = country.FirstDivisionChampionship().previousEditions.ContainsKey(_absoluteYear - 1) ? country.FirstDivisionChampionship().previousEditions[_absoluteYear - 1].Winner() : null;
            }
            _continentalClubs = GetContinentalClubs(round);
        }

        protected Dictionary<Club, Qualification> GetContinentalClubs(Round round)
        {
            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(_tournament);
            Country country = localisation as Country;
            Dictionary<Club, Qualification> continentalClubs = new Dictionary<Club, Qualification>();
            if (country != null)
            {
                int weekStartContinental = country.Continent.GetContinentalClubTournaments().First().rounds.First().programmation.initialisation.WeekNumber;
                int weekEndContinental = country.Continent.GetContinentalClubTournaments().First().rounds.Last().programmation.end.WeekNumber;
                int continentalYear = _year; //International tournament edition where clubs are qualified
                //If the domestic league calendar is not the same than continental association calendar (civil year vs rolling year), clubs are qualified for international tournaments playing one year after
                if (_tournament.rounds.Last().programmation.end.WeekNumber > weekStartContinental)
                {
                    continentalYear += 1;
                }
                //If the international tournament is set on a civil year
                if (weekEndContinental < weekStartContinental)
                {
                    continentalYear += 1;
                }
                if (round == _tournament.GetLastChampionshipRound())
                {
                    continentalClubs = _year > -1 ? country.Continent.GetClubsQualifiedForInternationalCompetitions(country, continentalYear) : country.Continent.GetClubsQualifiedForInternationalCompetitions(country, true);
                }
            }
            Console.WriteLine("Continental clubs : " + continentalClubs.Count);
            return continentalClubs;
        }

        protected void PrintSanctions(StackPanel spHost, Round round, double sizeMultiplier)
        {
            foreach (Club c in round.clubs)
            {
                int pointsDeduction = round.GetPointsDeduction(c);
                if (pointsDeduction > 0)
                {
                    List<SanctionType> clubSanctions = new List<SanctionType>();
                    if(round.pointsDeduction.ContainsKey(c))
                    {
                        foreach (PointDeduction pd in round.pointsDeduction[c])
                        {
                            if (!clubSanctions.Contains(pd.sanctionType))
                            {
                                clubSanctions.Add(pd.sanctionType);
                            }
                        }
                    }
                    string reasons = "";
                    foreach (SanctionType st in clubSanctions)
                    {
                        reasons = String.Format("{0}, {1}", reasons, st.ToString());
                    }
                    reasons = reasons.Length > 2 ? reasons.Remove(0, 2) : reasons;
                    spHost.Children.Add(ViewUtils.CreateTextBlock(String.Format("{0} : {1} points ({2})", c.name, -pointsDeduction, reasons), StyleDefinition.styleTextPlain, (int)(14 * sizeMultiplier), -1));
                }
            }
            Country ctry = Session.Instance.Game.kernel.LocalisationTournament(round.Tournament) as Country;
            if(ctry != null)
            {
                foreach(Club c in round.clubs)
                {
                    if(ctry.GetRetrogradations().Keys.ToList().Contains(c))
                    {
                        spHost.Children.Add(ViewUtils.CreateTextBlock(String.Format("{0} retrogradé en {1}", c.name, ctry.GetRetrogradations()[c].name), StyleDefinition.styleTextPlain, (int)(14 * sizeMultiplier), -1));
                    }
                }
            }
        }

        public abstract override void Full(StackPanel spRanking);

        protected Border CreateBorder()
        {
            Border border = new Border();
            border.CornerRadius = new CornerRadius(2);
            border.Padding = new Thickness(2);
            border.BorderBrush = Brushes.Transparent;
            return border;
        }

        private string InternationalQualificationColor(Qualification q)
        {
            string color = "";
            if (q.tournament.level == 1 && q.tournament.rounds[q.roundId] as GroupsRound != null)
            {
                color = StyleDefinition.slotQualification1a;
            }
            else if (q.tournament.level == 1 && q.tournament.rounds[q.roundId] as GroupsRound == null)
            {
                color = StyleDefinition.slotQualification1b;
            }
            else if (q.tournament.level == 2 && q.tournament.rounds[q.roundId] as GroupsRound != null)
            {
                color = StyleDefinition.slotQualification2a;
            }
            else if (q.tournament.level == 2 && q.tournament.rounds[q.roundId] as GroupsRound == null)
            {
                color = StyleDefinition.slotQualification2b;
            }
            else if (q.tournament.level == 3)
            {
                color = StyleDefinition.slotQualification3a;
            }
            return color;
        }

        private SolidColorBrush GetQualificationColor(Qualification q, List<Club> clubs, int qualificationsToTournamentNextRounds)
        {
            int index = q.ranking > 0 ? q.ranking - 1 : clubs.Count + q.ranking;
            Club club = clubs[index];
            int clubNextLevel = Array.FindIndex(_retrogradations, w => w.Contains(club)) + 1;
            int roundLevel = _tournament.level;
            bool nationalTeamTournament = Round().clubs.Count > 0 && ((Round().clubs[0] as NationalTeam) != null);
            nationalTeamTournament = nationalTeamTournament || Session.Instance.Game.kernel.LocalisationTournament(_tournament) as Country == null; //Include all international tournaments

            string color = StyleDefinition.solidColorBrushColorTransparent;

            if (q.tournament.isChampionship)
            {
                if (q.tournament.level < roundLevel || (clubNextLevel > 0 && clubNextLevel < roundLevel))
                {
                    color = StyleDefinition.slotPromotion;
                }
                else if ((clubNextLevel - roundLevel) > 1)
                {
                    color = StyleDefinition.slotRetrogradation;
                }
                else if (q.tournament.level > roundLevel)
                {
                    color = clubNextLevel == roundLevel ? StyleDefinition.slotBackground : StyleDefinition.slotRelegation; //Don't forget case were club is rescued
                }
                else if (q.tournament.level == roundLevel && q.roundId > _tournament.rounds.IndexOf(Round()))
                {
                    color = StyleDefinition.slotBarrage;
                }
            }
            else if (nationalTeamTournament)
            {
                if (q.tournament.level == roundLevel && q.tournament != _tournament)
                {
                    color = StyleDefinition.slotQualification1a;
                }
                else if (q.tournament.level == roundLevel && q.tournament == _tournament)
                {
                    color = StyleDefinition.slotQualification1b;
                }
                else if (q.tournament.level > roundLevel)
                {
                    color = StyleDefinition.slotBarrageRelegation;
                }
            }

            SolidColorBrush lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
            int counter = 0;
            foreach (KeyValuePair<Club, Qualification> kvp in _continentalClubs)
            {
                counter++;
                if (club == kvp.Key && counter > qualificationsToTournamentNextRounds)
                {
                    color = InternationalQualificationColor(kvp.Value);
                    if(color != "")
                    {
                        lineColor = Application.Current.TryFindResource(color) as SolidColorBrush;
                    }
                }
            }
            return lineColor;
        }

        protected bool QualificationCanLeadToRelegation(Round r, Tournament tournamentReference)
        {
            bool res = false;
            foreach (Qualification q in r.qualifications)
            {
                if (q.isNextYear && q.tournament.level > tournamentReference.level)
                {
                    res = true;
                }
            }
            if (!res)
            {
                foreach (Qualification q in r.qualifications)
                {
                    if (!q.isNextYear && q.tournament == tournamentReference && !q.tournament.IsInternational() && q.tournament.isChampionship)
                    {
                        res = QualificationCanLeadToRelegation(q.tournament.rounds[q.roundId], tournamentReference);
                    }
                }
            }

            return res;
        }

        protected void InitColumns(Grid grid)
        {
            int cols = _reduced ? 6 : 11;
            float[] colsWidths = _reduced ? new float[] { 8, 10, 70, 10, 10, 10 } : new float[] { 8, 10, 70, 10, 10, 10, 10, 10, 10, 10, 10 };
            for (int col = 0; col < cols; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(colsWidths[col], GridUnitType.Star) });
            }
        }

        protected void FillRanking(Grid grid, int startRow, List<Club> ranking, List<Qualification> qualifications, bool isRankingByRank)
        {
            double fontSize = (double)Application.Current.FindResource(StyleDefinition.fontSizeRegular);
            double logoSize = fontSize * 5 / 3;
            int rows = 1 + ranking.Count; //Rows numbers
            for (int row = 0; row < rows; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(fontSize * 1.8, GridUnitType.Pixel) });
            }

            int i = 0;

            List<Border> borders = new List<Border>();

            // Get international qualifications
            // Search if the round is an archived round to get qualified teams on the right year
            // Else get qualification for the current season
            ILocalisation localisation = Session.Instance.Game.kernel.LocalisationTournament(_tournament);
            Country country = localisation as Country;

            //If we choose to focus on a team, we center the ranking on the team and +-2 other teams around
            int indexTeam = -1;
            if (_focusOnTeam && _team != null)
            {
                List<Club> clubs = new List<Club>();
                //TODO: Center existing ranking on club
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

            AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_team").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 0, 3);
            AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_points").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 3);
            AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_played").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 4);
            if (!_reduced)
            {
                AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_wins").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 5);
                AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_draws").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 6);
                AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_loses").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 7);
                AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_goals_for").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 8);
                AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_goals_against").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, 9);
            }
            AddElementToGrid(grid, ViewUtils.CreateTextBlock(Application.Current.FindResource("str_ranking_goals_diff").ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier), startRow, _reduced ? 5 : 10);

            foreach (Club c in ranking)
            {
                i++;

                Border borderRanking = CreateBorder();
                TextBlock tbRanking = ViewUtils.CreateTextBlock(i.ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                borderRanking.Child = tbRanking;
                AddElementToGrid(grid, borderRanking, startRow+i, 0);
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
                AddElementToGrid(grid, imageRanking, startRow+i, 1);

                StringBuilder cupWinnerStr = new StringBuilder("");
                foreach (KeyValuePair<Tournament, Club> kvp in _cupsWinners)
                {
                    if (kvp.Value == c)
                    {
                        cupWinnerStr.Append(" (").Append(kvp.Key.shortName).Append(") ");
                    }
                }
                if (_championshipTitleHolder == c)
                {
                    cupWinnerStr.Append(" (TT) ");
                }
                TextBlock tbClub = ViewUtils.CreateTextBlockOpenWindow<Club>(c, OpenClub, (_tournament.isChampionship ? c.extendedName(_tournament, _absoluteYear - 1) : c.shortName) + cupWinnerStr.ToString(), StyleDefinition.styleTextPlain, fontSize * _sizeMultiplier, -1);
                AddElementToGrid(grid, tbClub, startRow + i, 2);
                TextBlock tbPoints = ViewUtils.CreateTextBlock(Round().Points(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier, -1, null, null, true);
                AddElementToGrid(grid, tbPoints, startRow + i, 3);
                TextBlock tbPlayed = ViewUtils.CreateTextBlock(Round().Played(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                AddElementToGrid(grid, tbPlayed, startRow + i, 4);
                if (!_reduced)
                {
                    TextBlock tbWins = ViewUtils.CreateTextBlock(Round().Wins(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(grid, tbWins, startRow + i, 5);
                    TextBlock tbDraws = ViewUtils.CreateTextBlock(Round().Draws(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(grid, tbDraws, startRow + i, 6);
                    TextBlock tbLoses = ViewUtils.CreateTextBlock(Round().Loses(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(grid, tbLoses, startRow + i, 7);
                    TextBlock tbGoalsFor = ViewUtils.CreateTextBlock(Round().GoalsFor(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(grid, tbGoalsFor, startRow + i, 8);
                    TextBlock tbGoalsAgainst = ViewUtils.CreateTextBlock(Round().GoalsAgainst(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                    AddElementToGrid(grid, tbGoalsAgainst, startRow + i, 9);
                }
                TextBlock tbGoalsDifference = ViewUtils.CreateTextBlock(Round().Difference(c, _rankingType).ToString(), StyleDefinition.styleTextPlainCenter, fontSize * _sizeMultiplier);
                AddElementToGrid(grid, tbGoalsDifference, startRow + i, _reduced ? 5 : 10);
            }

            int level = _tournament.level;

            //Only show colors when the ranking is not focused on a team
            if (!_focusOnTeam)
            {
                if(ranking.Count > 0)
                {
                    //Teams qualified for next rounds in this tournament
                    int qualificationsToTournamentNextRounds = qualifications.Count(q => !q.isNextYear && q.tournament == _tournament && !QualificationCanLeadToRelegation(q.tournament.rounds[q.roundId], _tournament));
                    foreach (Qualification q in qualifications)
                    {
                        int index = q.ranking > 0 ? q.ranking - 1 : ranking.Count + q.ranking;
                        SolidColorBrush color = GetQualificationColor(q, ranking, qualificationsToTournamentNextRounds);
                        borders[index].Background = color;
                    }
                    if(!isRankingByRank)
                    {
                        foreach (Qualification q in Round().qualifications)
                        {
                            if (q.qualifies != 0)
                            {
                                int index = q.ranking > 0 ? q.ranking - 1 : ranking.Count + q.ranking;
                                string hypoteticalQualification = InternationalQualificationColor(q);
                                if (hypoteticalQualification != "")
                                {
                                    SolidColorBrush color = Application.Current.TryFindResource(hypoteticalQualification) as SolidColorBrush;
                                    borders[index].Background = color;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                SolidColorBrush color = new SolidColorBrush((System.Windows.Media.Color)Application.Current.TryFindResource("ColorDate"));
                color.Opacity = 0.6;
                borders[indexTeam].Background = color;
            }
        }

    }
}
