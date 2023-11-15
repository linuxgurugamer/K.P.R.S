using ClickThroughFix;
using SpaceTuxUtility;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using static KPRS.RegisterToolbar;
using static KPRS.Statics;
using System.Collections;
using KSP.Localization;
using System.Text;

namespace KPRS.PartModules
{
    public class KPBR_TransmitterPartModule : PartModule, IModuleInfo
    {
        internal bool StationSelected { get { return selectedStation != null && selectedStation.Length > 0; } }

        [KSPField(isPersistant = true, guiName = "Radio station")]
        internal string selectedStation = "";

        [KSPField(isPersistant = true, guiName = "Location")]
        internal string location = "";

        [KSPField(isPersistant = true)]
        internal bool isTransmitter;

        [KSPField(isPersistant = true)]
        public double consumeRate = 2f;

        [KSPField(isPersistant = true)]
        public bool Active = true;

        public bool HasPower = true;

        internal bool LocationSelected { get { return location != null && location.Length > 0; } }

        internal const float TRANS_SEL_HEIGHT = 400f;
        internal const float TRANS_SEL_WIDTH = 400f;
        bool guiActive = false;
        int radioWinId;


        [KSPEvent(guiActive = true, guiActiveEditor = true, requireFullControl = false, guiActiveUncommand = true,
            guiName = "Select Station")]
        public void ToggleStationSelection()
        {
            ToggleStationSelectionWin();
        }

        [KSPEvent(guiActive = false, requireFullControl = false, guiActiveUncommand = true,
            guiName = "Activate Station")]
        public void ActivateStation()
        {
            this.location = FlightGlobals.ActiveVessel.mainBody.bodyName;
            Events["ActivateStation"].guiActive = false;
            Events["DeactivateStation"].guiActive = true;

            Events["ToggleStationSelection"].guiActiveEditor = false;
            Events["ToggleStationSelection"].guiActive = false;

            Active = true;
        }

        [KSPEvent(guiActive = true, requireFullControl = false, guiActiveUncommand = true,
            guiName = "Deactivate Station")]
        public void DeactivateStation()
        {
            this.location = "";
            transmitterList[selectedStation] = new Transmitter(this);
            Events["ActivateStation"].guiActive = true;
            Events["DeactivateStation"].guiActive = false;

            Events["ToggleStationSelection"].guiActiveEditor = true;
            Events["ToggleStationSelection"].guiActive = true;
            Active = false;
        }

        bool IsTransmitter()
        {
            Log.Info("IsTransmitter, Vessel: " + vessel.name + ", part: " + part.name);
            if (this.part.variants == null)
            {
                Log.Info("IsTransmitter, part.variants is null");
                Log.Info("vessel name: " + this.vessel.name);
                return true;
            }
            else
            {
                string type = this.part.variants.SelectedVariant.Name;
                Log.Info("vessel: " + this.vessel.name + ", Transmitter type: " + type);
                isTransmitter = (type == "Transmitter");
                return isTransmitter;
            }
        }

        List<Part> transmitterPartList = new List<Part>();
        List<Part> amplifierPartList = new List<Part>();
        internal void CheckForActiveTransmitter()
        {
            if (IsTransmitter())
            {
                transmitterPartList = new List<Part>();
                Active = true;
                int activeCnt = 0;

                var mList = vessel.FindPartModulesImplementing<KPBR_TransmitterPartModule>();
                foreach (var m in mList)
                {
                    if (!transmitterPartList.Contains(m.part))
                    {
                        transmitterPartList.Add(m.part);
                        if (m.location != "")
                        {
                            //Active = false;
                            activeCnt++;
                        }
                    }
                }
                if (mList.Count > 1)
                {
                    Log.Info("TransmitterPartModule, Multiple radio transmitters found on vessel: " + mList.Count + ", active: " + activeCnt);
                }
                if (activeCnt > 1)
                {
                    Log.Info("TransmitterPartModule, Multiple active radio transmitters found on vessel");
                }
                var aList = vessel.FindPartModulesImplementing<KPBR_TransmitterAmplifier>();
                foreach (var a in aList)
                    amplifierPartList.Add(a.part);

                Log.Info("TransmitterPartModule, Number of amplifiers found: " + aList.Count);
                Log.Info("TransmitterPartModule, Vessel: " + part.vessel.vesselName + ", total Transmitters found: " + transmitterPartList.Count +
                    ", total Amplifiers found: " + amplifierPartList.Count);
            }
        }

        private string EC = "ElectricCharge";

