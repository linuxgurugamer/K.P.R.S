using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KPRS.RegisterToolbar;

namespace KPRS
{
    public  class Statics
    {
        internal const string STATION = "STATION";
        internal const string PLAYLIST = "KPRS_PLAYLIST";
        internal const string TRACKS = "TRACKS";
        internal const string TRACK = "track";

        internal static Dictionary<string, Station> stationList = new Dictionary<string, Station>();
        internal static Dictionary<string, PlayList> playlist = new Dictionary<string, PlayList>();
        internal static Dictionary<string, Transmitter> transmitterList = new Dictionary<string, Transmitter>();

        internal static Dictionary<string, VesselInfo> vesselInfoList = new Dictionary<string, VesselInfo>();
    }
}
