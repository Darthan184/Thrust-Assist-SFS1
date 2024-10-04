using System.Linq; // contains extensions

namespace ThrustAssistMod
{
    public class Updater : UnityEngine.MonoBehaviour
    {
        private static double NormaliseAngle(double input) => ((input + 540) % 360) - 180;
        private static double lastTime=0;
        private const double timeStep=0.2;

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
                    UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Updater.AssistOff-{0}] {1}", tracePoint ,excp.ToString());
                }
            }
        }

        private void Update()
        {
            string tracePoint = "S-02";

            if (ThrustAssistMod.UI.IsActive)
            {
                try
                {
                    double thisTime=SFS.World.WorldTime.main.worldTime;
                    if (lastTime==0 || lastTime>thisTime) lastTime=thisTime;

                    if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
                    {
                        if (!SFS.World.PlayerController.main.HasControl(SFS.UI.MsgDrawer.main))
                        {
                            ThrustAssistMod.UI.AssistOff();
                        }
                        else if (thisTime>lastTime+timeStep)
                        {
                            if (ThrustAssistMod.UI.AssistOn)
                            {
                                lastTime=thisTime;

                                UnityEngine.Vector2 thrustVector = UnityEngine.Vector2.zero;

                                foreach (SFS.Parts.Modules.EngineModule oneEngine in rocket.partHolder.GetModules<SFS.Parts.Modules.EngineModule>())
                                {
                                    if (oneEngine.engineOn.Value)
                                    {
                                        UnityEngine.Vector2 direction = (UnityEngine.Vector2)oneEngine.transform.TransformVector(oneEngine.thrustNormal.Value);

                                        thrustVector += direction*oneEngine.thrust.Value;
                                    }
                                }

                                double maxThrust = thrustVector.magnitude;
//~                                 double angle =  NormaliseAngle(location.position.AngleDegrees - rocket.GetRotation());
                                if (maxThrust<0.01)
                                {
                                    ThrustAssistMod.UI.AssistOff();
                                }
                                else
                                {
                                    double mass = rocket.mass.GetMass();

                                    // positive is up or left
                                    SFS.World.Location location = rocket.location.Value;
                                    Double2 forward = ((Double2)thrustVector/maxThrust).Rotate(0.0 - (location.position.AngleRadians - System.Math.PI / 2.0));
                                    double targetThrottle =ThrustAssistMod.UI.TargetThrottle;
                                    double landingVelocity =ThrustAssistMod.UI.LandingVelocity;
                                    double targetHeight = ThrustAssistMod.UI.TargetHeight;
                                    Double2 velocity =
                                        location.velocity.Rotate(0.0 - (location.position.AngleRadians - System.Math.PI / 2.0));
                                    double gravity = -location.planet.GetGravity(location.Radius);
                                    double height =  location.TerrainHeight - rocket.GetSizeRadius();
                                    double centrifugal = velocity.x*velocity.x/location.Radius;
                                    Double2 environmentAcceleration =Double2.up*(gravity + centrifugal);
                                    Double2 maxRocketAcceleration = forward * 9.8 * maxThrust / mass;
                                    Double2 maxRocketDeceleration = Double2.zero;
                                    Double2 targetDistance = new Double2
                                        (
                                            -NormaliseAngle(ThrustAssistMod.UI.Marker - location.position.AngleDegrees)
                                                *ThrustAssistMod.Utility.radiansPerDegree*location.Radius
                                            ,targetHeight-height
                                        );
                                    Double2 targetVelocity=Double2.zero;

                                    if (!ThrustAssistMod.UI.AssistMark) targetDistance.x=0;
                                    if (!ThrustAssistMod.UI.AssistSurface) targetDistance.y=0;

                                    if ( Double2.Dot(maxRocketAcceleration,targetDistance)<0)
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
                                                -2.0*targetDistance.y
                                                *(
                                                    (System.Math.Sign(targetDistance.y)==System.Math.Sign(forward.y))
                                                    ?(
                                                        (preferredAcceleration.y>0)
                                                        ?preferredAcceleration.y
                                                        :(
                                                            (fastestAcceleration.y>0.5)
                                                            ?0.5
                                                            :fastestAcceleration.y
                                                        )
                                                    )
                                                    :preferredDeceleration.y
                                                )
                                            ); // will be NaN if pointing the wrong way or descending and max vertical accelleration <gravity

                                        if (double.IsNaN(targetVelocity.y)) targetVelocity.y=0;

                                        if
                                            (
                                                targetHeight==0
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
                                                -2.0*targetDistance.x
                                                *(
                                                    (System.Math.Sign(targetDistance.x)==System.Math.Sign(forward.x))
                                                    ?(
                                                        (preferredAcceleration.x>0)
                                                        ?preferredAcceleration.x
                                                        :(
                                                            (fastestAcceleration.x>0.5)
                                                            ?0.5
                                                            :fastestAcceleration.x
                                                        )
                                                    )
                                                    :preferredDeceleration.x
                                                )
                                            ); // will be NaN if pointing the wrong way

                                        if (double.IsNaN(targetVelocity.x)) targetVelocity.x=0;
                                    }
                                    else
                                    {
                                        targetVelocity.x = 0;
                                    }

                                    targetAcceleration = (targetVelocity-velocity)/(2.0*timeStep) - environmentAcceleration;

                                    if (!ThrustAssistMod.UI.AssistSurface) targetAcceleration.y=0;
                                    if (!ThrustAssistMod.UI.AssistMark) targetAcceleration.x=0;

                                    targetAccelerationMagnitude = targetAcceleration.magnitude;

                                    if (targetAccelerationMagnitude<0.001)
                                    {
                                        portionThrustOnTarget = 0;
                                    }
                                    else
                                    {
                                        portionThrustOnTarget = Double2.Dot(targetAcceleration,forward)/targetAccelerationMagnitude;
                                    }

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

                                    ThrustAssistMod.UI.Note = "";
//~                                     ThrustAssistMod.UI.Note += string.Format(" V: ({0:N1},{1:N1}) m/s", velocity.x, velocity.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" D: ({0:N1},{1:N1}) m", targetDistance.x, targetDistance.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" Max R. A: ({0:N1},{1:N1}) m/s2", maxRocketAcceleration.x, maxRocketAcceleration.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" Max R. -A: ({0:N1},{1:N1}) m/s2", maxRocketDeceleration.x, maxRocketDeceleration.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" Max A: ({0:N1},{1:N1}) m/s2", fastestAcceleration.x, fastestAcceleration.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" Max -A: ({0:N1},{1:N1}) m/s2", fastestDeceleration.x, fastestDeceleration.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" Pref A: ({0:N1},{1:N1}) m/s2", preferredAcceleration.x, preferredAcceleration.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" Pref -A: ({0:N1},{1:N1}) m/s2", preferredDeceleration.x, preferredDeceleration.y);

//~                                     ThrustAssistMod.UI.Note += string.Format(" gravity: {0:N1} m/s2", gravity);
//~                                     ThrustAssistMod.UI.Note += string.Format(" centrifugal: {0:N1} m/s2", centrifugal);
//~                                     ThrustAssistMod.UI.Note += string.Format(" centrifugal+gravity: {0:N1} m/s2", centrifugal+gravity);


                                    if (ThrustAssistMod.UI.AssistMark && ThrustAssistMod.UI.AssistSurface)
                                    {
                                        ThrustAssistMod.UI.Note += string.Format(" Target V: ({0:N1},{1:N1}) m/s", targetVelocity.x, targetVelocity.y);
                                        ThrustAssistMod.UI.Note += string.Format(" Target A: ({0:N1},{1:N1}) m/s2", targetAcceleration.x, targetAcceleration.y);
                                    }
                                    else if (ThrustAssistMod.UI.AssistMark)
                                    {
                                        ThrustAssistMod.UI.Note += string.Format(" Target V: {0:N1} m/s", targetVelocity.x);
                                        ThrustAssistMod.UI.Note += string.Format(" Target A: {0:N1} m/s2", targetAcceleration.x);
                                    }
                                    else if (ThrustAssistMod.UI.AssistSurface)
                                    {
                                        ThrustAssistMod.UI.Note += string.Format(" Target V: {0:N1} m/s", targetVelocity.y);
                                        ThrustAssistMod.UI.Note += string.Format(" Target A: {0:N1} m/s2", targetAcceleration.y);
                                    }
                                    ThrustAssistMod.UI.Note += string.Format(" Portion: {0:P1}", portionThrustOnTarget);
                                    ThrustAssistMod.UI.Note += string.Format(" Target T: {0:N1} t", targetThrust);

//~                                     rocket.throttle.throttlePercent.Value=(float)throttle;
//~                                     rocket.throttle.throttleOn.Value=(throttle>=0.01);
                                }
                            }
                            else
                            {
                                ThrustAssistMod.UI.Note = "";
                            }
                        }
                    }
                }
                catch (System.Exception excp)
                {
                    UnityEngine.Debug.LogErrorFormat("[ThrustAssistMod.Updater.Update-{0}] {1}", tracePoint ,excp.ToString());
                }
            }
        }
    }
}
