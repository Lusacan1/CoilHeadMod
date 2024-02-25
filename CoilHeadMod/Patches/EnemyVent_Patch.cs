using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoilHeadMod.Patches
{
    [HarmonyPatch(typeof(EnemyVent))]
    internal class EnemyVent_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void SpawnDebugCoil(EnemyVent __instance)
        {
            if (Plugin.instance.hasSpawned || !Plugin.instance.debugCoil) return;
            Plugin.instance.hasSpawned = true;

            Plugin.instance.rm.SpawnEnemyGameObject(
                __instance.floorNode.position,
                __instance.floorNode.eulerAngles.y,
                __instance.enemyTypeIndex,
                Plugin.instance.coilHead);

            Plugin.instance.DebugLog("Spawned debug coil <3");
        }
    }
}
