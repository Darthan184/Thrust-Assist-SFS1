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
                            ThrustAssistMod.UI.AssistOn=false;
                        }
                        else if (thisTime>lastTime+timeStep && ThrustAssistMod.UI.AssistOn)
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
                                ThrustAssistMod.UI.AssistOn = false;
                            }
                            else
                            {
                                // positive is up or left
                                SFS.World.Location location = rocket.location.Value;
                                Double2 forward =  Double2.up.Rotate(0.0 - (location.position.AngleRadians - System.Math.PI / 2.0));
                                double targetThrottle =ThrustAssistMod.UI.TargetThrottle;
                                double landingVelocity =ThrustAssistMod.UI.LandingVelocity;
                                double targetHeight = ThrustAssistMod.UI.TargetHeight;
                                Double2 velocity = location.velocity.Rotate(0.0 - (location.position.AngleRadians - System.Math.PI / 2.0));
                                double gravity = -location.planet.GetGravity(location.Radius);
                                double height =  location.TerrainHeight - rocket.GetSizeRadius();
                                double centrifugal = velocity.x*velocity.x*location.Radius;
                                Double2 targetDistance = new Double2
                                    (
                                        NormaliseAngle(ThrustAssistMod.UI.Marker -location.position.AngleDegrees)*ThrustAssistMod.Utility.radiansPerDegree*location.Radius
                                        ,targetHeight-height
                                    );


                                Double2 environmentAcceleration =Double2.up*(gravity + centrifugal/2);
                                Double2 maxRocketAcceleration = forward * 9.8 * maxThrust / rocket.mass.GetMass();
                                Double2 maxRocketDeceleration = maxRocketAcceleration;

                                if (System.Math.Sign(targetDistance.x)==System.Math.Sign(maxRocketAcceleration.x))
                                {
                                    maxRocketDeceleration.x=0;
                                }
                                else
                                {
                                    maxRocketAcceleration.x=0;
                                }

                                if (System.Math.Sign(targetDistance.y)==System.Math.Sign(maxRocketAcceleration.y))
                                {
                                    maxRocketDeceleration.y=0;
                                }
                                else
                                {
                                    maxRocketAcceleration.y=0;
                                }

                                Double2 targetVelocity;

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

                                Double2 targetThrust;
                                double chosenThrust = 0;
                                double throttle=0;

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

                                if (ThrustAssistMod.UI.MarkerOn)
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
                                else if
                                    (
                                        System.Math.Abs(velocity.x)<5.0
                                        || System.Math.Abs(targetVelocity.y)<System.Math.Abs(velocity.x)*0.1
                                        || System.Math.Abs(velocity.y)<System.Math.Abs(velocity.x)*0.1
                                    )
                                {
                                    targetVelocity.x = 0;
                                }
                                else
                                {
                                    targetVelocity.x = targetVelocity.y*velocity.x/velocity.y;
                                }

                                if
                                    (
                                        targetHeight==0
                                        && ( targetVelocity.y<0 || height<0.5)
                                        && -targetVelocity.y<landingVelocity
                                    ) targetVelocity.y= -landingVelocity;

                                targetThrust = (targetVelocity-velocity)/(2.0*timeStep) - environmentAcceleration;

                                double targetThrust_Dot = Double2.Dot(targetThrust,forward);

                                if (targetThrust_Dot<0.01)
                                {
                                    chosenThrust = 0;
                                }
                                else if (targetThrust_Dot>maxThrust)
                                {
                                    chosenThrust=maxThrust;
                                }
                                else
                                {
                                    chosenThrust=targetThrust_Dot;
                                }

                                if (chosenThrust<0.0005)
                                {
                                    throttle = 0;
                                }
                                else
                                {
                                    throttle = chosenThrust/maxThrust;
                                }

                                rocket.throttle.throttlePercent.Value=(float)throttle;
                                rocket.throttle.throttleOn.Value=(throttle>=0.01);
                                ThrustAssistMod.UI.TargetVelocity = targetVelocity.y;
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
