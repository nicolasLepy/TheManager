using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TheManager;


/*
 * Trying to procedurally reproduces
 * 
 * <DataGrid IsReadOnly="True" MouseDoubleClick="DgClubProchainsMatchs_MouseDoubleClick" Height="100" Name="dgClubProchainsMatchs" Style="{StaticResource StyleDataGrid}" CellStyle="{StaticResource StyleDataCellGrid}" ColumnHeaderStyle="{StaticResource StyleDataHeaderGrid}">
<DataGrid.Columns>
	<DataGridTextColumn Binding="{Binding ShortName}" Width="3*">
		<DataGridTextColumn.ElementStyle>
			<Style TargetType="{x:Type TextBlock}">
				<Setter Property="Background" Value="{Binding Competition, Converter={StaticResource DataGridTournamentColumnConverter}}"/>
				<Setter Property="Foreground" Value="AntiqueWhite"/>
			</Style>
		</DataGridTextColumn.ElementStyle>
	</DataGridTextColumn>
	<DataGridTemplateColumn Width="2*">
		<DataGridTemplateColumn.CellTemplate>
			<DataTemplate>
				<Image Source="{Binding LogoD}" />
			</DataTemplate>
		</DataGridTemplateColumn.CellTemplate>
	</DataGridTemplateColumn>
	<DataGridTextColumn Header="" Binding="{Binding Equipe1}" FontStyle="Italic" Width="9*">
		<DataGridTextColumn.ElementStyle>
			<Style TargetType="TextBlock">
				<Setter Property="HorizontalAlignment" Value="Center"/>
				<Setter Property="FontStyle" Value="Italic"/>
			</Style>
		</DataGridTextColumn.ElementStyle>
	</DataGridTextColumn>
	<DataGridTextColumn Header="" Binding="{Binding Score}" Width="5*">
		<DataGridTextColumn.ElementStyle>
			<Style TargetType="TextBlock">
				<Setter Property="HorizontalAlignment" Value="Center"/>
				<Setter Property="Foreground" Value="LightGray"/>
			</Style>
		</DataGridTextColumn.ElementStyle>
	</DataGridTextColumn>
	<DataGridTextColumn Header="" Binding="{Binding Equipe2}" FontStyle="Italic" Width="9*">
		<DataGridTextColumn.ElementStyle>
			<Style TargetType="TextBlock">
				<Setter Property="HorizontalAlignment" Value="Center"/>
				<Setter Property="FontStyle" Value="Italic"/>
			</Style>
		</DataGridTextColumn.ElementStyle>
	</DataGridTextColumn>
	<DataGridTemplateColumn Width="2*">
		<DataGridTemplateColumn.CellTemplate>
			<DataTemplate>
				<Image Source="{Binding LogoE}" />
			</DataTemplate>
		</DataGridTemplateColumn.CellTemplate>
	</DataGridTemplateColumn>
</DataGrid.Columns>
</DataGrid>
 * 
 * 
 */

namespace TheManager_GUI.VueClassement
{