        void Start()
        {
            Log.Info("KPBR_TransmitterPartModule.Start");
#if false
            if (HighLogic.LoadedSceneIsFlight)
            {
                Log.Info("vessel: " + this.part.vessel.vesselName);
                Log.Info("selectedStation: " + selectedStation.ToString());
                Log.Info("location: " + location.ToString());
            }
#endif
            if (HighLogic.LoadedSceneIsFlight)
                CheckForActiveTransmitter();

            if (StationSelected && selectedStation != "" && !HighLogic.LoadedSceneIsEditor)
            {
                Events["ToggleStationSelection"].guiActiveEditor = false;
                Events["ToggleStationSelection"].guiActive = false;

                Fields["selectedStation"].guiActive = true;
                Fields["location"].guiActive = true;

                if (this.location != null && this.location != "")
                {
                    if (!part.vessel.Landed && part.vessel.situation != Vessel.Situations.ORBITING)
                        this.location = "";
                    else
                    {
                        Events["ActivateStation"].guiActive = false;
                        Events["DeactivateStation"].guiActive = true;
                    }
                    transmitterList[selectedStation] = new Transmitter(this);
                }
            }
            if (location == null || location == "")
            {
                Events["ActivateStation"].guiActive = false;
            }
            radioWinId = WindowHelper.NextWindowId("K.P.R.S-TransStationSel");
            StartCoroutine(SlowUpdate());
        }

        internal class PartId
        {
            uint partPersistentId;
            internal bool Active;
            internal PartId(uint partPersistentId, bool active)
            {
                this.partPersistentId = partPersistentId;
                this.Active = active;
            }
        }

        internal class VesselParts
        {
            internal uint vesselPersistentId;

            internal Dictionary<uint, PartId> partIDdict;

            internal VesselParts(uint vesselPid)
            {
                vesselPersistentId = vesselPid;
                partIDdict = new Dictionary<uint, PartId>();
            }
        }


        static Dictionary<uint, VesselParts> vesselPartIds = new Dictionary<uint, VesselParts>();
        static System.Random r = new System.Random(DateTime.Now.Millisecond);

        IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (this.location != "")
                    {
                        string curLoc = this.location;
                        if (!part.vessel.Landed && part.vessel.situation != Vessel.Situations.ORBITING)
                        {
                            this.location = "";
                            Events["ActivateStation"].guiActive = false;
                            Events["DeactivateStation"].guiActive = false;
                        }
                        else
                        {
                            Events["ActivateStation"].guiActive = false;
                            Events["DeactivateStation"].guiActive = true;
                        }
                    }
                    else
                    {
                        if (part.vessel.Landed || part.vessel.situation == Vessel.Situations.ORBITING)
                        {
                            if (selectedStation == "")
                            {
                                Events["ActivateStation"].guiActive = false;
                                Events["DeactivateStation"].guiActive = false;
                            }
                            else
                            {
                                Events["ActivateStation"].guiActive = true;
                                Events["DeactivateStation"].guiActive = false;
                            }
                        }
                    }
                }
            }
        }

        void ToggleStationSelectionWin()
        {
            guiActive = !guiActive;
        }

        internal void OnGUI()
        {
            if (guiActive)
            {
                if (Config.config.KspSkin)
                {
                    GUI.skin = HighLogic.Skin;
                }
                KPBR.Instance.transWindowRect = ClickThruBlocker.GUILayoutWindow(radioWinId, KPBR.Instance.transWindowRect, RadioWin,
                     "KPBR Transmitter Station Selection");
            }
        }

        Vector2 stationPos;
        void RadioWin(int id)
        {

            using (new GUILayout.VerticalScope())
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Label("Select Station for this transmitter");
                    GUILayout.Label("(blue stations are already used and cannot be selected)");
                }
                stationPos = GUILayout.BeginScrollView(stationPos, GUILayout.Height(TRANS_SEL_HEIGHT - 60));
                foreach (var station in stationList)
                {
                    if (!station.Value.selected)
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            if (station.Key == selectedStation)
                            {
                                GUILayout.Label("<b>" + station.Key + "</b>", labelFontBoldYellow);
                            }
                            else
                            {
                                if (GUILayout.Button(station.Key, GUI.skin.label))
                                {
                                    selectedStation = station.Key;
                                }
                            }
                        }
                    }
                    else
                        GUILayout.Label("<b>" + station.Key + "</b>", labelFontBoldBlue);

                }
                GUILayout.EndScrollView();

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Select Station"))
                    {
                        guiActive = false;
                        Events["ActivateStation"].guiActive = true;
                        Fields["selectedStation"].guiActive = true;
                        Fields["location"].guiActive = true;
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Clear Selected"))
                    {
                        selectedStation = "";
                    }
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Close"))
                    {
                        guiActive = false;
                    }
                    GUILayout.FlexibleSpace();
                }
            }
            GUI.DragWindow();

        }

        public string GetModuleTitle()
        {
            return "Radio Transmitter";
        }
        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

        public string GetPrimaryField()
        {
            return "";
        }
        //public override string GetInfo()
        //{
        //    return "KPBR_TransmitterPartModule" +resHandler.PrintModuleResources() ;
        //}
        /* ************************************************************************************************
     * Function Name: GetInfo
     * Input: None
     * Output: Information that is displayed through the details pane in the KSP editor.
     * Purpose: This function will generate the report that you see in the mouse over in the KSP
     * editor.
     * ************************************************************************************************/
        public override string GetInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine("KPBR_Radio Transmitter");

            sb.AppendLine();


            sb.AppendLine("<color=#99ff00ff>Requires:</color>");
            sb.AppendLine("- ElectricCharge: " + string.Format("{0:0.0##}", consumeRate) + "/sec");

            sb.AppendLine();

            return sb.ToString();
        }

        public void FixedUpdate()
        {
            if (location != null && location != "")
            {
                var demand = consumeRate * TimeWarp.deltaTime;
                double PowerDemand = part.RequestResource("ElectricCharge", demand);
                // Yuck, floating point error here, need to add a tiny bit to demand to bypass
                if (PowerDemand + 0.000001f < demand)
                {
                    Log.Info("FixedUpdate, demand: " + demand + ", PowerDemand: " + PowerDemand + ", consumeRate: " + consumeRate + ", deltaTime: " + TimeWarp.deltaTime);
                    HasPower = false;
                }
                else
                    HasPower = true;
            }
        }

