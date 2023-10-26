using AxMapWinGIS;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using TheManager;
using TheManager_GUI.Styles;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        private List<CheckBox> _tournamentCheckboxes;
        private List<StackPanel> _tournamentStackPanels;

        private void FillWidgets()
        {
            _tournamentStackPanels = new List<StackPanel>();
            _tournamentStackPanels.Add(spOptions1);
            _tournamentStackPanels.Add(spOptions2);
            _tournamentStackPanels.Add(spOptions3);
            _tournamentStackPanels.Add(spOptions4);
            _tournamentStackPanels.Add(spOptions5);
            foreach (Theme t in Theme.themes)
            {
                comboBoxThemes.Items.Add(t);
            }
            comboBoxThemes.SelectionChanged += ComboBoxThemes_SelectionChanged;

            foreach (Langue langue in Langue.langues)
            {
                comboBoxLangue.Items.Add(langue);
            }
            comboBoxLangue.SelectionChanged += ComboBoxLangue_SelectionChanged;

            _tournamentCheckboxes = new List<CheckBox>();
            int i = 0;
            foreach (Tournament c in Session.Instance.Game.kernel.Competitions)
            {
                CheckBox cb = new CheckBox();
                cb.IsChecked = Session.Instance.Game.options.tournamentsToExport.Contains(c);
                cb.Content = c.name;
                cb.Style = FindResource(StyleDefinition.styleCheckBox) as Style;
                _tournamentStackPanels[i % _tournamentStackPanels.Count].Children.Add(cb);
                _tournamentCheckboxes.Add(cb);
                i++;
            }
        }

        private void ComboBoxThemes_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Theme t = comboBoxThemes.SelectedItem as Theme;
            System.Windows.Media.Color backgroundColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.backgroundColor);
            System.Windows.Media.Color mainColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.mainColor);
            System.Windows.Media.Color secondaryColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.secondaryColor);
            System.Windows.Media.Color promotionColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.promotionColor);
            System.Windows.Media.Color dateColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.DateColor);
            System.Windows.Media.Color upperPlayOffColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.upperPlayOffColor);
            System.Windows.Media.Color bottomPlayOffColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.bottomPlayOffColor);
            System.Windows.Media.Color relegationColor = (System.Windows.Media.Color)ColorConverter.ConvertFromString(t.relegationColor);
            Application.Current.Resources["BackgroundColor"] = backgroundColor;
            Application.Current.Resources["Color1"] = mainColor;
            Application.Current.Resources["Color2"] = secondaryColor;
            Application.Current.Resources["ColorDate"] = dateColor;
            Application.Current.Resources["Promotion"] = promotionColor;
            Application.Current.Resources["UpperPlayOff"] = upperPlayOffColor;
            Application.Current.Resources["LowerPlayOff"] = bottomPlayOffColor;
            Application.Current.Resources["Relegation"] = relegationColor;
            Application.Current.Resources["Font"] = new FontFamily(t.fontFamily);
            t.SetAsCurrentTheme();
        }

        private void ComboBoxLangue_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Langue langue = comboBoxLangue.SelectedItem as Langue;
            langue.SetAsCurrentLangue();
        }


        public OptionsView()
        {
            InitializeComponent();
            FillWidgets();
            cbTransferts.IsChecked = Session.Instance.Game.options.transfersEnabled;
            cbSimuler.IsChecked = Session.Instance.Game.options.simulateGames;
            cbReduceSave.IsChecked = Session.Instance.Game.options.reduceSaveSize;
            comboBoxLangue.SelectedItem = Langue.current;
            if(Theme.current == null)
            {
                Console.WriteLine("Current theme : " + Theme.current);
                Theme.themes[0].SetAsCurrentTheme();
            }
            comboBoxThemes.SelectedItem = Theme.current;

        }

        private void BtnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            Session.Instance.Game.options.tournamentsToExport.Clear();
            foreach(CheckBox cb in _tournamentCheckboxes)
            {
                Tournament comp = Session.Instance.Game.kernel.String2Tournament(cb.Content.ToString());
                if (cb.IsChecked == true)
                {
                    Session.Instance.Game.options.tournamentsToExport.Add(comp);
                }
            }
            Session.Instance.Game.options.transfersEnabled = (bool)cbTransferts.IsChecked;
            Session.Instance.Game.options.simulateGames = (bool)cbSimuler.IsChecked;
            Session.Instance.Game.options.reduceSaveSize = (bool)cbReduceSave.IsChecked;

            Close();
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

    }
}
