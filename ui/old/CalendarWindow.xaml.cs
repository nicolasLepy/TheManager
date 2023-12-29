using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tm;
using tm.Tournaments;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour CalendarWindow.xaml
    /// </summary>
    public partial class CalendarWindow : Window
    {

        private DateTime _date;
        public CalendarWindow()
        {
            InitializeComponent();

            _date = Session.Instance.Game.date.AddDays(-Session.Instance.Game.date.Day+1);
            imgBtnJourneeGauche.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\left.png"));
            imgBtnJourneeDroite.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\right.png"));
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            Calendar();
        }

        private void Calendar()
        {
            lbDate.Content = _date.ToString("MMMM yyyy");
            spCalendar.Children.Clear();
            StackPanel spLine = new StackPanel();
            spLine.Orientation = Orientation.Horizontal;
            DateTime dateMonth = new DateTime(_date.Year, _date.Month, 1);
            int daysCount = DateTime.DaysInMonth(_date.Year, _date.Month);
            for (int i = 0; i < daysCount; i++)
            {
                if(dateMonth.DayOfWeek == DayOfWeek.Monday && spLine.Children.Count > 0)
                {
                    spCalendar.Children.Add(spLine);
                    spLine = new StackPanel();
                    spLine.Orientation = Orientation.Horizontal;
                }

                List<Tournament> dayTournaments = new List<Tournament>();
                foreach(Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if(t.IsInternational() || Session.Instance.Game.kernel.LocalisationTournament(t) == Session.Instance.Game.club.Country())
                    {
                        foreach (Round r in t.rounds)
                        {
                            foreach (GameDay gd in r.programmation.gamesDays)
                            {
                                DateTime dt = gd.ConvertToDateTime(Session.Instance.Game.date.Year);
                                if (Utils.IsBeforeWithoutYear(dt, r.DateInitialisationRound()))
                                {
                                    dt = gd.ConvertToDateTime(Session.Instance.Game.date.Year + 1);
                                }
                                if (Utils.CompareDatesWithoutYear(dt, dateMonth))
                                {
                                    dayTournaments.Add(t);
                                }
                            }
                        }
                    }
                }

                spLine.Children.Add(ViewUtils.CreateCalendarItem(dateMonth, Utils.CompareDates(dateMonth, Session.Instance.Game.date), null, dayTournaments));

                dateMonth = dateMonth.AddDays(1);
            }
            spCalendar.Children.Add(spLine);
        }

        private void btnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void btnJourneeDroite_Click(object sender, RoutedEventArgs e)
        {
            _date = _date.AddMonths(1);
            Calendar();
        }

        private void btnJourneeGauche_Click(object sender, RoutedEventArgs e)
        {
            _date = _date.AddMonths(-1);
            Calendar();
        }
    }
}
