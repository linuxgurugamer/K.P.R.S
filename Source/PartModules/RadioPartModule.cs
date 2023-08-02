using Steamworks;
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
    internal class RadioPartModule : PartModule, IModuleInfo, IResourceConsumer
    {
        [KSPField(isPersistant = true)]
        internal float power = 1;// Preamp power

        internal bool StationSelected { get { return selectedStation != null && selectedStation.Length > 0; } }

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

        private List<PartResourceDefinition> consumedResources;

        public List<PartResourceDefinition> GetConsumedResources()
        {
            Log.Info("RadioPartModule.GetConsumedResources");
            return consumedResources;
        }

        [KSPField(isPersistant = true)]
        internal float preampPower = 0f;

         override public void OnAwake()
        {
            if (consumedResources == null)
                consumedResources = new List<PartResourceDefinition>();
            else
                consumedResources.Clear();

            int i = 0;
            for (int count = resHandler.inputResources.Count; i < count; i++)
            {
                consumedResources.Add(PartResourceLibrary.Instance.GetDefinition(resHandler.inputResources[i].name));
            }
            base.OnAwake();
        }


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
            return "KPRS Radio Receiver " + resHandler.PrintModuleResources();
        }

    }
}
