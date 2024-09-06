namespace ThrustAssistMod
{
    public class Utility
    {
        public static double MetersToDegrees(double distance)
        {
            double radius = SFS.World.PlayerController.main.player.Value.location.Value.planet.Radius;

            if (radius*System.Math.PI>distance)
            {
                return 180.0*distance/(radius*System.Math.PI);
            }
            else
            {
                return 0;
            }
        }

        public static double NormaliseAngle(double input) => ((input + 540.0) % 360.0) - 180.0;
    }
}
