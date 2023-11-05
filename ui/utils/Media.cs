using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TheManager;
using WMPLib;

namespace TheManager_GUI.utils
{

    public class ThreadDuree
    {

        private readonly WindowsMediaPlayer _player;
        private readonly int _length;
        private readonly int _offset;

        public ThreadDuree(WindowsMediaPlayer player, int duree, int offset)
        {
            _player = player;
            _length = duree;
            _offset = offset;
        }
        
        public void ThreadProc()
        {
            Thread.Sleep(_offset * 1000);
            _player.controls.play();
            Thread.Sleep(_length * 1000);
            _player.close();
        }

    }

    public class ThreadDureeWAV
    {

        private readonly SoundPlayer _player;
        private readonly int _length;
        private readonly int _offset;

        public ThreadDureeWAV(SoundPlayer player, int length, int offset)
        {
            _player = player;
            _length = length;
            _offset = offset;
        }

        public void ThreadProc()
        {
            Thread.Sleep(_offset * 1000);
            _player.Play();
            Thread.Sleep(_length * 1000);
            _player.Stop();

        }

    }

    public class ThreadEventGoal
    {
        private readonly Thread _thread;

        private string _path;

        public string Path { get => _path; }

        public ThreadEventGoal(string path, int offset, int length)
        {
            _path = path;
            _thread = new Thread(() =>
            {
                var c = new System.Windows.Media.MediaPlayer();
                c.Open(new Uri(Utils.PathSong(path)));
                if (offset > 0)
                {
                    Thread.Sleep(offset * 1000);
                }
                c.Play();
                if (length > 0)
                {
                    Thread.Sleep(length * 1000);
                    c.Stop();
                }
                else
                {
                    double time = c.NaturalDuration.HasTimeSpan ? c.NaturalDuration.TimeSpan.TotalMilliseconds : 15000;
                    if (time > 15000)
                    {
                        time = 15000;
                    }
                    Thread.Sleep((int)time);
                    c.Stop();
                }
                _path = "";
            });
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            _thread.Abort();
        }

    }

    public class MediaWAV
    {
        private readonly List<ThreadEventGoal> _players;

        public bool IsPlaying(string sound)
        {
            bool res = false;
            foreach(ThreadEventGoal tb in _players)
            {
                if (tb.Path == sound)
                {
                    res = true;
                }
            }

            return res;
        }

        public MediaWAV()
        {
            _players = new List<ThreadEventGoal>();
        }

        public void AddSound(string path, bool loop, int length = 0, int offset = 0)
        {
            if(!IsPlaying(path))
            {
                ThreadEventGoal tb = new ThreadEventGoal(path, offset, length);
                _players.Add(tb);
                tb.Start();
            }
        }

        public void Background(Match m)
        {
            List<AudioSource> sources = new List<AudioSource>();
            foreach (AudioSource source in Session.Instance.Game.kernel.audioSources)
            {
                if (source.Min <= m.stadium.capacity && source.Max > m.stadium.capacity && source.Type == AudioType.Background)
                {
                    sources.Add(source);
                }
            }
            if(sources.Count > 0)
            {
                AudioSource picked = sources[Session.Instance.Random(0, sources.Count)];
                AddSound(picked.getPath(), false, 15);
            }
        }


        public void EventGoal(Match m)
        {
            List<AudioSource> sources = new List<AudioSource>();
            foreach(AudioSource source in Session.Instance.Game.kernel.audioSources)
            {
                if(source.Min <= m.stadium.capacity && source.Max > m.stadium.capacity && source.Type == AudioType.Event)
                {
                    sources.Add(source);
                }
            }
            AudioSource picked = sources[Session.Instance.Random(0, sources.Count)];
            AddSound(picked.getPath(), false, 15);
        }

        public void Destroy()
        {
            foreach (ThreadEventGoal p in _players)
            {
                try
                {
                    p.Stop();
                }
                catch { Utils.Debug("Impossible de fermer le thread"); }
            }
        }
    }
}