using LibNoise.Modifiers;
using SpaceTuxUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KPRS.RegisterToolbar;

#if false
PLAYLIST
{
	name = Construction
	loop = True
	shuffle = True
	preloadTime = 5
	pauseOnGamePause = True
	disableAfterPlay = False
	tracks
	{
		track = KSP_Construction01
		track = KSP_Construction02
		track = KSP_Construction03
		track = Groove Grove
		track = Brittle Rille
		track = Investigations
	}
}
#endif

namespace KPRS
{
    internal class PlayList
    {
        internal string name;
        internal bool loop;
        internal int preloadTime;
        internal bool pauseOnGamePause;
        internal bool disableAfterPlay;
        internal List<string> tracks;
        
        internal PlayList(ConfigNode node)
        {
            this.name = node.SafeLoad("name", "");
            this.loop = node.SafeLoad("loop", true);
            this.preloadTime = node.SafeLoad("preloadTime", 5);
            this.pauseOnGamePause = node.SafeLoad("pauseOnGamePause", true);
            this.disableAfterPlay = node.SafeLoad("disableAfterPlay", false);
            this.tracks = new List<string>();
            ConfigNode tracksNode = node.GetNode(Statics.TRACKS);

#if false
            var tvl = tracksNode.GetValuesList(Statics.TRACK);
            Log.Info("Playlist: " + name + ", tvl.Count: " + tvl.Count());
#endif
            foreach (string s in tracksNode.GetValuesList(Statics.TRACK))
            {
                tracks.Add(s);
            }
            Log.Info("LoadedPlaylist: " + this.ToString());
        }

        public override string ToString()
        {
            string ret = "name: " + name + ", loop: " + loop + ", preloadTime: " + preloadTime + ", pauseOnGamePause: " + pauseOnGamePause + ", disableAfterPlay: " + disableAfterPlay;
            ret = ret + ", # Tracks: " + tracks.Count()+", TRACKS: ";
            foreach (var s in tracks)
            {
                ret = ret  + s+ ", ";
            }
            return ret;
        }

    }
}
