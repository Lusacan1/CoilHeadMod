using CoilHeadMod;
using HarmonyLib;
using Unity.Netcode;

namespace CoilHeadMod.Patches
{
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class GameNetworkManagerPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void AddToPrefabs(ref GameNetworkManager __instance)
        {
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(Plugin.instance.netManagerPrefab);
        }
    }
}