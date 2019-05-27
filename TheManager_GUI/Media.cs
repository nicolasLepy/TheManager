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

    public class ThreadDuree
    {

        private WindowsMediaPlayer _player;
        private int _duree;
        private int _decalage;

        public ThreadDuree(WindowsMediaPlayer player, int duree, int decalage)
        {
            _player = player;
            _duree = duree;
            _decalage = decalage;
        }
        
        public void ThreadProc()
        {
            Thread.Sleep(_decalage * 1000);
            _player.controls.play();
            Thread.Sleep(_duree * 1000);
            _player.close();
        }

    }

    public class Media
    {

        private List<WindowsMediaPlayer> _players;

        
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
                ThreadDuree td = new ThreadDuree(wplayer, duree, decalage);
                //Thread t = new Thread(() => ThreadDuree(wplayer, duree, decalage));
                Thread t = new Thread(new ThreadStart(td.ThreadProc));
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

        public void But(Match m)
        {
            if (m.Affluence < 12000) But4000(m.Domicile.MusiqueBut);
            else But12000(m.Domicile.MusiqueBut);
        }

        public void But4000(string musique)
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
            AjouterSon(musique, false, 15,2);
        }

        public void But12000(string musique)
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
            AjouterSon(musique, false, 15,2);
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