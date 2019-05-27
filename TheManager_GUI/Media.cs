using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using TheManager;
using WMPLib;

namespace TheManager_GUI
{
    public class Media
    {

        private List<WindowsMediaPlayer> _players;



        private void ThreadDuree(WindowsMediaPlayer player, int duree, int decalage)
        {
            Thread.Sleep(decalage * 1000);
            player.controls.play();
            Thread.Sleep(duree * 1000);
            player.close();
        }

        private void Player_MediaError(object pMediaObject)
        {
            MessageBox.Show("Ne peut pas jouer le son.");
        }

        public void AjouterSon(string chemin, bool boucle, int duree = 0, int decalage = 0)
        {
            WindowsMediaPlayer wplayer = new WindowsMediaPlayer();
            wplayer.MediaError += new _WMPOCXEvents_MediaErrorEventHandler(Player_MediaError);
            wplayer.URL = Utils.CheminSon(chemin);
            Console.WriteLine(wplayer.URL);
            if(boucle)
            {
                wplayer.settings.setMode("loop", true);
            }
            _players.Add(wplayer);
            if(duree != 0)
            {
                Thread t = new Thread(() => ThreadDuree(wplayer, duree, decalage));
                t.Start();
            }
            else
            {
                wplayer.controls.play();

            }
        }

        public void Ambiance4000()
        {
            int random = Session.Instance.Random(1, 3);
            switch(random)
            {
                case 1:
                    AjouterSon("Ambiances\\Ambiance_4000",true);
                    break;
                case 2:
                    AjouterSon("Ambiances\\Ambiance_4000_2", true);
                    break;
            }
        }

        public void Ambiance12000()
        {
            int random = Session.Instance.Random(1, 2);
            switch (random)
            {
                case 1:
                    AjouterSon("Ambiances\\Ambiance_12000", true);
                    break;
            }
        }

        public void But4000()
        {
            int random = Session.Instance.Random(1, 4);
            switch (random)
            {
                case 1:
                    AjouterSon("Ambiances\\But_4000", false, 15);
                    break;
                case 2:
                    AjouterSon("Ambiances\\But_4000_2", false, 15);
                    break;
                case 3:
                    AjouterSon("Ambiances\\But_4000_3", false, 15);
                    break;
            }
            AjouterSon("montpellier", false, 15,2);
        }

        public void But12000()
        {
            int random = Session.Instance.Random(1, 5);
            switch (random)
            {
                case 1:
                    AjouterSon("Ambiances\\But_12000", false, 15);
                    break;
                case 2:
                    AjouterSon("Ambiances\\But_12000_2", false, 15);
                    break;
                case 3:
                    AjouterSon("Ambiances\\But_12000_3", false, 15);
                    break;
                case 4:
                    AjouterSon("Ambiances\\But_12000_4", false, 15);
                    break;
            }
            AjouterSon("montpellier", false, 15,2);
        }

        public Media()
        {
            _players = new List<WindowsMediaPlayer>();
        }

        public void Detruire()
        {
            foreach(WindowsMediaPlayer wmp in _players)
            {
                wmp.close();
            }
        }
    }
}