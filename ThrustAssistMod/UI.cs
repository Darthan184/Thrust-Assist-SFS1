using System.Linq; // contains extensions

namespace ThrustAssistMod
{
    public class UI
    {
        #region "Private classes"
            private class _UpDownValueLog
            {
                string _formatText;
                double _maxLog;
                double _minLog;
                string _minText="";
                int _steps;
                double _valueLog;
                private SFS.UI.ModGUI.Label _value_Label;

                public string MinText
                {  get { return  _minText; } set {  _minText=value; } }

                public bool IsMinimum
                {  get { return  _valueLog==_minLog; }}

                public double MinValue
                {  get { return System.Math.Log(_minLog); }}

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

                    if (_valueLog==_minLog && _minText!="")
                    {
                        _value_Label.Text = _minText;
                    }
                    else
                    {
                        _value_Label.Text = string.Format(_formatText, System.Math.Exp(_valueLog));
                    }
                }

                private void Value_MinButton_Click()
                {
                    _valueLog=_minLog;
                    if (_minText!="")
                    {
                        _value_Label.Text = _minText;
                    }
                    else
                    {
                        _value_Label.Text = string.Format(_formatText, System.Math.Exp(_valueLog));
                    }
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
        #endregion

        #region "Private fields"
            // Create a GameObject for your window to attach to.
            private static UnityEngine.GameObject windowHolder;

            // Random window ID to avoid conflicts with other mods.
            private static readonly int MainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();

            private static bool _assistOn=false;
            private static SFS.UI.ModGUI.ButtonWithLabel _assistOn_Button;
            private static bool _isActive=false;
            private static double _marker=90;
            private static SFS.UI.ModGUI.Button _markerOnOff_Button;
            private static bool _markerOn=false;
            private static _UpDownValueLog _targetThrottle_UDV;
            private static _UpDownValueLog _landingVelocity_UDV;
            private static _UpDownValueLog _targetHeight_UDV;
            private static SFS.UI.ModGUI.Label _debug_Label;
            private static SFS.UI.ModGUI.Label _targetVelocity_Label=null;
        #endregion

        #region "Private methods"
            private static  System.Collections.Generic.SortedSet<double> GetMarkerTargets()
            {
                System.Collections.Generic.SortedSet<double> angles = new System.Collections.Generic.SortedSet<double>();
                SFS.WorldBase.Planet planet=SFS.World.PlayerController.main.player.Value.location.Value.planet;

                foreach(SFS.World.Maps.Landmark oneLandmark in planet.landmarks)
                {
                    angles.Add(ThrustAssistMod.Utility.NormaliseAngle(oneLandmark.data.angle)); // degrees? radians?, assume degrees
                }

                foreach(SFS.World.Rocket oneRocket in SFS.World.GameManager.main.rockets)
                {
                    if (oneRocket.location.planet==planet && oneRocket.location.velocity.Value.magnitude<0.01)
                    {
                        angles.Add(ThrustAssistMod.Utility.NormaliseAngle(oneRocket.location.position.Value.AngleDegrees));
                    }
                }

                if (SFS.Base.planetLoader.spaceCenter.address == planet.DisplayName)
                {
                    Double2 launchPadPos = SFS.Base.planetLoader.spaceCenter.LaunchPadLocation.position;
                    angles.Add(launchPadPos.AngleDegrees);
                }

                if (angles.Count>0)
                {
                    double lastAngle = angles.Max-360;
                    System.Collections.Generic.List<double> newAngles = new System.Collections.Generic.List<double>();

                    foreach(double oneAngle in angles)
                    {
                        int interpolateCount = (int)System.Math.Floor((oneAngle-lastAngle)/36.0);

                        if (interpolateCount>1)
                            for (int i=1;i<interpolateCount;i++)
                            {
                                newAngles.Add
                                    (
                                        ThrustAssistMod.Utility.NormaliseAngle
                                            (
                                                lastAngle + (oneAngle-lastAngle)*(double)i/(double)interpolateCount
                                            )
                                    );
                            }

                        lastAngle = oneAngle;
                    }
                    angles.UnionWith(newAngles);
                }

                System.Text.StringBuilder debugText = new System.Text.StringBuilder();

                foreach(double oneAngle in angles)
                {
                    if (debugText.Length>0) debugText.Append(", ");
                    debugText.AppendFormat("{0:f1}",oneAngle);
                }

                DebugItem=debugText.ToString();

                return angles;
            }

            private static void Marker_BackLandmark_Click()
            {
                System.Collections.Generic.SortedSet<double> angles = GetMarkerTargets();

                if (angles.Count==0)
                {
                    Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker + 36);
                }
                else
                {
                    double? result=null;

                    foreach (double oneAngle in angles)
                    {
                        if (oneAngle>Marker)
                        {
                            result=oneAngle;
                            break;
                        }
                    }

                    if (result == null)
                    {
                        Marker=angles.Min;
                    }
                    else
                    {
                        Marker=result.Value;
                    }
                }
            }

