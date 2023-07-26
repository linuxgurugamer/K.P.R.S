using KSP.UI.Screens.SpaceCenter;
using KSPAchievements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KPRS
{
    public class VesselInfo
    {
        public const string ConfigNodeName = "VesselInfo";

        public string vesselName;
        double storedEC;
        double predictedECOut;
        double lastUpdate;

        Guid id;

        public double StoredEC { get { return storedEC; } set { storedEC = value; } }
        public double PredictedECOut { get { return predictedECOut; } set { predictedECOut = value; } }
        public double LastUpdate { get { return lastUpdate; } set { lastUpdate = value; } }

        internal VesselInfo(Vessel vessel, double storedEC = 0, double predictedECOut = 0, double currentTime = 0)
        {
            vesselName = vessel.vesselName;
            id = vessel.id;
            this.storedEC = storedEC;
            this.predictedECOut = predictedECOut;
            this.lastUpdate = currentTime;
        }

        internal VesselInfo(string vesselName, double currentTime)
        {
            this.vesselName = vesselName;
            lastUpdate = currentTime;
        }

        internal static VesselInfo Load(ConfigNode node)
        {
            string vesselName = "Unknown";
            node.TryGetValue("vesselName", ref vesselName);
            double lastUpdate = 0f;
            node.TryGetValue("lastUpdate", ref lastUpdate);

            VesselInfo info = new VesselInfo(vesselName, lastUpdate);
            node.TryGetValue("storedEC", ref info.storedEC);
            node.TryGetValue("predictedECOut", ref info.predictedECOut);
            node.TryGetValue("id", ref info.id);

            return info;
        }

        internal ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselName", vesselName);
            node.AddValue("storedEC", storedEC);
            node.AddValue("predictedECOut", predictedECOut);
            node.AddValue("id", id);

            return node;
        }

        internal void ClearAmounts()
        {
            storedEC = 0f;
            predictedECOut = 0f;
        }

    }

}
