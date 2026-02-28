using GorillaNetworking;
using GorillaNetworking.Store;
using HarmonyLib;
using static liquidclient.Menu.Main;

namespace liquid.Patches.Menu
{
    [HarmonyPatch(typeof(BundleManager), nameof(BundleManager.CheckIfBundlesOwned))]
    public class PostGetData
    {
        public static bool CosmeticsInitialized;
        private static void Postfix()
        {
            CosmeticsInitialized = true;
            CosmeticsOwned = CosmeticsController.instance.concatStringCosmeticsAllowed;
        }
    }
}