            private static void Marker_BackLarge_Click()
            {
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker + ThrustAssistMod.Utility.MetersToDegrees(50));
            }

            private static void Marker_BackSmall_Click()
            {
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker + ThrustAssistMod.Utility.MetersToDegrees(5));
            }

            private static void Marker_ForwardLandmark_Click()
            {
                System.Collections.Generic.SortedSet<double> angles = GetMarkerTargets();

                if (angles.Count==0)
                {
                    Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker - 36);
                }
                else
                {
                    double? result=null;

                    foreach (double oneAngle in angles.Reverse())
                    {
                        if (oneAngle<Marker)
                        {
                            result=oneAngle;
                            break;
                        }
                    }

                    if (result == null)
                    {
                        Marker=angles.Max;
                    }
                    else
                    {
                        Marker=result.Value;
                    }
                }
           }

            private static void Marker_ForwardLarge_Click()
            {
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker - ThrustAssistMod.Utility.MetersToDegrees(50));
            }

            private static void Marker_ForwardSmall_Click()
            {
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker - ThrustAssistMod.Utility.MetersToDegrees(5));
            }

            private static void Marker_OnOff_Click()
            {
                MarkerOn = !MarkerOn;
            }

            private static void Marker_UpdateDisplay()
            {
                if (_markerOn)
                {
                    _markerOnOff_Button.Text = string.Format("{0:N3}°", _marker);
                }
                else
                {
                    _markerOnOff_Button.Text = "Marker";
//~                     // ######################
//~                     Double2 launchPadPos = SFS.Base.planetLoader.spaceCenter.LaunchPadLocation.position;
//~                     UnityEngine.Vector2  fp32_launchPadPos = SFS.World.WorldView.ToLocalPosition(launchPadPos);
//~                     _debug_Label.Text = string.Format("lp:{0}",fp32_launchPadPos);
//~                     // ######################
                }
            }

        #endregion

        #region "Public properties"
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
                        if (_assistOn_Button!=null) _assistOn_Button.label.Text="Assist: Surface";
                    }
                    else
                    {
                        if (_assistOn_Button!=null) _assistOn_Button.label.Text="Assist: Off";
                        if (_targetVelocity_Label!=null) _targetVelocity_Label.Text="";
                    }
                }
            }

            public static string DebugItem
            {
                set
                {
                    _debug_Label.Text = value;
                }
            }
            public static double Marker
            {
                get
                {
                    return _marker;
                }
                set
                {
                    _marker=value;
                    Marker_UpdateDisplay();
                }
            }

            public static bool MarkerOn
            {
                get
                {
                    return _markerOn;
                }
                set
                {
                    _markerOn=value;
                    Marker_UpdateDisplay();
                }
            }

            public static bool IsActive
            { get { return _isActive;}}

            public static double LandingVelocity
            {
                get
                {
                    if (_landingVelocity_UDV==null)
                    {
                        return 0;
                    }
                    else
                    {
                        return _landingVelocity_UDV.Value;
                    }
                }
                set
                {
                    if (_targetHeight_UDV!=null)
                    {
                        _landingVelocity_UDV.Value=value ;
                    }
                }
            }

            public static double TargetHeight
            {
                get
                {
                    if (_targetHeight_UDV==null)
                    {
                        return 0;
                    }
                    else
                    {
                        return (_targetHeight_UDV.IsMinimum)?0:_targetHeight_UDV.Value;
                    }
                }
                set
                {
                    if (_targetHeight_UDV!=null)
                    {
                        _targetHeight_UDV.Value=(value<_targetHeight_UDV.MinValue)?_targetHeight_UDV.MinValue:value ;
                    }
                }
            }

            public static double TargetThrottle
            {
                get
                {
                    if (_targetThrottle_UDV==null)
                    {
                        return 0;
                    }
                    else
                    {
                        return _targetThrottle_UDV.Value;
                    }
                }
                set
                {
                    if (_targetThrottle_UDV!=null)
                    {
                        _targetThrottle_UDV.Value=value ;
                    }
                }
            }

            public static double TargetVelocity
            {
                set
                {
                    if (_targetVelocity_Label!=null)
                    {
                        if (double.IsNaN(value))
                        {
                            _targetVelocity_Label.Text="Target V: ???" ;
                        }
                        else
                        {
                            _targetVelocity_Label.Text=string.Format("Target V: {0:N1} m/s", value) ;
                        }
                    }
                }
            }
        #endregion

        #region "Public methods"
            public static void ShowGUI()
            {
                _isActive = false;
                // Create the window holder, attach it to the currently active scene so it's removed when the scene changes.
                windowHolder = SFS.UI.ModGUI.Builder.CreateHolder(SFS.UI.ModGUI.Builder.SceneToAttach.CurrentScene, "ThrustAssistMod GUI Holder");
                UnityEngine.Vector2Int pos = SettingsManager.settings.windowPosition;
                SFS.UI.ModGUI.Window window = SFS.UI.ModGUI.Builder.CreateWindow(windowHolder.transform, MainWindowID, 360, 290, pos.x, pos.y, true, true, 0.95f, "Thrust Assist");

                // Create a layout group for the window. This will tell the GUI builder how it should position elements of your UI.
                window.CreateLayoutGroup(SFS.UI.ModGUI.Type.Vertical, UnityEngine.TextAnchor.MiddleCenter,10f);

                window.gameObject.GetComponent<SFS.UI.DraggableWindowModule>().OnDropAction += () =>
                {
                    ThrustAssistMod.SettingsManager.settings.windowPosition = UnityEngine.Vector2Int.RoundToInt(window.Position);
                    ThrustAssistMod.SettingsManager.Save();
                };

                _assistOn_Button = SFS.UI.ModGUI.Builder.CreateButtonWithLabel(window, 290,30, 0,0, "Assist: Off","Toggle",ChangeAssistOn);
                _targetHeight_UDV = new _UpDownValueLog(window, "Height: {0:N1} m", 32 , 1, 1024);
                _targetHeight_UDV.MinText = "Height: Surface";
                _landingVelocity_UDV = new _UpDownValueLog(window, "Land at: {0:N1} m/s", 4, 1.0 , 10);
                _targetThrottle_UDV = new _UpDownValueLog(window, "Throttle: {0:P0}", 0.8, 0.05 , 1.0, 15);

                {
                    SFS.UI.ModGUI.Container marker_Container =  SFS.UI.ModGUI.Builder.CreateContainer(window);
                    marker_Container.CreateLayoutGroup(SFS.UI.ModGUI.Type.Horizontal, UnityEngine.TextAnchor.MiddleCenter,5f);

                    SFS.UI.ModGUI.Builder.CreateButton(marker_Container,40,30,0,0,Marker_BackLandmark_Click,"<<<");
                    SFS.UI.ModGUI.Builder.CreateButton(marker_Container,30,30,0,0,Marker_BackLarge_Click,"<<");
                    SFS.UI.ModGUI.Builder.CreateButton(marker_Container,30,30,0,0,Marker_BackSmall_Click,"<");

                    _markerOnOff_Button=SFS.UI.ModGUI.Builder.CreateButton(marker_Container,120,30,0,0,Marker_OnOff_Click,"Marker");

                    SFS.UI.ModGUI.Builder.CreateButton(marker_Container,30,30,0,0,Marker_ForwardSmall_Click,">");
                    SFS.UI.ModGUI.Builder.CreateButton(marker_Container,30,30,0,0,Marker_ForwardLarge_Click,">>");
                    SFS.UI.ModGUI.Builder.CreateButton(marker_Container,40,30,0,0,Marker_ForwardLandmark_Click,">>>");
                }

                _debug_Label =  SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 30);
                _debug_Label.AutoFontResize = false;
                _debug_Label.FontSize = 15;

//~                 _targetVelocity_Label =  SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 30);
//~                 _targetVelocity_Label.AutoFontResize = false;
//~                 _targetVelocity_Label.FontSize = 25;

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
        #endregion
    }
}
