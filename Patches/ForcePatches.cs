using HarmonyLib;

namespace liquidclient.Patches.Menu
{
    public class ForcePatches
    {
        public static bool enabled;

        [HarmonyPatch(typeof(ForceVolume), nameof(ForceVolume.OnTriggerEnter))]
        public class OnTriggerEnter
        {
            public static bool Prefix() =>
                !enabled;
        }

        [HarmonyPatch(typeof(ForceVolume), nameof(ForceVolume.OnTriggerExit))]
        public class OnTriggerExit
        {
            public static bool Prefix() =>
                !enabled;
        }

        [HarmonyPatch(typeof(ForceVolume), nameof(ForceVolume.OnTriggerStay))]
        public class OnTriggerStay
        {
            public static bool Prefix() =>
                !enabled;
        }
    }
}