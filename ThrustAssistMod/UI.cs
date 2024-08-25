namespace ThrustAssistMod
{
    public class UI
    {
        // Create a GameObject for your window to attach to.
        private static UnityEngine.GameObject windowHolder;

        // Random window ID to avoid conflicts with other mods.
        private static readonly int MainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();

//~         private static SFS.UI.ModGUI.Label _ascentAcceleration_Label;
//~         private static SFS.UI.ModGUI.Label _ascentVelocity_Label;
//~         private static SFS.UI.ModGUI.Label _gravity_Label;
//~         private static SFS.UI.ModGUI.Label _height_Label;

        private static bool _assistOn=false;
        private static SFS.UI.ModGUI.Label _minThrottle_Label;
        private static SFS.UI.ModGUI.Slider _minThrottle_Slider;
        private static SFS.UI.ModGUI.Label _targetHeight_Label;
        private static SFS.UI.ModGUI.Slider _targetHeight_Slider;
        private static SFS.UI.ModGUI.Label _throttle_Label;
        public static ThrustAssistMod.Updater updater;

//~         public static double AscentAcceleration
//~         {
//~             set
//~             {
//~                 _ascentAcceleration_Label.Text = string.Format("Ascent A: {0:N2} m/s^2",value);
//~             }
//~         }

//~         public static double AscentVelocity
//~         {
//~             set
//~             {
//~                 _ascentVelocity_Label.Text = string.Format("Ascent V: {0:N2} m/s",value);
//~             }
//~         }

//~         public static double Gravity
//~         {
//~             set
//~             {
//~                 _gravity_Label.Text = string.Format("Gravity: {0:N2} m/s^2",value);
//~             }
//~         }

//~         public static double Height
//~         {
//~             set
//~             {
//~                 _height_Label.Text = string.Format("Height: {0:N1} m",value);
//~             }
//~         }

        public static bool AssistOn
        { get { return _assistOn;} set { _assistOn=value;} }

        public static double MinThrottle
        {
            get
            {
                double minThrottle=System.Math.Pow(10,_minThrottle_Slider.Value);
                _minThrottle_Label.Text = string.Format("Min Throttle: {0:p2}",minThrottle);
                return minThrottle;
            }
        }

        public static double TargetHeight
        {
            get
            {
                double targetHeight=System.Math.Pow(10,_targetHeight_Slider.Value);
                _targetHeight_Label.Text = string.Format("Target Height: {0:n1} m",targetHeight);
                return targetHeight;
            }
        }

        public static double Throttle
        {
            set
            {
                _throttle_Label.Text = string.Format("Throttle: {0:P2}",value);
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

//~             _gravity_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Gravity: -");
//~             _gravity_Label.AutoFontResize=false;
//~             _gravity_Label.FontSize=25;

//~             _height_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Height: -");
//~             _height_Label.AutoFontResize=false;
//~             _height_Label.FontSize=25;

//~             _ascentVelocity_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Ascent V: -");
//~             _ascentVelocity_Label.AutoFontResize=false;
//~             _ascentVelocity_Label.FontSize=25;

//~             _ascentAcceleration_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 30, 0, 0, "Ascent A: -");
//~             _ascentAcceleration_Label.AutoFontResize=false;
//~             _ascentAcceleration_Label.FontSize=25;

            SFS.UI.ModGUI.Builder.CreateToggleWithLabel(window, 290,40, GetAssistOn, ChangeAssistOn,0,0,"Assist On");

            _targetHeight_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Target Height: -");
            _targetHeight_Label.AutoFontResize=false;
            _targetHeight_Label.FontSize=25;

            _targetHeight_Slider= SFS.UI.ModGUI.Builder.CreateSlider(window, 290, 0, (0f,4f));

            _minThrottle_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Min Throttle: -");
            _minThrottle_Label.AutoFontResize=false;
            _minThrottle_Label.FontSize=25;

            _minThrottle_Slider=SFS.UI.ModGUI.Builder.CreateSlider(window, 290, -3f, (-3f,0f));

            _throttle_Label = SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 20, 0, 0, "Throttle: -");
            _throttle_Label.AutoFontResize=false;
            _throttle_Label.FontSize=25;
        }

        public static bool GetAssistOn()
        { return _assistOn; }

        public static void ChangeAssistOn()
        { _assistOn=!_assistOn; }
    }
}
