namespace ThrustAssistMod
{
    public class UI
    {
        private class _UpDownValueLog
        {
            string _formatText;
            double _maxLog;
            double _minLog;
            int _steps;
            double _valueLog;
            private SFS.UI.ModGUI.Label _value_Label;

            public double Value
            {
                get
                {
                    return System.Math.Exp(_valueLog);
                }
                set
                {
                    _valueLog = System.Math.Log(value);
                    _value_Label.Text = string.Format(_formatText, value);
                }
            }

            private void Value_DownButton_Click()
            {
                _valueLog -= (_maxLog-_minLog)/_steps;
                if (_valueLog<_minLog) _valueLog=_minLog;
                _value_Label.Text = string.Format(_formatText, System.Math.Exp(_valueLog));
            }

            private void Value_MinButton_Click()
            {
                _valueLog=_minLog;
                _value_Label.Text = string.Format(_formatText, System.Math.Exp(_valueLog));
            }

            private void Value_MaxButton_Click()
            {
                _valueLog=_maxLog;
                _value_Label.Text = string.Format(_formatText, System.Math.Exp(_valueLog));
            }

            private void Value_UpButton_Click()
            {
                _valueLog += (_maxLog-_minLog)/_steps;
                if (_valueLog>_maxLog) _valueLog=_maxLog;
                _value_Label.Text = string.Format(_formatText, System.Math.Exp(_valueLog));
            }

            public _UpDownValueLog
                (
                    UnityEngine.Transform window
                    ,string formatText
                    ,double value=1.0
                    ,double min=1.0
                    ,double max=10.0
                    ,int steps=10
                )
            {
                _formatText=formatText;
                _maxLog = System.Math.Log(max);
                _minLog = System.Math.Log(min);
                _steps=steps;
                _valueLog=System.Math.Log(value);

                SFS.UI.ModGUI.Container value_Container =  SFS.UI.ModGUI.Builder.CreateContainer(window);
                value_Container.CreateLayoutGroup(SFS.UI.ModGUI.Type.Horizontal, UnityEngine.TextAnchor.MiddleCenter,5f);

                SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_MinButton_Click,"<<");
                SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_DownButton_Click,"<");

                _value_Label = SFS.UI.ModGUI.Builder.CreateLabel(value_Container, 210, 20, 0, 0, string.Format(_formatText, value));
                _value_Label.AutoFontResize=false;
                _value_Label.FontSize=25;

                SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_UpButton_Click,">");
                SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_MaxButton_Click,">>");
            }
        }

        // Create a GameObject for your window to attach to.
        private static UnityEngine.GameObject windowHolder;

        // Random window ID to avoid conflicts with other mods.
        private static readonly int MainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();
        private static bool _assistOn=false;
        private static SFS.UI.ModGUI.ButtonWithLabel _assistOn_Button;
        private static bool _isActive=false;
        private static _UpDownValueLog _minThrottle_UDV;
        private static _UpDownValueLog _minVelocity_UDV;
        private static _UpDownValueLog _targetHeight_UDV;
        public static ThrustAssistMod.Updater updater;

        public static bool AssistOn
        {
            get
            {
                return _assistOn;
            }
            set
            {
                _assistOn=value;

                if (_assistOn)
                {
                    _assistOn_Button.label.Text="Assist: Surface";
                }
                else
                {
                    _assistOn_Button.label.Text="Assist: Off";
                }
            }
        }

        public static bool IsActive
        { get { return _isActive;}}

        public static double MinThrottle
        { get { return _minThrottle_UDV.Value; } set { _minThrottle_UDV.Value=value ;} }

        public static double MinVelocity
        { get { return _minVelocity_UDV.Value; } set { _minVelocity_UDV.Value=value ;} }

        public static double TargetHeight
        { get { return _targetHeight_UDV.Value; } set { _targetHeight_UDV.Value=value ;} }

        public static void ShowGUI()
        {
            _isActive = false;
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

            _assistOn_Button = SFS.UI.ModGUI.Builder.CreateButtonWithLabel(window, 290,30, 0,0, "Assist: Off","Toggle",ChangeAssistOn);
            _targetHeight_UDV = new _UpDownValueLog(window, "Height: {0:N0} m", 40 , 1, 10000);
            _minVelocity_UDV = new _UpDownValueLog(window, "Land at: {0:N1} m/s", 4, 0.5 , 10, 10);
            _minThrottle_UDV = new _UpDownValueLog(window, "Min Throttle: {0:P0}", 0.8, 0.01 , 1.0, 20);

            _isActive = true;
        }

        public static void GUIInActive()
        {
            _isActive = false;
        }

        public static void ChangeAssistOn()
        {
            AssistOn=!_assistOn;

            if (!_assistOn) ThrustAssistMod.Updater.SwitchOff();
        }
    }
}
