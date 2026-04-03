namespace ThrustAssistMod
{
    [System.Serializable]
    public class UserSettings
    {
        public bool debug = false;
        public double timeStep = 0.2;
        public UnityEngine.Vector2Int windowPosition = new UnityEngine.Vector2Int( -850, -500 );
    }

    public class SettingsManager : UITools.ModSettings<UserSettings>
    {
        public static SettingsManager Main { get; private set; }
        public static System.Action Save { get; private set; }

        protected override SFS.IO.FilePath SettingsFile => new SFS.IO.FolderPath(ThrustAssistMod.Main.main.ModFolder).ExtendToFile("Settings.txt");

        public static void Init()
        {
            Main = new SettingsManager();
            Main.Initialize();
        }

        protected override void RegisterOnVariableChange(System.Action onChange)
        {
            UnityEngine.Application.quitting += Save = onChange;
        }
    }
}
