namespace ThrustAssistMod
{
    public class UI
    {
        // Create a GameObject for your window to attach to.
        private static UnityEngine.GameObject windowHolder;

        // Random window ID to avoid conflicts with other mods.
        private static readonly int MainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();

        private static SFS.UI.ModGUI.Label _ascentAcceleration_Label;
        private static SFS.UI.ModGUI.Label _ascentVelocity_Label;
        private static SFS.UI.ModGUI.Label _gravity_Label;
        private static SFS.UI.ModGUI.Label _height_Label;
        public static ThrustAssistMod.Updater updater;

        public static double AscentAcceleration
        {
            set
            {
                _ascentAcceleration_Label.Text = string.Format("Ascent A: {0:N2} m/s^2",value);
            }
        }

        public static double AscentVelocity
        {
            set
            {
                _ascentVelocity_Label.Text = string.Format("Ascent V: {0:N2} m/s",value);
            }
        }

        public static double Gravity
        {
            set
            {
                _gravity_Label.Text = string.Format("Gravity: {0:N2} m/s^2",value);
            }
        }

        public static double Height
        {
            set
            {
                _height_Label.Text = string.Format("Height: {0:N1} m",value);
            }
        }

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

            _gravity_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Gravity: -");
            _gravity_Label.AutoFontResize=false;
            _gravity_Label.FontSize=25;

            _height_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Height: -");
            _height_Label.AutoFontResize=false;
            _height_Label.FontSize=25;

            _ascentVelocity_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Ascent V: -");
            _ascentVelocity_Label.AutoFontResize=false;
            _ascentVelocity_Label.FontSize=25;

            _ascentAcceleration_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 30, 0, 0, "Ascent A: -");
            _ascentAcceleration_Label.AutoFontResize=false;
            _ascentAcceleration_Label.FontSize=25;
        }
    }
}
