using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Imaging;
using tm;

namespace TheManager_GUI
{
    /// <summary>
    /// Logique d'interaction pour MediaWindow.xaml
    /// </summary>
    public partial class MediaWindow : Window
    {

        private readonly Media _media;
        private List<int> _indexOrders;

        public MediaWindow(Media media)
        {
            InitializeComponent();
            imgBtnQuitter.Source = new BitmapImage(new Uri(System.IO.Directory.GetCurrentDirectory() + "\\" + Utils.imagesFolderName + "\\return.png"));
            _media = media;
            _indexOrders = new List<int>();
            imgLogo.Source = new BitmapImage(new Uri(Utils.MediaLogo(media)));
            Map();
        }

        private void Map()
        {
            
        }

        private void btnQuitter_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
