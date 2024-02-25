using BepInEx.Logging;
using DunGen;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace CoilHeadMod.Patches
{
    [HarmonyPatch(typeof(SpringManAI))]
    internal class SpringManAI_Patch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Update")]
        static void CoilLogic(ref bool ___hasStopped) 
        { 
            // Saved to check if it changed in the PostFix
            Plugin.instance.hasStopped = ___hasStopped;
        }

        [HarmonyPostfix]
        [HarmonyPatch("Update")]
        static void LateCoilLogic(SpringManAI __instance, ref bool ___hasStopped)
        {
            // Passive teleport

            float timeSinceEncounter = Time.timeSinceLevelLoad - Plugin.instance.lastEncounter;
            if (Plugin.instance.tpTimer >= 0 && timeSinceEncounter > Plugin.instance.tpTimer)
                TPCoil(__instance, ref ___hasStopped, true);

            // Teleport on look away

            if (Plugin.instance.hasStopped == true)
            {
                Plugin.instance.lastEncounter = Time.timeSinceLevelLoad;

                if (Plugin.instance.hasStopped != ___hasStopped)
                {
                    Plugin.instance.LogSource.LogMessage(Plugin.modGUID + " Player looked away");

                    if (Time.timeSinceLevelLoad >= Plugin.instance.nextTPChance)
                        Plugin.instance.nextTPChance = Time.timeSinceLevelLoad + UnityEngine.Random.value * (Plugin.instance.tpMax - Plugin.instance.tpMin) + Plugin.instance.tpMin;
                    else return;

                    if (UnityEngine.Random.value <= Plugin.instance.tpChance / 100)
                    {
                        TPCoil(__instance, ref ___hasStopped, false);
                    }
                }
            }
        }

        static void TPCoil(SpringManAI __instance, ref bool ___hasStopped, bool isPassiveTP)
        {
            Plugin.instance.lastEncounter = Time.timeSinceLevelLoad;

            // Random position on the navmesh around a random vent

            var spawns = GameObject.FindGameObjectsWithTag("EnemySpawn");
            EnemyVent spawn = spawns[UnityEngine.Random.Range(0, spawns.Length - 1)].GetComponent<EnemyVent>();
            var pos = Plugin.instance.rm.GetRandomNavMeshPositionInRadius(spawn.floorNode.position, 50f);

            // Teleports (see "NetworkHandler.cs" for the rpc)

            Plugin.instance.DebugLog("Sent TP Rpc");
            Plugin.instance.netHandler.TPCoilServerRpc(__instance, ref ___hasStopped, pos, isPassiveTP);
        }
    }
}
