using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;


namespace liquidclient.Patches.Internal
{
    [HarmonyPatch(typeof(VRRig), nameof(VRRig.UpdateFriendshipBracelet))]
    public class BraceletPatch
    {
        public static bool enabled;

        public static bool Prefix(VRRig __instance) =>
            !enabled;
    }
}