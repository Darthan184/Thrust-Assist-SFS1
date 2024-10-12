namespace ThrustAssistMod
{
    public class Utility
    {
        public const double radiansPerDegree=0.01745329238474369;
        public const double degreesPerRadian=57.2957799569164486;

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

        public static double NormaliseAngle(double input) =>
            (
                (
                    System.Math.Sign(input) * (System.Math.Abs(input)  % 360)
                    + 540.0
                ) % 360.0
            ) - 180.0;
    }
}
