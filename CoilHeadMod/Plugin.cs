using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Reflection;

namespace CoilHeadMod
{
    // Note that i don't have a fucking clue what im doing here

    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        // Mod Setup

        public const string modGUID = "CoilHeadMod.Lusacan1";
        public const string modName = "CoilTeleport";
        public const string modVersion = "1.0.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        // Config Variables

        public float tpChance;
        public float tpMin;
        public float tpMax;

        public float tpTimer;
        public float aggroChance;

        public bool debugCoil;
        public bool bigLogs;

        // Variables

        public bool hasSpawned = false;
        public bool hasStopped = false;
        public float nextTPChance = 0f;
        public float lastEncounter = 0f;

        public EnemyType coilHead;

        public ManualLogSource LogSource;
        public RoundManager rm;

        public GameObject netManagerPrefab;
        public NetworkHandler netHandler;
        public static Plugin instance;

        void Awake()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types) {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods) {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0) {
                        method.Invoke(null, null);
                    }
                }
            }

            instance = this;

            string assetDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "netcode");
            AssetBundle bundle = AssetBundle.LoadFromFile(assetDir);

            netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/NetworkManager/NetworkManager.prefab");
            netManagerPrefab.AddComponent<NetworkHandler>();
            netHandler = netManagerPrefab.GetComponent<NetworkHandler>();

            LogSource = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            DebugLog("Loaded version (" + modVersion + ")");

            var title = "Teleport";

            ConfigEntry<float> configTeleportChance = Config.Bind(title, // Category
                "Teleport Chance", // Name
                33.3f, // Default
                "The chance for the coil head to teleport when you look away (1-100)\nset to -1 to disable"); // Description
            tpChance = configTeleportChance.Value; // Applies to the variable

            ConfigEntry<float> configTeleportMin = Config.Bind(title,
                "Minimum Cooldown",
                1f,
                "Minimum time between the chances of the coil head teleporting away");
            tpMin = configTeleportMin.Value;

            ConfigEntry<float> configTeleportMax = Config.Bind(title,
                "Maximum Cooldown",
                10f,
                "Maximum time between the chances of the coil head teleporting away");
            tpMax = configTeleportMax.Value;

            title = "Passive Teleport";

            ConfigEntry<float> configTeleportTimer = Config.Bind(title,
                "Timer to Teleport",
                20f,
                "After this many seconds without encountering a player the coil head will teleport\nset to -1 to disable");
            tpTimer = configTeleportTimer.Value;

            ConfigEntry<float> configChaseChance = Config.Bind(title,
                "Chance to Chase",
                50f,
                "The chance for the coil head to chase a player after passively teleporting");
            aggroChance = configChaseChance.Value;

            title = "Debug";

            ConfigEntry<bool> configDebugCoil = Config.Bind(title,
                "Debug Coil", 
                false,
                "Spawn a coil inside the dungeon before landing");
            debugCoil = configDebugCoil.Value;

            ConfigEntry<bool> configBigLogs = Config.Bind(title,
                "Ingame Logs",
                false,
                "Displays a HUD tip for mod logs");
            bigLogs = configBigLogs.Value;

            harmony.PatchAll();
        }

        public void DebugLog(string txt)
        {
            LogSource.LogMessage(txt);
            if (HUDManager.Instance && bigLogs) HUDManager.Instance.DisplayTip(modGUID, txt);
        }
    }
}
