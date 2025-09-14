using System.Linq; // contains extensions

namespace ThrustAssistMod
{
    public class UI
    {
        #region "Public enums"
            public enum ANAIS_State_Enum { Off, OrbitChange, FinalApproach }
        #endregion

        #region "Private classes"
            private class _UpDownValueLog
            {
                SFS.UI.ModGUI.Button _downButton=null;
                SFS.UI.ModGUI.Button _downFineButton=null;
                double _fineStep;
                string _formatText;
                bool _isUnused=false;
                double _max;
                SFS.UI.ModGUI.Button _maxButton=null;
                double _maxLog;
                double _min;
                SFS.UI.ModGUI.Button _minButton=null;
                double _minLog;
                string _minText="";
                int _steps;
                SFS.UI.ModGUI.Button _upButton=null;
                SFS.UI.ModGUI.Button _upFineButton=null;
                double _valueLog;
                double _valueFine;
                private SFS.UI.ModGUI.Label _value_Label;

                public string FormatText
                {
                    get { return  _formatText; }
                    set
                    {
                        _formatText=value;

                        if (_valueLog==_minLog && _minText!="")
                        {
                            _value_Label.Text = _minText;
                        }
                        else
                        {
                            _value_Label.Text = string.Format(_formatText, Value);
                        }
                    }
                }

                public bool IsUnused
                {
                    get { return _isUnused; }
                    set
                    {
                        _isUnused = value;
                        _value_Label.Color = _isUnused?UnityEngine.Color.grey:UnityEngine.Color.white;
                    }
                }

                public bool IsMinimum
                {  get { return  (_valueLog==_minLog && _valueFine<=0); }}

                public string MinText
                {
                    get { return  _minText; }
                    set
                    {
                        _minText=value;

                        if (_valueLog==_minLog && _minText!="")
                        {
                            _value_Label.Text = _minText;
                        }
                        else
                        {
                            _value_Label.Text = string.Format(_formatText, Value);
                        }
                    }
                }

                public double MinValue
                {  get { return System.Math.Log(_minLog); }}

                public double Value
                {
                    get
                    {
                        double result = System.Math.Exp(_valueLog)+_valueFine;

                        if (result<=_min)
                        {
                            result =_min;
                        }
                        else if (result>=_max)
                        {
                            result =_max;
                        }
                        return result;
                    }
                    set
                    {
                        _valueLog = System.Math.Log(value);
                        _valueFine = 0;
                        _value_Label.Text = string.Format(_formatText, value);
                    }
                }

                private void Value_DownButton_Click()
                {
                    _valueFine = 0;
                    _valueLog -= (_maxLog-_minLog)/_steps;
                    if (_valueLog<_minLog) _valueLog=_minLog;

                    if (_valueLog==_minLog && _minText!="")
                    {
                        _value_Label.Text = _minText;
                    }
                    else
                    {
                        _value_Label.Text = string.Format(_formatText, Value);
                    }
                }

                private void Value_DownFineButton_Click()
                {
                    _valueFine -= _fineStep;

                    if (Value>_min)
                    {
                        _value_Label.Text = string.Format(_formatText, Value);
                    }
                    else
                    {
                        _valueLog=_minLog;
                        _valueFine=0;
                        if  (_minText!="") _value_Label.Text = _minText;
                    }
                }

                private void Value_MinButton_Click()
                {
                    _valueFine = 0;
                    _valueLog=_minLog;
                    if (_minText!="")
                    {
                        _value_Label.Text = _minText;
                    }
                    else
                    {
                        _value_Label.Text = string.Format(_formatText, Value);
                    }
                }

                private void Value_MaxButton_Click()
                {
                    _valueFine = 0;
                    _valueLog=_maxLog;
                    _value_Label.Text = string.Format(_formatText, Value);
                }

                private void Value_UpButton_Click()
                {
                    _valueFine = 0;
                    _valueLog += (_maxLog-_minLog)/_steps;
                    if (_valueLog>_maxLog) _valueLog=_maxLog;
                    _value_Label.Text = string.Format(_formatText, Value);
                }

                private void Value_UpFineButton_Click()
                {
                    _valueFine += _fineStep;

                    if (Value>=_max)
                    {
                        _valueLog=_maxLog;
                        _valueFine=0;
                    }
                    _value_Label.Text = string.Format(_formatText,Value);
                }

