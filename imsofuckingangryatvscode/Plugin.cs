using BepInEx;
using BepInEx.Logging;
using Configgy;
using ULTRAKILL;
using System.ComponentModel.Design;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using static Configgy.UI.DynUI;

namespace gravmod
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, "Gravity Controller", MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private static bool gravityReset = false;
        internal static new ManualLogSource Logger { get; private set; } = null!;
        private ConfigBuilder config;

        [Configgable("", "Mod Enabled", 0, "")]
        public static bool modEnabled = true;

        [Configgable("", "X Direction", 2, "The direction of the gravity. 0 will not affect the player, but setting it to 1 will send the player left. Default: 0")]
        public static float xDir = 0;

        [Configgable("", "Y Direction", 3, "The direction of the gravity. 0 will not affect the player, but setting it to 1 will send the player down. Default: 1")]
        public static float yDir = 1;

        [Configgable("", "Z Direction", 4, "The direction of the gravity. 0 will not affect the player, but setting it to 1 will send the player backwards. Default: 0")]
        public static float zDir = 0;

        [Configgable("", "X Multiplier", 6, "How much to multiply the gravity in the x axis by. Default: 1")]
        public static float xMult = 1;

        [Configgable("", "Y Multiplier", 7, "How much to multiply the gravity in the y axis by. Default: 1")]
        public static float yMult = 1;

        [Configgable("", "Z Multiplier", 8, "How much to multiply the gravity in the z axis by. Default: 1")]
        public static float zMult = 1;

        [Configgable("", "Debug Info", 10, "")]
        private static bool debugMode = false;

        private void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            gameObject.hideFlags = HideFlags.DontSaveInEditor;
            SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
            config = new ConfigBuilder("Lakeull.GravChanger", "GravChanger");
            config.BuildAll(); //Generates config menu from any Cwonfiggable attribute tagged  static fields in your plugin.
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode lsm)
        {
            InvokeRepeating(nameof(ReAssignGravity), 1f, 0.1f);
        }

        public void ReAssignGravity()
        {
            if (SceneHelper.CurrentScene == "Main Menu" || SceneHelper.CurrentScene == "Intro")
            {
                return;
            }
            if (NewMovement.Instance == null)
            {
                Logger.LogError("newmovement is null");
                return;
            }
            if (CameraController.Instance == null)
            {
                Logger.LogError("camera controller fucked");
            }
            if (GameStateManager.Instance == null)
            {
                Logger.LogError("game state manager fucked");
                return;
            }
            
            bool inElevator = ((GameStateManager.Instance.IsStateActive("pit-falling") == true) || (NewMovement.Instance.activated == false));
            // prevent any random ass moving in the elevator (this took 3 fucking hours i hate everyyt
            if (inElevator)
            {
                NewMovement.Instance.ResetGravity(true);
            }
            // reset the gravity if disabled
            else if(!modEnabled && !gravityReset)
            {
                NewMovement.Instance.ResetGravity(true);
                gravityReset = true;
            } 
            // proceed if the mod is enabled
            else if (modEnabled)
            {
                // check to see they dont all equal 0
                if (xDir + yDir + zDir == xDir - yDir - zDir)
                {
                    Logger.LogError("Error: directions cannot all equal 0! if you want 0 gravity use the multipliers instead.");

                    return;
                }

                // return the vector 
                Vector3 pVector = new Vector3(xDir, yDir, zDir).normalized;
                // multiply the vector times the multipliers and 40
                Vector3 pVectorCalc = new Vector3(pVector.x * 40f * xMult, pVector.y * -40f * yMult, pVector.z * 40f * zMult);
                NewMovement.Instance.SwitchGravity(pVectorCalc, false, false);
                gravityReset = false;

                // log debug info
                if (debugMode)
                {
                    Logger.LogInfo("vector: " + pVector);
                    Logger.LogInfo("final calculated vector: " + pVectorCalc);
                    Logger.LogInfo("gravity has been reset: " + gravityReset);
                    Logger.LogInfo("scene: " + SceneHelper.CurrentScene);
                }
            }
        }
    }
}
