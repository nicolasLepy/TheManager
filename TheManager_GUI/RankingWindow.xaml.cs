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
using TheManager;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour RankingWindow.xaml
    /// </summary>
    public partial class RankingWindow : Window
    {
        public RankingWindow()
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            FillFifaRanking();
        }

        private void FillFifaRanking()
        {
            int i = 0;

            StackPanel spHead = new StackPanel();
            spHead.Orientation = Orientation.Horizontal;
            spHead.Children.Add(ViewUtils.CreateLabel("", "StyleLabel2Center", -1, 50, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("Nation", "StyleLabel2", -1, 315, null, null, true));
            spHead.Children.Add(ViewUtils.CreateLabel("Points", "StyleLabel2Center", -1, 75, null, null, true));
            spRanking.Children.Add(spHead);

            foreach (NationalTeam nt in Session.Instance.Game.kernel.FifaRanking())
            {
                i++;
                StackPanel spTeam = new StackPanel();
                spTeam.Orientation = Orientation.Horizontal;
                spTeam.Children.Add(ViewUtils.CreateLabel(i.ToString(), "StyleLabel2Center", -1, 50));
                spTeam.Children.Add(ViewUtils.CreateFlag(nt.country, 40, 25));
                spTeam.Children.Add(ViewUtils.CreateLabel(nt.name, "StyleLabel2", -1, 275));
                spTeam.Children.Add(ViewUtils.CreateLabel(nt.officialFifaPoints.ToString("0.00"), "StyleLabel2Center", -1, 75));
                spRanking.Children.Add(spTeam);
            }
        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
