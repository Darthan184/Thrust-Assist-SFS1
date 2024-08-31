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

                    if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket && ThrustAssistMod.UI.AssistOn)
                    {
                        if (thisTime>lastTime+timeStep)
                        {
                            lastTime=thisTime;

                            // positive is up for all vertical values
                            double minThrottle =ThrustAssistMod.UI.MinThrottle;
                            double minVelocity =ThrustAssistMod.UI.MinVelocity;
                            double targetHeight = ThrustAssistMod.UI.TargetHeight;

                            SFS.World.Location location = rocket.location.Value;
                            double thrust = rocket.partHolder.GetModules<SFS.Parts.Modules.EngineModule>().Sum((SFS.Parts.Modules.EngineModule a) => (a.engineOn.Value)?a.thrust.Value:0)
                                + rocket.partHolder.GetModules<SFS.Parts.Modules.BoosterModule>().Sum((SFS.Parts.Modules.BoosterModule b) => b.thrustVector.Value.magnitude);
                            double maxRocketAcceleration = 9.8 * thrust / rocket.mass.GetMass();
                            double angle =  NormaliseAngle(location.position.AngleDegrees - rocket.GetRotation());
                            double maxRocketAcceleration_Vertical = maxRocketAcceleration*System.Math.Cos(System.Math.PI*angle/180);
                            double velocity_Vertical = location.VerticalVelocity;
                            double gravity = -location.planet.GetGravity(location.Radius);
                            double height =  location.TerrainHeight - rocket.GetSizeRadius();
                            double targetDistance_Vertical = targetHeight-height;

                            double fastestAcceleration_GoingUp = ((maxRocketAcceleration_Vertical>0)?maxRocketAcceleration_Vertical:0) + gravity;
                            double fastestAcceleration_GoingDown = ((maxRocketAcceleration_Vertical>0)?0:maxRocketAcceleration_Vertical) + gravity;
                            double preferredAcceleration_GoingUp = ((maxRocketAcceleration_Vertical>0)?maxRocketAcceleration_Vertical*0.8:0) + gravity;
                            double preferredAcceleration_GoingDown = ((maxRocketAcceleration_Vertical>0)?0:maxRocketAcceleration_Vertical*0.8) + gravity;

//~                             double preferredStoppingDistance_Vertical = - 2.0 * velocity_Vertical * velocity_Vertical /
//~                                     (
//~                                         (velocity_Vertical>0)
//~                                         ?preferredAcceleration_GoingDown
//~                                         :preferredAcceleration_GoingUp
//~                                     );
//~                             double minStoppingDistance_Vertical = - 2.0 * velocity_Vertical * velocity_Vertical /
//~                                     (
//~                                         (velocity_Vertical>0)
//~                                         ?fastestAcceleration_GoingDown
//~                                         :fastestAcceleration_GoingUp
//~                                     );

                            double targetVelocity_Vertical = System.Math.Sign(targetDistance_Vertical)* System.Math.Sqrt
                                (
                                    -2.0*targetDistance_Vertical
                                    *(
                                        (targetDistance_Vertical<0)
                                        ?((preferredAcceleration_GoingUp<0)?fastestAcceleration_GoingUp:preferredAcceleration_GoingUp)
                                        :preferredAcceleration_GoingDown
                                    )
                                ); // will be NaN if descending and max vertical accelleration <gravity

//~                             double targetVelocity_Vertical =  (maxRocketAcceleration_Vertical>gravity)
//~                                 ?System.Math.Sqrt
//~                                     (
//~                                         0.8*(maxRocketAcceleration_Vertical-gravity)*System.Math.Abs(targetDistance_Vertical)
//~                                     )*System.Math.Sign(targetDistance_Vertical)
//~                                 :0;

//~                             double targetAcceleration_Vertical =  -2.0*velocity_Vertical*velocity_Vertical/targetDistance_Vertical;

                            double chosenAcceleration_Vertical = 0;
                            double throttle=0;


                            if (double.IsNaN(targetVelocity_Vertical)) targetVelocity_Vertical=0;

                            if
                                (
                                    targetHeight<1.5
                                    && ( targetVelocity_Vertical<0 || height<0.5)
                                    && -targetVelocity_Vertical<minVelocity
                                ) targetVelocity_Vertical= -minVelocity;

                            ThrustAssistMod.UI.TargetVelocity = targetVelocity_Vertical;

                            chosenAcceleration_Vertical = (targetVelocity_Vertical-velocity_Vertical)/(2.0*timeStep);

                            if (chosenAcceleration_Vertical>fastestAcceleration_GoingUp)
                            {
                                chosenAcceleration_Vertical=fastestAcceleration_GoingUp;
                            }
                            else if (chosenAcceleration_Vertical<fastestAcceleration_GoingDown)
                            {
                                chosenAcceleration_Vertical=fastestAcceleration_GoingDown;
                            }
//~                             if (System.Math.Abs(targetDistance_Vertical)<1.0 && targetHeight>1.5)   // near target and not landing
//~                             {
//~                                 chosenAcceleration_Vertical=-gravity; // hover
//~                             }
//~                             else if (targetDistance_Vertical>0)  // target is up
//~                                 if
//~                                     (
//~                                         velocity_Vertical<=0
//~                                         || targetAcceleration_Vertical>System.Math.Min(maxRocketAcceleration_Vertical,0)*minThrottle+gravity
//~                                     )
//~                                 {
//~                                     chosenAcceleration_Vertical=maxRocketAcceleration_Vertical+gravity;
//~                                 }
//~                                 else
//~                                 {
//~                                     chosenAcceleration_Vertical = targetAcceleration_Vertical;
//~                                 }
//~                             }
//~                             else // target is down
//~                             {
//~                                 if
//~                                     (
//~                                         velocity_Vertical>=0
//~                                         || targetAcceleration_Vertical<System.Math.Max(maxRocketAcceleration_Vertical,0)*minThrottle+gravity
//~                                     )
//~                                 {
//~                                     chosenAcceleration_Vertical=gravity-maxRocketAcceleration_Vertical;
//~                                 }
//~                                 else
//~                                 {
//~                                     chosenAcceleration_Vertical = targetAcceleration_Vertical;
//~                                 }
//~                             }

//~                                 if (targetVelocity_Vertical<0 && -targetVelocity_Vertical>minVelocity)  minVelocity= -targetVelocity_Vertical;

//~                                 if (targetHeight<1.5 && chosenAcceleration_Vertical*timeStep + velocity_Vertical >-minVelocity)
//~                                 {
//~                                     chosenAcceleration_Vertical = - (velocity_Vertical+minVelocity)/timeStep;
//~                                 }

                            if (maxRocketAcceleration_Vertical<0.001)
                            {
                                throttle = 0;
                            }
                            else
                            {
                                throttle = (chosenAcceleration_Vertical-gravity)/maxRocketAcceleration_Vertical;
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
                            rocket.throttle.throttleOn.Value=(throttle>=minThrottle-0.00001);
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
