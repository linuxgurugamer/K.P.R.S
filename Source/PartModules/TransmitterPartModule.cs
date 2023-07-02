using ClickThroughFix;
using SpaceTuxUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static KSP.UI.Screens.Settings.SettingsSetup;
using static KPRS.RegisterToolbar;
using static KPRS.Statics;
using System.Collections;
using KSP.Localization;

namespace KPRS.PartModules
{
    public class TransmitterPartModule : PartModule, IModuleInfo
    {
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
        public override string GetInfo()
        {
            return "TransmitterPartModule";
        }


        internal bool StationSelected { get {  return selectedStation != null && selectedStation.Length>0; } }

        [KSPField(isPersistant = true, guiName = "Radio station")]
        internal string selectedStation = "";

        [KSPField(isPersistant = true, guiName = "Location")]
        internal string location = "";

        internal bool LocationSelected {  get { return location !=null && location.Length > 0; } }

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

        [KSPEvent(guiActive = true, requireFullControl = false, guiActiveUncommand = true,
            guiName = "Activate Station")]
        public void ActivateStation()
        {
            this.location = FlightGlobals.ActiveVessel.mainBody.bodyName;
            Events["ActivateStation"].guiActive = false;
            Events["DeactivateStation"].guiActive = true;

            Events["ToggleStationSelection"].guiActiveEditor = false;
            Events["ToggleStationSelection"].guiActive = false;

        }

        [KSPEvent(guiActive = true, requireFullControl = false, guiActiveUncommand = true,
            guiName = "Deactivate Station")]
        public void DeactivateStation()
        {
            this.location = "";
            transmitterList[selectedStation] = new Transmitter(selectedStation, this.location);
            Events["ActivateStation"].guiActive = true;
            Events["DeactivateStation"].guiActive = false;

            Events["ToggleStationSelection"].guiActiveEditor = true;
            Events["ToggleStationSelection"].guiActive = true;

        }

        void Start()
        {
            Log.Info("TransmitterPartModule.Start");
            if (HighLogic.LoadedSceneIsFlight)
            {
                Log.Info("vessel: " + this.part.vessel.vesselName);
                Log.Info("selectedStation: " + selectedStation.ToString());
                Log.Info("location: " + location.ToString());
            }

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
                    transmitterList[selectedStation] = new Transmitter(selectedStation, this.location);
                }
            }
            if (location == null || location == "")
            {
                Events["ActivateStation"].guiActive = false;
            }
            radioWinId = WindowHelper.NextWindowId("K.P.R.S-TransStationSel");
            StartCoroutine(SlowUpdate());
        }

        IEnumerator SlowUpdate()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                if (this.location != "")
                {
                    string curLoc = this.location;
                    if (!part.vessel.Landed && part.vessel.situation != Vessel.Situations.ORBITING)
                    {
                        this.location = "";
                        Events["ActivateStation"].guiActive = true;
                        Events["DeactivateStation"].guiActive = false;
                        transmitterList[selectedStation] = new Transmitter(selectedStation, this.location);
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


    }

}
