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
                double gravity = location.planet.GetGravity(location.Radius);
                double height =  location.TerrainHeight - rocket.GetSizeRadius();

                ThrustAssistMod.UI.AscentAcceleration = ascentAcceleration;
                ThrustAssistMod.UI.AscentVelocity = ascentVelocity;
                ThrustAssistMod.UI.Gravity = gravity;
                ThrustAssistMod.UI.Height = height;
            }
        }
    }
}
