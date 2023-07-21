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

namespace KPRS
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public class KPBR : MonoBehaviour
    {

        internal static KPBR Instance { get; set; }
        internal const string MODID = "K.P.R.S.";
        internal const string MODNAME = "Kerbal Public Radio Service";

        const float HEIGHT = 353f;
        const float WIDTH = 800f;

        [KSPField(isPersistant = true)]
        private Rect windowRect = new Rect(50f, 50, WIDTH, HEIGHT);

        [KSPField(isPersistant = true)]
        internal Rect transWindowRect = new Rect(50f, 25f, KPBR_TransmitterPartModule.TRANS_SEL_WIDTH, KPBR_TransmitterPartModule.TRANS_SEL_HEIGHT);

        static internal ToolbarControl toolbarControl = null;
        Texture2D facePlateImg = null;
        Texture2D powerImg = null;
        Texture2D shuffleImg = null;
        Texture2D shuffleNowImg = null;
        Texture2D[] dialImg = new Texture2D[8];
        Texture2D redBtn = new Texture2D(11, 15);
        Texture2D greenBtn = new Texture2D(11, 15);
        Texture2D preampBtn = new Texture2D(60, 11);

        int volImgNum = 3;
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
                //staticSoundPlayer.PlaySound();
            }
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


            if (!ToolbarControl.LoadImageFromFile(ref dialImg[0], KSPUtil.ApplicationRootPath + "GameData/KPRS/PluginData/Textures/" + DIAL))
            {
                Log.Error("Unable to load dial image");
            }

            for (int i = 1; i < 8; i++)
            {
                if (!ToolbarControl.LoadImageFromFile(ref dialImg[i], KSPUtil.ApplicationRootPath + "GameData/KPRS/PluginData/Textures/" + DIAL + "-" + (45 * i).ToString()))
                {
                    Log.Error("Unable to load dial image");
                }

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
        // This is a method to do housekeeping tasks that can take time and dont need to be done every clock tick
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
                    if (playActivePlaylist != null && playerOn)
                    {
                        playActivePlaylist.Play("SlowUpdate");
                        staticSoundPlayer.PlaySound("SlowUpdate");
                    }
                }
                yield return new WaitForSeconds(1f);
                //Log.Info("SlowUpdate, cnt: " + cnt);
                if (cnt++ > 5)
                {
                    cnt = 0;
                    if (HighLogic.LoadedSceneIsFlight)
                    {
                        activeRadioAntenna = FlightGlobals.ActiveVessel.FindPartModuleImplementing<RadioPartModule>();
                        if (activeRadioAntenna == null)
                        {
                            Log.Info("No activeRadioAntenna");
                        }
                        else
                            SetPreampButtonColor(activeRadioAntenna.preampPower
                                );

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
                                    foreach (var transmitterPartModule in transmitterPartModuleList)
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
                                        Statics.playlist[Statics.stationList[activeRadioAntenna.selectedStation].playlist]);
                                else
                                    playActivePlaylist.NewPlayActivePlaylist("SlowUpdate",
                                    Statics.playlist[Statics.stationList[activeRadioAntenna.selectedStation].playlist]);
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

#if false
        public float angle = 15f;
        bool doRotate = false;
        void InitDialInfo(ref Texture2D dialImg, Vector2 size, out Vector2 pivot)
        {
            //GUI.DrawTexture(new Rect(632, 185, 150, 150), dialImg);

            size = new Vector2(dialImg.width, dialImg.height);
            var pos = new Vector2(0, 0);
            var rect = new Rect(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, size.x, size.y);
            pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
        }

        void RotateImage( ref Texture2D dialImg, Vector2 size,float angle)
        {
            Vector2 pivot;

            InitDialInfo(ref dialImg, size, out pivot);
            //Matrix4x4 matrixBackup = GUI.matrix;
            GUIUtility.RotateAroundPivot(angle, pivot);
            GUI.DrawTexture(new Rect(632, 185, 150, 150), dialImg);
            //GUI.matrix = matrixBackup;
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
                playActivePlaylist.NewPlayActivePlaylist("PlaySelectedStation", Statics.playlist[s.playlist]);
            }
            else
                playActivePlaylist = new PlayActivePlaylist("PlaySelectedStation", Statics.playlist[s.playlist]);
            playerOn = true;
        }

        void ClearSelectedStation(string selectedStation)
        {
            playActivePlaylist.Clear("ClearSelectedStation, activeRadioAntenna.selectedStation: " + activeRadioAntenna.selectedStation);
        }

        Vector2 stationPos;

        Rect notificationRect = new Rect(128f, 35f, 384, 45);

        Rect stationRect = new Rect(128f, 70f, 384, HEIGHT - 140 - 35);
        Rect viewRect = new Rect(0, 0, 300, HEIGHT);
        int start = 0;
        Boolean toggle = false;
        void RadioWin(int id)
        {
            if (activeRadioAntenna == null)
            {
                GUI.Label(notificationRect, "No active radio antenna found", labelFontBoldYellow);
            }
            else
            {
                if (activeRadioAntenna.selectedStation != "")
                {
                    GUI.Label(notificationRect, "Currently Playing: " + activeRadioAntenna.selectedStation, labelFontBoldLarge);
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
                                if (GUI.Button(new Rect(30, 25 * (lineCnt - start), WIDTH - 70, 20), channelDescr, labelFontBoldYellow))
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
                                    GUI.Button(new Rect(30, 25 * (lineCnt - start), WIDTH - 70, 20), channelDescr, labelFontBoldBlue);
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
                            } else
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
                GUI.DrawTexture(new Rect(31, 134, 11, 15), greenBtn);
            else
                GUI.DrawTexture(new Rect(31, 134, 11, 15), redBtn);


            // Now the buttons, starting at the left
            GUIStyle btnCenter = new GUIStyle();
            btnCenter.imagePosition = ImagePosition.ImageOnly;


            //alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(new Rect(53, 131, 43, 22), powerImg, btnCenter)) // style: NonSelectableWindowStyle))
            {
                playerOn = !playerOn;
                soundPlayer.ToggleSound();
                staticSoundPlayer.ToggleSound();
            }
            if (GUI.Button(new Rect(28, 174, 28, 22), "↓"))
            {
                if (activeRadioAntenna != null && Input.GetMouseButtonUp(0))
                {
                    activeRadioAntenna.preampPower = Mathf.Max(activeRadioAntenna.preampPower - 1f, 0);
                    SetPreampButtonColor(activeRadioAntenna.preampPower);

                }
            }
            if (GUI.Button(new Rect(68, 174, 28, 22), "↑"))
            {
                if (activeRadioAntenna != null && Input.GetMouseButtonUp(0))
                {
                    activeRadioAntenna.preampPower = Mathf.Min(activeRadioAntenna.preampPower + 1f, 10);
                    SetPreampButtonColor(activeRadioAntenna.preampPower);

                }
            }
            GUI.DrawTexture(new Rect(32, 158, 60, 11), preampBtn);

            // Now the row of buttons at the bottom, left to right
            if (GUI.Button(new Rect(161, 300, 46, 22), activeRadioAntenna != null && activeRadioAntenna.preset1 != "" ? Statics.stationList[activeRadioAntenna.preset1].abbr.ToString() : ""))
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
            if (GUI.Button(new Rect(229, 300, 46, 22), activeRadioAntenna != null && activeRadioAntenna.preset2 != "" ? Statics.stationList[activeRadioAntenna.preset2].abbr.ToString() : ""))
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
            if (GUI.Button(new Rect(297, 300, 46, 22), activeRadioAntenna != null && activeRadioAntenna.preset3 != "" ? Statics.stationList[activeRadioAntenna.preset3].abbr.ToString() : ""))
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
            if (GUI.Button(new Rect(365, 300, 46, 22), activeRadioAntenna != null && activeRadioAntenna.preset4 != "" ? Statics.stationList[activeRadioAntenna.preset4].abbr.ToString() : ""))
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
            if (GUI.Button(new Rect(433, 300, 46, 22), activeRadioAntenna != null && activeRadioAntenna.preset5 != "" ? Statics.stationList[activeRadioAntenna.preset5].abbr.ToString() : ""))
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
            if (GUI.Button(new Rect(555, 131, 40, 19), shuffleImg, btnCenter))
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
            if (GUI.Button(new Rect(555, 165, 40, 19), shuffleNowImg, btnCenter))
            {
                if (activeRadioAntenna != null)
                {
                    playActivePlaylist.ShufflePlaylist();
                    playerOn = true;
                }
            }
            if (GUI.Button(new Rect(555, 199, 40, 19), "k"))
            {

            }
            // 187
            if (GUI.Button(new Rect(555, 233, 40, 19), "l"))
            {

            }

            // Two buttons at the lower-right, just to the left of the dial
            if (GUI.Button(new Rect(539, 296, 36, 20), "v"))
            {
                volImgNum = Mathf.Max(0, volImgNum - 1);
            }
            if (GUI.Button(new Rect(588, 296, 36, 20), "^"))
            {
                volImgNum = Mathf.Min(7, volImgNum + 1);
            }
            float radioVolume = volImgNum * 14.285f;
            //Log.Info("radioVolume: " + radioVolume);
            if (activeRadioAntenna != null)
            {
                float transHeightAdjustment = 0;
                if (Statics.transmitterList.ContainsKey(activeRadioAntenna.selectedStation))
                    transHeightAdjustment = Statics.transmitterList[activeRadioAntenna.selectedStation].towerHeight / BASE_ANTENNA_HEIGHT;

                //Log.Info("transHeightAdjustment: " + transHeightAdjustment + ", BASE_TRANS_POWER: " + BASE_TRANSMITTER_POWER);
                float maxVol = Mathf.Min(100, radioVolume * transHeightAdjustment);


                //Log.Info("activeRadioAntenna: " +  activeRadioAntenna);
                //Log.Info("Statics.transmitterList[activeRadioAntenna.selectedStation]: " + Statics.transmitterList[activeRadioAntenna.selectedStation]);
                if (Statics.transmitterList.ContainsKey(activeRadioAntenna.selectedStation))
                {
                    double Distance = Vector3d.Distance(FlightGlobals.ActiveVessel.GetWorldPos3D(), Statics.transmitterList[activeRadioAntenna.selectedStation].vessel.GetWorldPos3D()); ;
                    float signalStrength = (float)(Statics.stationList[activeRadioAntenna.selectedStation].power *
                        transHeightAdjustment * BASE_TRANSMITTER_POWER *
                        Mathf.Max(1f, activeRadioAntenna.preampPower) / Distance);

                    //Log.Info("Vessel: " + Statics.transmitterList[activeRadioAntenna.selectedStation].vessel.vesselName + ", Distance: " + Distance.ToString("F1") +
                    //    ", SignalStrength: " + signalStrength +
                    //    ", station power: " + Statics.stationList[activeRadioAntenna.selectedStation].power);

                    radioVolume *= Mathf.Min(1f, signalStrength);

                    float staticVolume = 1f - Mathf.Min(1, (float)(signalStrength));


                    soundPlayer.SetVolume(radioVolume);
                    staticVolume *= volImgNum * 14.285f;

                    staticSoundPlayer.SetVolume(staticVolume);
                    //Log.Info("staticVolume: " + staticVolume  + ", activeRadioAntenna.preampPower: " + activeRadioAntenna.preampPower);
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
#if false
            if (doRotate)
            {
                Log.Info("Rotating image");
                RotateImage();
                doRotate = false;
            }
            else
#endif

            //GUI.Button(new Rect(632, 185, 150, 150), dialImg[imgNum], style: NonSelectableWindowStyle);
            GUI.DrawTexture(new Rect(632, 185, 150, 150), dialImg[(5 + volImgNum) % 8]);

            GUI.DragWindow();

        }
    }
}
