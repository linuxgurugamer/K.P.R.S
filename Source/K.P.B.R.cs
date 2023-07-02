using UnityEngine;
using ClickThroughFix;
using ToolbarControl_NS;
using UnityEngine;
using SpaceTuxUtility;
using KSP.UI.Screens;
using System.Collections;

using static KPRS.RegisterToolbar;
using KPRS.PartModules;
using Steamworks;
using Expansions.Missions.Actions;
using static ConfigNode;
using System.Media;
using JetBrains.Annotations;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KPRS
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    public partial class KPBR : MonoBehaviour
    {
        internal static KPBR Instance { get; set; }
        internal const string MODID = "K.P.R.S.";
        internal const string MODNAME = "Kerbal Public Radio Service";

        const float HEIGHT = 353f;
        const float WIDTH = 800f;

        [KSPField(isPersistant = true)]
        private Rect windowRect = new Rect(50f, 50, WIDTH, HEIGHT);

        [KSPField(isPersistant = true)]
        internal Rect transWindowRect = new Rect(50f, 25f, TransmitterPartModule.TRANS_SEL_WIDTH, TransmitterPartModule.TRANS_SEL_HEIGHT);

        static internal ToolbarControl toolbarControl = null;
        Texture2D facePlateImg = null;
        Texture2D powerImg = null;
        Texture2D[] dialImg = new Texture2D[8];
        Texture2D redBtn = new Texture2D(11, 15);
        Texture2D greenBtn = new Texture2D(11, 15);
        int imgNum = 0;
        float imageSizeAdjustment;

        const string FACEPLATE = "ATSPTX705";
        const string DIAL = "dial";
        const string POWER = "power";

        internal static SoundPlayer soundplayer;
        internal const string SOUND_DIR = "K.P.R.S/";

        PlayActivePlaylist playActivePlaylist = null;

        bool playerOn = true;

        void Awake()
        {
            GameObject myplayer = new GameObject();
            myplayer.AddComponent<SoundPlayer>();

            soundplayer = SoundPlayer.Instance;

        }

        void onGameSceneLoadRequested(GameScenes scene)
        {
            if (HighLogic.LoadedSceneIsFlight)
                soundplayer.StopSound();
        }
        void onLevelWasLoadedGUIReady(GameScenes newScene)
        {
            if (HighLogic.LoadedSceneIsFlight)
                soundplayer.Initialize(SOUND_DIR + "ambience1"); // Initializes the player, does some housekeeping
        }


        public void Start()
        {
            Instance = this;
            DontDestroyOnLoad(this);
            AddToolbarButton();
            radioWinId = WindowHelper.NextWindowId("K.B.R.S-Radio");

            GameEvents.onLevelWasLoadedGUIReady.Add(onLevelWasLoadedGUIReady);
            GameEvents.onGameSceneLoadRequested.Add(onGameSceneLoadRequested);

            if (ToolbarControl.LoadImageFromFile(ref facePlateImg, KSPUtil.ApplicationRootPath + "GameData/K.P.R.S/PluginData/Textures/" + FACEPLATE))
            {
                imageSizeAdjustment = WIDTH / facePlateImg.width;
                windowRect = new Rect(50f, 25f, WIDTH, WIDTH * facePlateImg.height / facePlateImg.width);
                windowRect = new Rect(50f, 50, WIDTH, imageSizeAdjustment * facePlateImg.height);

            }
            else
                Log.Error("Unable to load faceplate image");

            if (!ToolbarControl.LoadImageFromFile(ref powerImg, KSPUtil.ApplicationRootPath + "GameData/K.P.R.S/PluginData/Textures/" + POWER))
            {
                Log.Error("Unable to load power image");
            }
            if (!ToolbarControl.LoadImageFromFile(ref dialImg[0], KSPUtil.ApplicationRootPath + "GameData/K.P.R.S/PluginData/Textures/" + DIAL))
            {
                Log.Error("Unable to load dial image");
            }
            //else
            //    InitDialInfo();
            for (int i = 1; i < 8; i++)
            {
                if (!ToolbarControl.LoadImageFromFile(ref dialImg[i], KSPUtil.ApplicationRootPath + "GameData/K.P.R.S/PluginData/Textures/" + DIAL + "-" + (45 * i).ToString()))
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


            if (HighLogic.LoadedSceneIsFlight)
                soundplayer.Initialize(SOUND_DIR + "ambience1"); // Initializes the player, does some housekeeping
#if false
            soundplayer.SetVolume(1f);
            soundplayer.PlaySound();
#endif
            StartCoroutine(SlowUpdate());
        }

        void DumpConfigNode(ConfigNode node)
        {

            foreach (Value n in node.values)
            {
                Log.Info("Value name: " + n.name + ", value: " + n.value);
            }
        }

        RadioPartModule activeRadioAntenna = null;
        bool doRotate = false;
        int cnt = 0;

        //
        // This is a method to do housekeeping tasks that can take time and dont need to be done every clock tick
        //
        IEnumerator SlowUpdate()
        {
            int cnt = 0;

            yield return new WaitForSeconds(1f);

            while (true)
            {
                if (HighLogic.LoadedSceneIsFlight)
                {
                    if (playActivePlaylist != null && playerOn)
                        playActivePlaylist.Play();
                }
                yield return new WaitForSeconds(1f);
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
 #if false
                       Log.Info("SlowUpdate, cnt: " + cnt++ + ", playerOn: " + playerOn + ", activeRadioAntenna: " + (activeRadioAntenna != null).ToString());
                        Log.Info(" ");
                        Log.Info("Updating Transmitter list, total number of vessels: " + FlightGlobals.Vessels.Count);
                        Log.Info(" ");
#endif
                        Statics.transmitterList.Clear();
                        foreach (var s in Statics.stationList)
                            s.Value.selected = false;

                        for (int idx = 0; idx < FlightGlobals.Vessels.Count; idx++)
                        {
                            Vessel vessel = FlightGlobals.Vessels[idx];
#if false
                        if (vessel && vessel.vesselType != VesselType.SpaceObject)
                            Log.Info("vesselLoaded: " + vessel.loaded + ": " + vessel.vesselName);
#endif
                            if (vessel && (vessel.vesselType >= VesselType.Probe && vessel.vesselType <= VesselType.Base))
                            {
                                if (vessel.loaded)
                                {
#if false
                                foreach (var p in vessel.parts)
                                {
                                    Log.Info("  Part: " + p.name);
                                    foreach (var m in p.Modules)
                                    {
                                        Log.Info("    Module: " + m.moduleName);
                                    }
                                }
#endif
                                    var t = vessel.FindPartModuleImplementing<TransmitterPartModule>();
#if false
                                    if (t != null)
                                    {
                                        Log.Info("vesselLoaded: " + vessel.loaded + ": " + vessel.vesselName);
                                        Log.Info("StationSelected: " + t.StationSelected + ", LocationSelected: " + t.LocationSelected);
                                    }
#endif
                                    if (t != null && t.StationSelected && t.LocationSelected)
                                    {
#if false
                                        Log.Info("Loaded, antenna, selectedStation: " + t.selectedStation + ", location: " + t.location);
#endif

                                        Statics.transmitterList[t.selectedStation] = new Transmitter(t.selectedStation, t.location);
                                        if (Statics.stationList.ContainsKey(t.selectedStation))
                                            Statics.stationList[t.selectedStation].selected = true;
                                    }
                                }
                                else
                                {
                                    var t = vessel.FindPartModuleImplementing<TransmitterPartModule>();
                                    if (t != null && t.StationSelected && t.LocationSelected)
                                    {
                                        Statics.transmitterList[t.selectedStation] = new Transmitter(t.selectedStation, t.location);
#if false
                                        Log.Info("Unloaded, antenna, selectedStation: " + t.selectedStation + ", location: " + t.location);
#endif
                                    }

                                    for (int idx3 = 0; idx3 < vessel.protoVessel.protoPartSnapshots.Count; idx3++)
                                    {
                                        ProtoPartSnapshot p = vessel.protoVessel.protoPartSnapshots[idx3];
                                        Part part = p.partPrefab;
                                        //ConfigNode[] modules = part.partInfo.partConfig.GetNodes("MODULE");
                                        for (int modIdx = 0; modIdx < p.modules.Count; modIdx++)
                                        {
                                            ProtoPartModuleSnapshot module = p.modules[modIdx];

                                            if (module.moduleName == "TransmitterPartModule")
                                            {
                                                //Log.Info("Value count: " + module.CountValues);
                                                //Log.Info("Node count: " + module.CountNodes);
                                                //Log.Info("value: selectedStation, present: " + module.moduleValues.HasValue("selectedStation").ToString());
                                                //Log.Info("value: location, present: " + module.HasValue("location").ToString());
                                                //Log.Info("module.HasData: " + module.HasData);

                                                //foreach (Value n in module.moduleValues.values)
                                                //{
                                                //    Log.Info("Value name: " + n.name + ", value: " + n.value);
                                                //}

                                                string selectedStation = module.moduleValues.SafeLoad("selectedStation", "");
                                                bool stationSelected = (selectedStation.Length > 0);
                                                string location = module.moduleValues.SafeLoad("location", "");
                                                bool locationSelected = (location.Length > 0);
                                                if (stationSelected && locationSelected)
                                                {
                                                    Statics.transmitterList[selectedStation] = new Transmitter(selectedStation, location);
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
                                Log.Info("SelectedStation: " +  activeRadioAntenna.selectedStation);
                                playActivePlaylist = new PlayActivePlaylist("SlowUpdate",
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
                    "K.P.R.S/PluginData/Textures/K.P.R.S",
                    "K.P.R.S/PluginData/Textures/K.P.R.S",
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
        public Vector2 size;
        Vector2 pos = new Vector2(0, 0);
        Rect rect;
        Vector2 pivot;
        void InitDialInfo()
        {
            //GUI.DrawTexture(new Rect(632, 185, 150, 150), dialImg);

            size = new Vector2(dialImg.width, dialImg.height);
            var pos = new Vector2(0,0);
            var rect = new Rect(pos.x - size.x * 0.5f, pos.y - size.y * 0.5f, size.x, size.y);
            var pivot = new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f);
        }
#endif

        void PlaySelectedStation(string selectedStation)
        {
            activeRadioAntenna.selectedStation = selectedStation;
            if (!Statics.stationList.ContainsKey(selectedStation))
                Log.Error("Station not found in stationlist: " + selectedStation);
            var s = Statics.stationList[activeRadioAntenna.selectedStation];
            Log.Info("Station: " + selectedStation + ", playlist: " + s.playlist);
            Log.Info("Playlist info: " + Statics.playlist[s.playlist].ToString());
            foreach (var p in Statics.playlist[s.playlist].tracks)
            {

            }
            if (playActivePlaylist != null)
            {
                soundplayer.StopSound();
                playActivePlaylist = null;
            }
            playActivePlaylist = new PlayActivePlaylist("PlaySelectedStation", Statics.playlist[s.playlist]);
            playerOn = true;
        }

        Vector2 stationPos;
        Rect stationRect = new Rect(128f, 35f, 384, HEIGHT - 140);
        Rect viewRect = new Rect(0, 0, 300, HEIGHT);
        int start = 0;
        void RadioWin(int id)
        {
            //using (new GUILayout.VerticalScope())
            {
                if (activeRadioAntenna == null)
                    GUILayout.Label("No active radio antenna found");
                else
                {
                    GUIStyle horizScrollbarStyle = new GUIStyle(GUI.skin.horizontalScrollbar);
                    horizScrollbarStyle.fixedHeight = 0;
                    GUIStyle vertScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);

                    stationPos = GUI.BeginScrollView(stationRect, stationPos, viewRect, false, true, GUIStyle.none, vertScrollbarStyle);
                    float cnt = 0;
                    foreach (var transmitter in Statics.transmitterList)
                    {
                        Station station = null;
                        string channelDescr = "";

                        if (Statics.stationList.ContainsKey(transmitter.Key))
                        {
                            station = Statics.stationList[transmitter.Key];
                            channelDescr = "Stn: " + station.channelNumber.ToString() + " - ";
                        }

                        channelDescr = channelDescr + transmitter.Key;

                        if (activeRadioAntenna.selectedStation == transmitter.Key)
                        {
                            if (transmitter.Value.location != null && transmitter.Value.location != "")
                            {
                                GUI.Button(new Rect(30, 25 * (cnt - start), WIDTH - 70, 20), channelDescr, labelFontBoldYellow);
                            }
                            else
                            {
                                GUI.Button(new Rect(30, 25 * (cnt - start), WIDTH - 70, 20), channelDescr, labelFontBoldBlue);
                            }
                        }
                        else
                        {
                            if (GUI.Button(new Rect(30, 25 * (cnt - start), WIDTH - 70, 20), channelDescr, GUI.skin.label))
                            {
                                if (activeRadioAntenna.selectedStation == transmitter.Key)
                                {
                                    activeRadioAntenna.selectedStation = "";
                                    soundplayer.StopSound();
                                    playActivePlaylist = null;
                                    playerOn = false;
                                }
                                else
                                {
                                    PlaySelectedStation(transmitter.Key);
#if false
                                    activeRadioAntenna.selectedStation = transmitter.Key;
                                    if (!Statics.stationList.ContainsKey(transmitter.Key))
                                        Log.Error("Station not found in stationlist: " + transmitter.Key);
                                    var s = Statics.stationList[activeRadioAntenna.selectedStation];
                                    Log.Info("Station: " + transmitter.Key + ", playlist: " + s.playlist);
                                    if (playActivePlaylist != null)
                                    {
                                        soundplayer.StopSound();
                                        playActivePlaylist = null;
                                    }
                                    playActivePlaylist = new PlayActivePlaylist(
                                        Statics.playlist[Statics.stationList[activeRadioAntenna.selectedStation].playlist]);
                                    playerOn = true;
#endif
                                }
                            }
                        }
                        cnt++;
                    }
                    //for (int i = (int)cnt; i < 25; i++)
                    //{
                    //    GUI.Button(new Rect(30, 25 * (i - start), WIDTH - 70, 20), "This is a test of line # " + i.ToString(), GUI.skin.label);
                    //}
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


                //alignment = TextAnchor.MiddleCenter; ;
                if (GUI.Button(new Rect(53, 131, 43, 22), powerImg, btnCenter)) // style: NonSelectableWindowStyle))
                {
                    playerOn = !playerOn;
                    soundplayer.ToggleSound();
                }
                if (GUI.Button(new Rect(28, 174, 28, 22), "b"))
                {
                    if (Input.GetMouseButtonUp(0))
                    {
                        Log.Info("left-click b down");
                    }
                    if (Input.GetMouseButtonUp(1))
                    {
                        Log.Info("right-click b down");
                    }

                }
                if (GUI.Button(new Rect(68, 174, 28, 22), "c"))
                {

                }

                // Now the row of buttons at the bottom, left to right
                //162x302
                // 206x336
                if (GUI.Button(new Rect(161, 300, 46, 22), activeRadioAntenna!=null? Statics.stationList[ activeRadioAntenna.preset1].abbr.ToString():""))
                {
                    if (activeRadioAntenna != null)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (activeRadioAntenna.preset1 != null)
                            {
                                PlaySelectedStation(activeRadioAntenna.preset1);
                            }
                        }
                        if (Input.GetMouseButtonUp(1))
                        {
                            activeRadioAntenna.preset1 = activeRadioAntenna.selectedStation;
                        }
                    }
                }
                if (GUI.Button(new Rect(229, 300, 46, 22), activeRadioAntenna != null ? Statics.stationList[activeRadioAntenna.preset2].abbr.ToString() : ""))
                {
                    if (activeRadioAntenna != null)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (activeRadioAntenna.preset2 != null)
                            {
                                PlaySelectedStation(activeRadioAntenna.preset2);
                            }
                        }
                        if (Input.GetMouseButtonUp(1))
                        {
                            activeRadioAntenna.preset2 = activeRadioAntenna.selectedStation;
                        }
                    }
                }
                if (GUI.Button(new Rect(297, 300, 46, 22), activeRadioAntenna != null ? Statics.stationList[activeRadioAntenna.preset3].abbr.ToString() : ""))
                {
                    if (activeRadioAntenna != null)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (activeRadioAntenna.preset3 != null)
                            {
                                PlaySelectedStation(activeRadioAntenna.preset3);
                            }
                        }
                        if (Input.GetMouseButtonUp(1))
                        {
                            activeRadioAntenna.preset3 = activeRadioAntenna.selectedStation;
                        }
                    }
                }
                if (GUI.Button(new Rect(365, 300, 46, 22), activeRadioAntenna != null ? Statics.stationList[activeRadioAntenna.preset4].abbr.ToString() : ""))
                {
                    if (activeRadioAntenna != null)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (activeRadioAntenna.preset4 != null)
                            {
                                PlaySelectedStation(activeRadioAntenna.preset4);
                            }
                        }
                        if (Input.GetMouseButtonUp(1))
                        {
                            activeRadioAntenna.preset4 = activeRadioAntenna.selectedStation;
                        }
                    }
                }
                if (GUI.Button(new Rect(433, 300, 46, 22), activeRadioAntenna != null ? Statics.stationList[activeRadioAntenna.preset5].abbr.ToString() : ""))
                {
                    if (activeRadioAntenna != null)
                    {
                        if (Input.GetMouseButtonUp(0))
                        {
                            if (activeRadioAntenna.preset5 != null)
                            {
                                PlaySelectedStation(activeRadioAntenna.preset5);
                            }
                        }
                        if (Input.GetMouseButtonUp(1))
                        {
                            activeRadioAntenna.preset5 = activeRadioAntenna.selectedStation;
                        }
                    }
                }

                // Now the 4 vertical buttons
                // 555x153
                // 591x172
                if (GUI.Button(new Rect(555, 131, 40, 19), "shuffle"))
                {
                    if (activeRadioAntenna.StationSelected)
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
                if (GUI.Button(new Rect(555, 165, 40, 19), "j"))
                {

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

                    imgNum--;
                    if (imgNum < 0)
                        imgNum = 7;
                }
                if (GUI.Button(new Rect(588, 296, 36, 20), "^"))
                {
                    imgNum++;
                    if (imgNum > 7)
                        imgNum = 0;

                }
#if false
                if (doRotate)
                {
                    Log.Info("Rotating image");
                    InitDialInfo();
                    //Matrix4x4 matrixBackup = GUI.matrix;
                    GUIUtility.RotateAroundPivot(angle, pivot);
                    GUI.DrawTexture(new Rect(632, 185, 150, 150), dialImg);
                    //GUI.matrix = matrixBackup;
                    doRotate = false;
                } else
#endif

                //GUI.Button(new Rect(632, 185, 150, 150), dialImg[imgNum], style: NonSelectableWindowStyle);
                GUI.DrawTexture(new Rect(632, 185, 150, 150), dialImg[imgNum]);

            }
            GUI.DragWindow();

        }


    }
}
