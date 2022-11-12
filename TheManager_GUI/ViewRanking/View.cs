using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public abstract class View
    {
        public abstract void Full(StackPanel spRanking);

        public void OpenPlayer(Player p)
        {
            Windows_Joueur wj = new Windows_Joueur(p);
            wj.Show();
        }

        public void OpenClub(Club c)
        {
            if (c as CityClub != null)
            {
                Windows_Club wc = new Windows_Club(c as CityClub);
                wc.Show();
            }
            else if(c as ReserveClub != null)
            {
                Windows_Club wc = new Windows_Club((c as ReserveClub).FannionClub);
                wc.Show();
            }
            else if(c as NationalTeam != null)
            {
                CountryWindow cw = new CountryWindow(c as NationalTeam);
                cw.Show();
            }
        }

        public void OpenMatch(Match m)
        {
            Windows_Match wm = new Windows_Match(m);
            wm.Show();
        }


    }
}
