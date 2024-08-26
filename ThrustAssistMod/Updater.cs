using System.Linq; // contains extensions

namespace ThrustAssistMod
{
    public class Updater : UnityEngine.MonoBehaviour
    {
        private static double NormaliseAngle(double input) => ((input + 540) % 360) - 180;
        private static double lastTime=0;


        private void Update()
        {
            string tracePoint = "S-01";

            if (ThrustAssistMod.UI.IsActive)
            {
                try
                {
                    double thisTime=SFS.World.WorldTime.main.worldTime;
                    if (lastTime==0 || lastTime>thisTime) lastTime=thisTime;

                    if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket && ThrustAssistMod.UI.AssistOn)
                    {
                        if (thisTime>lastTime+0.2)
                        {
                            lastTime=thisTime;

                            double minThrottle =ThrustAssistMod.UI.MinThrottle;
                            double minVelocity =ThrustAssistMod.UI.MinVelocity;
                            double targetHeight = ThrustAssistMod.UI.TargetHeight;

                            SFS.World.Location location = rocket.location.Value;
                            double thrust = rocket.partHolder.GetModules<SFS.Parts.Modules.EngineModule>().Sum((SFS.Parts.Modules.EngineModule a) => (a.engineOn.Value)?a.thrust.Value:0)
                                + rocket.partHolder.GetModules<SFS.Parts.Modules.BoosterModule>().Sum((SFS.Parts.Modules.BoosterModule b) => b.thrustVector.Value.magnitude);
                            double accelerationMax = 9.8 * thrust / rocket.mass.GetMass();
                            double angle =  NormaliseAngle(location.position.AngleDegrees - rocket.GetRotation());
                            double maxAscentAcceleration = accelerationMax*System.Math.Cos(System.Math.PI*angle/180);
                            double ascentVelocity = location.VerticalVelocity;
                            double gravity = -location.planet.GetGravity(location.Radius);
                            double height =  location.TerrainHeight - rocket.GetSizeRadius();
                            double targetDistance = targetHeight-height;
                            double targetAcceleration = 0;
                            double targetDeceleration =  -2*ascentVelocity*ascentVelocity/targetDistance;
                            double throttle=0;

                            if (height<1 && targetHeight<1.5)
                            {
                                targetAcceleration=0;
                            }
                            if (System.Math.Abs(targetDistance)<1)
                            {
                                targetAcceleration=-gravity;
                            }
                            else if (targetDistance>0)
                            {
                                if
                                    (
                                        ascentVelocity<=0
                                        || targetDeceleration>System.Math.Min(maxAscentAcceleration,0)*minThrottle+gravity
                                    )
                                {
                                    targetAcceleration=maxAscentAcceleration+gravity;
                                }
                                else
                                {
                                    targetAcceleration = targetDeceleration;
                                }
                            }
                            else
                            {
                                if
                                    (
                                        ascentVelocity>=0
                                        || targetDeceleration<System.Math.Max(maxAscentAcceleration,0)*minThrottle+gravity
                                    )
                                {
                                    targetAcceleration=gravity-maxAscentAcceleration;
                                }
                                else
                                {
                                    targetAcceleration = targetDeceleration;
                                }
                            }

                            if (maxAscentAcceleration<0.001)
                            {
                                throttle = 0;
                            }
                            else
                            {
                                throttle = (targetAcceleration-gravity)/maxAscentAcceleration;
                            }

                            if (throttle>1)
                            {
                                throttle = 1;
                            }
                            else if (throttle<0)
                            {
                                throttle = 0;
                            }

                            rocket.throttle.throttlePercent.Value=(float)throttle;

                            if (targetHeight<1.5)
                            {
                                rocket.throttle.throttleOn.Value=(throttle>=minThrottle-0.00001 && -ascentVelocity>minVelocity );
                            }
                            else
                            {
                                rocket.throttle.throttleOn.Value=(throttle>=minThrottle-0.00001);
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
