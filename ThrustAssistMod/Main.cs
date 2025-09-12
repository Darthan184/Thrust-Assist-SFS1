using System.Linq; // contains extensions

namespace ThrustAssistMod
{
    public class Main : ModLoader.Mod
    {

        public override string ModNameID => "thrustassistmod";
        public override string DisplayName => "Thrust Assist";
        public override string Author => "Darthan";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v0.7";
        public override string Description => "Thrust Assistance Mod";
        public override System.Collections.Generic.Dictionary<string, string> Dependencies { get; } =
            new System.Collections.Generic.Dictionary<string, string> { { "UITools", "1.1.5" } };

//~         public System.Collections.Generic.Dictionary<string, SFS.IO.FilePath> UpdatableFiles =>
//~             new System.Collections.Generic.Dictionary<string, SFS.IO.FilePath>()
//~                 {
//~                     {
//~                         "https://github.com/Darthan184/Thrust-Assist-SFS1/releases/latest/ThrustAssistMod.dll"
//~                         , new SFS.IO.FolderPath(ModFolder).ExtendToFile("ThrustAssistMod.dll")
//~                     }
//~                 };


        public static ModLoader.Mod main;
        public static SFS.IO.FolderPath modFolder;
        public static ThrustAssistMod.Updater updater;
        public static ThrustAssistMod.Displayer displayer;
        public static HarmonyLib.Traverse ANAISTraverse = null;


        // This initializes the patcher. This is required if you use any Harmony patches.
        static HarmonyLib.Harmony patcher;


        // This method runs before anything from the game is loaded. This is where you should apply your patches, as shown below.
        public override void Early_Load()
        {
            main = this;
            modFolder = new SFS.IO.FolderPath(ModFolder);
            patcher = new HarmonyLib.Harmony(ModNameID);
            patcher.PatchAll();
        }

        // This tells the loader what to run when your mod is loaded.
        public override void Load()
        {
            ThrustAssistMod.SettingsManager.Load();
            UnityEngine.GameObject.DontDestroyOnLoad((updater = new UnityEngine.GameObject("Thrust Assist-Updater").AddComponent<ThrustAssistMod.Updater>()).gameObject);
            ModLoader.Helpers.SceneHelper.OnWorldSceneLoaded += ThrustAssistMod.UI.ShowGUI;
            ModLoader.Helpers.SceneHelper.OnWorldSceneUnloaded += ThrustAssistMod.UI.GUIInActive;
            try
            {
                System.Reflection.Assembly ANAISAssembly = ModLoader.Loader.main.GetLoadedMods().First(mod => mod.ModNameID == "ANAIS").GetType().Assembly;
                System.Type velocityArrowPatch = ANAISAssembly.GetTypes().First(type => type.Name == "VelocityArrowDrawer_OnLocationChange_Patch");
                ANAISTraverse = HarmonyLib.Traverse.Create(velocityArrowPatch);
            }
            catch
            {
                ANAISTraverse=null;
            }
        }
    }
}
