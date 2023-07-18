using KPRS.PartModules;
using SpaceTuxUtility;
using static KPRS.RegisterToolbar;
using static ConfigNode;

namespace KPRS
{
    internal class Transmitter
    {
        internal string location;
        internal string selectedStation;
        internal Vessel vessel;
        internal float towerHeight;

        internal Transmitter(string selStat, string loc = null, Vessel vessel = null)
        {
            location = loc;
            selectedStation = selStat;
            this.vessel = vessel;
            GetTowerHeight();
        }

        internal Transmitter(KPBR_TransmitterPartModule t)
        {
            location = t.location;
            selectedStation = t.selectedStation;
            this.vessel = t.part.vessel;
            GetTowerHeight();
        }

        internal void GetTowerHeight()
        {
            Log.Info("GetTowerHeight");
            towerHeight = 0;
            if (vessel.loaded)
            {
                var t = vessel.FindPartModulesImplementing<KPRS_ModuleAntenna>();
                foreach (var a in t)
                {
                    towerHeight += a.height;
                }
                Log.Info("Transmitter, vessel: " + vessel.name + ", Num AntennaModules: " + t.Count + ", towerHeight: " + towerHeight);
            }
            else
            {
#if false
                var t = vessel.FindPartModulesImplementing<KPRS_ModuleAntenna>();
                if (t != null)
                {
                    Log.Info("Transmitter, Unloaded: " + vessel.vesselName);
                    Log.Info("Num KPRS_ModuleAntenna: " + t.Count);
                    foreach (var t1 in t)
                        Log.Info("Antenna height  from vessel: " + t1.height);
                }
                else
                    Log.Info("Unloaded: " + vessel.loaded + ": " + vessel.vesselName + ", no KPRS_ModuleAntenna found");
#endif
                Log.Info("Transmitter, Unloaded: " + vessel.vesselName);

                for (int idx3 = 0; idx3 < vessel.protoVessel.protoPartSnapshots.Count; idx3++)
                {
                    ProtoPartSnapshot p = vessel.protoVessel.protoPartSnapshots[idx3];
                    Part part = p.partPrefab;

                    for (int modIdx = 0; modIdx < p.modules.Count; modIdx++)
                    {
                        ProtoPartModuleSnapshot module = p.modules[modIdx];

                        if (module.moduleName == "KPRS_ModuleAntenna")
                        {
                            float height = module.moduleValues.SafeLoad("height", -1f);
                            towerHeight += height;
                            Log.Info("Antenna, from protoVessel.protoPartSnapshots, height: " + height);
                            //DumpConfigNode(module.moduleValues);
                        }
                    }
                }
            }
        }
#if false
        void DumpConfigNode(ConfigNode node)
        {

            foreach (Value n in node.values)
            {
                Log.Info("Value name: " + n.name + ", value: " + n.value);
            }
        }
#endif
    }
}
