using System.Linq; // contains extensions

namespace ThrustAssistMod
{
    public class Updater : UnityEngine.MonoBehaviour
    {
        private static double NormaliseAngle(double input) => ((input + 540) % 360) - 180;

        private void Update()
        {
            if (SFS.World.PlayerController.main.player.Value is SFS.World.Rocket rocket)
            {
                SFS.World.Location location = rocket.location.Value;
                double thrust = rocket.partHolder.GetModules<SFS.Parts.Modules.EngineModule>().Sum((SFS.Parts.Modules.EngineModule a) => (a.engineOn.Value)?a.thrust.Value:0) + rocket.partHolder.GetModules<SFS.Parts.Modules.BoosterModule>().Sum((SFS.Parts.Modules.BoosterModule b) => b.thrustVector.Value.magnitude);
                double accelerationMax = 9.8 * thrust / rocket.mass.GetMass();
                double angle =  NormaliseAngle(location.position.AngleDegrees - rocket.GetRotation());
                double ascentAcceleration = accelerationMax*System.Math.Cos(System.Math.PI*angle/180);
                double ascentVelocity = location.VerticalVelocity;
                double gravity = -location.planet.GetGravity(location.Radius);
                double height =  location.TerrainHeight - rocket.GetSizeRadius();
                double targetDistance = ThrustAssistMod.UI.TargetHeight-height;
                double targetAcceleration = 0;
                double throttle=0;

                if (System.Math.Abs(targetDistance)<1)
                {
                    targetAcceleration=-gravity;
                }
                else if (System.Math.Sign(ascentVelocity)== System.Math.Sign(targetDistance))
                {
                    targetAcceleration = -2*ascentVelocity*ascentVelocity/targetDistance;
                }
                else if (targetDistance>0)
                {
                    targetAcceleration=ascentAcceleration+gravity;
                }
                else
                {
                    targetAcceleration=gravity-ascentAcceleration;
                }

                if (ascentAcceleration<0.001)
                {
                    throttle = 0;
                }
                else
                {
                    throttle = (targetAcceleration-gravity)/ascentAcceleration;
                }

                if (throttle>1)
                {
                    throttle = 1;
                }
                else if (throttle<1)
                {
                    throttle = 0;
                }

                ThrustAssistMod.UI.Throttle = throttle;

//~                 ThrustAssistMod.UI.AscentAcceleration = ascentAcceleration;
//~                 ThrustAssistMod.UI.AscentVelocity = ascentVelocity;
//~                 ThrustAssistMod.UI.Gravity = gravity;
//~                 ThrustAssistMod.UI.Height = height;
            }
        }
    }
}
