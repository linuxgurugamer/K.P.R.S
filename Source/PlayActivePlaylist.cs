using System.Collections.Generic;
using static KPRS.RegisterToolbar;


namespace KPRS
{
    public class PlayActivePlaylist
    {
        List<string> activePlaylist = new List<string>();
        List<string> playlist;
        internal bool Shuffle { get; set; }

        internal void ToggleShuffle()
        {
            Shuffle = !Shuffle;
            Play();
        }

        internal PlayActivePlaylist(string caller, PlayList playlist)
        {
            Log.Info("PlayActivePlayList, caller: " + caller);
            this.activePlaylist = new List<string>(playlist.tracks);
            this.playlist = playlist.tracks;
            foreach (var a in playlist.tracks)
                Log.Info("Playlist: " + a.ToString());
        }

        ~PlayActivePlaylist()
        {
            Log.Info("~PlayActivePlaylist");
            KPBR.soundplayer.StopSound();
            activePlaylist = null;
            playlist = null;
        }

        internal void ShufflePlaylist()
        {
            this.activePlaylist = new List<string>(playlist);
            activePlaylist.Shuffle();
        }

        internal void Play()
        {
            if (activePlaylist != null && !KPBR.soundplayer.SoundPlaying())
            {
                if (activePlaylist.Count > 0)
                {
                    if (KPBR.soundplayer.SoundPlaying())
                    { 
                        KPBR.soundplayer.StopSound();
                    }
                    KPBR.soundplayer.LoadNewSound(activePlaylist[0]);
                    KPBR.soundplayer.PlaySound();
                    activePlaylist.RemoveAt(0);
                }
                else
                {
                    if (Shuffle)
                    {
                        ShufflePlaylist();
                    }
                    else
                        this.activePlaylist = new List<string>(playlist);
                }
            }
        }
    }
}