                public _UpDownValueLog
                    (
                        UnityEngine.Transform window
                        ,string formatText
                        ,double value=1.0
                        ,double min=1.0
                        ,double max=10.0
                        ,int steps=10
                        ,double fineStep=0
                    )
                {
                    _fineStep = fineStep;
                    _formatText=formatText;
                    _max = max;
                    _maxLog = System.Math.Log(max);
                    _min = min;
                    _minLog = System.Math.Log(min);
                    _steps=steps;
                    _valueFine=0;
                    _valueLog=System.Math.Log(value);

                    SFS.UI.ModGUI.Container value_Container =  SFS.UI.ModGUI.Builder.CreateContainer(window);
                    value_Container.CreateLayoutGroup(SFS.UI.ModGUI.Type.Horizontal, UnityEngine.TextAnchor.MiddleCenter,5f);

                    if (_fineStep!=0)
                    {
                        _minButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_MinButton_Click,"<<<");
                        _downButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_DownButton_Click,"<<");
                        _downFineButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_DownFineButton_Click,"<");
                    }
                    else
                    {
                        _downButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_MinButton_Click,"<<");
                        _downFineButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_DownButton_Click,"<");
                    }

                    _value_Label = SFS.UI.ModGUI.Builder.CreateLabel(value_Container, (_fineStep!=0)?140:210, 20, 0, 0, string.Format(_formatText, value));
                    _value_Label.AutoFontResize=false;
                    _value_Label.FontSize=25;

                    if (_fineStep!=0)
                    {
                        _upFineButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_UpFineButton_Click,">");
                        _upButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_UpButton_Click,">>");
                        _maxButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_MaxButton_Click,">>>");
                    }
                    else
                    {
                        _upFineButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_UpButton_Click,">");
                        _upButton=SFS.UI.ModGUI.Builder.CreateButton(value_Container,30,30,0,0,Value_MaxButton_Click,">>");
                    }
                }
            }
        #endregion

        #region "Private fields"
            // Create a GameObject for your window to attach to.
            private static UnityEngine.GameObject windowHolder;

            // Random window ID to avoid conflicts with other mods.
            private static readonly int MainWindowID = SFS.UI.ModGUI.Builder.GetRandomID();

            private static ANAIS_State_Enum _anais_State=ANAIS_State_Enum.Off;
            private static bool _assistANAIS=false;
            private static bool _assistSurface=false;
            private static bool _assistMark=false;
            private static SFS.UI.ModGUI.Button _assistANAIS_Button;
            private static SFS.UI.ModGUI.Button _assistMark_Button;
            private static SFS.UI.ModGUI.Button _assistOff_Button;
            private static SFS.UI.ModGUI.Label _assistSelect_Label;
            private static SFS.UI.ModGUI.Button _assistSurface_Button;
            private static bool _debug=false;
            private static bool _isActive=false;
            private static _UpDownValueLog _landingVelocity_UDV;
            private static double _marker=90;
            private static int _markerDirection = 0;
            private static SFS.UI.ModGUI.Button _markerOnOff_Button;
            private static bool _markerOn=false;
            private const double _markerStep_Large=1000;
            private const double _markerStep_Small=30;
            private static SFS.UI.ModGUI.Label _note_Label;
            private static _UpDownValueLog _minDistance_UDV;
            private static _UpDownValueLog _targetThrottle_UDV;
        #endregion

        #region "Private methods"
            private static void AssistChanged()
            {
                if (_isActive && _assistMark_Button!=null && _assistSurface_Button!=null && _assistOff_Button!=null && _assistANAIS_Button!=null && _assistSelect_Label!=null)
                {
                    _assistANAIS_Button.Active=(_anais_State!=ANAIS_State_Enum.Off);
                    _assistANAIS_Button.Text=_assistANAIS?"ANAIS":"anais";
                    _assistMark_Button.Active=_markerOn;
                    _assistMark_Button.Text=_assistMark?"Mark":"mark";
                    _assistSurface_Button.Text=_assistSurface?"Surf.":"surf";

                    if (!_assistSurface && !_assistMark && !_assistANAIS)
                    {
                        _assistOff_Button.Text="Off";
                        ThrustAssistMod.Updater.SwitchOff();
                    }
                    else
                    {
                        _assistOff_Button.Text="off";
                    }

                    if (_assistSurface)
                    {
                        _minDistance_UDV.IsUnused = false;
                    }
                    else if (_assistANAIS && _anais_State==ANAIS_State_Enum.FinalApproach)
                    {
                        _minDistance_UDV.IsUnused = false;
                    }
                    else
                    {
                        _minDistance_UDV.IsUnused = true;
                    }

                    _landingVelocity_UDV.IsUnused = !_assistSurface;

                    if (_assistSurface || _assistMark)
                    {
                        _targetThrottle_UDV.IsUnused = false;
                    }
                    else if (_assistANAIS && _anais_State==ANAIS_State_Enum.FinalApproach)
                    {
                        _targetThrottle_UDV.IsUnused = false;
                    }
                    else
                    {
                        _targetThrottle_UDV.IsUnused = true;
                    }

                    if (_assistSurface)
                    {
                        _minDistance_UDV.FormatText = "H: {0:F1} m" ;
                        _minDistance_UDV.MinText = "H: Surface";
                    }
                    else if (_assistANAIS)
                    {
                        _minDistance_UDV.FormatText = "D: {0:F1} m" ;
                        _minDistance_UDV.MinText = "D: min";
                    }

                    if (_assistSurface && _assistMark)
                    {
                        _assistSelect_Label.Text="Assist: Both";
                    }
                    else if (_assistSurface)
                    {
                        _assistSelect_Label.Text="Assist: Surface";
                    }
                    else if (_assistMark)
                    {
                        _assistSelect_Label.Text="Assist: Mark";
                    }
                    else if (_assistANAIS)
                    {
                        _assistSelect_Label.Text="Assist: ANAIS";
                    }
                    else
                    {
                        _assistSelect_Label.Text="Assist: Off";
                    }
                }
            }

            public static void AssistANAIS_Button_Click()
            {
                AssistANAIS=!AssistANAIS;

                if (AssistANAIS)
                {
                    AssistMark=false;
                    AssistSurface=false;
                }
                AssistChanged();
            }

            public static void AssistMark_Button_Click()
            {
                AssistMark=!AssistMark;
                AssistANAIS=false;
//~                 if (AssistMark) AssistSurface=false;
                AssistChanged();
            }

            public static void AssistOff_Button_Click()
            {
                AssistOff();
            }

            public static void AssistSurface_Button_Click()
            {
                AssistSurface=!AssistSurface;
                AssistANAIS=false;
//~                 if (AssistSurface) AssistMark=false;
                AssistChanged();
            }

            private static System.Collections.Generic.SortedSet<double> GetMarkerTargets()
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

