using System.Collections.Generic;
using static KPRS.RegisterToolbar;


namespace KPRS
{
    public class PlayActivePlaylist
    {
        List<string> activePlaylistTracks = new List<string>();
        string currentlyPlaying;
        public string CurrentlyPlaying { set { currentlyPlaying = value; LetterCnt = 0; lastLetterCnt = -1; } get { if (currentlyPlaying == "") return "";  return "Currently Playing: " + currentlyPlaying + "  "; } }
        public int LetterCnt {  get; set; }

        string cachedCurrentlyPlaying = "";
        int lastLetterCnt = -1;
        public void CachedCurrentlyPlaying(string cache, int cnt)
        {
            cachedCurrentlyPlaying = cache;
            lastLetterCnt = cnt;
        }
        public int LastLetterCnt {  get {  return lastLetterCnt; } }
        public string GetCachedCurrentlyPlaying { get { return cachedCurrentlyPlaying; } }


        List<string> playlistTracks;
        string callsignTrack;
        bool callsignPlayed = false;

        PlayList playlist;
        internal bool Shuffle { get; set; }

        internal void ToggleShuffle()
        {
#if false
            Log.Info("ToggleShuffle");
#endif
            Shuffle = !Shuffle;
            Play("ToggleShuffle");
        }

        internal PlayActivePlaylist(string caller, PlayList playlist, string channelCallsign)
        {
#if false
            Log.Info("PlayActivePlayList, caller: " + caller);
#endif
            NewPlayActivePlaylist("PlayActivePlaylist", playlist, channelCallsign);

#if false
            this.activePlaylistTracks = new List<string>(playlist.tracks);
            this.playlistTracks = playlist.tracks;
            foreach (var a in playlist.tracks)
                Log.Info("Playlist: " + a.ToString());
            this.playlist = playlist;
#endif
        }

        internal void Clear(string caller)
        {
            if (activePlaylistTracks != null)
            {
                activePlaylistTracks.Clear();
                CurrentlyPlaying = "";
            }
            this.playlist = null;
            this.playlistTracks = null;
        }

        internal void NewPlayActivePlaylist(string caller, PlayList playlist, string channelCallsign)
        {
#if false
            Log.Info("NewPlayActivePlaylist, caller: " + caller + ", playlist name: " + playlist.name);
#endif
            Clear("NewPlayActivePlayList");

            this.activePlaylistTracks = new List<string>(playlist.tracks);
            CurrentlyPlaying = "";
            this.playlistTracks = playlist.tracks;
            this.callsignTrack = channelCallsign;
            callsignPlayed = false;

#if false
            foreach (var a in playlist.tracks)
                Log.Info("Playlist: " + a.ToString());
#endif
            this.playlist = playlist;
        }

#if false
        ~PlayActivePlaylist()
        {
            Log.Info("~PlayActivePlaylist, playlist: " + this.playlist.name);
        }
#endif

        internal void ShufflePlaylist()
        {
#if false
            Log.Info("ShufflePlaylist");
#endif
            KPBR.soundPlayer.StopSound();

            this.activePlaylistTracks = new List<string>(playlistTracks);
            CurrentlyPlaying = "";

            activePlaylistTracks.Shuffle();
            Play("ShufflePlaylist");
        }

        internal void Play(string caller)
        {
#if false
            Log.Info("PlayActivePlaylist caller: " + caller);
#endif
            if (activePlaylistTracks != null && !KPBR.soundPlayer.SoundPlaying() && !KPBR.soundPlayer.loadingSong)
            {
                if (KPBR.soundPlayer.readyToPlay)
                {
                    KPBR.soundPlayer.PlaySound("Play");
                    
                }
                else
                {
                    if (!callsignPlayed)
                    {
                        KPBR.soundPlayer.PlaySound("PlayActivePlaylist.Play, removing item from index 0");
                        KPBR.soundPlayer.LoadNewSound(callsignTrack);
                        callsignPlayed=true;

                    }
                    else
                    {
                        if (activePlaylistTracks.Count > 0)
                        {
#if false
                    if (KPBR.soundPlayer.SoundPlaying())
                    {
                        KPBR.soundPlayer.StopSound();
                    }
#endif
                            KPBR.soundPlayer.PlaySound("PlayActivePlaylist.Play, removing item from index 0");
                            KPBR.soundPlayer.LoadNewSound(activePlaylistTracks[0]);
                            CurrentlyPlaying = activePlaylistTracks[0].Substring(activePlaylistTracks[0].LastIndexOf('/')+1);
                            activePlaylistTracks.RemoveAt(0);
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
                                if (playlistTracks != null)
                                    this.activePlaylistTracks = new List<string>(playlistTracks);
                                else
                                {
                                    this.activePlaylistTracks = new List<string>();
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
