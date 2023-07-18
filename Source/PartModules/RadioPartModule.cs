using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static KPRS.RegisterToolbar;

namespace KPRS.PartModules
{
    //
    // This module is added by a ModuleManager script toall parts which have a DataTransmitter
    //
    internal class RadioPartModule : PartModule, IModuleInfo
    {
        [KSPField(isPersistant =true)]
        internal float power = 1;// Preamp power

        internal bool StationSelected {  get { return selectedStation!= null && selectedStation.Length > 0; } }

        [KSPField(isPersistant = true)]
        internal string selectedStation = "";

        [KSPField(isPersistant = true)]
        internal bool shuffle = false;

        [KSPField(isPersistant = true)]
        internal string preset1 = "";

        [KSPField(isPersistant = true)]
        internal string preset2 = "";

        [KSPField(isPersistant = true)]
        internal string preset3 = "";

        [KSPField(isPersistant = true)]
        internal string preset4 = "";

        [KSPField(isPersistant = true)]
        internal string preset5 = "";

        [KSPField(isPersistant = true)]
        int volume = 4;     //multiplied by 12.5 to get a number between 0 and 100

        [KSPField(isPersistant = true)]
        internal float preampPower = 0f;
        void Start()
        {
            Log.Info("RadioPartModule.Start");
            if (HighLogic.LoadedSceneIsFlight)
            {
                Log.Info("vessel: " + this.part.vessel.vesselName);
                Log.Info("power: " + power);
                Log.Info("selectedStation: " + selectedStation.ToString());
            }
            
        }

        public string GetModuleTitle()
        {
            return "Radio Receiver";
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
            return "RadioPartModule";
        }

    }
}
