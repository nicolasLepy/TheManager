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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TheManager_GUI.Styles;
using Image = System.Windows.Controls.Image;

namespace TheManager_GUI.controls
{

    public struct ControlInternationalRankingItem
    {
        public ControlInternationalRankingItem(string name, Image flag, int ranking, int rankingProgression, List<double> previousCoefs, double currentCoef, List<int> slots)
        {
            Name = name;
            Flag = flag;
            Ranking = ranking;
            RankingProgression = rankingProgression;
            PreviousCoefs = previousCoefs;
            CurrentCoef = currentCoef;
            Slots = slots;
        }

        public string Name { get; }
        public Image Flag { get; }
        public int Ranking { get; }
        public int RankingProgression { get; }
        public List<double> PreviousCoefs { get; }
        public double CurrentCoef { get; }
        public List<int> Slots { get; }
    }

    /// <summary>
    /// Logique d'interaction pour ControlInternationalRanking.xaml
    /// </summary>
    public partial class ControlInternationalRanking : UserControl
    {
        private readonly List<ControlInternationalRankingItem> items;
        private readonly List<string> previousCoefficientTitles;
        private readonly List<string> internationalQualificationsSlot;

        public ControlInternationalRanking(List<ControlInternationalRankingItem> items, List<string> previousCoefficientTitles, List<string> internationalQualificationsSlot)
        {
            InitializeComponent();
            this.items = items;
            this.previousCoefficientTitles = previousCoefficientTitles;
            this.internationalQualificationsSlot = internationalQualificationsSlot;
            Initialize();
        }

        public TextBlock DeltaRankingView(int delta)
        {
            string sign = delta > 0 ? "+" : "";
            string text = delta != 0 ? String.Format("{0} {1}", sign, delta) : "";
            string brush = delta > 0 ? StyleDefinition.colorPositive : StyleDefinition.colorNegative;
            return ViewUtils.CreateTextBlock(text, StyleDefinition.styleTextPlainCenter, -1, -1, Application.Current.FindResource(brush) as Brush);
        }

        public void FillTitle()
        {
            ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(FindResource("str_coefficient").ToString(), StyleDefinition.styleTextPlainCenter), 0, 4 + previousCoefficientTitles.Count);
            for (int i = 0; i < previousCoefficientTitles.Count; i++)
            {
                ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(previousCoefficientTitles[i], StyleDefinition.styleTextPlainCenter), 0, 4 + i);
            }
            for(int i = 0; i < internationalQualificationsSlot.Count; i++)
            {
                ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(internationalQualificationsSlot[i], StyleDefinition.styleTextPlainCenter), 0, 5 + previousCoefficientTitles.Count + i);
            }
        }

        public void FillContent()
        {
            for(int i = 0; i < items.Count; i++)
            {
                ControlInternationalRankingItem item = items[i];
                ViewUtils.AddElementToGrid(gridMain, DeltaRankingView(item.RankingProgression), i+1, 0);
                ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(item.Ranking.ToString(), StyleDefinition.styleTextPlainCenter), i+1, 1);
                ViewUtils.AddElementToGrid(gridMain, item.Flag, i+1, 2);
                ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(item.Name, StyleDefinition.styleTextPlain), i+1, 3);
                ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(String.Format("{0:0.00}", item.CurrentCoef), StyleDefinition.styleTextPlainCenter, -1, -1, null, null, true), i + 1, 4 + previousCoefficientTitles.Count);
                for(int j = 0; j < item.PreviousCoefs.Count; j++)
                {
                    ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(String.Format("{0:0.00}", item.PreviousCoefs[j]), StyleDefinition.styleTextPlainCenter), i + 1, 4 + j);
                }
                for (int j = 0; j < item.Slots.Count; j++)
                {
                    ViewUtils.AddElementToGrid(gridMain, ViewUtils.CreateTextBlock(item.Slots[j].ToString(), StyleDefinition.styleTextPlainCenter), i + 1, 5 + item.PreviousCoefs.Count + j);
                }
            }
        }

        public void Initialize()
        {
            gridMain.ColumnDefinitions.Clear();
            gridMain.RowDefinitions.Clear();
            gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) }); //delta Ranking
            gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) }); //Ranking
            gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(10, GridUnitType.Star) }); //Flag
            gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(100, GridUnitType.Star) }); //Name
            int columns = 1 + previousCoefficientTitles.Count + internationalQualificationsSlot.Count;
            for (int i = 0; i < columns; i++)
            {
                gridMain.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(15, GridUnitType.Star) });
            }
            for (int i = 0; i < items.Count+1; i++)
            {
                gridMain.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(25, GridUnitType.Pixel) });
            }
            Console.WriteLine(gridMain.ColumnDefinitions.Count + ", " + gridMain.RowDefinitions.Count);
            FillTitle();
            FillContent();
        }
    }
}