#if false
        #region BackgroundProcessing

        private const String MAIN_POWER_NAME = "ElectricCharge";

        //This method is called by the BackgroundProcessing DLL, if the user has installed it. Otherwise it will never be called.
        //It will consume ElectricCharge for vessels with active transmitter parts for vessels that are unloaded
        public static void FixedBackgroundUpdate(Vessel v, uint partFlightID, Func<Vessel, float, string, float> resourceRequest, ref Object data)
        {
            if (Time.timeSinceLevelLoad < 2.0f || CheatOptions.InfiniteElectricity) // Check not loading level
            {
                return;
            }
#if false
            if (DFIntMemory.Instance && DFIntMemory.Instance.BGRinstalled) //If Background Resources mod is installed. Don't do BackgroundProcessing Mod work.
            {
                return;
            }
#endif
            bool debug = true;
#if false
            try
            {
                debug = DeepFreeze.Instance.DFsettings.debugging;
            }
            catch
            {
                Utilities.Log("DeepFreeze FixedBackgroundUpdate failed to get debug setting");
            }
#endif
            if (debug) Debug.Log("FixedBackgroundUpdate vesselID " + v.id + " partID " + partFlightID);
#if false
            // If the user does not have ECreqdForFreezer option ON, then we do nothing and return
            if (!DeepFreeze.Instance.DFsettings.ECreqdForFreezer)
            {
                //if (debug) Debug.Log("FixedBackgroundUpdate ECreqdForFreezer is OFF, nothing to do");
                return;
            }
#endif
            // If the vessel this module is attached to is NOT stored in the DeepFreeze dictionary of known deepfreeze vessels we can't do anything, But this should NEVER happen.
            VesselInfo vslinfo;
            if (!DeepFreeze.Instance.DFgameSettings.knownVessels.TryGetValue(v.id, out vslinfo))
            {
                if (debug) Debug.Log("FixedBackgroundUpdate unknown vessel, cannot process");
                return;
            }
            //Except if there are no frozen crew on board we don't need to consume any EC
            if (vslinfo.numFrznCrew == 0)
            {
                //if (debug) Debug.Log("FixedBackgroundUpdate No Frozen Crew on-board, nothing to do");
                return;
            }
            PartInfo partInfo;
            if (!DeepFreeze.Instance.DFgameSettings.knownFreezerParts.TryGetValue(partFlightID, out partInfo))
            {
                if (debug) Debug.Log("FixedBackgroundUpdate Can't get the Freezer Part Information, so cannot process");
                return;
            }
            // OK now we have something to do for real.
            // Calculate the time since last consumption of EC, then calculate the EC required and request it from BackgroundProcessing DLL.
            // If the vessel runs out of EC the DeepFreezeGUI class will handle notifying the user, not here.
            double currenttime = Planetarium.GetUniversalTime();
            if (Utilities.timewarpIsValid(5))
            {
                double timeperiod = currenttime - partInfo.timeLastElectricity;
                if (timeperiod >= 1f && partInfo.numFrznCrew > 0) //We have frozen Kerbals, consume EC
                {
                    double Ecreqd = partInfo.frznChargeRequired / 60.0f * timeperiod * vslinfo.numFrznCrew;
                    if (debug) Debug.Log("FixedBackgroundUpdate timeperiod = " + timeperiod + " frozenkerbals onboard part = " + vslinfo.numFrznCrew + " ECreqd = " + Ecreqd);
                    float Ecrecvd = 0f;
                    Ecrecvd = resourceRequest(v, (float)Ecreqd, MAIN_POWER_NAME);

                    if (debug) Debug.Log("Consumed Freezer EC " + Ecreqd + " units");

                    if (Ecrecvd >= (float)Ecreqd * 0.99)
                    {
                        if (OnGoingECMsg != null) ScreenMessages.RemoveMessage(OnGoingECMsg);
                        partInfo.timeLastElectricity = (float)currenttime;
                        partInfo.deathCounter = currenttime;
                        partInfo.outofEC = false;
                        partInfo.ECWarning = false;
                        vslinfo.storedEC -= Ecrecvd;
                    }
                    else
                    {
                        if (debug) Debug.Log("FixedBackgroundUpdate DeepFreezer Ran out of EC to run the freezer");
                        if (!partInfo.ECWarning)
                        {
                            ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_DF_00072"), 10.0f, ScreenMessageStyle.UPPER_CENTER); //#autoLOC_DF_00072 = Insufficient electric charge to monitor frozen kerbals.
                            partInfo.ECWarning = true;
                            partInfo.deathCounter = currenttime;
                        }
                        if (OnGoingECMsg != null) ScreenMessages.RemoveMessage(OnGoingECMsg);
                        OnGoingECMsg = ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_DF_00073", (deathRoll - (currenttime - partInfo.deathCounter)).ToString("######0"))); //#autoLOC_DF_00073 = \u0020Freezer Out of EC : Systems critical in <<1>> secs
                        partInfo.outofEC = true;
                        if (debug) Debug.Log("FixedBackgroundUpdate deathCounter = " + partInfo.deathCounter);
                        if (currenttime - partInfo.deathCounter > deathRoll)
                        {
                            if (DeepFreeze.Instance.DFsettings.fatalOption)
                            {
                                if (debug) Debug.Log("FixedBackgroundUpdate deathRoll reached, Kerbals all die...");
                                partInfo.deathCounter = currenttime;
                                //all kerbals dies
                                var kerbalsToDelete = new List<string>();
                                foreach (KeyValuePair<string, KerbalInfo> kerbal in DeepFreeze.Instance.DFgameSettings.KnownFrozenKerbals)
                                {
                                    if (kerbal.Value.partID == partFlightID && kerbal.Value.vesselID == v.id && kerbal.Value.type != ProtoCrewMember.KerbalType.Tourist)
                                    {
                                        kerbalsToDelete.Add(kerbal.Key);
                                    }
                                }
                                foreach (string deathKerbal in kerbalsToDelete)
                                {
                                    DeepFreeze.Instance.KillFrozenCrew(deathKerbal);
                                    ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_DF_00074", deathKerbal), 10.0f, ScreenMessageStyle.UPPER_CENTER); //#autoLOC_DF_00074 = <<1>> died due to lack of Electrical Charge to run cryogenics
                                    if (debug) Debug.Log("FixedBackgroundUpdate DeepFreezer - kerbal " + deathKerbal + " died due to lack of Electrical charge to run cryogenics");
                                }
                                kerbalsToDelete.ForEach(id => DeepFreeze.Instance.DFgameSettings.KnownFrozenKerbals.Remove(id));
                            }
                            else //NON Fatal option - emergency thaw all kerbals.
                            {
                                // Cannot emergency thaw in background processing. It is expected that DeepFreezeGUI will pick up that EC has run out and prompt the user to switch to the vessel.
                                // When the user switches to the vessel the DeepFreezer partmodule will detect no EC is available and perform an emergency thaw procedure.
                                if (debug) Debug.Log("FixedBackgroundUpdate DeepFreezer - EC has run out non-fatal option");
                            }
                        }
                    }
                }
            }
            else  //Timewarp is too high
            {
                if (debug) Debug.Log("FixedBackgroundUpdate Timewarp is too high to backgroundprocess");
                partInfo.outofEC = false;
            }
        }

#endregion BackgroundProcessing
#endif

    }

}
