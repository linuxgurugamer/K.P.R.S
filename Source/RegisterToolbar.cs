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
        internal static int resourceID = -1;


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

            labelFontBoldLarge = new GUIStyle(GUI.skin.label);
            labelFontBoldLarge.fontStyle = FontStyle.Bold;
            labelFontBoldLarge.fontSize = 22;

            labelFontBoldRed = new GUIStyle(GUI.skin.label);
            labelFontBoldRed.fontStyle = FontStyle.Bold;
            labelFontBoldRed.normal.textColor = Color.red;

            vertScrollbarStyle = new GUIStyle(GUI.skin.verticalScrollbar);

        }

        void Start()
        {
            ToolbarControl.RegisterMod(KPBR.MODID, KPBR.MODNAME);
            InitLog();
            LoadConfigs.GetConfigs();

            PartResourceDefinition electricCharge = PartResourceLibrary.Instance.GetDefinition("ElectricCharge");

            if (electricCharge != null)
                resourceID = electricCharge.id;
            else
                Log.Error("ElectricCharge not found");
        }

#if true
        bool initted = false;
        internal static GUIStyle labelFontBoldYellow;
        internal static GUIStyle labelFontBoldRed;
        internal static GUIStyle labelFontBoldBlue;
        internal static GUIStyle vertScrollbarStyle;
        internal static GUIStyle labelFontBoldLarge;

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
