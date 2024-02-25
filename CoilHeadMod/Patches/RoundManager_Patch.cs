using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace CoilHeadMod.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManager_Patch
    {
        [HarmonyPostfix]
        [HarmonyPatch("Start")]
        static void StartFunc(RoundManager __instance)
        {
            Plugin.instance.rm = __instance;
            Plugin.instance.coilHead = GetEnemies().Find(x => x.enemyType.name == "SpringMan").enemyType;
            Plugin.instance.hasSpawned = false;
        }

        static List<SpawnableEnemyWithRarity> GetEnemies()
        {
            var gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
            List<SpawnableEnemyWithRarity> enemies = null;
            foreach (var gameObject in gameObjects)
                if (gameObject.name == "Environment")
                    enemies = gameObject.GetComponentInChildren<Terminal>().moonsCatalogueList.SelectMany(x => x.Enemies, (k, v) => v).ToList();
            return enemies;
        }
    }
}
