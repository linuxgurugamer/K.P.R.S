using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading.Tasks;
using SpaceTuxUtility;
using static KPRS.RegisterToolbar;

namespace KPRS
{

    internal static class LoadConfigs
    {
        static internal void GetConfigs()
        {
            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes(Statics.STATION))
            {
                var station = new Station(node);
                Statics.stationList[station.name] = station;
            }

            foreach (ConfigNode node in GameDatabase.Instance.GetConfigNodes(Statics.PLAYLIST))
            {
                var playlist = new PlayList(node);
                Statics.playlist[playlist.name] = playlist;
            }


        }
    }
}
