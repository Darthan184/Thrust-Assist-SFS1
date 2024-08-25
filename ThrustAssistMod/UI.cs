namespace ThrustAssistMod
{
    public class UI
    {
        // Create a GameObject for your window to attach to.
        static UnityEngine.GameObject windowHolder;

        // Random window ID to avoid conflicts with other mods.
        static readonly int MainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();

        /*
        Call this method when you want to show your UI.
        */
        public static void ShowGUI()
        {
            // Create the window holder, attach it to the currently active scene so it's removed when the scene changes.
            windowHolder = SFS.UI.ModGUI.Builder.CreateHolder(SFS.UI.ModGUI.Builder.SceneToAttach.CurrentScene, "ThrustAssistMod GUI Holder");
            UnityEngine.Vector2Int pos = SettingsManager.settings.windowPosition;
            SFS.UI.ModGUI.Window window = SFS.UI.ModGUI.Builder.CreateWindow(windowHolder.transform, MainWindowID, 360, 290, pos.x, pos.y, true, true, 0.95f, "Thrust Assist");

            // Create a layout group for the window. This will tell the GUI builder how it should position elements of your UI.
            window.CreateLayoutGroup(SFS.UI.ModGUI.Type.Vertical);

            window.gameObject.GetComponent<SFS.UI.DraggableWindowModule>().OnDropAction += () =>
            {
                ThrustAssistMod.SettingsManager.settings.windowPosition = UnityEngine.Vector2Int.RoundToInt(window.Position);
                ThrustAssistMod.SettingsManager.Save();
            };
            SFS.UI.ModGUI.Builder.CreateButtonWithLabel(window, 290, 50, 0, 0, "Button Label", "Button Text", ButtonMethod);
        }

        /*
        Method to pass into the button element to give it functionality
        */
        static void ButtonMethod()
        {
            SFS.UI.MsgDrawer.main.Log("Hello world");
        }
    }
}
