using UnityEngine;
using File = System.IO.File;
using KSP_Log;
using KSP.UI.Screens;
using ToolbarControl_NS;

namespace KPRS
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class RegisterToolbar : MonoBehaviour
    {
        internal static Log Log = null;


        internal void InitLog()
        {
#if DEBUG
            Log = new Log("K.P.R.S.", Log.LEVEL.INFO);
#else
                Log = new Log("K.P.R.S.", Log.LEVEL.ERROR);
#endif
        }

        internal void InitStyle()
        {
            labelFontBoldYellow = new GUIStyle(GUI.skin.label);
            labelFontBoldYellow.fontStyle = FontStyle.Bold;
            labelFontBoldYellow.normal.textColor = Color.yellow;

            labelFontBoldBlue = new GUIStyle(GUI.skin.label);
            labelFontBoldBlue.fontStyle = FontStyle.Bold;
            labelFontBoldBlue.normal.textColor = Color.blue;
        }

        void Start()
        {
            ToolbarControl.RegisterMod(KPBR.MODID, KPBR.MODNAME);
            InitLog();
            LoadConfigs.GetConfigs();
        }

#if true
        bool initted = false;
        internal static GUIStyle labelFontBoldYellow;
        internal static GUIStyle labelFontBoldBlue;

        void OnGUI()
        {
            if (!initted)
            {
                initted = true;
                InitStyle();
            }
        }
#endif
    }
}
