using DunGen;
using DunGen.Adapters;
using System;
using Unity.Netcode;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.AI;

namespace CoilHeadMod {
    public class NetworkHandler : NetworkBehaviour {
        [ServerRpc(RequireOwnership = false)]
        public void TPCoilServerRpc(SpringManAI __instance, ref bool ___hasStopped, Vector3 pos, bool isPassiveTP)
        {
            Plugin.instance.DebugLog("Received TP Rpc " + pos);
            
            // currentBehaviourStateIndex activates chase if set to 1 and goes back to roaming if set to 1
            if (isPassiveTP && UnityEngine.Random.value <= Plugin.instance.aggroChance / 100)
                __instance.currentBehaviourStateIndex = 1;
            else __instance.currentBehaviourStateIndex = 0;

            NavMeshAgent nvAgent = __instance.gameObject.GetComponent<NavMeshAgent>();
            nvAgent.Warp(pos);
        }
    }
}