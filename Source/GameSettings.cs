using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using static KPRS.RegisterToolbar;

namespace KPRS
{


    public class PartInfo
    {
        internal const string ConfigNodeName = "PartInfo";

        public Guid vesselID;
        public string PartName;
        public double timeLastElectricity;
        public double transmitChargeRequired;
        public bool outofEC;
        public bool isTransmitter;
        public double lastUpdate;

        internal PartInfo(Guid vesselid, string PartName, double currentTime)
        {
            vesselID = vesselid;
            this.PartName = PartName;
            outofEC = false;
            lastUpdate = currentTime;
        }

        internal static PartInfo Load(ConfigNode node)
        {
            string PartName = "Unknown";
            node.TryGetValue("PartName", ref PartName);
            double lastUpdate = 0.0;
            node.TryGetValue("lastUpdate", ref lastUpdate);
            string tmpVesselID = "";
            node.TryGetValue("vesselID", ref tmpVesselID);
            Guid vesselID = Guid.Empty;
            try
            {
                vesselID = new Guid(tmpVesselID);
            }
            catch (Exception ex)
            {
                vesselID = Guid.Empty;
                Debug.Log("DFInterface - Load of GUID VesselID for known part failed Err: " + ex);
            }
            PartInfo info = new PartInfo(vesselID, PartName, lastUpdate);

            node.TryGetValue("timeLastElectricity", ref info.timeLastElectricity);
            node.TryGetValue("outofEC", ref info.outofEC);
            node.TryGetValue("isTransmitter", ref info.isTransmitter);
            return info;
        }

        internal ConfigNode Save(ConfigNode config)
        {
            ConfigNode node = config.AddNode(ConfigNodeName);
            node.AddValue("vesselID", vesselID);
            node.AddValue("PartName", PartName);
            node.AddValue("timeLastElectricity", timeLastElectricity);
            node.AddValue("outofEC", outofEC);
            node.AddValue("isTransmitter", isTransmitter);
            
            node.AddValue("lastUpdate", lastUpdate);
            return node;
        }

        internal void ClearAmounts()
        {
            timeLastElectricity = 0f;
        }
    }
    internal class GameSettings
    {
        // This class stores the KPRS Gamesettings config node.
        // which includes the following Dictionaries
        // knownVessels - all vessels in the save game that contain a Transmitter partmodule
        // knownTransmitterParts - all parts in the save game that contain a Transmitter partmodule

        public const string configNodeName = "KPBRGameSettings";
        public bool Enabled;
        internal Dictionary<Guid, VesselInfo> knownVessels;
        internal Dictionary<uint, PartInfo> knownTransmitterParts;

        internal GameSettings()
        {
            Enabled = true;
            knownVessels = new Dictionary<Guid, VesselInfo>();
            knownTransmitterParts = new Dictionary<uint, PartInfo>();
        }

        internal void Load(ConfigNode node)
        {
            knownVessels.Clear();
            knownTransmitterParts.Clear();

            if (node.HasNode(configNodeName))
            {
                ConfigNode settingsNode = node.GetNode(configNodeName);
                settingsNode.TryGetValue("Enabled", ref Enabled);

                knownVessels.Clear();
                var vesselNodes = settingsNode.GetNodes(VesselInfo.ConfigNodeName);
                foreach (ConfigNode vesselNode in vesselNodes)
                {
                    if (vesselNode.HasValue("Guid"))
                    {
                        Guid id = new Guid(vesselNode.GetValue("Guid"));
                        Log.Info("DFGameSettings Loading Guid = " + id);
                        VesselInfo vesselInfo = VesselInfo.Load(vesselNode);
                        knownVessels[id] = vesselInfo;
                    }
                }
                Log.Info("KPRSGameSettings finished loading KnownVessels");
                knownTransmitterParts.Clear();
                var partNodes = settingsNode.GetNodes(PartInfo.ConfigNodeName);
                foreach (ConfigNode partNode in partNodes)
                {
                    if (partNode.HasValue("flightID"))
                    {
                        uint id = uint.Parse(partNode.GetValue("flightID"));
                        Log.Info("KPRSGameSettings Loading flightID = " + id);
                        PartInfo partInfo = PartInfo.Load(partNode);
                        knownTransmitterParts[id] = partInfo;
                    }
                }
                Log.Info("KPRSGameSettings finished loading KnownParts");
            }
            Log.Info("KPRSGameSettings Loading Complete");
        }

        internal void Save(ConfigNode node)
        {
            ConfigNode settingsNode;
            if (node.HasNode(configNodeName))
            {
                settingsNode = node.GetNode(configNodeName);
            }
            else
            {
                settingsNode = node.AddNode(configNodeName);
            }

            settingsNode.AddValue("Enabled", Enabled);


            foreach (var entry in knownVessels)
            {
                ConfigNode vesselNode = entry.Value.Save(settingsNode);
                Log.Info("KPRSGameSettings Saving Guid = " + entry.Key);
                vesselNode.AddValue("Guid", entry.Key);
            }

            foreach (var entry in knownTransmitterParts)
            {
                ConfigNode partNode = entry.Value.Save(settingsNode);
                Log.Info("KPRSGameSettings Saving part flightID = " + entry.Key);
                partNode.AddValue("flightID", entry.Key);
            }


            Log.Info("DFGameSettings Saving Complete");
        }

        internal void DmpKnownVessels()
        {
            Log.Info("Dump of KnownVessels");
            if (!knownVessels.Any())
            {
                Log.Info("KnownVessels is EMPTY.");
            }
            else
            {
                foreach (KeyValuePair<Guid, VesselInfo> vessel in knownVessels)
                {
                    Log.Info("Vessel = " + vessel.Key + " Name = " + vessel.Value.vesselName);
                }
            }
        }

    }
}