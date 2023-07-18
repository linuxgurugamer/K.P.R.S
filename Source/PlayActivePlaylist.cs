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
            Log.Info("ToggleShuffle");
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
        }

        internal void ShufflePlaylist()
        {
            Log.Info("ShufflePlaylist");
              KPBR.soundPlayer.StopSound();
          this.activePlaylist = new List<string>(playlist);
            activePlaylist.Shuffle();
            Play();
        }

        internal void Play()
        {
            if (activePlaylist != null && !KPBR.soundPlayer.SoundPlaying())
            {
                if (activePlaylist.Count > 0)
                {
                    if (KPBR.soundPlayer.SoundPlaying())
                    { 
                        KPBR.soundPlayer.StopSound();
                    }
                    KPBR.soundPlayer.LoadNewSound(activePlaylist[0]);
                    KPBR.soundPlayer.PlaySound();
                    activePlaylist.RemoveAt(0);
                }
                else
                {
                    if (Shuffle)
                    {
                        ShufflePlaylist();
                    }
                    else
                    {
                        KPBR.soundPlayer.StopSound();
                        this.activePlaylist = new List<string>(playlist);
                    }
                }
            }
        }
    }
}