//~                 System.Text.StringBuilder debugText = new System.Text.StringBuilder();

//~                 foreach(double oneAngle in angles)
//~                 {
//~                     if (debugText.Length>0) debugText.Append(", ");
//~                     debugText.AppendFormat("{0:f1}",oneAngle);
//~                 }

//~                 DebugItem=debugText.ToString();

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
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker + ThrustAssistMod.Utility.MetersToDegrees(_markerStep_Large));
            }

            private static void Marker_BackSmall_Click()
            {
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker + ThrustAssistMod.Utility.MetersToDegrees(_markerStep_Small));
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
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker - ThrustAssistMod.Utility.MetersToDegrees(_markerStep_Large));
            }

            private static void Marker_ForwardSmall_Click()
            {
                Marker = ThrustAssistMod.Utility.NormaliseAngle(Marker - ThrustAssistMod.Utility.MetersToDegrees(_markerStep_Small));
            }

            private static void Marker_OnOff_Click()
            {
                MarkerOn = !MarkerOn;
            }

            private static void Marker_UpdateDisplay()
            {
                if (_isActive)
                {
                    if (_markerOn)
                    {
                        _markerOnOff_Button.Text = string.Format("{0:N3}°", _marker);

                        if (_markerDirection<0)
                        {
                            _markerOnOff_Button.Text = "<" +  _markerOnOff_Button.Text;
                        }
                        else if (_markerDirection>0)
                        {
                            _markerOnOff_Button.Text =  _markerOnOff_Button.Text + ">";
                        }
                    }
                    else
                    {
                        _markerOnOff_Button.Text = "Marker";
//~                     // ######################
//~                     Double2 launchPadPos = SFS.Base.planetLoader.spaceCenter.LaunchPadLocation.position;
//~                     UnityEngine.Vector2  fp32_launchPadPos = SFS.World.WorldView.ToLocalPosition(launchPadPos);
//~                     _note_Label.Text = string.Format("lp:{0}",fp32_launchPadPos);
//~                     // ######################
                    }
                }
                AssistChanged();
            }


        #endregion

        #region "Public properties"
            public static ANAIS_State_Enum ANAIS_State
            {
                get
                {
                    return _anais_State;
                }
            }

            public static bool AssistANAIS
            {
                get
                {
                    return (_anais_State != ANAIS_State_Enum.Off) && _assistANAIS;
                }
                set
                {
                    bool assistANAIS_Old = _assistANAIS;
                    _assistANAIS=((_anais_State != ANAIS_State_Enum.Off) && value);
                    if (assistANAIS_Old != _assistANAIS) AssistChanged();
                }
            }

            public static bool AssistMark
            {
                get
                {
                    return _markerOn && _assistMark;
                }
                set
                {
                    bool assistMark_Old = _assistMark;
                    _assistMark=(_markerOn && value);
                    if (assistMark_Old != _assistMark) AssistChanged();
                }
            }

            public static bool AssistOn
            {
                get
                {
                    return _assistMark || _assistSurface || _assistANAIS;
                }
            }

            public static bool AssistSurface
            {
                get
                {
                    return _assistSurface;
                }
                set
                {
                    bool assistSurface_Old = _assistSurface;
                    _assistSurface=value;
                    if (assistSurface_Old != _assistSurface) AssistChanged();
                }
            }

            public static bool Debug
            {
                get
                {
                    return _debug;
                }
                set
                {
                    _debug=value;
                   _note_Label.FontSize = _debug?15:25;
                   SettingsManager.settings.debug = _debug;
                    ThrustAssistMod.SettingsManager.Save();
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

            public static int MarkerDirection
            {
                get { return _markerDirection; }
                set
                {
                    int oldValue = _markerDirection;
                    _markerDirection = value;
                    if (oldValue!=value) Marker_UpdateDisplay();
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

                    if (!_markerOn)
                    {
                        _assistMark=false;
                    }
                }
            }

            public static bool IsActive
            { get { return _isActive;}}

            public static double LandingVelocity
            {
                get
                {
                    if (_isActive && _landingVelocity_UDV!=null)
                    {
                        return _landingVelocity_UDV.Value;
                    }
                    else
                    {
                        return 0;
                    }
                }
                set
                {
                    if (_isActive && _minDistance_UDV!=null)
                    {
                        _landingVelocity_UDV.Value=value ;
                    }
                }
            }

            public static string Note
            {
                get
                {
                    if (_isActive)
                    {
                        return _note_Label.Text;
                    }
                    else
                    {
                        return "";
                    }
                }
                set
                {
                    if (_isActive) _note_Label.Text = value;
                }
            }

            public static double MinDistance
            {
                get
                {
                    if (_isActive && _minDistance_UDV!=null)
                    {
                        return (_minDistance_UDV.IsMinimum)?0:_minDistance_UDV.Value;
                    }
                    else
                    {
                        return 0;
                    }
                }
                set
                {
                    if (_isActive && _minDistance_UDV!=null)
                    {
                        _minDistance_UDV.Value=(value<_minDistance_UDV.MinValue)?_minDistance_UDV.MinValue:value ;
                    }
                }
            }

            public static double TargetThrottle
            {
                get
                {
                    if (_isActive && _targetThrottle_UDV!=null)
                    {
                        return _targetThrottle_UDV.Value;
                    }
                    else
                    {
                        return 0;
                    }
                }
                set
                {
                    if (_isActive && _targetThrottle_UDV!=null)
                    {
                        _targetThrottle_UDV.Value=value ;
                    }
                }
            }

        #endregion

        #region "Public methods"
            public static void ANAISCheck()
            {
                ANAIS_State_Enum anais_State_Old = _anais_State;

                if (Main.ANAISTraverse==null)
                {
                    _anais_State = ANAIS_State_Enum.Off;
                }
                else
                {
                    switch ( Main.ANAISTraverse.Field("_navState").GetValue().ToString())
                    {
                        case "DEFAULT": _anais_State =ANAIS_State_Enum.Off; break;
                        case "ANAIS_TRANSFER_PLANNED": _anais_State =ANAIS_State_Enum.OrbitChange; break;
                        default: _anais_State =ANAIS_State_Enum.FinalApproach; break;
                    }
                }

                if (_anais_State == ANAIS_State_Enum.Off) _assistANAIS=false;
                if (anais_State_Old != _anais_State) AssistChanged();
            }

            public static void AssistOff()
            {
                _assistANAIS=false;
                _assistMark=false;
                _assistSurface=false;
                AssistChanged();
            }

            public static void ShowGUI()
            {
                _isActive = false;
                // Create the window holder, attach it to the currently active scene so it's removed when the scene changes.
                windowHolder = SFS.UI.ModGUI.Builder.CreateHolder(SFS.UI.ModGUI.Builder.SceneToAttach.CurrentScene, "ThrustAssistMod GUI Holder");
                UnityEngine.Vector2Int pos = SettingsManager.settings.windowPosition;
                _debug =  SettingsManager.settings.debug;
//~                 SFS.UI.ModGUI.Window window = SFS.UI.ModGUI.Builder.CreateWindow(windowHolder.transform, MainWindowID, 360, 290, pos.x, pos.y, true, true, 0.95f, "Thrust Assist");
                SFS.UI.ModGUI.Window window = SFS.UI.ModGUI.Builder.CreateWindow(windowHolder.transform, MainWindowID, 360, 400, pos.x, pos.y, true, true, 0.95f, "Thrust Assist");

                // Create a layout group for the window. This will tell the GUI builder how it should position elements of your UI.
                window.CreateLayoutGroup(SFS.UI.ModGUI.Type.Vertical, UnityEngine.TextAnchor.MiddleCenter,10f);

                window.gameObject.GetComponent<SFS.UI.DraggableWindowModule>().OnDropAction += () =>
                {
                    ThrustAssistMod.SettingsManager.settings.windowPosition = UnityEngine.Vector2Int.RoundToInt(window.Position);
                    ThrustAssistMod.SettingsManager.Save();
                };

                _assistSelect_Label =  SFS.UI.ModGUI.Builder.CreateLabel(window, 140, 30, 0, 0, "Assist: Off");
                {
                    SFS.UI.ModGUI.Container assistSelect_Container =  SFS.UI.ModGUI.Builder.CreateContainer(window);
                    assistSelect_Container.CreateLayoutGroup(SFS.UI.ModGUI.Type.Horizontal, UnityEngine.TextAnchor.MiddleCenter,5f);
                    _assistSelect_Label.TextAlignment=TMPro.TextAlignmentOptions.Left;
                    _assistOff_Button = SFS.UI.ModGUI.Builder.CreateButton(assistSelect_Container,60,30,0,0,AssistOff_Button_Click,"Off");
                    _assistSurface_Button = SFS.UI.ModGUI.Builder.CreateButton(assistSelect_Container,60,30,0,0,AssistSurface_Button_Click,"Surf.");
                    _assistMark_Button = SFS.UI.ModGUI.Builder.CreateButton(assistSelect_Container,60,30,0,0,AssistMark_Button_Click,"Mark");
                    _assistANAIS_Button = SFS.UI.ModGUI.Builder.CreateButton(assistSelect_Container,60,30,0,0,AssistANAIS_Button_Click,"ANAIS");
                }

                _minDistance_UDV = new _UpDownValueLog(window, "H: {0:F1} m", 32 , 1, 1024,10,0.5);
                _minDistance_UDV.MinText = "H: Surface";
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

                _note_Label =  SFS.UI.ModGUI.Builder.CreateLabel(window, 290, 100);
                _note_Label.AutoFontResize = false;
                _note_Label.FontSize = _debug?15:25;
                _note_Label.TextAlignment=TMPro.TextAlignmentOptions.Top;

                _isActive = true;
                Marker_UpdateDisplay();
            }

            public static void GUIInActive()
            {
                _isActive = false;
            }
        #endregion
    }
}
