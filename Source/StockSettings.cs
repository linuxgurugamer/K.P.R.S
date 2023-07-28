using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP.Localization;


namespace KPRS
{
    // http://forum.kerbalspaceprogram.com/index.php?/topic/147576-modders-notes-for-ksp-12/#comment-2754813
    // search for "Mod integration into Stock Settings
    // HighLogic.CurrentGame.Parameters.CustomParams<StockSettings>().
    public class StockSettings : GameParameters.CustomParameterNode
    {
        public override string Title { get { return ""; } }
        public override GameParameters.GameMode GameMode { get { return GameParameters.GameMode.ANY; } }
        public override string Section { get { return "KPRS"; } }
        public override string DisplaySection { get { return "KPRS"; } }
        public override int SectionOrder { get { return 1; } }
        public override bool HasPresets { get { return false; } }


        [GameParameters.CustomParameterUI("Use ElectricCharge",
            toolTip = "")]
        public bool useEC = true;

        [GameParameters.CustomFloatParameterUI("EC Usage Multiplier", minValue = 0.1f, maxValue = 5.0f, stepCount = 101, displayFormat = "F2", asPercentage = false,
            toolTip = "How much ec is used in normal operation.  This is multiplied with the standard EC usage for the parts to come up with a final EC cost")]
        public float ecUsageMultipler = 1f;

        [GameParameters.CustomFloatParameterUI("Initial Volume", minValue = 0f, maxValue = 1.0f, stepCount = 101, displayFormat = "F2", asPercentage = false)]
        public float initialVolume = 0.5f;

        [GameParameters.CustomFloatParameterUI("Minimum Signal Strength", minValue = 0f, maxValue = .5f, stepCount = 101, displayFormat = "F2", asPercentage = false,
            toolTip = "The minimum strength of a signal being received.  This is an artifical minimum to allow you to receive any signal, anywhere, regardless of distance")]
        public float minSignalStrength = 0.01f;

        [GameParameters.CustomFloatParameterUI("UI Scale", minValue = 50f, maxValue = 150f, stepCount = 101, displayFormat = "F0", asPercentage = false,
            toolTip = "Size of the radio box")]
        public float uiScale = 100.0f;


        public override void SetDifficultyPreset(GameParameters.Preset preset)
        { }

        public override bool Enabled(MemberInfo member, GameParameters parameters)
        {
            return true;
        }
        public override bool Interactible(MemberInfo member, GameParameters parameters)
        {
            if (member.Name == "ecUsageMultipler")
            {
                if (useEC)
                {
                    if (ecUsageMultipler == 0)
                        ecUsageMultipler = 1f;
                    return true;
                }
                ecUsageMultipler = 0f;
                //return false;
            }
            return true;

        }
        public override IList ValidValues(MemberInfo member)
        { return null; }
    }

}
