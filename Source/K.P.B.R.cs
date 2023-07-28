using UnityEngine;
using ClickThroughFix;
using ToolbarControl_NS;
using SpaceTuxUtility;
using KSP.UI.Screens;
using System.Collections;

using static KPRS.RegisterToolbar;
using KPRS.PartModules;
using static ConfigNode;
using UnityEngine.UI;
using System;
using static KSP.UI.Screens.RDNode;
using static EdyCommonTools.Spline;

namespace KPRS
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class KPBR : MonoBehaviour
    {

        float UIScale = 1f;
        internal static KPBR Instance { get; set; }
        internal const string MODID = "K.P.R.S.";
        internal const string MODNAME = "Kerbal Public Radio Service";


        const float BASE_HEIGHT = 353f;
        const float BASE_WIDTH = 800f;

        float HEIGHT = BASE_HEIGHT;
        float WIDTH = BASE_WIDTH;

        [KSPField(isPersistant = true)]
        private Rect windowRect = new Rect(50f, 50, BASE_WIDTH, BASE_HEIGHT);

        [KSPField(isPersistant = true)]
        internal Rect transWindowRect = new Rect(50f, 25f, KPBR_TransmitterPartModule.TRANS_SEL_WIDTH, KPBR_TransmitterPartModule.TRANS_SEL_HEIGHT);

        static internal ToolbarControl toolbarControl = null;
        Texture2D facePlateImg = null;
        Texture2D powerImg = null;
        Texture2D shuffleImg = null;
        Texture2D shuffleNowImg = null;
        Texture2D dialImg; // = new Texture2D();
        Texture2D redBtn = new Texture2D(11, 15);
        Texture2D greenBtn = new Texture2D(11, 15);
        Texture2D preampBtn = new Texture2D(60, 11);

        const float INITIAL_VOL_ANGLE = 150f;

        float volAngle = INITIAL_VOL_ANGLE;
        float lastAngle = INITIAL_VOL_ANGLE;

        //float preampPower = 0;
        float imageSizeAdjustment;

        const string FACEPLATE = "ATSPTX705";
        const string DIAL = "dial";
        const string POWER = "power";

        internal static SoundPlayer soundPlayer;
        internal static SoundPlayer staticSoundPlayer;
        internal const string SOUND_DIR = "KPRS/";

        PlayActivePlaylist playActivePlaylist = null;

        bool playerOn = true;
        bool volumesInitted = false;

        internal GameSettings gameSettings;

        const float BASE_TRANSMITTER_POWER = 46400000f;
        const float BASE_ANTENNA_HEIGHT = 8.95f;

        void Awake()
        {
            GameObject myplayer = new GameObject();
            soundPlayer = myplayer.AddComponent<SoundPlayer>();
            soundPlayer.Name = "soundPlayer";

            staticSoundPlayer = myplayer.AddComponent<SoundPlayer>();
            staticSoundPlayer.Name = "staticSoundPlayer";

            gameSettings = new GameSettings();

        }

        void onGameSceneLoadRequested(GameScenes scene)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                soundPlayer.StopSound();
                staticSoundPlayer.StopSound();
                if (playActivePlaylist != null)
                    playActivePlaylist.Clear("onGameSceneLoadRequested");
            }
        }

        void onLevelWasLoadedGUIReady(GameScenes newScene)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                soundPlayer.Initialize(); // Initializes the player, does some housekeeping
                staticSoundPlayer.Initialize();
                staticSoundPlayer.LoadNewSound(SOUND_DIR + "PluginData/Static/static");
                staticSoundPlayer.audioSource.loop = true;
                //staticSoundPlayer.PlaySound();
            }
        }

        Rect powerLEDRect = new Rect(31, 154, 11, 15);
        Rect powerBtnRect = new Rect(53, 151, 43, 22);
        Rect preAmpDownRect = new Rect(28, 174, 28, 22);
        Rect preAmpUpRect = new Rect(68, 174, 28, 22);
        Rect preampBtnRect = new Rect(32, 158, 60, 11);

        Rect preset1BtnRect = new Rect(161, 320, 46, 22);
        Rect preset2BtnRect = new Rect(229, 320, 46, 22);
        Rect preset3BtnRect = new Rect(297, 320, 46, 22);
        Rect preset4BtnRect = new Rect(365, 320, 46, 22);
        Rect preset5BtnRect = new Rect(433, 320, 46, 22);

        Rect shuffleImgRect = new Rect(555, 151, 40, 19);
        Rect shuffleNowImgRect = new Rect(555, 185, 40, 19);

        Rect kRect = new Rect(555, 219, 40, 19);
        Rect lRect = new Rect(555, 253, 40, 19);

        Rect volDownRect = new Rect(539, 316, 36, 20);
        Rect volUpRect = new Rect(588, 316, 36, 20);

        Vector2 volImgSizeSize = new Vector2(150, 150);

        const float VOL_DIAL_RADIUS = 75f;

        Rect volumeDialRect = new Rect(632, 205, 2 * VOL_DIAL_RADIUS, 2 * VOL_DIAL_RADIUS);

        Rect guiScaleRect = new Rect(100, 10, 616, 10f);
        Rect ScaledRect(float x, float y, float width, float height)
        {
            return new Rect(x * UIScale, y * UIScale, width * UIScale, height * UIScale);
        }
        void OnGameSettingsApplied()
        {
            UIScale = HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().uiScale / 100f;

            HEIGHT = BASE_HEIGHT * UIScale;
            WIDTH = BASE_WIDTH * UIScale;

            windowRect = new Rect(windowRect.x, windowRect.y, WIDTH, HEIGHT);
            viewRect = new Rect(0, 20, 300 * UIScale, HEIGHT);

            // Use ScaledRect when ALL the values are scaled

            stationRect = ScaledRect(128f, 90f, 384, BASE_HEIGHT - 175);

            notificationRect = ScaledRect(128f, 55f, 384, 45f);
            viewRect = ScaledRect(0, 20, 300, BASE_HEIGHT);
            powerLEDRect = ScaledRect(31, 154, 11, 15);
            powerBtnRect = ScaledRect(53, 151, 43, 22);
            preAmpDownRect = ScaledRect(28, 194, 28, 22);
            preAmpUpRect = ScaledRect(68, 194, 28, 22);
            preampBtnRect = ScaledRect(32, 178, 60, 11);

            preset1BtnRect = ScaledRect(161, 320, 46, 22);
            preset2BtnRect = ScaledRect(229, 320, 46, 22);
            preset3BtnRect = ScaledRect(297, 320, 46, 22);
            preset4BtnRect = ScaledRect(365, 320, 46, 22);
            preset5BtnRect = ScaledRect(433, 320, 46, 22);

            shuffleImgRect = ScaledRect(555, 151, 40, 19);
            shuffleNowImgRect = ScaledRect(555, 185, 40, 19);

            kRect = ScaledRect(555, 219, 40, 19);
            lRect = ScaledRect(555, 253, 40, 19);

            volDownRect = ScaledRect(539, 316, 36, 20);
            volUpRect = ScaledRect(588, 316, 36, 20);

            volumeDialRect = ScaledRect(632, 205, 2 * VOL_DIAL_RADIUS, 2 * VOL_DIAL_RADIUS);
            guiScaleRect = ScaledRect(100, 10, 616, 10f);

            volImgSizeSize = new Vector2(150 * UIScale, 150 * UIScale);


            radioLabelFontBoldYellow.fontSize =
                radioLabelFontBoldBlue.fontSize =
                labelFontBoldRed.fontSize = (int)(16f * UIScale);
            labelFontBoldLarge.fontSize = (int)(22f * UIScale);
        }

        void onVesselChange(Vessel v)
        {
            Log.Info("onVesselChange");
            if (HighLogic.LoadedSceneIsFlight)
            {
                soundPlayer.StopSound();
                staticSoundPlayer.StopSound();
                if (playActivePlaylist != null)
                    playActivePlaylist.Clear("onVesselChange");
                volumesInitted = false;
                guiActive = false;

                volAngle = lastAngle = HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().initialVolume;

            }
        }

        public void Start()
        {
            Log.Info("KPBR.Start");
            Instance = this;
            DontDestroyOnLoad(this);
            AddToolbarButton();
            radioWinId = WindowHelper.NextWindowId("K.B.R.S-Radio");

            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelWasLoadedGUIReady);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);
            GameEvents.onVesselChange.Add(onVesselChange);
            GameEvents.OnGameSettingsApplied.Add(OnGameSettingsApplied);

            if (ToolbarControl.LoadImageFromFile(ref facePlateImg, KSPUtil.ApplicationRootPath + "GameData/KPRS/PluginData/Textures/" + FACEPLATE))
            {
                imageSizeAdjustment = WIDTH / facePlateImg.width;
                windowRect = new Rect(50f, 25f, WIDTH, WIDTH * facePlateImg.height / facePlateImg.width);
                windowRect = new Rect(50f, 50, WIDTH, imageSizeAdjustment * facePlateImg.height);

            }
            else
                Log.Error("Unable to load faceplate image");

            if (!ToolbarControl.LoadImageFromFile(ref powerImg, KSPUtil.ApplicationRootPath + "GameData/KPRS/PluginData/Textures/" + POWER))
            {
                Log.Error("Unable to load power image");
            }
            if (!ToolbarControl.LoadImageFromFile(ref shuffleNowImg, KSPUtil.ApplicationRootPath + "GameData/KPRS/PluginData/Textures/" + "shuffle"))
            {
                Log.Error("Unable to load shuffleNow image");
            }
            if (!ToolbarControl.LoadImageFromFile(ref shuffleImg, KSPUtil.ApplicationRootPath + "GameData/KPRS/PluginData/Textures/" + "shuffle-crossing-arrows"))
            {
                Log.Error("Unable to load shuffle image");
            }


            if (!ToolbarControl.LoadImageFromFile(ref dialImg, KSPUtil.ApplicationRootPath + "GameData/KPRS/PluginData/Textures/" + DIAL))
            {
                Log.Error("Unable to load dial image");
            }

            var pixels = redBtn.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = new Color32(255, 0, 0, 255);
            redBtn.SetPixels32(pixels);
            redBtn.Apply();

            pixels = greenBtn.GetPixels32();
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = new Color32(0, 255, 0, 255);
            greenBtn.SetPixels32(pixels);
            greenBtn.Apply();

            SetPreampButtonColor(0);

            StartCoroutine(SlowUpdate());
            OnGameSettingsApplied();
        }


        void SetPreampButtonColor(float val)
        {
            var pixels = preampBtn.GetPixels32();
            byte b = (byte)Mathf.Floor(25.5f * val);
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = new Color32(b, b, (byte)(255 - b), 255);
            }
            preampBtn.SetPixels32(pixels);
            preampBtn.Apply();
        }

        void DumpConfigNode(ConfigNode node)
        {

            foreach (Value n in node.values)
            {
                Log.Info("Value name: " + n.name + ", value: " + n.value);
            }
        }

        RadioPartModule activeRadioAntenna = null;
        bool slowUpdateStarted = false;
        //
        // This is a method to do housekeeping tasks that can take time and don't need to be done every clock tick
        //
        IEnumerator SlowUpdate()
        {
            if (slowUpdateStarted)
                yield break;
            slowUpdateStarted = true;
            Log.Info("KPBR.SlowUpdate");

            int cnt = 0;

            yield return new WaitForSeconds(1f);

            while (true)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (playActivePlaylist != null && playerOn && volumesInitted)
                    {
                        playActivePlaylist.Play("SlowUpdate");
                        staticSoundPlayer.PlaySound("SlowUpdate");
                    }
                }
                yield return new WaitForSeconds(1f);
                //Log.Info("SlowUpdate, cnt: " + cnt);
                if (cnt++ > 5)
                {
                    Statics.vesselInfoList.Clear();
                    cnt = 0;
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        activeRadioAntenna = FlightGlobals.ActiveVessel.FindPartModuleImplementing<RadioPartModule>();
                        if (activeRadioAntenna != null)
                            SetPreampButtonColor(activeRadioAntenna.preampPower);

#if false
                        Log.Info("SlowUpdate, playerOn: " + playerOn + ", activeRadioAntenna: " + (activeRadioAntenna != null).ToString());
                        Log.Info(" ");
                        Log.Info("Updating Transmitter list, total number of vessels: " + FlightGlobals.Vessels.Count);
                        Log.Info(" ");
#endif
                        Statics.transmitterList.Clear();
                        foreach (var s in Statics.stationList)
                            s.Value.selected = false;
                        Vessel v = FlightGlobals.ActiveVessel;
                        //Log.Info("v.GetOrbitDriver().pos: " + v.GetOrbitDriver().pos.ToString());
                        //Log.Info("v.GetOrbitDriver().orbit.pos: " + v.GetOrbitDriver().orbit.pos.ToString());

                        for (int idx = 0; idx < FlightGlobals.Vessels.Count; idx++)
                        {
                            Vessel vessel = FlightGlobals.Vessels[idx];
                            if (vessel && (vessel.vesselType == VesselType.Debris || (vessel.vesselType >= VesselType.Probe && vessel.vesselType <= VesselType.Base)))
                            {
                                var Distance = Vector3d.Distance(v.GetVessel().GetWorldPos3D(), vessel.GetWorldPos3D());

#if false
                               Log.Info("SlowUpdate, Vessel.name: " + vessel.vesselName + ", Distance: " + Distance);
#endif

                                if (vessel.loaded)
                                {
                                    var transmitterPartModuleList = vessel.FindPartModulesImplementing<KPBR_TransmitterPartModule>();
                                    if (transmitterPartModuleList != null && transmitterPartModuleList.Count > 0)
                                    {
                                        VesselInfo vesselInfo = new VesselInfo(vessel);

                                        vessel.GetConnectedResourceTotals(RegisterToolbar.resourceID, out double amount, out double maxAmount);
                                        vesselInfo.StoredEC = amount;

                                        foreach (KPBR_TransmitterPartModule transmitterPartModule in transmitterPartModuleList)
                                        {
#if false
                                            if (t != null)
                                            {
                                                Log.Info("vesselLoaded: " + vessel.loaded + ": " + vessel.vesselName);
                                                Log.Info("StationSelected: " + t.StationSelected + ", LocationSelected: " + t.LocationSelected);
                                            }
                                            else
                                                Log.Info("vesselLoaded: " + vessel.loaded + ": " + vessel.vesselName + ", no KPBR_TransmitterPartModule found");
#endif
                                            if (transmitterPartModule != null && transmitterPartModule.StationSelected && transmitterPartModule.LocationSelected)
                                            {
#if false
                                        Log.Info("Loaded, antenna, selectedStation: " + t.selectedStation + ", location: " + t.location);
#endif

                                                Statics.transmitterList[transmitterPartModule.selectedStation] = new Transmitter(transmitterPartModule);
                                                if (Statics.stationList.ContainsKey(transmitterPartModule.selectedStation))
                                                    Statics.stationList[transmitterPartModule.selectedStation].selected = true;
                                            }
                                        }
                                    }
                                }
                                else
                                {
#if false
                                    var transmitterPartsList = vessel.FindPartModulesImplementing<KPBR_TransmitterPartModule>();
                                    foreach (var transmitterPart in transmitterPartsList)
                                    {


                                        if (transmitterPart != null)
                                        {
                                            Log.Info("Unloaded: " + vessel.loaded + ": " + vessel.vesselName);
                                            Log.Info("StationSelected: " + transmitterPart.StationSelected + ", LocationSelected: " + transmitterPart.LocationSelected);
                                        }
                                        else
                                            Log.Info("Unloaded: " + vessel.loaded + ": " + vessel.vesselName + ", no KPBR_TransmitterPartModule found");

                                        if (transmitterPart != null && transmitterPart.StationSelected && transmitterPart.LocationSelected)
                                        {
                                            Statics.transmitterList[transmitterPart.selectedStation] = new Transmitter(transmitterPart);
                                            Log.Info("Unloaded, antenna, selectedStation: " + transmitterPart.selectedStation + ", location: " + transmitterPart.location);
                                        }
                                    }
#endif

                                    for (int idx3 = 0; idx3 < vessel.protoVessel.protoPartSnapshots.Count; idx3++)
                                    {
                                        ProtoPartSnapshot p = vessel.protoVessel.protoPartSnapshots[idx3];
                                        Part part = p.partPrefab;
                                        //ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");
                                        for (int modIdx = 0; modIdx < p.modules.Count; modIdx++)
                                        {
                                            ProtoPartModuleSnapshot module = p.modules[modIdx];

                                            if (module.moduleName == "KPBR_TransmitterPartModule")
                                            {
                                                string selectedStation = module.moduleValues.SafeLoad("selectedStation", "");
                                                bool stationSelected = (selectedStation.Length > 0);
                                                string location = module.moduleValues.SafeLoad("location", "");
                                                bool active = module.moduleValues.SafeLoad("active", true);
                                                bool locationSelected = (location.Length > 0);
                                                if (stationSelected && locationSelected)
                                                {
                                                    Statics.transmitterList[selectedStation] = new Transmitter(selectedStation, location, vessel, active);
#if false
                                                    Log.Info("Unloaded, antenna, selectedStation: " + selectedStation + ", location: " + location);
#endif
                                                }
#if false
                                                else
                                                    Log.Info("Unloaded,antenna, no selected station, selectedStation: " + selectedStation + ", locationSelected: " + location);
#endif
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (activeRadioAntenna != null && activeRadioAntenna.selectedStation != null && playActivePlaylist == null && playerOn)
                        {
                            if (Statics.stationList.ContainsKey(activeRadioAntenna.selectedStation) &&
                                    Statics.playlist.ContainsKey(Statics.stationList[activeRadioAntenna.selectedStation].playlist))
                            {
#if false
                               Log.Info("SelectedStation: " + activeRadioAntenna.selectedStation);
#endif
                                if (playActivePlaylist == null)
                                    playActivePlaylist = new PlayActivePlaylist("SlowUpdate",
                                        Statics.playlist[Statics.stationList[activeRadioAntenna.selectedStation].playlist], Statics.stationList[activeRadioAntenna.selectedStation].channelCallSign);
                                else
                                    playActivePlaylist.NewPlayActivePlaylist("SlowUpdate",
                                    Statics.playlist[Statics.stationList[activeRadioAntenna.selectedStation].playlist], Statics.stationList[activeRadioAntenna.selectedStation].channelCallSign);
                            }
                        }
                    }
                    else
                        activeRadioAntenna = null;
                }
            }
        }

        void AddToolbarButton()
        {

            if (toolbarControl == null)
            {
                toolbarControl = gameObject.AddComponent<ToolbarControl>();
                toolbarControl.AddToAllToolbars(WinToggle, WinToggle,
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW,
                    MODID,
                    "K.P.R.S.Btn",
                    "KPRS/PluginData/Textures/K.P.R.S",
                    "KPRS/PluginData/Textures/K.P.R.S",
                    MODNAME
                );
            }
        }


        void WinToggle()
        {
            guiActive = !guiActive;
        }

        static GUIStyle _NonSelectableWindowStyle;
        static GUIStyle NonSelectableWindowStyle
        {
            get
            {
                if (_NonSelectableWindowStyle == null)
                {
#if false
                    GUIStyle s = new GUIStyle(GUI.skin.window);

                    s.onNormal.background = null;
                    s.border = new RectOffset(0, 0, 0, 0);
                    s.margin = new RectOffset(0, 0, 0, 0);
                    s.padding = new RectOffset(0, 0, 0, 0);
                    s.overflow = new RectOffset(0, 0, 0, 0);
                    s.imagePosition = ImagePosition.ImageOnly;
                    s.stretchHeight = true;
                    s.stretchWidth = true;

                    _NonSelectableWindowStyle = s;
#endif
                    _NonSelectableWindowStyle = new GUIStyle();
                }
                return _NonSelectableWindowStyle;
            }
        }

        int radioWinId;


        bool guiActive = false;
        internal void OnGUI()
        {
            if (guiActive && HighLogic.LoadedSceneIsFlight)
            {
                if (Config.config.KspSkin)
                {
                    GUI.skin = HighLogic.Skin;
                }
                windowRect = ClickThruBlocker.GUIWindow(radioWinId, windowRect, RadioWin,
                      facePlateImg, style: NonSelectableWindowStyle);
            }
        }

#if true
        //public float angle = 15f;
        bool doRotate = false;
        bool volumeDialInfoInitted = false;
        Vector2 volumeDialPivot;
        //Rect volumeDialRect;
        void InitDialInfo(ref Texture2D dialImg, Rect localPosition, Vector2 size, out Rect newRect)
        {
            var pos = new Vector2(localPosition.x, localPosition.y);
            newRect = new Rect(pos.x, pos.y, size.x, size.y);
            volumeDialPivot = new Vector2(newRect.xMin + newRect.width * 0.5f, newRect.yMin + newRect.height * 0.5f);
            volumeDialInfoInitted |= true;

        }

        enum ButtonImage { Volume, amp }
        void RotateImage(ButtonImage btn, ref Texture2D dialImg, Vector2 size, float angle)
        {
            //Vector2 pivot;
            //if (!volumeDialInfoInitted && btn == ButtonImage.Volume)
            InitDialInfo(ref dialImg, volumeDialRect, size, out Rect newRect);
            Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, volumeDialPivot);
            GUI.DrawTexture(newRect, dialImg);
            GUI.matrix = matrixBackup;
        }
#endif

        void PlaySelectedStation(string selectedStation)
        {
            Log.Info("PlaySelectedStation, selectedStation: " + selectedStation);
            if (activeRadioAntenna.selectedStation != "")
                ClearSelectedStation(selectedStation);
            activeRadioAntenna.selectedStation = selectedStation;
            if (!Statics.stationList.ContainsKey(selectedStation))
                Log.Error("Station not found in stationlist: " + selectedStation);
            var s = Statics.stationList[activeRadioAntenna.selectedStation];
            Log.Info("Station: " + selectedStation + ", playlist: " + s.playlist);
            Log.Info("Playlist info: " + Statics.playlist[s.playlist].ToString());

            if (playActivePlaylist != null)
            {
                soundPlayer.StopSound();
                playActivePlaylist.NewPlayActivePlaylist("PlaySelectedStation", Statics.playlist[s.playlist], Statics.stationList[selectedStation].channelCallSign);
            }
            else
                playActivePlaylist = new PlayActivePlaylist("PlaySelectedStation", Statics.playlist[s.playlist], Statics.stationList[selectedStation].channelCallSign);
            playerOn = true;
        }

        void ClearSelectedStation(string selectedStation)
        {
            playActivePlaylist.Clear("ClearSelectedStation, activeRadioAntenna.selectedStation: " + activeRadioAntenna.selectedStation);
        }

        bool buttonDownFlag = false;
        bool volumeButtonDownFlag = false;

        float curAngle;

        /**
         * Fetches angle relative to screen centre point
         * where 12 O'Clock is 0 and 3 O'Clock is 90 degrees
         * 
         * @param screenPoint
         * @return angle in degress from 0-360.
         */
        public float GetAngle(Vector2 screenPoint, Vector2 center)
        {
            float dx = screenPoint.x - center.x;
            // Minus to correct for coord re-mapping
            float dy = -(screenPoint.y - center.y);

            float inRads = Mathf.Atan2(dy, dx);

            // We need to map to coord system when 0 degree is at 3 O'clock, 270 at 12 O'clock
            if (inRads < 0)
                inRads = Mathf.Abs(inRads);
            else
                inRads = 2 * Mathf.PI - inRads;

            var a = Mathf.Rad2Deg * inRads - 90;
            a = (a < 0) ? a + 360 : a;
            return Mathf.Round(a);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                buttonDownFlag = true;
            }
            else
            {
                if (Input.GetMouseButtonUp(0))
                    buttonDownFlag = volumeButtonDownFlag = false;
            }

            Vector2 MouseOffset;
            MouseOffset.x = Mouse.screenPos.x - windowRect.x;
            MouseOffset.y = Mouse.screenPos.y - windowRect.y;
            Vector2 center;
            center.x = volumeDialRect.left + VOL_DIAL_RADIUS * UIScale;
            center.y = volumeDialRect.top + VOL_DIAL_RADIUS * UIScale;


            if (buttonDownFlag && volumeDialRect.Contains(MouseOffset))
            {
                var Dist = Vector2.Distance(MouseOffset, center);
                if (Dist <= VOL_DIAL_RADIUS * UIScale)
                {
                    if (!volumeButtonDownFlag)
                    {
                        //lastVolumeMouseLoc = MouseOffset;
                        lastAngle = GetAngle(center, MouseOffset);

                        volumeButtonDownFlag = true;
                    }
                    else
                    {
                        curAngle = GetAngle(center, MouseOffset);

                        lastAngle = curAngle;
                    }
                }
                else
                    volumeButtonDownFlag = false;
            }
            else
                volumeButtonDownFlag = false;

        }


        Vector2 stationPos;


        Rect notificationRect = new Rect(128f, 35f, 384, 45);

        Rect stationRect = new Rect(128f, 70f, 384, BASE_HEIGHT - 175);
        Rect viewRect = new Rect(0, 0, 300, BASE_HEIGHT);
        int start = 0;
        Boolean toggle = false;

        int dialAngle = 0;

        void RadioWin(int id)
        {
            var oldUiScale = HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().uiScale;
            HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().uiScale = GUI.HorizontalSlider(guiScaleRect, HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().uiScale, 50, 150f);
            if (HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().uiScale != oldUiScale)
                GameEvents.OnGameSettingsApplied.Fire();
            //OnGameSettingsApplied();

            if (activeRadioAntenna == null)
            {
                GUI.Label(notificationRect, "No active radio antenna found", radioLabelFontBoldYellow);
            }
            else
            {
                if (activeRadioAntenna.selectedStation != "")
                {
                    GUI.Label(notificationRect, "Currently Playing: " + activeRadioAntenna.selectedStation, labelFontBoldLarge);
                }
                else
                {
                    GUI.Label(notificationRect, "No station selected", radioLabelFontBoldYellow);
                }

                stationPos = GUI.BeginScrollView(stationRect, stationPos, viewRect, false, true, GUIStyle.none, RegisterToolbar.vertScrollbarStyle);
                float lineCnt = 0;
                foreach (var transmitter in Statics.transmitterList)
                {
                    Station station = null;
                    string channelDescr = "";

                    if (Statics.stationList.ContainsKey(transmitter.Key))
                    {
                        station = Statics.stationList[transmitter.Key];
                        channelDescr = "Stn: " + station.channelNumber.ToString() + " - ";

                        channelDescr = channelDescr + transmitter.Key;

                        if (activeRadioAntenna.selectedStation == transmitter.Key)
                        {
                            if (transmitter.Value.location != null && transmitter.Value.location != "" && transmitter.Value.Active)
                            {
                                if (GUI.Button(new Rect(30, 25 * (lineCnt - start), WIDTH - 70, 20), channelDescr, radioLabelFontBoldYellow))
                                {
                                    soundPlayer.StopSound();
                                    if (playActivePlaylist != null)
                                        playActivePlaylist.Clear("RadioWin");
                                    playerOn = false;
                                    ClearSelectedStation(transmitter.Key);
                                }
                            }
                            else
                            {
                                if (transmitter.Value.Active)
                                    GUI.Button(new Rect(30, 25 * (lineCnt - start), WIDTH - 70, 20), channelDescr, radioLabelFontBoldBlue);
                                else
                                    GUI.Button(new Rect(30, 25 * (lineCnt - start), WIDTH - 70, 20), channelDescr, labelFontBoldRed);
                            }
                        }
                        else
                        {
                            if (transmitter.Value.Active)
                            {
                                if (GUI.Button(new Rect(30, 25 * (lineCnt - start), WIDTH - 70, 20), channelDescr, GUI.skin.label))
                                {

#if false
                                if (activeRadioAntenna.selectedStation == transmitter.Key)
                                {
                                    activeRadioAntenna.selectedStation = "";
                                    soundPlayer.StopSound();
                                    if (playActivePlaylist != null)
                                        playActivePlaylist.Clear("RadioWin");
                                    playerOn = false;
                                }
                                else
#endif
                                    {
                                        if (playActivePlaylist != null)
                                            playActivePlaylist.Clear("RadioWin");
                                        soundPlayer.StopSound();
                                        //if (toggle)
                                        PlaySelectedStation(transmitter.Key);
                                        toggle = !toggle;
                                    }
                                }
                            }
                            else
                            {
                                GUI.Button(new Rect(30, 25 * (lineCnt - start), WIDTH - 70, 20), channelDescr, labelFontBoldRed);
                            }
                        }
                        lineCnt++;
                    }
                }
                GUI.EndScrollView();
            }

            // leds
            if (playerOn)
                GUI.DrawTexture(powerLEDRect, greenBtn);
            else
                GUI.DrawTexture(powerLEDRect, redBtn);


            // Now the buttons, starting at the left
            GUIStyle btnCenter = new GUIStyle();
            btnCenter.imagePosition = ImagePosition.ImageOnly;


            //alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(powerBtnRect, powerImg, btnCenter)) // style: NonSelectableWindowStyle))
            {
                playerOn = !playerOn;
                soundPlayer.ToggleSound();
                staticSoundPlayer.ToggleSound();
            }
            if (GUI.Button(preAmpDownRect, "↓"))
            {
                if (activeRadioAntenna != null && Input.GetMouseButtonUp(0))
                {
                    activeRadioAntenna.preampPower = Mathf.Max(activeRadioAntenna.preampPower - 1f, 0);
                    SetPreampButtonColor(activeRadioAntenna.preampPower);

                }
            }
            if (GUI.Button(preAmpUpRect, "↑"))
            {
                if (activeRadioAntenna != null && Input.GetMouseButtonUp(0))
                {
                    activeRadioAntenna.preampPower = Mathf.Min(activeRadioAntenna.preampPower + 1f, 10);
                    SetPreampButtonColor(activeRadioAntenna.preampPower);

                }
            }
            GUI.DrawTexture(preampBtnRect, preampBtn);

            // Now the row of buttons at the bottom, left to right
            if (GUI.Button(preset1BtnRect, activeRadioAntenna != null && activeRadioAntenna.preset1 != "" ? Statics.stationList[activeRadioAntenna.preset1].abbr.ToString() : ""))
            {
                if (activeRadioAntenna != null)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (activeRadioAntenna.preset1 != "")
                        {
                            PlaySelectedStation(activeRadioAntenna.preset1);
                        }
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        if (activeRadioAntenna.preset1 == "")
                            activeRadioAntenna.preset1 = activeRadioAntenna.selectedStation;
                        else
                            activeRadioAntenna.preset1 = "";
                    }
                }
            }
            if (GUI.Button(preset2BtnRect, activeRadioAntenna != null && activeRadioAntenna.preset2 != "" ? Statics.stationList[activeRadioAntenna.preset2].abbr.ToString() : ""))
            {
                if (activeRadioAntenna != null)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (activeRadioAntenna.preset2 != "")
                        {
                            PlaySelectedStation(activeRadioAntenna.preset2);
                        }
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        if (activeRadioAntenna.preset2 == "")
                            activeRadioAntenna.preset2 = activeRadioAntenna.selectedStation;
                        else activeRadioAntenna.preset2 = "";
                    }
                }
            }
            if (GUI.Button(preset3BtnRect, activeRadioAntenna != null && activeRadioAntenna.preset3 != "" ? Statics.stationList[activeRadioAntenna.preset3].abbr.ToString() : ""))
            {
                if (activeRadioAntenna != null)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (activeRadioAntenna.preset3 != "")
                        {
                            PlaySelectedStation(activeRadioAntenna.preset3);
                        }
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        if (activeRadioAntenna.preset3 == "")
                            activeRadioAntenna.preset3 = activeRadioAntenna.selectedStation;
                        else activeRadioAntenna.preset3 = "";
                    }
                }
            }
            if (GUI.Button(preset4BtnRect, activeRadioAntenna != null && activeRadioAntenna.preset4 != "" ? Statics.stationList[activeRadioAntenna.preset4].abbr.ToString() : ""))
            {
                if (activeRadioAntenna != null)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (activeRadioAntenna.preset4 != "")
                        {
                            PlaySelectedStation(activeRadioAntenna.preset4);
                        }
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        if (activeRadioAntenna.preset4 == "")
                            activeRadioAntenna.preset4 = activeRadioAntenna.selectedStation;
                        else
                            activeRadioAntenna.preset4 = "";
                    }
                }
            }
            if (GUI.Button(preset5BtnRect, activeRadioAntenna != null && activeRadioAntenna.preset5 != "" ? Statics.stationList[activeRadioAntenna.preset5].abbr.ToString() : ""))
            {
                if (activeRadioAntenna != null)
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        if (activeRadioAntenna.preset5 != "")
                        {
                            PlaySelectedStation(activeRadioAntenna.preset5);
                        }
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        if (activeRadioAntenna.preset5 == "")
                            activeRadioAntenna.preset5 = activeRadioAntenna.selectedStation;
                        else activeRadioAntenna.preset5 = "";
                    }
                }
            }

            // Now the 4 vertical buttons
            // 555x153
            // 591x172
            if (GUI.Button(shuffleImgRect, shuffleImg, btnCenter))
            {
                if (activeRadioAntenna != null && activeRadioAntenna.StationSelected)
                {
                    activeRadioAntenna.shuffle = !activeRadioAntenna.shuffle;
                    if (playActivePlaylist != null)
                    {
                        playActivePlaylist.ToggleShuffle();
                        playerOn = true;
                    }
                }
            }
            // 187
            if (GUI.Button(shuffleNowImgRect, shuffleNowImg, btnCenter))
            {
                if (activeRadioAntenna != null)
                {
                    playActivePlaylist.ShufflePlaylist();
                    playerOn = true;
                }
            }
            if (GUI.Button(kRect, "k"))
            {

            }
            // 187
            if (GUI.Button(lRect, "l"))
            {

            }

            // Two buttons at the lower-right, just to the left of the dial
            Vector2 m = Mouse.screenPos;
            m.x = m.x - windowRect.x;
            m.y = m.y - windowRect.y;

            GUI.Button(volDownRect, "v");
            if ((volDownRect).Contains(m) && (buttonDownFlag))
            {
                lastAngle = volAngle = Mathf.Max(0, volAngle - 1);
            }
            GUI.Button(volUpRect, "^");
            if ((volUpRect).Contains(m) && (buttonDownFlag))
            {
                lastAngle = volAngle = Mathf.Min(300, volAngle + 1);
            }

            if (volAngle < lastAngle)
                volAngle = Mathf.Min(300, 1 + volAngle);
            if (volAngle > lastAngle)
                volAngle--;

            float radioVolume = 0.0033333333333333f * volAngle;    // 0-1



            if (activeRadioAntenna != null)
            {
                float transHeightAdjustment = 0;
                if (Statics.transmitterList.ContainsKey(activeRadioAntenna.selectedStation))
                    transHeightAdjustment = Statics.transmitterList[activeRadioAntenna.selectedStation].towerHeight / BASE_ANTENNA_HEIGHT;

                //Log.Info("transHeightAdjustment: " + transHeightAdjustment + ", BASE_TRANS_POWER: " + BASE_TRANSMITTER_POWER);
                //float maxVol = Mathf.Min(1, radioVolume * transHeightAdjustment);


                if (Statics.transmitterList.ContainsKey(activeRadioAntenna.selectedStation))
                {
                    double Distance = Vector3d.Distance(FlightGlobals.ActiveVessel.GetWorldPos3D(), Statics.transmitterList[activeRadioAntenna.selectedStation].vessel.GetWorldPos3D()); ;
                    float preampFinal = activeRadioAntenna.preampPower;
                    float signalStrength = (float)(Statics.stationList[activeRadioAntenna.selectedStation].power *
                        transHeightAdjustment * BASE_TRANSMITTER_POWER *
                        Mathf.Max(1f, preampFinal) / Distance);

                    signalStrength = Mathf.Max(signalStrength, HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().minSignalStrength);
                    var finalRadioVolume = radioVolume * Mathf.Min(1f, signalStrength);

                    float staticVolume = radioVolume - finalRadioVolume;

                    soundPlayer.SetVolume(finalRadioVolume);
                    staticSoundPlayer.SetVolume(staticVolume);
#if false
                    Log.Info("finalRadioVolume: " + finalRadioVolume + ", radioVolume: " + radioVolume + ", staticVolume: " + staticVolume +
                        ", signalStrength: " + signalStrength +
                        ", preampFinal: " + preampFinal);
#endif
                    volumesInitted = true;
                }
                else
                {
                    if (activeRadioAntenna.selectedStation != "")
                        Log.Error("Missing key in Statics.transmitterlist: " + activeRadioAntenna.selectedStation);
                    staticSoundPlayer.SetVolume(radioVolume);
                }
            }
            else
            {
                staticSoundPlayer.SetVolume(radioVolume);
                //Log.Info("staticVolume set without active station");
            }

            //Log.Info("Rotating image");
            RotateImage(ButtonImage.Volume, ref dialImg, volImgSizeSize, volAngle);
            doRotate = false;
            if (!volumeButtonDownFlag)
                GUI.DragWindow();

        }
    }
}