    public struct MatchElement : IEquatable<MatchElement>
    {
        public string Hour { get; set; }
        public Tournament Tournament { get; set; }
        public string ShortName { get; set; }
        public string HomeLogo { get; set; }
        public string AwayLogo { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public string Score { get; set; }
        public int Attendance { get; set; }
        public string Odd1 { get; set; }
        public string OddN { get; set; }
        public string Odd2 { get; set; }
        public TheManager.Match Match { get; set; }
        public bool Equals(MatchElement other)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Fill a datagrid with games data
    /// </summary>
    public class MatchesDataGridView
    {

        private readonly Panel _panel;
        private readonly DataGrid _grid;
        /// <summary>
        /// A special line to show date
        /// </summary>
        private readonly bool _showSeparatedDate;
        private readonly bool _showAttendance;
        private readonly bool _showOdds;
        private readonly bool _showHour;
        private readonly bool _showTournament;
        private readonly List<Match> _matches;

        public MatchesDataGridView(Panel panel, List<Match> matches, bool showHour, bool showDateSeparated, bool showAttendance, bool showOdds, bool showTournament)
        {
            _panel = panel;
            _showTournament = showTournament;
            _showAttendance = showAttendance;
            _showSeparatedDate = showDateSeparated;
            _showOdds = showOdds;
            _showHour = showHour;
            _matches = matches;
            _grid = new DataGrid();
        }

        public void Refresh()
        {

            DataGrid dg = _grid;
            dg.IsReadOnly = true;
            dg.MouseDoubleClick += DgMouseDoubleClick;
            dg.Name = "dgMatches";
            dg.ColumnWidth = DataGridLength.SizeToHeader;
            dg.Style = Application.Current.TryFindResource("StyleDataGrid") as Style;
            dg.CellStyle = Application.Current.TryFindResource("StyleDataCellGrid") as Style;
            dg.ColumnHeaderStyle = Application.Current.TryFindResource("StyleDataHeaderGrid") as Style;

            Binding bind;

            if (_showHour)
            {
                DataGridTextColumn tcHour = new DataGridTextColumn();
                bind = new Binding("Hour");
                tcHour.Binding = bind;
                tcHour.Width = new DataGridLength(4, DataGridLengthUnitType.Star);
                tcHour.ElementStyle = Application.Current.TryFindResource("MatchesDataGridTeamName") as Style;
                dg.Columns.Add(tcHour);
            }

            if (_showTournament)
            {
                DataGridTextColumn tcShortName = new DataGridTextColumn();
                bind = new Binding("ShortName");
                tcShortName.Binding = bind;
                tcShortName.Width = new DataGridLength(3, DataGridLengthUnitType.Star);
                tcShortName.ElementStyle = Application.Current.TryFindResource("MatchesDataGridTournamentName") as Style;
                dg.Columns.Add(tcShortName);
            }

            DataGridTemplateColumn templateLogoHome = new DataGridTemplateColumn();
            templateLogoHome.Width = new DataGridLength(2, DataGridLengthUnitType.Star);
            DataTemplate cellTemplateHome = ViewUtils.CreateImageTemplate("HomeLogo");
            templateLogoHome.CellTemplate = cellTemplateHome;

            DataGridTemplateColumn templateLogoAway = new DataGridTemplateColumn();
            templateLogoAway.Width = new DataGridLength(2, DataGridLengthUnitType.Star);
            DataTemplate cellTemplateAway = ViewUtils.CreateImageTemplate("AwayLogo");
            templateLogoAway.CellTemplate = cellTemplateAway;

            DataGridTextColumn tcHomeTeamName = new DataGridTextColumn();
            bind = new Binding("HomeTeam");
            tcHomeTeamName.Binding = bind;
            tcHomeTeamName.Width = new DataGridLength(9, DataGridLengthUnitType.Star);
            tcHomeTeamName.ElementStyle = Application.Current.TryFindResource("MatchesDataGridTeamName") as Style;

            DataGridTextColumn tcAwayTeamName = new DataGridTextColumn();
            bind = new Binding("AwayTeam");
            tcAwayTeamName.Binding = bind;
            tcAwayTeamName.Width = new DataGridLength(9, DataGridLengthUnitType.Star);
            tcAwayTeamName.ElementStyle = Application.Current.TryFindResource("MatchesDataGridTeamName") as Style;

            DataGridTextColumn tcScore = new DataGridTextColumn();
            bind = new Binding("Score");
            tcScore.Binding = bind;
            tcScore.Width = new DataGridLength(5, DataGridLengthUnitType.Star);
            tcScore.ElementStyle = Application.Current.TryFindResource("MatchesDataGridScore") as Style;

            dg.Columns.Add(templateLogoHome);
            dg.Columns.Add(tcHomeTeamName);
            dg.Columns.Add(tcScore);
            dg.Columns.Add(tcAwayTeamName);
            dg.Columns.Add(templateLogoAway);

            if (_showAttendance)
            {
                DataGridTextColumn tcAttendance = new DataGridTextColumn();
                bind = new Binding("Attendance");
                tcAttendance.Binding = bind;
                tcAttendance.Width = new DataGridLength(5, DataGridLengthUnitType.Star);
                tcAttendance.ElementStyle = Application.Current.TryFindResource("MatchesDataGridScore") as Style;
                dg.Columns.Add(tcAttendance);
            }

            if (_showOdds)
            {
                DataGridTextColumn tcOdd1 = new DataGridTextColumn();
                bind = new Binding("Odd1");
                tcOdd1.Binding = bind;
                tcOdd1.Width = new DataGridLength(3, DataGridLengthUnitType.Star);
                tcOdd1.ElementStyle = Application.Current.TryFindResource("MatchesDataGridScore") as Style;
                DataGridTextColumn tcOddN = new DataGridTextColumn();
                bind = new Binding("OddN");
                tcOddN.Binding = bind;
                tcOddN.Width = new DataGridLength(3, DataGridLengthUnitType.Star);
                tcOddN.ElementStyle = Application.Current.TryFindResource("MatchesDataGridScore") as Style;
                DataGridTextColumn tcOdd2 = new DataGridTextColumn();
                bind = new Binding("Odd2");
                tcOdd2.Binding = bind;
                tcOdd2.Width = new DataGridLength(3, DataGridLengthUnitType.Star);
                tcOdd2.ElementStyle = Application.Current.TryFindResource("MatchesDataGridScore") as Style;
                dg.Columns.Add(tcOdd1);
                dg.Columns.Add(tcOddN);
                dg.Columns.Add(tcOdd2);
            }

            DateTime lastTime = new DateTime(2000, 1, 1);
            foreach (Match m in _matches)
            {    
                if (_showSeparatedDate && lastTime != m.day.Date)
                {
                    dg.Items.Add(new MatchElement { Score = m.day.ToShortDateString() });
                }
                lastTime = m.day.Date;

                string score = m.score1 + " - " + m.score2;
                if (!m.Played)
                {
                    score = m.day.ToShortDateString();
                }
                MatchElement me = new MatchElement { Hour = m.day.ToShortTimeString(), Match = m, HomeTeam = m.home.shortName, AwayTeam = m.away.shortName, Score = score, HomeLogo = Utils.Logo(m.home), AwayLogo = Utils.Logo(m.away), Attendance = m.attendance, Odd1 = m.odd1.ToString("0.00"), OddN = m.oddD.ToString("0.00"), Odd2 = m.odd2.ToString("0.00") };
                if (_showTournament)
                {
                    me.Tournament = m.Tournament;
                    me.ShortName = m.Tournament.shortName;
                }
                //dg.Items.Add(new MatchElement { Hour = m.day.ToShortTimeString(), Match = m, Tournament = m.Tournament, ShortName = m.Tournament.shortName, HomeTeam = m.home.shortName, AwayTeam = m.away.shortName, Score = score, HomeLogo = Utils.Logo(m.home), AwayLogo = Utils.Logo(m.away), Attendance = m.attendance, Odd1 = m.odd1.ToString("0.00"), OddN = m.oddD.ToString("0.00"), Odd2 = m.odd2.ToString("0.00") });
                dg.Items.Add(me);
            }
            _panel.Children.Clear();
            _panel.Children.Add(dg);
            dg.UpdateLayout();

        }

        private void DgMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MatchElement m = (MatchElement)_grid.SelectedItem;
            Windows_Match wm = new Windows_Match(m.Match);
            wm.Show();
        }
    }
}
