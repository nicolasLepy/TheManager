using System.Windows.Controls;
using TheManager;

namespace TheManager_GUI.VueClassement
{
    public abstract class View
    {

        public abstract void Show();
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
        }

        public void OpenMatch(Match m)
        {
            Windows_Match wm = new Windows_Match(m);
            wm.Show();
        }



    }
}
