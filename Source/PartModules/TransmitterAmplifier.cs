using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace KPRS.PartModules
{
    public class KPBR_TransmitterAmplifier : PartModule, IModuleInfo
    {
        [KSPField(isPersistant = true, guiName = "Transmitter Amplifier")]
        float amplification = 1;

        public string GetModuleTitle()
        {
            return "Transmitter Amplifier";
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
            return "TransmitterAmplifier";
        }

    }
}
