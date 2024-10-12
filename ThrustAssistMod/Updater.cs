using System.Linq; // contains extensions

namespace ThrustAssistMod
{
    public class Updater : UnityEngine.MonoBehaviour
    {
        #region "Private fields"
            private static double _deorbitMarker_Minimum=90;
            private static bool _deorbitMarker_On=false;
            private static double _deorbitMarker_Target=90;
            private static double _lastTime=0;
            private static double _timeStep=0.2;
        #endregion

        #region "Private methods"
            private void Update()
            {
                string tracePoint = "S-01";

                if (ThrustAssistMod.UI.IsActive)
                {
                    try
                    {
                        double thisTime=SFS.World.WorldTime.main.worldTime;
                        if (_lastTime==0 || _lastTime>thisTime) _lastTime=thisTime;

                        if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                        {
                            System.Text.StringBuilder note = new System.Text.StringBuilder();

                            if (thisTime>_lastTime+_timeStep)
                            {
                                _lastTime=thisTime;

                                UnityEngine.Vector2 thrustVector = UnityEngine.Vector2.zero;
                                double mass = rocket.mass.GetMass();
                                SFS.World.Location location = rocket.location.Value;
                                Double2 velocity =
                                    location.velocity.Rotate(0.0 - (location.position.AngleRadians - System.Math.PI / 2.0));
                                double gravity = -location.planet.GetGravity(location.Radius);
                                double height =  location.TerrainHeight - rocket.GetSizeRadius();
                                double centrifugal = velocity.x*velocity.x/location.Radius;

                                foreach (SFS.Parts.Modules.EngineModule oneEngine in rocket.partHolder.GetModules<SFS.Parts.Modules.EngineModule>())
                                {
                                    if (oneEngine.engineOn.Value)
                                    {
                                        UnityEngine.Vector2 direction = (UnityEngine.Vector2)oneEngine.transform.TransformVector(oneEngine.thrustNormal.Value);

                                        thrustVector += direction*oneEngine.thrust.Value;
                                    }
                                }

                                double maxThrust = thrustVector.magnitude;
                                double targetThrottle =ThrustAssistMod.UI.TargetThrottle;

                                if (ThrustAssistMod.UI.MarkerOn && velocity.sqrMagnitude>0.01)
                                {
                                    double maxAcceleration= 9.8 * maxThrust / mass;
                                    Double2 stoppingDistance_Target = velocity*velocity/(2.0*maxAcceleration*targetThrottle);
                                    Double2 stoppingDistance_Minimum = velocity*velocity/(2.0*maxAcceleration);

                                    _deorbitMarker_Minimum=
                                        ThrustAssistMod.Utility.NormaliseAngle
                                            (
                                                ThrustAssistMod.UI.Marker
                                                + System.Math.Sign(velocity.x)*(stoppingDistance_Minimum.x/location.Radius)*ThrustAssistMod.Utility.degreesPerRadian
                                            );

                                    _deorbitMarker_Target=
                                        ThrustAssistMod.Utility.NormaliseAngle
                                            (
                                                ThrustAssistMod.UI.Marker
                                                + System.Math.Sign(velocity.x)*(stoppingDistance_Target.x/location.Radius)*ThrustAssistMod.Utility.degreesPerRadian
                                            );

                                    _deorbitMarker_On=true;
                                }
                                else
                                {
                                    _deorbitMarker_On=false;
                                }

                                if (ThrustAssistMod.UI.AssistOn && !SFS.World.PlayerController.main.HasControl(SFS.UI.MsgDrawer.main))
                                {
                                    ThrustAssistMod.UI.AssistOff();
                                }

                                if (ThrustAssistMod.UI.AssistOn)
                                {
                                    if (maxThrust<0.01)
                                    {
                                        ThrustAssistMod.UI.AssistOff();
                                    }
                                    else
                                    {
                                        // positive is up or left
                                        Double2 forward = ((Double2)thrustVector/maxThrust).Rotate(0.0 - (location.position.AngleRadians - System.Math.PI / 2.0));
                                        double landingVelocity =ThrustAssistMod.UI.LandingVelocity;
                                        double targetHeight = ThrustAssistMod.UI.TargetHeight;
                                        Double2 environmentAcceleration =Double2.up*(gravity + centrifugal);
                                        Double2 maxRocketAcceleration = forward * 9.8 * maxThrust / mass;
                                        Double2 maxRocketDeceleration = Double2.zero;
                                        Double2 targetDistance = new Double2
                                            (
                                                -ThrustAssistMod.Utility.NormaliseAngle(ThrustAssistMod.UI.Marker - location.position.AngleDegrees)
                                                    *ThrustAssistMod.Utility.radiansPerDegree*location.Radius
                                                ,targetHeight-height
                                            );
                                        Double2 targetVelocity=Double2.zero;

                                        if (!ThrustAssistMod.UI.AssistMark) targetDistance.x=0;
                                        if (!ThrustAssistMod.UI.AssistSurface) targetDistance.y=0;

                                        if ( Double2.Dot(forward,targetDistance)<0)
                                        {
                                            maxRocketDeceleration=maxRocketAcceleration;
                                            maxRocketAcceleration=Double2.zero;
                                        }


        //~                                 double maxRocketAcceleration.y = maxRocketAcceleration*System.Math.Cos(System.Math.PI*angle/180);
        //~                                 double velocity.y = location.VerticalVelocity;
        //~                                 double targetDistance.y = targetHeight-height;

                                        Double2 fastestAcceleration = maxRocketAcceleration + environmentAcceleration;
                                        Double2 preferredAcceleration = maxRocketAcceleration*targetThrottle + environmentAcceleration;
                                        Double2 fastestDeceleration = maxRocketDeceleration + environmentAcceleration;
                                        Double2 preferredDeceleration = maxRocketDeceleration*targetThrottle + environmentAcceleration;

        //~                                 double fastestAcceleration.y = ((maxRocketAcceleration.y>0)?maxRocketAcceleration.y:0) + gravity + centrifugal/2;
        //~                                 double fastestDeceleration.y = ((maxRocketAcceleration.y>0)?0:maxRocketAcceleration.y) + gravity + centrifugal/2;
        //~                                 double preferredAcceleration.y = ((maxRocketAcceleration.y>0)?maxRocketAcceleration.y*targetThrottle:0) + gravity + centrifugal/2;
        //~                                 double preferredDeceleration.y = ((maxRocketAcceleration.y>0)?0:maxRocketAcceleration.y*targetThrottle) + gravity + centrifugal/2;

                                        Double2 targetAcceleration = Double2.zero;
                                        double targetAccelerationMagnitude = 0;
                                        double portionThrustOnTarget = 0;
                                        double targetThrust = 0;
                                        double chosenThrust = 0;
                                        double throttle=0;

                                        if (ThrustAssistMod.UI.AssistSurface)
                                        {
                                            targetVelocity.y = System.Math.Sign(targetDistance.y)* System.Math.Sqrt
                                                (
                                                    2.0*targetDistance.y
                                                    *System.Math.Sign(targetDistance.y)*System.Math.Sign(forward.y)
                                                    *
                                                        (
                                                            (System.Math.Sign(forward.y)*preferredDeceleration.y>0)
                                                            ?preferredDeceleration.y
                                                            :(
                                                                (System.Math.Abs(fastestDeceleration.y)>0.5)
                                                                ?System.Math.Sign(forward.y)*0.5
                                                                :fastestDeceleration.y
                                                            )
                                                        )
    //~                                                 *(
    //~                                                     (System.Math.Sign(targetDistance.y)==System.Math.Sign(forward.y))
    //~                                                     ?(
    //~                                                         (System.Math.Sign(forward.y)*preferredAcceleration.y>0)
    //~                                                         ?preferredAcceleration.y
    //~                                                         :(
    //~                                                             (System.Math.Abs(fastestAcceleration.y)>0.5)
    //~                                                             ?System.Math.Sign(fastestAcceleration.y)*0.5
    //~                                                             :fastestAcceleration.y
    //~                                                         )
    //~                                                     )
    //~                                                     :-(
    //~                                                         (System.Math.Sign(forward.y)*preferredDeceleration.y>0)
    //~                                                         ?preferredDeceleration.y
    //~                                                         :(
    //~                                                             (System.Math.Abs(fastestDeceleration.y)>0.5)
    //~                                                             ?System.Math.Sign(forward.y)*0.5
    //~                                                             :fastestDeceleration.y
    //~                                                         )
    //~                                                     )
    //~                                                 )
                                                ); // will be NaN if pointing the wrong way or descending and max vertical accelleration <gravity

//~                                             if (double.IsNaN(targetVelocity.y)) targetVelocity.y=0;

                                            if
                                                (
                                                    !double.IsNaN(targetVelocity.y)
                                                    && targetHeight==0
                                                    && ( targetVelocity.y<0 || height<0.5)
                                                    && -targetVelocity.y<landingVelocity
                                                ) targetVelocity.y= -landingVelocity;
                                        }
                                        else
                                        {
                                            targetVelocity.y = 0;
                                        }

                                        if (ThrustAssistMod.UI.AssistMark)
                                        {
                                            targetVelocity.x = System.Math.Sign(targetDistance.x)* System.Math.Sqrt
                                                (
                                                    2.0*targetDistance.x
                                                    *(
                                                        (System.Math.Sign(targetDistance.x)==System.Math.Sign(forward.x))
                                                        ?preferredAcceleration.x:-preferredDeceleration.x
                                                    )
                                                ); // will be NaN if pointing the wrong way

//~                                             if (double.IsNaN(targetVelocity.x)) targetVelocity.x=0;
                                        }
                                        else
                                        {
                                            targetVelocity.x = 0;
                                        }

                                        targetAcceleration = (targetVelocity-velocity)/_timeStep - environmentAcceleration;

                                        if
                                            (
                                                double.IsNaN(targetAcceleration.y)
                                                || !ThrustAssistMod.UI.AssistSurface
                                                || (targetAcceleration.y*velocity.y>0 && targetDistance.y<0)
                                                || System.Math.Abs(forward.y)<0.02
                                            ) targetAcceleration.y=0;

                                        if
                                            (
                                                double.IsNaN(targetAcceleration.x)
                                                || !ThrustAssistMod.UI.AssistMark
                                                || targetAcceleration.x*velocity.x>0
                                                || System.Math.Abs(forward.x)<0.02
//~                                                 || velocity.x*targetDistance.x<0
                                            ) targetAcceleration.x=0;

                                        targetAccelerationMagnitude = targetAcceleration.magnitude;

                                        if (targetAccelerationMagnitude<0.001)
                                        {
                                            portionThrustOnTarget = 0;
                                        }
                                        else if (targetAcceleration.x!=0 && targetAcceleration.y!=0)
                                        {
                                            portionThrustOnTarget = System.Math.Min
                                                (
                                                    System.Math.Sign(targetAcceleration.x)*forward.x
                                                    ,System.Math.Sign(targetAcceleration.y)*forward.y
                                                );
                                        }
                                        else if (targetAcceleration.x!=0)
                                        {
                                            portionThrustOnTarget = System.Math.Sign(targetAcceleration.x)*forward.x;
                                        }
                                        else if (targetAcceleration.y!=0)
                                        {
                                            portionThrustOnTarget = System.Math.Sign(targetAcceleration.y)*forward.y;
                                        }
//~                                         else
//~                                         {
//~                                             portionThrustOnTarget = Double2.Dot(targetAcceleration,forward)/targetAccelerationMagnitude;
//~                                         }

                                        if (portionThrustOnTarget<0.01)
                                        {
                                            targetThrust=0;
                                        }
                                        else
                                        {
                                            targetThrust = mass*targetAccelerationMagnitude/(9.8*portionThrustOnTarget);
                                        }

                                        if (targetThrust<0.01)
                                        {
                                            chosenThrust = 0;
                                        }
                                        else if (targetThrust>maxThrust)
                                        {
                                            chosenThrust=maxThrust;
                                        }
                                        else
                                        {
                                            chosenThrust=targetThrust;
                                        }

                                        if (chosenThrust<0.0005)
                                        {
                                            throttle = 0;
                                        }
                                        else
                                        {
                                            throttle = chosenThrust/maxThrust;
                                        }

                                        if (ThrustAssistMod.UI.Debug)
                                        {
                                            if (ThrustAssistMod.UI.AssistMark && ThrustAssistMod.UI.AssistSurface)
                                            {
                                                note.AppendFormat("Target V: ({0:N1},{1:N1}) m/s", targetVelocity.x, targetVelocity.y);
                                                note.AppendFormat(", Target A: ({0:N1},{1:N1}) m/s2", targetAcceleration.x, targetAcceleration.y);
                                            }
                                            else if (ThrustAssistMod.UI.AssistMark)
                                            {
                                                note.AppendFormat("Target V: {0:N1} m/s", targetVelocity.x);
                                                note.AppendFormat(", Target A: {0:N1} m/s2", targetAcceleration.x);
                                            }
                                            else if (ThrustAssistMod.UI.AssistSurface)
                                            {
                                                note.AppendFormat("Target V: {0:N1} m/s", targetVelocity.y);
                                                note.AppendFormat(", Target A: {0:N1} m/s2", targetAcceleration.y);
                                            }
                                            note.AppendFormat(", Portion: {0:P1}", portionThrustOnTarget);
                                            note.AppendFormat(", Target T: {0:N1} t", targetThrust);
                                        }
                                        else
                                        {
                                            if (ThrustAssistMod.UI.AssistMark || ThrustAssistMod.UI.AssistSurface)
                                            {
                                                if ( double.IsNaN(targetVelocity.x)) targetVelocity.x=0;
                                                if ( double.IsNaN(targetVelocity.y)) targetVelocity.y=0;
                                                note.AppendFormat("Target V: {0:N1} m/s", targetVelocity.magnitude);
                                            }
                                        }
                                        rocket.throttle.throttlePercent.Value=(float)throttle;
                                        rocket.throttle.throttleOn.Value=(throttle>=0.01);
                                    }
                                }
                                if (!ThrustAssistMod.UI.AssistOn)
                                {
                                    note.Clear();
                                }
                                {
                                    double timeToImpact=-height/velocity.y;

                                    if (ThrustAssistMod.UI.Debug || (timeToImpact>0 && timeToImpact<100))
                                    {
                                        if (note.Length!=0) note.Append(ThrustAssistMod.UI.Debug?", ":"\n");

                                        note.AppendFormat
                                            (
                                                "Land in {0:F0} s"
                                                ,-height/velocity.y
                                            );
                                    }
                                }

                                ThrustAssistMod.UI.Note = note.ToString();
                            }
                        }
                    }
                    catch (System.Exception excp)
                    {
                        UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Updater.Update-{0}] {1}", tracePoint ,excp.ToString());
                    }
                }
            }
        #endregion

        #region "Public properties"
           public static double TimeStep
            {
                get
                {
                    return _timeStep;
                }
                set
                {
                    _timeStep=value;
                    SettingsManager.settings.timeStep = _timeStep;
                    ThrustAssistMod.SettingsManager.Save();
                }
            }

           public static double DeorbitMarker_Minimum
            {
                get
                {
                    return _deorbitMarker_Minimum;
                }
            }

            public static bool DeorbitMarker_On
            {
                get
                {
                    return _deorbitMarker_On;
                }
            }

            public static double DeorbitMarker_Target
            {
                get
                {
                    return _deorbitMarker_Target;
                }
            }
        #endregion

        #region "Internal methods"
            internal static void SwitchOff()
            {
                string tracePoint = "S-01";

                if (ThrustAssistMod.UI.IsActive)
                {
                    try
                    {
                        if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                        {
                            rocket.throttle.throttlePercent.Value=1;
                            rocket.throttle.throttleOn.Value=false;
                        }
                    }
                    catch (System.Exception excp)
                    {
                        UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Updater.SwitchOff-{0}] {1}", tracePoint ,excp.ToString());
                    }
                }
            }
        #endregion
    }
}
