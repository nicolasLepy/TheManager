using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TheManager;
using TheManager.Tournaments;
using TheManager_GUI.controls;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour CalendarView.xaml
    /// </summary>
    public partial class CalendarView : Window
    {
        private DateTime _date;

        public CalendarView()
        {
            _date = Session.Instance.Game.date.AddDays(-Session.Instance.Game.date.Day + 1);
            InitializeComponent();
            Calendar();
        }

        private void buttonMonthLeft_Click(object sender, RoutedEventArgs e)
        {
            _date = _date.AddMonths(-1);
            Calendar();
        }

        private void buttonMonthRight_Click(object sender, RoutedEventArgs e)
        {
            _date = _date.AddMonths(1);
            Calendar();
        }

        private void Calendar()
        {
            gridCalendar.Children.Clear();
            textMonth.Text = _date.ToString("MMMM yyyy");

            DateTime dateMonth = new DateTime(_date.Year, _date.Month, 1);
            int daysCount = DateTime.DaysInMonth(_date.Year, _date.Month);
            int currentRow = 0;
            int currentCol = (int)(dateMonth.DayOfWeek + 6) % 7;
            for (int i = 0; i < daysCount; i++)
            {
                if (dateMonth.DayOfWeek == DayOfWeek.Monday)
                {
                    currentCol = 0;
                    currentRow++;
                }

                List<Tournament> dayTournaments = new List<Tournament>();
                foreach (Tournament t in Session.Instance.Game.kernel.Competitions)
                {
                    if (t.IsInternational() || Session.Instance.Game.kernel.LocalisationTournament(t) == Session.Instance.Game.club.Country())
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

                Match clubMatch = null;
                foreach (Match m in Session.Instance.Game.club.Games)
                {
                    if (Utils.CompareDates(m.day, dateMonth))
                    {
                        clubMatch = m;
                    }
                }

                ControlCalendarItem control = new ControlCalendarItem(dateMonth, Utils.CompareDates(dateMonth, Session.Instance.Game.date), clubMatch, dayTournaments);
                ViewUtils.AddElementToGrid(gridCalendar, control, currentRow, currentCol + 1);

                currentCol = currentCol + 1;
                dateMonth = dateMonth.AddDays(1);
            }
        }

        /* EVENTS HANDLER */

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int mParam, int lParam);

        private void spControlBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            SendMessage(helper.Handle, 161, 2, 0);
        }

        private void spControlBar_MouseEnter(object sender, MouseEventArgs e)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }

        private void buttonQuit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
