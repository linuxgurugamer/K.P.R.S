using static KPRS.RegisterToolbar;

namespace KPRS.PartModules
{
    internal class KPRS_ModuleAntenna: PartModule
    {
#pragma warning disable 0649
        [KSPField(isPersistant = true)]
        public float height;
#pragma warning restore 0649

#if false
        void Start()
        {
            Log.Info("KPRS_ModuleAntenna, part: " + this.part.partName + ", height; " + height.ToString());
        }
#endif
    }
}
