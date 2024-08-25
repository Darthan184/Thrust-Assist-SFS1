namespace ThrustAssistMod
{
    public class Main : ModLoader.Mod
    {

        public override string ModNameID => "thrustassistmod";
        public override string DisplayName => "Thrust Assist";
        public override string Author => "Darthan";
        public override string MinimumGameVersionNecessary => "1.5.10.2";
        public override string ModVersion => "v0.1";
        public override string Description => "Thrust Assistance Module";
        public override System.Collections.Generic.Dictionary<string, string> Dependencies { get; } =
            new System.Collections.Generic.Dictionary<string, string> { { "UITools", "1.1.5" } };

//~         public System.Collections.Generic.Dictionary<string, SFS.IO.FilePath> UpdatableFiles =>
//~             new System.Collections.Generic.Dictionary<string, SFS.IO.FilePath>()
//~                 {
//~                     {
//~                         "https://github.com/Darthan184/Thrust-Assist-SFS1/releases/latest/download/ThrustAssistMod.dll"
//~                         , new SFS.IO.FolderPath(ModFolder).ExtendToFile("ThrustAssistMod.dll")
//~                     }
//~                 };


        public static ModLoader.Mod mod;
        public static SFS.IO.FolderPath modFolder;

        // This initializes the patcher. This is required if you use any Harmony patches.
        static HarmonyLib.Harmony patcher;


        // This method runs before anything from the game is loaded. This is where you should apply your patches, as shown below.
        public override void Early_Load()
        {
            mod = this;
            modFolder = new SFS.IO.FolderPath(ModFolder);
            patcher = new HarmonyLib.Harmony("thrustassistmod");
            patcher.PatchAll();
        }

        // This tells the loader what to run when your mod is loaded.
        public override void Load()
        {
            ThrustAssistMod.SettingsManager.Load();
            ModLoader.Helpers.SceneHelper.OnWorldSceneLoaded += UI.ShowGUI;
        }
    }
}
