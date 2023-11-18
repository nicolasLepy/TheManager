using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheManager;

namespace TheManager_GUI.utils
{
    public static class Handlers
    {

        public static void OpenPlayer(Player p)
        {
            PlayerView view = new PlayerView(p);
            view.Show();
        }

        public static void OpenClub(Club c)
        {
            if (c as CityClub != null)
            {
                ClubView wc = new ClubView(c as CityClub);
                wc.Show();
            }
            else if (c as ReserveClub != null)
            {
                ClubView wc = new ClubView((c as ReserveClub).FannionClub);
                wc.Show();
            }
            else if (c as NationalTeam != null)
            {
                CountryView cw = new CountryView(c as NationalTeam);
                cw.Show();
            }
        }

        public static void OpenMatch(Match m)
        {
            MatchView view = new MatchView(m);
            view.Show();
        }

        public static void OpenTournament(Tournament t)
        {
            TournamentView view = new TournamentView(t);
            view.Show();
        }

    }
}
